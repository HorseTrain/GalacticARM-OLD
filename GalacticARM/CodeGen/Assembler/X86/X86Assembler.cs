using GalacticARM.CodeGen.Intermediate;
using GalacticARM.Context;
using Iced.Intel;
using System;
using System.Collections.Generic;
using System.IO;
using static Iced.Intel.AssemblerRegisters;


namespace GalacticARM.CodeGen.Assembler.X86
{
    //RAX -> Return
    //RCX -> Operation Pointer
    public partial class X86Assembler
    {
        public Iced.Intel.Assembler c                   { get; set; }
        public RegisterSize CurrentSize                 { get; set; }

        public Operation CurrentOperation               { get; set; }

        public X86Assembler()
        {
            c = new Iced.Intel.Assembler(64);

            InitAllocater();
        }

        public byte[] GetMachineCode()
        {
            const ulong RIP = 0x1234_5678_1000_0000;
            var stream = new MemoryStream();
            c.Assemble(new StreamCodeWriter(stream), RIP);

            byte[] Out = new byte[stream.Length];

            Buffer.BlockCopy(stream.ToArray(),0,Out,0,(int)stream.Length);

            return Out;
        }

        public NativeFunction GetNativeFunction()
        {
            return new NativeFunction(GetMachineCode());
        }

        public Iced.Intel.Label[] Lables { get; set; }

        public void Compile(OperationBlock block)
        {
            c.mov(r15,rcx);

            Lables = new Iced.Intel.Label[block.Operations.Count];

            for (int i = 0; i < block.Operations.Count; i++)
            {
                Lables[i] = c.CreateLabel();
            }

            for (int i = 0; i < block.Operations.Count; i++)
            {
                Operation o = block.Operations[i];

                CurrentSize = RegisterSize._64;

                if (o.Size == OperationSize.Int32)
                    CurrentSize = RegisterSize._32;

                CurrentOperation = o;

                c.Label(ref Lables[i]);

                X86ILGenerator.Funcs[(int)block.Operations[i].Name](this);

                UnlockAllRegisters();
            }
        }

        public dynamic GetReg(int arg,int Size = -1)
        {
            return GetRegRaw(CurrentOperation.Arguments[arg].Reg,Size != -1,Size);
        }

        public dynamic GetImm(int arg)
        {
            if (CurrentOperation.Size == OperationSize.Int64)
                return CurrentOperation.Arguments[arg].Imm;

            return (uint)CurrentOperation.Arguments[arg].Imm;
        }

        public dynamic GetRCX()
        {
            if (CurrentOperation.Size == OperationSize.Int32)
                return ecx;

            return rcx;
        }

        public dynamic RegPointerRaw(int Arg) => __[r15 + (CurrentOperation.Arguments[Arg].Reg * 8)];
    }
}
