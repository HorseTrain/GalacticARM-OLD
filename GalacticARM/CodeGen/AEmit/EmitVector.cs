using GalacticARM.CodeGen.Intermediate;
using GalacticARM.Decoding;
using GalacticARM.Decoding.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.AEmit
{
    public static class EmitVector
    {
        public static Operand GetVectorAddress(InstructionEmitContext context, int index) => context.Add(context.GetContextPointer(), context.Add(context.Const(InstructionEmitContext.GuestRegisterCount * 8,true),context.Const(index * 16)));

        public static void InsertIntToVector(InstructionEmitContext context, int reg, Operand data, int size, int index = 0)
        {
            Operand ptr = GetVectorAddress(context,reg);

            if (size >= 4)
            {
                context.ThrowUnknown();
            }

            context.StoreMem(context.Add(ptr,context.Const((1 << size) * index)),data,size);
        }

        public static void ClearVector(InstructionEmitContext context, int d)
        {
            InsertIntToVector(context,d,context.Const(0),3);
            InsertIntToVector(context, d, context.Const(0), 3,1);
        }

        public static Operand LoadIntFromVector(InstructionEmitContext context, int reg, int size, int index = 0)
        {
            Operand ptr = GetVectorAddress(context, reg);

            if (size >= 4)
            {
                context.ThrowUnknown();
            }

            Operand Extend(Operand src)
            {
                switch (size)
                {
                    case 0: return context.And(src, context.Const((uint)byte.MaxValue));
                    case 1: return context.And(src, context.Const((uint)ushort.MaxValue));
                    case 2: return context.And(src, context.Const(uint.MaxValue));
                    default: return src;
                }
            }

            return Extend(context.LoadMem(context.Add(ptr, context.Const((1 << size) * index))));
        }

        public static void Dup(InstructionEmitContext context)
        {
            OpCodeAdvsimdCopy opCode = (OpCodeAdvsimdCopy)context.CurrentOpCode;

            if (opCode.Info == Decoding.InstructionInfo.NULL)
            {
                Dup_general(context);
            }
            else
            {
                context.ThrowUnknown();
            }
        }

        public static void Ins(InstructionEmitContext context)
        {
            OpCodeAdvsimdCopy opCode = (OpCodeAdvsimdCopy)context.CurrentOpCode;

            int size = 0;
            int index = 0;

            if ((opCode.imm5 & 1) == 1)
            {
                size = 0;
            }
            else if ((opCode.imm5 & 0b11) == 0b10)
            {
                size = 1;
            }
            else if ((opCode.imm5 & 0b111) == 0b100)
            {
                size = 2;
            }
            else if ((opCode.imm5 & 0b1111) == 0b1000)
            {
                size = 3;
            }
            else
            {
                context.ThrowUnknown();
            }

            index = (opCode.imm5 >> (size + 1));

            Operand n = context.GetRegister(opCode.rn);

            InsertIntToVector(context,opCode.rd,n,size,index);
        }

        public static void Dup_general(InstructionEmitContext context)
        {
            OpCodeAdvsimdCopy opCode = (OpCodeAdvsimdCopy)context.CurrentOpCode;

            int size = 0;
            int q = 0;

            if ((opCode.imm5 & 1) == 1)
            {
                size = 0;
                q = 8;
            }
            else if ((opCode.imm5 & 0b11) == 0b10)
            {
                size = 1;
                q = 4;
            }
            else if ((opCode.imm5 & 0b111) == 0b100)
            {
                size = 2;
                q = 2;
            }
            else if ((opCode.imm5 & 0b1111) == 0b1000)
            {
                size = 3;
                q = 1;
            }
            else
            {
                context.ThrowUnknown();
            }

            Operand source = context.GetRegister(opCode.rn);

            if (opCode.q == 1)
                q *= 2;

            ClearVector(context,opCode.rd);

            for (int i = 0; i < q; i++)
            {
                InsertIntToVector(context,opCode.rd,source,size,i);
            }
        }

        public static ulong BuildScaledInt(int size, params int[] args)
        {
            size = (1 << size) - 1;

            ulong Out = 0;

            for (int i = 0; i < args.Length; i++)
            {
                Out |= ((ulong)(args[args.Length - i - 1] * size)) << (i << 3);
            }

            return Out;
        }

        public static void Movi(InstructionEmitContext context)
        {
            OpCodeAdvsimdModifiedImmediate opCode = (OpCodeAdvsimdModifiedImmediate)context.CurrentOpCode;

            int hi = ((opCode.RawOpCode >> 16) & 0x7);
            int low = ((opCode.RawOpCode >> 5) & 0x1F);

            ulong RawIMM = (ulong)((hi << 5) | low);

            switch (opCode.Info)
            {
                case InstructionInfo._8_bit:

                    int count = (opCode.q + 1) * 8;

                    Operand imm = context.Const(RawIMM);

                    ClearVector(context,opCode.rd);

                    for (int i = 0; i < count; i++)
                    {
                        InsertIntToVector(context,opCode.rd,imm,0,i);
                    }

                    break;

                case InstructionInfo._32_bit_shifted_immediate:

                    if (opCode.op == 1)
                    {
                        RawIMM = ~RawIMM;
                    }

                    int shift = ((opCode.cmode >> 1) & 0x3) << 3;

                    ClearVector(context,opCode.rd);

                    for (int i = 0; i < (opCode.q + 1) * 2; i++)
                    {
                        InsertIntToVector(context,opCode.rd,context.Const(RawIMM),2,i);
                    }

                    break;

                case InstructionInfo._64_bit_vector:

                    count = (opCode.q + 1);

                    imm = context.Const(BuildScaledInt(8, opCode.a, opCode.b, opCode.c, opCode.d, opCode.e, opCode.f, opCode.g, opCode.h));

                    ClearVector(context,opCode.rd);

                    for (int i = 0; i < count; i++)
                    {
                        InsertIntToVector(context,opCode.rd,imm,3,i);
                    }

                    break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void Vector_3_Same(InstructionEmitContext context, ILInstruction instruction, bool Not)
        {
            OpCodeAdvsimdThreeSame opCode = (OpCodeAdvsimdThreeSame)context.CurrentOpCode;

            int count = opCode.q + 1;

            List<Operand> Out = new List<Operand>();

            for (int i = 0; i < count; i++)
            {
                Operand n = LoadIntFromVector(context,opCode.rn,3,i);
                Operand m = LoadIntFromVector(context, opCode.rm, 3, i);

                if (Not)
                    m = context.Not(m);

                context.Block.Add(new Operation(instruction,n,m));

                Out.Add(n);
            }

            ClearVector(context,opCode.rd);

            for (int i = 0; i < count; i++)
            {
                InsertIntToVector(context,opCode.rd,Out[i],3,i);
            }
        }

        public static void Cnt(InstructionEmitContext context)
        {
            OpCodeAdvsimdTwoRegMisc opCode = (OpCodeAdvsimdTwoRegMisc)context.CurrentOpCode;

            context.CallFunctionFromPointer(context.GetDelegate(new _Void_U_U(Cnt_FB)),GetVectorAddress(context,opCode.rd), GetVectorAddress(context, opCode.rn));
        }

        public static int CountBits(byte b)
        {
            int res = 0;

            for (int i = 0; i < 8; i++)
            {
                res += ((b >> i) & 1);
            }

            return res;
        }

        public unsafe static void Cnt_FB(ulong des, ulong src)
        {
            //Look into more.
            Vector128<byte>* Des = (Vector128<byte>*)des;
            Vector128<byte>* Src = (Vector128<byte>*)src;

            Vector128<byte> res = new Vector128<byte>();

            for (int i = 0; i < 16; i++)
            {
                int bitcount = CountBits((*Src).GetElement(i));

                res = res.WithElement(i, (byte)bitcount);
            }

            *Des = res;
        }

        public static void Uaddlv(InstructionEmitContext context)
        {
            OpCodeAdvsimdAcrossLanes opCode = (OpCodeAdvsimdAcrossLanes)context.CurrentOpCode;

            int count = (opCode.q + 1) * (8 >> opCode.size);

            Operand res = context.Const(0);

            for (int i = 0; i < count; i++)
            {
                res = context.Add(res,LoadIntFromVector(context,opCode.rn,opCode.size,i));
            }

            ClearVector(context,opCode.rd);
            InsertIntToVector(context,opCode.rd,res,3);
        }
    }
}
