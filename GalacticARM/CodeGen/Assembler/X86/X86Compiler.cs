using GalacticARM.CodeGen.Intermediate;
using GalacticARM.Context;
using static Iced.Intel.AssemblerRegisters;
using Iced.Intel;
using System;
using System.IO;

namespace GalacticARM.CodeGen.Assembler.X86
{
    public partial class X86Compiler
    {
        public Iced.Intel.Assembler c;

        public Operation CurrentOperation;
        int ArmSize;

        public X86Compiler()
        {
            c = new Iced.Intel.Assembler(64);

            InitAllocator();
        }

        Iced.Intel.Label[] Labels;

        public void Compile(OperationBlock block)
        {
            Labels = new Iced.Intel.Label[block.Operations.Count];

            c.mov(r15,rcx);

            for (int i = 0; i < Labels.Length; i++)
            {
                Labels[i] = c.CreateLabel();
            }

            bool UL = false;

            for (int i = 0; i < block.Operations.Count; i++)
            {
                if (block.Operations[i].Name.ToString().Contains("Jump"))
                {
                    UL = true;
                }
            }

            //Console.WriteLine(UL);

            for (int i = 0; i < block.Operations.Count; i++)
            {
                c.Label(ref Labels[i]);

                CurrentOperation = block.Operations[i];

                UnlockAllRegisters();

                X86ILGenerator.Funcs[(int)CurrentOperation.Name](this);
                
                if (UL)
                UnloadAllRegisters();
            }

            
        }

        public NativeFunction GetNativeFunction()
        {
            MemoryStream stream = new MemoryStream();

            c.Assemble(new StreamCodeWriter(stream), 0x1234_5678_1000_0000);

            byte[] Out = new byte[stream.Length];

            Buffer.BlockCopy(stream.GetBuffer(),0,Out,0,Out.Length);

            return new NativeFunction(Out);
        }
    }
}
