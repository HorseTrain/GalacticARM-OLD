using ARMeilleure.Decoders;
using GalacticARM.CodeGen.Intermediate;
using GalacticARM.Context;
using GalacticARM.Decoding;
using GalacticARM.Decoding.Abstractions;
using GalacticARM.Memory;
using System;
using System.Collections.Generic;
using UltimateOrb;

namespace GalacticARM.CodeGen.AEmit
{
    public unsafe static class EmitALU
    {
        public static Operand Shift(InstructionEmitContext context, Operand m, int imm, ShiftType shift)
        {
            switch (shift)
            {
                case ShiftType.LSL: return context.ShiftLeft(m, context.Const((ulong)imm));
                case ShiftType.LSR: return context.ShiftRight(m, context.Const((ulong)imm));
                case ShiftType.ASR: return context.ShiftRightSigned(m, context.Const((ulong)imm));
                case ShiftType.ROR: return context.Or(context.ShiftRight(m, context.Const((ulong)imm)), context.ShiftLeft(m, context.Const((ulong)(context.Block.Size == OperationSize.Int32 ? 32 - imm : 64 - imm))));
                default: throw new NotImplementedException();
            }
        }

        public static Operand Shift(InstructionEmitContext context, Operand m, Operand n, ShiftType shift)
        {
            switch (shift)
            {
                case ShiftType.LSL: return context.ShiftLeft(m, n);
                case ShiftType.LSR: return context.ShiftRight(m, n);
                case ShiftType.ASR: return context.ShiftRightSigned(m, n);
                case ShiftType.ROR: return context.RotateRight(m,n);
                default: throw new NotImplementedException();
            }
        }

        public static Operand Extend(InstructionEmitContext context, Operand value, IntType type)
        {
            switch (type)
            {
                case IntType.UInt8: value = context.And(value,  context.Const((ulong)(byte.MaxValue))); break;
                case IntType.UInt16: value = context.And(value, context.Const((ulong)(ushort.MaxValue))); break;
                case IntType.UInt32: value = context.And(value, context.Const((ulong)(uint.MaxValue)));  break;

                case IntType.Int8: value = context.SignExtend8(value); break;
                case IntType.Int16: value = context.SignExtend16(value); break;
                case IntType.Int32: value = context.SignExtend32(value); break;
            }

            return value;
        }

        public static Operand GetAluN(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode)
            {
                case OpCodeAddSubtractImmediate op:
                {
                    return context.GetRegister(op.rn, true);
                }

                case OpCodeAddSubtractShiftedRegister op:
                {
                    return context.GetRegister(op.rn, false);
                }

                case OpCodeLogicalShiftedRegister op:
                {
                    return context.GetRegister(op.rn, false);
                }

                case OpCodeLogicalImmediate op:
                {
                    return context.GetRegister(op.rn, false);
                }

                case OpCodeAddSubtractExtendedRegister op:
                {
                    return context.GetRegister(op.rn, true);
                }

                default: return context.ThrowUnknown();
            }
        }

