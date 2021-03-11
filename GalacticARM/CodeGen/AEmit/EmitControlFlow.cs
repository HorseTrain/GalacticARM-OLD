using GalacticARM.CodeGen.Intermediate;
using GalacticARM.Decoding;
using GalacticARM.Decoding.Abstractions;
using System;

namespace GalacticARM.CodeGen.AEmit
{
    public class EmitControlFlow
    {
        public static Operand ConditionHolds(InstructionEmitContext context, Condition condition)
        {
            Operand Z = context.GetMisc(MiscRegister.Z);
            Operand C = context.GetMisc(MiscRegister.C);
            Operand N = context.GetMisc(MiscRegister.N);
            Operand V = context.GetMisc(MiscRegister.V);

            Operand IsOne(Operand f) => context.CompareEqual(f,context.Const(1));
            Operand IsZero(Operand f) => context.CompareEqual(f,context.Const(0));

            Operand Result = context.Const(0);

            switch ((Condition)((int)condition & ~1))
            {
                case Condition.EQ: Result = IsOne(Z); break;
                case Condition.CS: Result = IsOne(C); break;
                case Condition.MI: Result = IsOne(N); break;
                case Condition.VS: Result = IsOne(V); break;
                case Condition.HI: Result = context.And(IsOne(C), IsZero(Z)); break;
                case Condition.GE: Result = context.CompareEqual(N,V); break;
                case Condition.GT: Result = context.And(context.CompareEqual(N,V),IsZero(Z)); break;
                case Condition.AL: Result = context.Const(1); break;
            }

            if (((int)condition & 1) == 1 && (int)condition != 0b1111)
            {
                Result = context.InvertBool(Result);
            }

            return Result;
        }

        public delegate void func();

        public static void EmitIF(InstructionEmitContext context, Operand test, func yes, func no)
        {
            Label succ = context.CreateLabel();
            Label end = context.CreateLabel();

            context.JumpIf(succ,test);

            no();

            context.Jump(end);

            context.MarkLabel(succ);

            yes();

            context.MarkLabel(end);
        }

        public static void Csel(InstructionEmitContext context, bool invert, bool inc)
        {
            OpCodeConditionalSelect opCode = (OpCodeConditionalSelect)context.CurrentOpCode;

            Operand n = context.GetRegister(opCode.rn);
            Operand m = context.GetRegister(opCode.rm);

            if (invert)
            {
                m = context.Not(m);
            }

            if (inc)
            {
                m = context.Add(m, context.Const(1));
            }

            EmitIF(context, 
                
                ConditionHolds(context, (Condition)opCode.cond),
                
                delegate()
                {
                    context.SetRegister(opCode.rd,n);
                },

                delegate()
                {
                    context.SetRegister(opCode.rd,m);
                }
                
                );
        }

        public static void B_Cond(InstructionEmitContext context)
        {
            OpCodeConditionalBranchImmediate opCode = (OpCodeConditionalBranchImmediate)context.CurrentOpCode;

            ulong NewAddress = opCode.Address + (ulong)(((long)opCode.imm19 << 45) >> 43);

            EmitIF(context,
                
                ConditionHolds(context,(Condition)opCode.cond),

                delegate()
                {
                    context.SetPCImm(NewAddress);
                },

                delegate()
                {
                    context.AdvancePC();
                }
                
                );
        }

        public static void Cbz(InstructionEmitContext context, bool negate)
        {
            OpCodeCompareBranchImmediate opCode = (OpCodeCompareBranchImmediate)context.CurrentOpCode;

            ulong NewAddress = opCode.Address + (ulong)(((long)opCode.imm19 << 45) >> 43);

            Operand Test = context.CompareZero(context.GetRegister(opCode.rt));

            if (negate)
                Test = context.InvertBool(Test);

            EmitIF(context,

                Test,

                delegate ()
                {
                    context.SetPCImm(NewAddress);
                },

                delegate ()
                {
                    context.AdvancePC();
                }

                );
        }

        public static void Tbz(InstructionEmitContext context, bool negate)
        {
            OpCodeTestBitAndBranchImmediate opCode = (OpCodeTestBitAndBranchImmediate)context.CurrentOpCode;

            int bit = opCode.b40 | (opCode.b5 << 5);

            Operand t = context.GetRegister(opCode.rt);

            Operand test = context.And(context.ShiftRight(t,context.Const(bit)),context.Const(1));

            if (negate)
                test = context.InvertBool(test);

            EmitIF(context, test,

                delegate ()
                {
                    context.ReturnWithPC(context.Const(opCode.Address + (ulong)((((long)opCode.imm14) << 50) >> 48)));
                },

                delegate()
                {
                    context.AdvancePC();
                }

                );
        }

        //What is the difference between ret and br ?
        public static void Ret(InstructionEmitContext context)
        {
            OpCodeUnconditionalBranchRegister opCode = (OpCodeUnconditionalBranchRegister)context.CurrentOpCode;

            context.ReturnWithPC(context.GetRegister(opCode.rn));
        }

        public static void Br(InstructionEmitContext context, bool SetLink)
        {
            OpCodeUnconditionalBranchRegister opCode = (OpCodeUnconditionalBranchRegister)context.CurrentOpCode;

            if (SetLink)
            {
                context.SetRegister(30,context.Const(opCode.Address + 4));
            }

            context.ReturnWithPC(context.GetRegister(opCode.rn));
        }

        public static void Ccm(InstructionEmitContext context, bool negate)
        {
            Operand n = context.GetRegister(((context.CurrentOpCode.RawOpCode >> 5) & 0x1f));
            Operand m;

            Condition condition;
            int imm;

            if (context.CurrentOpCode is OpCodeConditionalCompareImmediate op)
            {
                m = context.Const(op.imm5);

                condition = (Condition)op.cond;
                imm = op.nzcv;
            }
            else if (context.CurrentOpCode is OpCodeConditionalCompareRegister opr)
            {
                m = context.GetRegister(opr.imm5);

                condition = (Condition)opr.cond;
                imm = opr.nzcv;
            }
            else
            {
                context.ThrowUnknown();

                return;
            }

            EmitIF(context, ConditionHolds(context, condition),
                
                delegate()
                {
                    Operand d;

                    if (negate)
                    {
                        d = context.Add(n, m);

                        context.SetMisc(MiscRegister.C, context.CompareLessThanUnsigned(d, n));
                        context.SetMisc(MiscRegister.V, context.CompareLessThan(context.And(context.Xor(d, n), context.Not(context.Xor(n, m))), context.Const(0)));
                    }
                    else
                    {
                        d = context.Subtract(n,m);

                        context.SetMisc(MiscRegister.C, context.CompareGreaterOrEqualUnsigned(n, m));
                        context.SetMisc(MiscRegister.V, context.CompareLessThan(context.And(context.Xor(d, n), context.Xor(n, m)), context.Const(0)));
                    }

                    EmitALU.CalculateNZ(context, d);
                },

                delegate()
                {
                    context.SetMisc(MiscRegister.N, context.Const((imm >> 3) & 1));
                    context.SetMisc(MiscRegister.Z, context.Const((imm >> 2) & 1));
                    context.SetMisc(MiscRegister.C, context.Const((imm >> 1) & 1));
                    context.SetMisc(MiscRegister.V, context.Const((imm) & 1));
                }
                
                );
        }
    }
}
