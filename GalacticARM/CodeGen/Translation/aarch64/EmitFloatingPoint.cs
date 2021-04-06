using GalacticARM.Decoding;
using GalacticARM.IntermediateRepresentation;
using GalacticARM.Runtime.Fallbacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Translation.aarch64
{
    public static partial class Emit64
    {
        public static void Fmov_General(TranslationContext context)
        {
            int sf = context.GetRaw("sf");
            int ftype = context.GetRaw("ftype");
            int rmode = context.GetRaw("rmode");
            int opcode = context.GetRaw("opcode");

            int rd = context.GetRaw("rd");
            int rn = context.GetRaw("rn");

            if (rmode == 0 && opcode == 1)
            {
                //gp -> vector scalar

                Operand vec = context.CreateVector();

                Operand n = context.GetRegister("rn");

                context.SetVectorElement(vec, n, 0, ftype + 2);

                context.SetVector(rd, vec);
            }
            else if (rmode == 0 && opcode == 0)
            {
                //vector scalar -> gp

                Operand vec = context.GetVector(rn);

                Operand n = context.GetVectorElement(vec, 0, 2 + ftype);//Fallbacks.GetVectorElement(context, rn, 0, 2 + ftype);

                context.SetRegister("rd", n);
            }
            else
            {
                context.ThrowUnknown();
            }
        }

        public static void Scvtf_Scalar_Integer(TranslationContext context) => CVTF_Scalar_Integer(context, true);
        public static void Ucvtf_Scalar_Integer(TranslationContext context) => CVTF_Scalar_Integer(context, false);

        public static void Fcvtzs_Scalar_Fixed(TranslationContext context) => Fcvtz_Scalar_Fixed(context, true, true);
        public static void Fcvtzu_Scalar_Fixed(TranslationContext context) => Fcvtz_Scalar_Fixed(context, false, true);

        public static void Fcvtzs_Scalar_Integer(TranslationContext context) => Fcvt_Scalar_Integer(context, true, true);
        public static void Fcvtzu_Scalar_Integer(TranslationContext context) => Fcvt_Scalar_Integer(context, false, true);
        public static void Fadd_Scalar(TranslationContext context) => FloatDSS(context, Instruction.Vector_Fadd);
        public static void Fdiv_Scalar(TranslationContext context) => FloatDSS(context, Instruction.Vector_Fdiv);
        public static void Fmul_Scalar(TranslationContext context) => FloatDSS(context, Instruction.Vector_Fmul);
        public static void Fnmul_Scalar(TranslationContext context) => FloatDSS(context, Instruction.Vector_Fmul, true);
        public static void Fsub_Scalar(TranslationContext context) => FloatDSS(context, Instruction.Vector_Fsub);
        public static void Fmax(TranslationContext context) => FCompare(context, true);
        public static void Fmin(TranslationContext context) => FCompare(context, false);
        public static void Ucvtf_Vector_Integer(TranslationContext context) => Cvtf_Vector_Integer(context, false);
        public static void Scvtf_Vector_Integer(TranslationContext context) => Cvtf_Vector_Integer(context, true);
        public static void Fcvtms_Scalar(TranslationContext context) => Fcvtm_Scalar(context, true, true);
        public static void Fcvtps_Scalar(TranslationContext context) => Fcvtm_Scalar(context, false, true);

        static void CVTF_Scalar_Integer(TranslationContext context, bool Singed)
        {
            int rn = context.GetRaw("rn");
            int rd = context.GetRaw("rd");

            int ftype = 2 + context.GetRaw("ftype");
            int sf = 2 + context.GetRaw("sf");

            Operand n = context.GetRegister("rn");

            Operand vec = context.CreateVector();

            context.ConvertToFloat(vec, n, sf, ftype, Singed);

            context.SetVector(rd, vec);
        }

        public static void Fcvtz_Scalar_Fixed(TranslationContext context, bool Singed, bool TowardZero)
        {
            int rd = context.GetRaw("rd");
            int rn = context.GetRaw("rn");

            int ftype = context.GetRaw("ftype") + 2;
            int sf = context.GetRaw("sf") + 2;

            int fbits = 64 - context.GetRaw("scale");

            Operand Source = context.GetVector(rn);

            if (fbits != 0)
            {
                ulong Mul = FloatImmOnSize((float)(1 << fbits),ftype);

                Source = context.ScalarOperation(Source,context.CreateVectorWith(Mul,ftype),ftype,Instruction.Vector_Fmul);
            }

            context.SetArgument(0, rd);
            context.SetArgument(1, context.GetVectorElement(Source,0,ftype));
            context.SetArgument(2, ftype);
            context.SetArgument(3, sf);
            context.SetArgument(4, Singed ? 1 : 0);
            context.SetArgument(5, TowardZero ? 1 : 0);

            context.Call(nameof(FallbackFloat.FB_Fcvtz_Scalar_Fixed),context.ContextPointer());
        }

        public static void Fcvt_Scalar_Integer(TranslationContext context, bool Singed, bool TowardsZero)
        {
            int rd = context.GetRaw("rd");
            int rn = context.GetRaw("rn");

            int ftype = context.GetRaw("ftype") + 2;
            int sf = context.GetRaw("sf") + 2;

            if (!TowardsZero)
                context.ThrowUnknown();

            Operand Ignore = context.CreateLabel();

            Operand Source = context.GetVector(rn);

            if (!Singed)
            {
                Operand flt = context.GetVectorElement(Source,0,ftype);

                //Check if neg. If yes, ignore and write zero.
                flt = context.And(context.ShiftRight(flt, ftype == 2 ? 31 : 63),1);

                context.SetRegister(rd,0);

                context.JumpIf(flt,Ignore);
            }

            Operand Result = context.CreateLocal(context.ConverToInt(Source,ftype,sf));

            context.SetRegister("rd",Result);

            context.MarkLabel(Ignore);
        }


        //NOTE: Add cel and floor
        public static void Fcvtm_Scalar(TranslationContext context, bool IsFloor, bool Singed)
        {
            int rd = context.GetRaw("rd");
            int rn = context.GetRaw("rn");

            int sf = context.GetRaw("sf") + 2;
            int ftype = context.GetRaw("ftype") + 2;

            Operand Source = context.GetVector(rn);

            if (!Singed)
            {
                context.ThrowUnknown();
            }

            Source = context.ConverToInt(Source, ftype, sf);

            context.SetRegister("rd", Source);
        }

        public static void FCompare(TranslationContext context, bool ISMax)
        {
            int type = context.GetRaw("ftype");

            int rd = context.GetRaw("rd");
            int rn = context.GetRaw("rn");
            int rm = context.GetRaw("rm");

            type += 2;

            Operand n = context.GetVector(rn);
            Operand m = context.GetVector(rm);

            Operand res = context.CreateVector();

            context.SetArgument(0, rn);
            context.SetArgument(1, rm);
            context.SetArgument(2, ISMax ? 1 : 0);
            context.SetArgument(3, type);

            EmitUniversal.EmitIf(context, context.Call(nameof(FallbackFloat.FCompare), context.ContextPointer()),

                delegate ()
                {
                    context.SetVectorElement(res, context.GetVectorElement(n, 0, type), 0, type, true);
                },

                delegate ()
                {
                    context.SetVectorElement(res, context.GetVectorElement(m, 0, type), 0, type, true);
                }

                );

            context.SetVector(rd, res);
        }

        public static void Fccmp(TranslationContext context)
        {
            int type = context.GetRaw("ftype") + 2;

            int rn = context.GetRaw("rn");
            int rm = context.GetRaw("rm");

            Condition cond = (Condition)context.GetRaw("cond");

            int nzcv = context.GetRaw("nzcv");

            EmitUniversal.EmitIf(context,

                ConditionHolds(context, cond),

                delegate ()
                {
                    context.SetArgument(0, rn);
                    context.SetArgument(1, rm);
                    context.SetArgument(2, type);

                    context.Call(nameof(FallbackFloat.Fcmp), context.ContextPointer());
                },

                delegate ()
                {
                    SetFlagsImm(context, (ulong)nzcv);
                }

                );
        }

        public static void FloatDSS(TranslationContext context, Instruction op, bool neg = false)
        {
            int rn = context.GetRaw("rn");
            int rm = context.GetRaw("rm");
            int rd = context.GetRaw("rd");

            int type = context.GetRaw("ftype") + 2;

            Operand n = context.GetVector(rn);
            Operand m = context.GetVector(rm);

            if (neg)
            {
                ulong bit = 1UL << ((8 << type) - 1);

                Operand mm = context.GetVectorElement(m,0, type);

                mm = context.Xor(mm, bit);

                context.SetVectorElement(m,mm,0, type);
            }

            Operand d = context.ScalarOperation(n, m, type, op);

            context.SetVector(rd, d);
        }

        public static void Fcvt(TranslationContext context)
        {
            int rd = context.GetRaw("rd");
            int rn = context.GetRaw("rn");

            int ftype = context.GetRaw("ftype") + 2;
            int opc = context.GetRaw("opc") + 2;

            context.SetArgument(0, rd);
            context.SetArgument(1, rn);
            context.SetArgument(2, ftype);
            context.SetArgument(3, opc);

            context.Call(nameof(FallbackFloat.ConvertPerc),context.ContextPointer());
        }

        public static void Fcsel(TranslationContext context)
        {
            int type = context.GetRaw("ftype");

            int rn = context.GetRaw("rn");
            int rm = context.GetRaw("rm");
            int rd = context.GetRaw("rd");

            Condition cond = (Condition)context.GetRaw("cond");

            int scale = 2 + type;

            Operand n = context.GetVectorElement(context.GetVector(rn), 0, scale);
            Operand m = context.GetVectorElement(context.GetVector(rm), 0, scale);

            Operand d = context.CreateVector();

            EmitUniversal.EmitIf(context,

                ConditionHolds(context, cond),

                delegate ()
                {
                    context.SetVectorElement(d, n, 0, scale);
                },

                delegate ()
                {
                    context.SetVectorElement(d, m, 0, scale);
                }

                );

            context.SetVector(rd, d);
        }

        public static void Fmov_Imm(TranslationContext context)
        {
            int rd = context.GetRaw("rd");

            int imm = context.GetRaw("imm");

            int size = context.GetRaw("ftype") + 2;

            Operand d = context.CreateVector();

            if (size == 2)
            {
                uint dat = ((uint)((uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(((uint)((uint)((uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(((uint)((byte)((byte)(0x0)))) << 0)) | ((uint)(((uint)((byte)((byte)(0x0)))) << 1)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 2)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 3)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 4)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 5)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 6)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 7)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 8)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 9)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 10)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 11)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 12)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 13)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 14)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 15)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 16)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 17)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 18)))))) << 0)) | ((uint)(((uint)((byte)((byte)((byte)((((ulong)(imm)) & ((ulong)(0xF)))))))) << 19)))) | ((uint)(((uint)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x4)))) & ((ulong)(0x3)))))))) << 23)))) | ((uint)(((uint)((byte)((byte)(((byte)(byte)(((byte)(byte)(((byte)(byte)(((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 0)) | ((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 1)))) | ((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 2)))) | ((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 3)))) | ((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 4)))))) << 25)))) | ((uint)(((uint)((byte)(((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1))))) != 0 ? 0U : 1U))) << 30)))) | ((uint)(((uint)((byte)((byte)((byte)((imm) >> (int)(0x7)))))) << 31)))));

                context.SetVectorElement(d,dat,0,2);
            }
            else if (size == 3)
            {
                ulong dat = ((ulong)((ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(((ulong)((ulong)((ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(((ulong)((byte)((byte)(0x0)))) << 0)) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 1)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 2)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 3)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 4)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 5)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 6)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 7)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 8)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 9)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 10)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 11)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 12)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 13)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 14)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 15)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 16)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 17)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 18)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 19)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 20)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 21)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 22)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 23)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 24)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 25)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 26)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 27)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 28)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 29)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 30)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 31)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 32)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 33)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 34)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 35)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 36)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 37)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 38)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 39)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 40)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 41)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 42)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 43)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 44)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 45)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 46)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 47)))))) << 0)) | ((ulong)(((ulong)((byte)((byte)((byte)((((ulong)(imm)) & ((ulong)(0xF)))))))) << 48)))) | ((ulong)(((ulong)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x4)))) & ((ulong)(0x3)))))))) << 52)))) | ((ulong)(((ulong)((byte)((byte)(((byte)(byte)(((byte)(byte)(((byte)(byte)(((byte)(byte)(((byte)(byte)(((byte)(byte)(((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 0)) | ((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 1)))) | ((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 2)))) | ((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 3)))) | ((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 4)))) | ((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 5)))) | ((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 6)))) | ((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 7)))))) << 54)))) | ((ulong)(((ulong)((byte)(((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1))))) != 0 ? 0U : 1U))) << 62)))) | ((ulong)(((ulong)((byte)((byte)((byte)((imm) >> (int)(0x7)))))) << 63)))));

                context.SetVectorElement(d, dat, 0, 3);
            }
            else
            {
                context.ThrowUnknown();
            }

            context.SetVector(rd,d);
        }

        public static void Fneg(TranslationContext context)
        {
            int rn = context.GetRaw("rn");

            int rd = context.GetRaw("rd");

            int type = context.GetRaw("ftype") + 2;

            ulong bit = 1UL << ((8 << type) - 1);

            Operand vec = context.GetVector(rn);

            Operand n = context.GetVectorElement(vec,0,type);

            n = context.Xor(n , bit);

            Operand res = context.CreateVector();

            context.SetVectorElement(res,n,0,type);

            context.SetVector(rd,res);
        }

        public static void Fabs(TranslationContext context)
        {
            int rn = context.GetRaw("rn");

            int rd = context.GetRaw("rd");

            int type = context.GetRaw("ftype") + 2;

            ulong bit = 1UL << ((8 << type) - 1);

            bit--;

            Operand src = context.GetVector(rn);
            Operand des = context.CreateVector();

            Operand n = context.GetVectorElement(src,0,type);

            n = context.And(n,bit);

            context.SetVectorElement(des,n,0,3);

            context.SetVector(rd,des);
        }

        public static void Fsqrt(TranslationContext context)
        {
            int rn = context.GetRaw("rn");

            int rd = context.GetRaw("rd");

            int type = context.GetRaw("ftype") + 2;

            context.Call(nameof(FallbackFloat.Fsqrt),context.ContextPointer(),rd,rn,type);
        }

        public static void Fcmp(TranslationContext context)
        {
            int type = context.GetRaw("ftype") + 2;

            int WithZero = context.GetRaw("opc");

            int rn = context.GetRaw("rn");
            int rm = context.GetRaw("rm");

            context.SetArgument(0, rn);
            context.SetArgument(1, rm);
            context.SetArgument(2, type);
            context.SetArgument(3, WithZero);

            context.Call(nameof(FallbackFloat.Fcmp),context.ContextPointer());
        }

        public static void Cvtf_Vector_Integer(TranslationContext context, bool Singed)
        {
            int rd = context.GetRaw("rd");
            int rn = context.GetRaw("rn");

            int size = context.GetRaw("sz") + 2;

            Operand Source = context.GetVectorElement(context.GetVector(rn),0,size);
            Operand des = context.CreateVector();

            context.ConvertToFloat(des,Source,size,size,Singed);

            context.SetVector(rd,des);
        }
    }
}