        public static Operand GetAluM(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode)
            {
                case OpCodeAddSubtractImmediate op:
                {
                    return context.Const((ulong)op.imm12 << (op.shift * 12));
                }

                case OpCodeAddSubtractShiftedRegister op:
                {
                    return Shift(context, context.GetRegister(op.rm, false), op.imm6, (ShiftType)op.shift);
                }

                case OpCodeLogicalShiftedRegister op:
                {
                    return Shift(context, context.GetRegister(op.rm, false), op.imm6, (ShiftType)op.shift);
                }

                case OpCodeLogicalImmediate op:
                {
                    var mask = DecoderHelper.DecodeBitMask(op.RawOpCode, true);

                    return context.Const(mask.WMask);
                }

                case OpCodeAddSubtractExtendedRegister op:
                {
                    Operand m = context.GetRegister(op.rm);

                    m = context.ShiftLeft(Extend(context, m, (IntType)op.option), context.Const(op.imm3));

                    return m;
                }

                default: return context.ThrowUnknown();
            }
        }

        public static int GetALUD(InstructionEmitContext context)
        {
            return context.CurrentOpCode.RawOpCode & 0x1f;
        }

        public static void Add(InstructionEmitContext context, bool IsSP)
        {
            context.SetRegister(GetALUD(context), context.Add(GetAluN(context), GetAluM(context)), IsSP);
        }

        public static void And(InstructionEmitContext context, bool Invert, bool IsSp)
        {
            Operand m = GetAluM(context);

            if (Invert)
                m = context.Not(m);

            context.SetRegister(GetALUD(context), context.And(GetAluN(context), m), IsSp);
        }

        public static void Ands(InstructionEmitContext context, bool Not)
        {
            Operand n = GetAluN(context);
            Operand m = GetAluM(context);

            if (Not)
            {
                m = context.Not(m);
            }

            Operand d = context.And(n, m);

            context.SetRegister(GetALUD(context), d);

            context.SetMisc(MiscRegister.V, context.Const(0));
            context.SetMisc(MiscRegister.C, context.Const(0));

            CalculateNZ(context, d);
        }

        public static void Orr(InstructionEmitContext context, bool Invert, bool IsSp)
        {
            Operand m = GetAluM(context);

            if (Invert)
                m = context.Not(m);

            context.SetRegister(GetALUD(context), context.Or(GetAluN(context), m), IsSp);
        }

        public static void Eor(InstructionEmitContext context, bool Invert, bool IsSp)
        {
            Operand m = GetAluM(context);

            if (Invert)
                m = context.Not(m);

            context.SetRegister(GetALUD(context), context.Xor(GetAluN(context), m), IsSp);
        }

        public static void CalculateNZ(InstructionEmitContext context, Operand d)
        {
            context.SetMisc(MiscRegister.Z, context.CompareZero(d));
            context.SetMisc(MiscRegister.N, context.CompareLessThan(d, context.Const(0)));
        }

        public static void Adds(InstructionEmitContext context)
        {
            Operand n = GetAluN(context);
            Operand m = GetAluM(context);

            Operand d = context.Add(n, m);

            CalculateNZ(context, d);

            context.SetMisc(MiscRegister.C, context.CompareLessThanUnsigned(d, n));
            context.SetMisc(MiscRegister.V, context.CompareLessThan(context.And(context.Xor(d,n),context.Not(context.Xor(n,m))),context.Const(0)));

            context.SetRegister(GetALUD(context), d);
        }

        public static void Sub(InstructionEmitContext context, bool IsSP)
        {
            context.SetRegister(GetALUD(context), context.Subtract(GetAluN(context), GetAluM(context)), IsSP);
        }

        public static void Subs(InstructionEmitContext context)
        {
            Operand n = GetAluN(context);
            Operand m = GetAluM(context);

            Operand d = context.Subtract(n, m);

            CalculateNZ(context, d);

            context.SetMisc(MiscRegister.C, context.CompareGreaterOrEqualUnsigned(n, m));
            context.SetMisc(MiscRegister.V, context.CompareLessThan(context.And(context.Xor(d,n),context.Xor(n,m)), context.Const(0)));

            context.SetRegister(GetALUD(context), d);
        }

        public static void Bfm(InstructionEmitContext context)
        {
            OpCodeBitfield opCode = (OpCodeBitfield)context.CurrentOpCode;

            var mask = DecoderHelper.DecodeBitMask(opCode.RawOpCode, false);

            Operand dst = context.GetRegister(opCode.rd);
            Operand src = context.GetRegister(opCode.rn);

            Operand bot = context.Or(context.And(dst, context.Not(context.Const(mask.WMask))), context.And(context.RotateRight(src, context.Const(opCode.immr)), context.Const(mask.WMask)));

            context.SetRegister(opCode.rd, context.Or(context.And(dst, context.Not(context.Const(mask.TMask))), context.And(bot, context.Const(mask.TMask))));
        }

        public static void Sbfm(InstructionEmitContext context)
        {
            OpCodeBitfield opCode = (OpCodeBitfield)context.CurrentOpCode;

            var mask = DecoderHelper.DecodeBitMask(opCode.RawOpCode, false);

            Operand src = context.GetRegister(opCode.rn);

            Operand bot = context.And(context.RotateRight(src, context.Const(opCode.immr)), context.Const((ulong)mask.WMask));

            Operand top = context.Subtract(context.Const(0), context.And(context.ShiftRight(src, context.Const(opCode.imms)), context.Const(1)));

            context.SetRegister(opCode.rd, context.Or(context.And(top, context.Not(context.Const(mask.TMask))), context.And(bot, context.Const(mask.TMask))));
        }

        public static void Ubfm(InstructionEmitContext context)
        {
            OpCodeBitfield opCode = (OpCodeBitfield)context.CurrentOpCode;

            var mask = DecoderHelper.DecodeBitMask(opCode.RawOpCode, false);

            Operand src = context.GetRegister(opCode.rn);

            Operand bot = context.And(context.RotateRight(src, context.Const(opCode.immr)), context.Const(mask.WMask));

            context.SetRegister(opCode.rd, context.And(bot, context.Const(mask.TMask)));
        }

        public static void Mul(InstructionEmitContext context, bool IsAdd)
        {
            OpCodeDataProcessing3Source opCode = (OpCodeDataProcessing3Source)context.CurrentOpCode;

            Operand n = context.GetRegister(opCode.rn);
            Operand m = context.GetRegister(opCode.rm);
            Operand a = context.GetRegister(opCode.ra);

            Operand d = context.Multiply(n, m);

            d = IsAdd ? context.Add(a, d) : context.Subtract(a, d);

            context.SetRegister(opCode.rd, d);
        }

        public static void Mull(InstructionEmitContext context, bool IsAdd, bool Signed)
        {
            OpCodeDataProcessing3Source opCode = (OpCodeDataProcessing3Source)context.CurrentOpCode;

            Operand GetUnsignedOrSignedExtended(int src)
            {
                Operand o = context.GetRegister(src);

                if (Signed)
                {
                    return context.SignExtend32(o);
                }

                return context.And(o, context.Const(uint.MaxValue));
            }

            Operand n = GetUnsignedOrSignedExtended(opCode.rn);
            Operand m = GetUnsignedOrSignedExtended(opCode.rm);
            Operand a = context.GetRegister(opCode.ra);

            Operand d = context.Multiply(n, m);

            d = IsAdd ? context.Add(a, d) : context.Subtract(a, d);

            context.SetRegister(opCode.rd, d);
        }

        public static void Mulh(InstructionEmitContext context, bool signed)
        {
            OpCodeDataProcessing3Source opCode = (OpCodeDataProcessing3Source)context.CurrentOpCode;

            ulong Args = 0;

            Args = (ulong)opCode.rn;
            Args |= (ulong)opCode.rm << 5;
            Args |= (ulong)opCode.rd << 10;

            if (signed)
                Args |= 1UL << 15;

            context.CallFunctionFromPointer(context.Const((ulong)DelegateCache.GetOrAdd(new _Void_U_U(mulh))),context.GetContextPointer(),context.Const(Args));
        }

        public static void mulh(ulong Block, ulong args)
        {
            ContextBlock* block = (ContextBlock*)Block;

            ulong n = args & 0x1f;
            ulong m = (args >> 5) & 0x1f;
            ulong des = (args >> 10) & 0x1f;
            bool Signed = ((args >> 15) & 1) == 1;

            ulong N = block->RegisterBuffer[n];
            ulong M = block->RegisterBuffer[m];

            if (Signed)
            {
                Int128 nn = (long)N;
                Int128 mm = (long)M;

                Int128 dd = nn * mm;

                block->RegisterBuffer[des] = (ulong)((dd >> 64) & ulong.MaxValue);
            }
            else
            {
                UInt128 nn = N;
                UInt128 mm = M;

                UInt128 dd = nn * mm;

                block->RegisterBuffer[des] = (ulong)((dd >> 64) & ulong.MaxValue);
            }
        }

        public static void Adr(InstructionEmitContext context, bool Paged)
        {
            OpCodePCRelAddressing opCode = (OpCodePCRelAddressing)context.CurrentOpCode;

            ulong imm = (ulong)((((long)opCode.RawOpCode << 40) >> 43) & ~3);
            imm |= (ulong)(((long)opCode.RawOpCode >> 29) & 3);

            Operand d = context.Const(Paged ? (opCode.Address + (imm << 12)) & ~0xFFFUL : opCode.Address + imm);

            context.SetRegister(opCode.rd, d);
        }

        public static void Mov(InstructionEmitContext context, bool Not)
        {
            OpCodeMoveWideImmediate opCode = (OpCodeMoveWideImmediate)context.CurrentOpCode;

            ulong dat = ((ulong)opCode.imm16) << (opCode.hw * 16);

            if (Not)
                dat = ~dat;

            context.SetRegister(opCode.rd, context.Const(dat));
        }

        public static void MovK(InstructionEmitContext context)
        {
            OpCodeMoveWideImmediate opCode = (OpCodeMoveWideImmediate)context.CurrentOpCode;

            ulong dat = ((ulong)opCode.imm16) << (opCode.hw * 16);

            Operand src = context.GetRegister(opCode.rd);

            src = context.And(src, context.Const(~(((ulong)ushort.MaxValue) << (opCode.hw * 16))));
            src = context.Or(src, context.Const(dat));

            context.SetRegister(opCode.rd, src);
        }

        public static void Div(InstructionEmitContext context, bool Signed)
        {
            OpCodeDataProcessing2Source opCode = (OpCodeDataProcessing2Source)context.CurrentOpCode;

            Operand n = context.GetRegister(opCode.rn);
            Operand m = context.GetRegister(opCode.rm);

            EmitControlFlow.EmitIF(context,

                context.CompareZero(m),

                delegate ()
                {
                    context.SetRegister(opCode.rd,context.Const(0));
                },

                delegate()
                {
                    if (Signed)
                    {
                        context.SetRegister(opCode.rd,context.Divide(n,m));
                    }
                    else
                    {
                        context.SetRegister(opCode.rd,context.Divide_Un(n,m));
                    }
                }
                
                );
        }

        public static void ShiftV(InstructionEmitContext context, ShiftType Type)
        {
            OpCodeDataProcessing2Source opCode = (OpCodeDataProcessing2Source)context.CurrentOpCode;

            Operand n = context.GetRegister(opCode.rn);
            Operand m = context.GetRegister(opCode.rm);

            m = context.Mod(m, context.Const(context.Block.Size == OperationSize.Int32 ? 32 : 64));

            context.SetRegister(opCode.rd,Shift(context,n,m,Type));
        }

        public static void Rbit(InstructionEmitContext context)
        {
            OpCodeDataProcessing1Source opCode = (OpCodeDataProcessing1Source)context.CurrentOpCode;

            Operand value = context.GetRegister(opCode.rn);

            if (context.Block.Size == OperationSize.Int64)
            {
                value = context.Or(context.ShiftRight(context.And(value,context.Const(0xaaaaaaaaaaaaaaaa)),  context.Const(1)),context.ShiftLeft(context.And(value,context.Const(0x5555555555555555)),context.Const(1)));
                value = context.Or(context.ShiftRight(context.And(value, context.Const(0xcccccccccccccccc)), context.Const(2)), context.ShiftLeft(context.And(value, context.Const(0x3333333333333333)), context.Const(2)));
                value = context.Or(context.ShiftRight(context.And(value, context.Const(0xf0f0f0f0f0f0f0f0)), context.Const(4)), context.ShiftLeft(context.And(value, context.Const(0x0f0f0f0f0f0f0f0f)), context.Const(4)));
                value = context.Or(context.ShiftRight(context.And(value, context.Const(0xff00ff00ff00ff00)), context.Const(8)), context.ShiftLeft(context.And(value, context.Const(0x00ff00ff00ff00ff)), context.Const(8)));
                value = context.Or(context.ShiftRight(context.And(value, context.Const(0xffff0000ffff0000)), context.Const(16)), context.ShiftLeft(context.And(value, context.Const(0x0000ffff0000ffff)), context.Const(16)));

                value = context.Or(context.ShiftRight(value,context.Const(32)),context.ShiftLeft(value,context.Const(32)));
            }
            else if (context.Block.Size == OperationSize.Int32)
            {
                value = context.Or(context.ShiftRight(context.And(value, context.Const(0xaaaaaaaa)), context.Const(1)), context.ShiftLeft(context.And(value, context.Const(0x55555555)), context.Const(1)));
                value = context.Or(context.ShiftRight(context.And(value, context.Const(0xcccccccc)), context.Const(2)), context.ShiftLeft(context.And(value, context.Const(0x33333333)), context.Const(2)));
                value = context.Or(context.ShiftRight(context.And(value, context.Const(0xf0f0f0f0)), context.Const(4)), context.ShiftLeft(context.And(value, context.Const(0x0f0f0f0f)), context.Const(4)));
                value = context.Or(context.ShiftRight(context.And(value, context.Const(0xff00ff00)), context.Const(8)), context.ShiftLeft(context.And(value, context.Const(0x00ff00ff)), context.Const(8)));

                value = context.Or(context.ShiftRight(value, context.Const(16)), context.ShiftLeft(value, context.Const(16)));
            }
            else
            {
                context.ThrowUnknown();
            }

            context.SetRegister(opCode.rd,value);
        }

        public static void Clz(InstructionEmitContext context)
        {
            context.SetSize(InstructionInfo.X);

            OpCodeDataProcessing1Source opCode = (OpCodeDataProcessing1Source)context.CurrentOpCode;

            ulong Ptr = (ulong)DelegateCache.GetOrAdd(new _Void_U_U_U_U(CountLeadingZeros));

            Operand des = context.Local();
            Operand source = context.GetRegister(opCode.rn);
            Operand size = context.Const(opCode.Info == InstructionInfo.W ? 32 : 64);

            context.CallFunctionFromPointer(context.Const(Ptr), context.GetContextPointer(),context.Const(des.Reg) ,source,size);

            context.SetRegister(opCode.rd,des);
        }

        static void CountLeadingZeros(ulong Block, ulong Des, ulong Source, ulong Size)
        {
            ContextBlock* block = (ContextBlock*)Block;

            int Out = 0;

            for (int i = (int)Size - 1; i >= 0; i--)
            {
                if (((Source >> i) & 1) == 0)
                {
                    Out++;
                }
                else
                {
                    break;
                }
            }

            block->RegisterBuffer[Des] = (ulong)Out;
        }
    }
}
