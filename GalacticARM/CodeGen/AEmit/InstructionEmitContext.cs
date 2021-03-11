using GalacticARM.CodeGen.Assembler.Msil;
using GalacticARM.CodeGen.Assembler.X86;
using GalacticARM.CodeGen.Intermediate;
using GalacticARM.Context;
using GalacticARM.Decoding;
using GalacticARM.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.AEmit
{
    public class InstructionEmitContext
    {
        public const int GuestRegisterCount = 32;
        public const int GuestSimdRegisterCount = 32 * 2;
        public const int MiscStart = GuestRegisterCount + GuestSimdRegisterCount;

        public const int GuestMiscCount = 20;

        public const int EvaluationSize = 20;   

        public OperationBlock Block         { get; set; }
        public AOpCode CurrentOpCode        { get; set; }
        public Operand ReturnArgument       { get; set; }
        public CompiledFunction MsilFunc    { get; set; }
        public ulong ArmSize                { get; set; }

        public InstructionEmitContext()
        {
            Block = new OperationBlock();

            Block.SetLocalOffset(GuestRegisterCount + GuestSimdRegisterCount + GuestMiscCount);

            ReturnArgument = Local();
        }

        public Operand ThrowUnknown()
        {
            throw new NotImplementedException($"{CurrentOpCode.Name} {MemoryManager.GetOpHex(CurrentOpCode.Address)}");
        }

        public void SetSize(InstructionInfo info)
        {
            Block.Size = OperationSize.Int64;

            if (info == InstructionInfo.W)
                Block.Size = OperationSize.Int32;
        }

        public Operand GetRegister(int reg, bool IsSP = false)
        {
            if ((reg == 31 && IsSP) || reg != 31)
            {
                Operand loc = Local();

                Block.Add(new Operation(ILInstruction.Copy,loc, Register(reg)));

                return loc; 
            }

            return Const(0);
        }

        public void SetRegister(int reg, Operand d, bool IsSP = false)
        {
            if ((reg == 31 && IsSP) || reg != 31)
            {
                Block.WriteRegister(reg,d);
            }
        }

        public void SetMisc(MiscRegister reg, Operand d)
        {
            Block.WriteRegister(MiscStart + (int)reg, d);
        }

        public Operand GetMisc(MiscRegister reg)
        {
            Operand loc = Block.Local();

            Block.Add(new Operation(ILInstruction.Copy, loc, Register(MiscStart + (int)reg)));

            return loc;
        }

        public void ReturnWithPC(Operand d)
        {
            Block.Add(new Operation(ILInstruction.Copy,ReturnArgument,d));
        }

        public Operand Add(Operand op0, Operand op1) => Block.AddOperation(ILInstruction.Add, op0, op1);
        public Operand And(Operand op0, Operand op1) => Block.AddOperation(ILInstruction.And, op0, op1);
        public Operand CompareEqual(Operand op0, Operand op1) => Block.AddOperation(ILInstruction.CompareEqual, op0, op1);
        public Operand CompareGreaterThan(Operand op0, Operand op1) => Block.AddOperation(ILInstruction.CompareGreaterThan, op0, op1);
        public Operand CompareGreaterThanUnsigned(Operand op0, Operand op1) => Block.AddOperation(ILInstruction.CompareGreaterThanUnsigned, op0, op1);
        public Operand CompareLessThan(Operand op0, Operand op1) => Block.AddOperation(ILInstruction.CompareLessThan, op0, op1);
        public Operand CompareLessThanUnsigned(Operand op0, Operand op1) => Block.AddOperation(ILInstruction.CompareLessThanUnsigned, op0, op1);
        public Operand Copy(Operand op0, Operand op1)
        {
            Block.AssertIsRegister(op0);
            Block.AssertIsRegister(op1);

            return Block.AddOperation(ILInstruction.Copy, op0, op1);
        }
        public Operand Divide(Operand op0, Operand op1) => Block.AddOperation(ILInstruction.Divide, op0, op1);
        public Operand Divide_Un(Operand op0, Operand op1) => Block.AddOperation(ILInstruction.Divide_Un, op0, op1);
        public Operand LoadMem(Operand op0, Operand op1) => Block.AddOperation(ILInstruction.LoadMem, op0, op1);
        public Operand LoadImmediate(Operand op0, Operand op1) => Block.AddOperation(ILInstruction.LoadImmediate, op0, op1);
        public Operand Mod(Operand op0, Operand op1) => Block.AddOperation(ILInstruction.Mod, op0, op1);
        public Operand Multiply(Operand op0, Operand op1) => Block.AddOperation(ILInstruction.Multiply, op0, op1);
        public Operand Not(Operand op0) => Block.AddOperation(ILInstruction.Not, op0);
        public Operand Or(Operand op0, Operand op1) => Block.AddOperation(ILInstruction.Or, op0, op1);
        public Operand ShiftLeft(Operand op0, Operand op1) => Block.AddOperation(ILInstruction.ShiftLeft, op0, op1);
        public Operand ShiftRight(Operand op0, Operand op1) => Block.AddOperation(ILInstruction.ShiftRight, op0, op1);
        public Operand ShiftRightSigned(Operand op0, Operand op1) => Block.AddOperation(ILInstruction.ShiftRightSigned, op0, op1);
        public Operand SignExtend16(Operand op0) => Block.AddOperation(ILInstruction.SignExtend16, op0);
        public Operand SignExtend32(Operand op0) => Block.AddOperation(ILInstruction.SignExtend32, op0);
        public Operand SignExtend8(Operand op0) => Block.AddOperation(ILInstruction.SignExtend8, op0);
        public Operand Subtract(Operand op0, Operand op1) => Block.AddOperation(ILInstruction.Subtract, op0, op1);
        public Operand Xor(Operand op0, Operand op1) => Block.AddOperation(ILInstruction.Xor, op0, op1);

        public Operand Register(int reg) => Block.Register(reg);
        public Operand Const(ulong imm = 0) => Block.Const(imm);
        public Operand Const(int imm = 0) => Block.Const((ulong)imm);
        public Operand Const(long imm = 0) => Block.Const((ulong)imm);

        public Operand CompareGreaterOrEqualUnsigned(Operand op0, Operand op1) => InvertBool(CompareLessThanUnsigned(op0, op1));

        public Operand CompareZero(Operand op0) => CompareEqual(op0, Const(0));
        public Operand InvertBool(Operand op0) => And(Not(op0),Const(1));
        public Operand RotateRight(Operand op0, Operand op1) => Or(ShiftRight(op0,op1),ShiftLeft(op0,Subtract(Const(Block.Size == OperationSize.Int32 ? 32 : 64),op1)));
        public Label CreateLabel() => Block.CreateLable();
        public void MarkLabel(Label label) => Block.MarkLabel(ref label);
        public void Jump(Label label) => Block.Jump(label);
        public void JumpIf(Label label, Operand test) => Block.JumpIf(label,test);

        public void SetPCImm(ulong imm)
        {
            ReturnWithPC(Const(imm));
        }

        public void AdvancePC()
        {
            SetPCImm(CurrentOpCode.Address + 4);
        }

        public Operand Local() => Block.Local();

        public Operand LoadMem(Operand Address)
        {
            return LoadMem(Local(),Address);
        }

        public void StoreMem(Operand Address, Operand Data, int size)
        {
            switch (size)
            {
                case 0: Block.Add(new Operation(ILInstruction.Store8,  Address, Data)); break;
                case 1: Block.Add(new Operation(ILInstruction.Store16, Address, Data)); break;
                case 2: Block.Add(new Operation(ILInstruction.Store32, Address, Data)); break;
                case 3: Block.Add(new Operation(ILInstruction.Store64, Address, Data)); break;
                default: ThrowUnknown(); break;
            }
        }

        public Operand GetContextPointer()
        {
            Operand Out = Local();

            Block.Add(new Operation(ILInstruction.GetContextPointer,Out));

            return Out;
        }

        public void CallFunctionFromPointer(params Operand[] CallArgumenrs)
        {
            SetSize(InstructionInfo.X);

            Operand[] Args = new Operand[CallArgumenrs.Length + 1];

            for (int i = 0; i < CallArgumenrs.Length; i++)
            {
                Args[i + 1] = CallArgumenrs[i];
            }

            Args[0] = Operand.Const((ulong)CallArgumenrs.Length - 1);

            //Arg 0 -> Function Type
            //Arg 1 -> Function
            //Arg 2... -> Arguments

            Block.Add(new Operation(ILInstruction.Call,Args));
        }

        public unsafe Operand GetDelegate(Delegate func) => Const((ulong)DelegateCache.GetOrAdd(func));

        public void SetFlags(Operand Source)
        {
            SetMisc(MiscRegister.N, And(ShiftRight(Source, Const(3)), Const(1)));
            SetMisc(MiscRegister.Z, And(ShiftRight(Source, Const(2)), Const(1)));
            SetMisc(MiscRegister.C, And(ShiftRight(Source, Const(1)), Const(1)));
            SetMisc(MiscRegister.V, And(ShiftRight(Source, Const(0)), Const(1)));
        }
    }
}
