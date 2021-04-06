using GalacticARM.IntermediateRepresentation;
using GalacticARM.Runtime.Fallbacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Translation.aarch64
{
    public static partial class Emit64
    {
        public static int GetVectorSize(int imm5)
        {
            for (int i = 0; i < 4; i++)
            {
                if ((imm5 & ((1 << (i + 1)) - 1)) == 1 << i)
                {
                    return i;
                }
            }

            throw new NotImplementedException();
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

        static void ClearVectorTop(TranslationContext context, Operand vector) => context.SetVectorElement(vector, 0, 1, 3, false);

        public static void Dup_General(TranslationContext context)
        {
            Operand n = context.GetRegister("rn");
            int rd = context.GetRaw("rd");
            int q = context.GetRaw("q");

            int imm = context.GetRaw("imm");

            int size = GetVectorSize(imm);

            Operand d = context.CreateVector();

            int iter = (16 >> size);

            for (int i = 0; i < iter >> (1 - q); i++)
            {
                context.SetVectorElement(d, n, i, size);
            }

            context.SetVector(rd, d);
        }

        public static void Movi(TranslationContext context)
        {
            int cmode = context.GetRaw("cmode");
            int op = context.GetRaw("op");
            int q = context.GetRaw("q");

            int a = context.GetRaw("a");
            int b = context.GetRaw("b");
            int c = context.GetRaw("c");
            int d = context.GetRaw("d");
            int e = context.GetRaw("e");
            int f = context.GetRaw("f");
            int g = context.GetRaw("g");
            int h = context.GetRaw("h");

            int rd = context.GetRaw("rd");

            int hi = ((context.CurrentOpCode.RawOpCode >> 16) & 0x7);
            int low = ((context.CurrentOpCode.RawOpCode >> 5) & 0x1F);

            ulong RawIMM = (ulong)((hi << 5) | low);

            Operand result = context.CreateVector();

            if (q == 1 && op == 1 && cmode == 0b1110)
            {
                //movi 64 bit vector

                int count = q + 1;

                ulong Imm = BuildScaledInt(8, a, b, c, d, e, f, g, h);

                for (int i = 0; i < count; i++)
                {
                    context.SetVectorElement(result, Imm, i, 3);
                }
            }
            else if (op == 0 && ((cmode & 0b1001) == 0))
            {
                if (op == 1)
                {
                    RawIMM = ~RawIMM;
                }

                int shift = ((cmode >> 1) & 0x3) << 3;

                for (int i = 0; i < (q + 1) * 2; i++)
                {
                    context.SetVectorElement(result, RawIMM, i, 2);
                }
            }
            else
            {
                context.ThrowUnknown();
            }

            context.SetVector(rd, result);
        }

        public static (int, int, int) GetElements(int imm5, int imm4)
        {
            int Size = imm5 & -imm5;

            switch (Size)
            {
                case 1: Size = 0; break;
                case 2: Size = 1; break;
                case 4: Size = 2; break;
                case 8: Size = 3; break;
            }

            int SrcIndex = imm4 >> Size;
            int DstIndex = imm5 >> (Size + 1);

            return (SrcIndex, DstIndex, Size);
        }

        public static void Ins_Element(TranslationContext context)
        {
            int imm5 = context.GetRaw("imm5");

            int imm4 = context.GetRaw("imm4");

            (int Src, int Des, int Size) = GetElements(imm5, imm4);

            int rn = context.GetRaw("rn");

            int rd = context.GetRaw("rd");

            Operand des = context.GetVector(rd);
            Operand src = context.GetVector(rn);

            context.SetVectorElement(des, context.GetVectorElement(src, Src, Size), Des, Size);

            context.SetVector(rd, des);
        }

        public static void Ins_General(TranslationContext context)
        {
            int imm = context.GetRaw("imm");

            int size = GetVectorSize(imm);

            int rd = context.GetRaw("rd");

            int index = imm >> (size + 1);

            Operand n = context.GetRegister("rn");

            Operand des = context.GetVector(rd);

            context.SetVectorElement(des, n, index, size);

            context.SetVector(rd, des);
        }

        public static void Orr_Vector(TranslationContext context) => VectorOperation(context, Instruction.Vector_Or, false);
        public static void Orn_Vector(TranslationContext context) => VectorOperation(context, Instruction.Vector_Or, true);

        public static void Eor_Vector(TranslationContext context) => VectorOperation(context, Instruction.Vector_Xor, false);
        public static void Eon_Vector(TranslationContext context) => VectorOperation(context, Instruction.Vector_Xor, true);

        public static void And_Vector(TranslationContext context) => VectorOperation(context, Instruction.Vector_And, false);
        public static void Bic_Vector(TranslationContext context) => VectorOperation(context, Instruction.Vector_And, true);

        public static void VectorOperation(TranslationContext context, Instruction instruction, bool not)
        {
            int rd = context.GetRaw("rd");
            int rn = context.GetRaw("rn");
            int rm = context.GetRaw("rm");

            int q = context.GetRaw("q");

            Operand n = context.GetVector(rn);
            Operand m = context.GetVector(rm);

            if (not)
                context.VectorNot(m);

            Operand d = context.VectorOperation(n, m, instruction, q == 0);

            context.SetVector(rd, d);
        }

        public static void Cnt(TranslationContext context)
        {
            int rd = context.GetRaw("rd");
            int rn = context.GetRaw("rn");

            int q = context.GetRaw("q");

            context.Call(nameof(Fallbackbits.Cnt), context.ContextPointer(), rd, rn);

            if (q == 0)
            {
                context.SetVectorElement(Operand.Vec(rd), 0, 1, 3);
            }
        }

        public static void Uaddlv(TranslationContext context)
        {
            int q = context.GetRaw("q");

            int rn = context.GetRaw("rn");
            int rd = context.GetRaw("rd");

            int size = context.GetRaw("size");

            int Size = 64 << q;

            int elms = (Size >> 3) >> Size;

            Operand src = context.GetVector(rn);
            Operand des = context.CreateVector();

            Operand res = context.GetVectorElement(src, 0, size);

            for (int i = 1; i < elms; i++)
            {
                res = context.Add(res, context.GetVectorElement(src, i, size));
            }

            context.SetVectorElement(des, res, 0, 3);

            context.SetVector(rd, des);
        }
    }
}
