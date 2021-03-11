using GalacticARM.CodeGen.Intermediate;
using GalacticARM.Context;
using GalacticARM.Decoding;
using GalacticARM.Decoding.Abstractions;
using System;
using System.Runtime.Intrinsics;
using static GalacticARM.CodeGen.AEmit.EmitVector;

namespace GalacticARM.CodeGen.AEmit
{
    public static class EmitFloatingPoint
    {
        public static void Fmov(InstructionEmitContext context)
        {
            OpCodeFloatingPointIntegerConversions opCode = (OpCodeFloatingPointIntegerConversions)context.CurrentOpCode;

            switch (opCode.Info)
            {
                case InstructionInfo._32_bit_to_single_precision:

                    ClearVector(context,opCode.rd);

                    InsertIntToVector(context,opCode.rd,context.GetRegister(opCode.rn),2);
                    
                    break;

                case InstructionInfo._64_bit_to_double_precision:

                    ClearVector(context, opCode.rd);

                    InsertIntToVector(context, opCode.rd, context.GetRegister(opCode.rn), 3);

                    break;

                case InstructionInfo.Single_precision_to_32_bit: context.SetRegister(opCode.rd, LoadIntFromVector(context, opCode.rn, 2)); break;
                case InstructionInfo.Double_precision_to_64_bit: context.SetRegister(opCode.rd, LoadIntFromVector(context, opCode.rn, 3)); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void CVTF_Scalar(InstructionEmitContext context, bool Signed)
        {
            OpCodeFloatingPointIntegerConversions opCode = (OpCodeFloatingPointIntegerConversions)context.CurrentOpCode;

            Operand signed = Operand.Const(Signed ? 1 : 0);

            Operand Des = context.GetRegister(opCode.rn);

            switch (opCode.Info)
            {
                case InstructionInfo._32_bit_to_single_precision:

                    context.Block.Add(new Operation(ILInstruction.F_IntConvertToFloat,Des,Operand.Const(0), Operand.Const(0),signed));

                    ClearVector(context, opCode.rd);
                    InsertIntToVector(context,opCode.rd,Des,2);

                    break;

                case InstructionInfo._32_bit_to_double_precision:

                    context.Block.Add(new Operation(ILInstruction.F_IntConvertToFloat, Des, Operand.Const(1), Operand.Const(0), signed));

                    ClearVector(context, opCode.rd);
                    InsertIntToVector(context, opCode.rd, Des, 3);

                    break;

                case InstructionInfo._64_bit_to_single_precision:

                    context.Block.Add(new Operation(ILInstruction.F_IntConvertToFloat, Des, Operand.Const(0), Operand.Const(1), signed));

                    ClearVector(context, opCode.rd);
                    InsertIntToVector(context, opCode.rd, Des, 2);

                    break;

                case InstructionInfo._64_bit_to_double_precision:

                    context.Block.Add(new Operation(ILInstruction.F_IntConvertToFloat, Des, Operand.Const(1), Operand.Const(1), signed));

                    ClearVector(context, opCode.rd);
                    InsertIntToVector(context, opCode.rd, Des, 3);

                    break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void FCVTZ_Scalar(InstructionEmitContext context, bool Signed)
        {
            OpCodeFloatingPointIntegerConversions opCode = (OpCodeFloatingPointIntegerConversions)context.CurrentOpCode;

            Operand signed = Operand.Const(Signed ? 1 : 0);

            Operand Des;

            switch (opCode.Info)
            {
                case InstructionInfo.Double_precision_to_32_bit:

                    Des = LoadIntFromVector(context,opCode.rn,3);

                    context.Block.Add(new Operation(ILInstruction.F_FloatConvertToInt, Des, Operand.Const(0), Operand.Const(1),signed));

                    context.SetRegister(opCode.rd,Des);

                    break;

                case InstructionInfo.Single_precision_to_32_bit:

                    Des = LoadIntFromVector(context, opCode.rn, 2);

                    context.Block.Add(new Operation(ILInstruction.F_FloatConvertToInt, Des, Operand.Const(0), Operand.Const(0), signed));

                    context.SetRegister(opCode.rd, Des);

                    break;

                case InstructionInfo.Double_precision_to_64_bit:

                    Des = LoadIntFromVector(context, opCode.rn, 3);

                    context.Block.Add(new Operation(ILInstruction.F_FloatConvertToInt, Des, Operand.Const(1), Operand.Const(1), signed));

                    context.SetRegister(opCode.rd, Des);

                    break;

                case InstructionInfo.Single_precision_to_64_bit:

                    Des = LoadIntFromVector(context, opCode.rn, 2);

                    context.Block.Add(new Operation(ILInstruction.F_FloatConvertToInt, Des, Operand.Const(1), Operand.Const(0), signed));

                    context.SetRegister(opCode.rd, Des);

                    break;

                default: context.ThrowUnknown(); break;
            }
        }

        public static void FCVT_TO(InstructionEmitContext context, bool IsCel, bool Signed)
        {
            OpCodeFloatingPointIntegerConversions opCode = (OpCodeFloatingPointIntegerConversions)context.CurrentOpCode;

            Operand Source = LoadIntFromVector(context,opCode.rn,opCode.type + 2);

            ulong Args = (ulong)opCode.type;

            if (IsCel)
                Args |= 1UL << 1;

            if (Signed)
                Args |= 1UL << 2;

            Args |= (ulong)opCode.Info << 10;

            context.CallFunctionFromPointer(context.GetDelegate(new _Void_U_U_U(Floor_Cel)),context.GetContextPointer(),context.Const(Source.Reg), context.Const(Args));

            context.SetRegister(opCode.rd,Source);
        }

        static unsafe double GetD(ulong Src)
        {
            return *(double*)(&Src);
        }
        static unsafe float GetF(uint Src)
        {
            return *(float*)(&Src);
        }

        public static unsafe void Floor_Cel(ulong Block, ulong Source, ulong Args)
        {
            ContextBlock* block = (ContextBlock*)Block;

            int Size = (int)Args & 1;
            bool IsCel = ((Args >> 1) & 1) == 1;
            bool Signed = ((Args >> 2) & 1) == 1;

            InstructionInfo info = (InstructionInfo)(Args >> 10);

            if (Signed)
            {
                if (IsCel)
                {
                    switch (info)
                    {
                        case InstructionInfo.Single_precision_to_32_bit:

                            float f = MathF.Ceiling(GetF((uint)block->RegisterBuffer[Source]));

                            block->RegisterBuffer[Source] = (uint)(int)f;

                            break;

                        default: throw new Exception();
                    }
                }
                else
                {
                    switch (info)
                    {
                        case InstructionInfo.Single_precision_to_32_bit:

                            float f = MathF.Floor(GetF((uint)block->RegisterBuffer[Source]));

                            block->RegisterBuffer[Source] = (uint)(int)f;

                            break;

                        case InstructionInfo.Double_precision_to_32_bit:

                            double d = Math.Floor(GetD(block->RegisterBuffer[Source]));

                            block->RegisterBuffer[Source] = (uint)(int)d;

                            break;

                        default: throw new Exception();
                    }
                }
            }
            else
            {
                throw new Exception();
            }
        }

        public static void F_Operation_Scalar(InstructionEmitContext context, ILInstruction instruction, bool Negate)
        {
            OpCodeFloatingPointDataProcessing2Source opCode = (OpCodeFloatingPointDataProcessing2Source)context.CurrentOpCode;

            int size = opCode.type;

            Operand n = LoadIntFromVector(context,opCode.rn,size + 2);
            Operand m = LoadIntFromVector(context,opCode.rm,size + 2);

            context.Block.Add(new Operation(instruction,n,m,Operand.Const((ulong)size)));

            if (Negate)
            {
                int Size = (32 + (32 * opCode.type)) - 1;

                n = context.Xor(n, context.Const(1UL << Size));
            }

            ClearVector(context, opCode.rd);
            InsertIntToVector(context,opCode.rd,n,size + 2);
        }

        public static void F_Compare(InstructionEmitContext context, bool IsMax)
        {
            OpCodeFloatingPointDataProcessing2Source opCode = (OpCodeFloatingPointDataProcessing2Source)context.CurrentOpCode;

            ulong Args = ((ulong)opCode.rd);
            Args |= ((ulong)opCode.rn) << 5;
            Args |= ((ulong)opCode.rm) << 10;

            Args |= IsMax ? ((ulong)1UL << 20) : 0;

            if (opCode.Info == InstructionInfo.Double_precision)
                Args |= 1UL << 25;

            context.CallFunctionFromPointer(context.GetDelegate(new _Void_U_U(F_Compare_FB)),context.GetContextPointer(),context.Const(Args));
        }

        public static unsafe void F_Compare_FB(ulong Block, ulong Args)
        {
            ulong rd = (Args) & 0x1f;
            ulong rn = (Args >> 5) & 0x1f;
            ulong rm = (Args >> 10) & 0x1f;

            bool ISMax = ((Args >> 20) & 1) == 1;
            bool Size = ((Args >> 25) & 1) == 1;

            ContextBlock* block = (ContextBlock*)Block;

            if (Size)
            {
                double n = GetDouble(block, (int)rn);
                double m = GetDouble(block, (int)rm);

                if (ISMax)
                {
                    SetDouble(block,(int)rd,n > m ? n : m);
                }
                else
                {
                    SetDouble(block, (int)rd, n < m ? n : m);
                }
            }
            else
            {
                float n = GetFloat(block, (int)rn);
                float m = GetFloat(block, (int)rm);

                block->RegisterBuffer[rd] = 0;

                if (ISMax)
                {
                    SetFloat(block, (int)rd, n > m ? n : m);
                }
                else
                {
                    SetFloat(block, (int)rd, n < m ? n : m);
                }
            }
        }

        public static void Fcsel(InstructionEmitContext context)
        {
            OpCodeFloatingPointConditionalSelect opCode = (OpCodeFloatingPointConditionalSelect)context.CurrentOpCode;

            Operand n = LoadIntFromVector(context,opCode.rn,opCode.type + 2);
            Operand m = LoadIntFromVector(context, opCode.rm, opCode.type + 2);

            ClearVector(context,opCode.rd);

            EmitControlFlow.EmitIF(context,
                
                EmitControlFlow.ConditionHolds(context,(Condition)opCode.cond),

                delegate()
                {
                    InsertIntToVector(context,opCode.rd,n,opCode.type + 2);
                },

                delegate()
                {
                    InsertIntToVector(context, opCode.rd,m,opCode.type + 2);
                }
                
                );
        }

        public static void Fcvtz(InstructionEmitContext context)
        {
            OpCodeFloatingPointFixedPointConversions opCode = (OpCodeFloatingPointFixedPointConversions)context.CurrentOpCode;

            context.CallFunctionFromPointer(context.GetDelegate(new _Void_U_U(Fcvtz_FB)), context.GetContextPointer(), context.Const(opCode.Address));
        }

        public static unsafe void Fcvtz_FB(ulong ContextBlock, ulong Args)
        {
            ContextBlock* block = (ContextBlock*)ContextBlock;

            OpCodeFloatingPointFixedPointConversions opCode = (OpCodeFloatingPointFixedPointConversions)OpCodeTable.DecodeOpCode(Args);

            if (opCode.sf == 0 && opCode.type == 0 && opCode.rmode == 3 && opCode.opcode <= 1)
            {
                //FCVTZ (scalar, fixed-point) Single-precision to 32-bit

                int fbits = 64 - opCode.scale;

                float f = GetFloat(block,opCode.rn);
                uint u = FloatToFixed32_0(f, fbits);

                if (opCode.rd != 31)
                {
                    block->RegisterBuffer[opCode.rd] = u;
                }
            }
            else if (opCode.sf == 0 && opCode.type == 1 && opCode.rmode == 3 && opCode.opcode <= 1)
            {
                //FCVTZ (scalar, fixed-point) Double-precision to 32-bit

                int fbits = 64 - opCode.scale;

                double f = GetDouble(block,opCode.rn);
                uint u = FloatToFixed32_1(f,fbits);

                if (opCode.rd != 31)
                {
                    block->RegisterBuffer[opCode.rd] = u;
                }

            }
            else
            {
                throw new NotImplementedException();
            }

            /*
            switch (Info)
            {
                case InstructionInfo.Double_precision_to_32_bit:

                    block->RegisterBuffer[rd] = FloatToFixed32_1(GetDouble(block, (int)rn),(int)(64 - scale));

                    break;

                case InstructionInfo.Single_precision_to_32_bit:

                    block->RegisterBuffer[rd] = FloatToFixed32_0(GetFloat(block, (int)rn), (int)(64 - scale));

                    break;

                default: throw new Exception(); break;
            }
            */
        }

        public static unsafe float GetFloat(ContextBlock* block, int index)
        {
            return *(float*)(((byte*)block) + ((32 * 8) + (index * 16)));
        }

        public static unsafe double GetDouble(ContextBlock* block, int index)
        {
            return *(double*)(((byte*)block) + ((32 * 8) + (index * 16)));
        }

        public static unsafe void SetFloat(ContextBlock* block, int index, float value)
        {
            block->RegisterBuffer[index] = 0;

            *(float*)(((byte*)block) + ((32 * 8) + (index * 16))) = value;
        }

        public static unsafe void SetDouble(ContextBlock* block, int index, double value)
        {
            *(double*)(((byte*)block) + ((32 * 8) + (index * 16))) = value;
        }

        public static uint FloatToFixed32_0(float fvalue, int fbits)
        {
            return unchecked((uint)(int)MathF.Round(fvalue * (1 << fbits)));
        }

        public static ulong FloatToFixed64_0(float fvalue, int fbits)
        {
            return unchecked((ulong)(long)MathF.Round(fvalue * (1 << fbits)));
        }

        public static uint FloatToFixed32_1(double fvalue, int fbits)
        {
            return unchecked((uint)(int)Math.Round(fvalue * (1 << fbits)));
        }

        public static ulong FloatToFixed64_1(double fvalue, int fbits)
        {
            return unchecked((ulong)(long)Math.Round(fvalue * (1 << fbits)));
        }

        public static void Fcvt(InstructionEmitContext context)
        {
            OpCodeFloatingPointDataProcessing1Source opCode = (OpCodeFloatingPointDataProcessing1Source)context.CurrentOpCode;

            Operand Load = LoadIntFromVector(context,opCode.rn,3);

            ClearVector(context, opCode.rd);

            switch (opCode.Info)
            {
                case InstructionInfo.Single_precision_to_double_precision:

                    context.Block.Add(new Operation(ILInstruction.F_ConvertPrecision,Load,Operand.Const(1), Operand.Const(0)));

                    InsertIntToVector(context, opCode.rd, Load, 3);

                    break;

                case InstructionInfo.Double_precision_to_single_precision:

                    context.Block.Add(new Operation(ILInstruction.F_ConvertPrecision, Load, Operand.Const(0), Operand.Const(1)));

                    InsertIntToVector(context, opCode.rd, Load, 2);

                    break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void Fneg(InstructionEmitContext context)
        {
            OpCodeFloatingPointDataProcessing1Source opCode = (OpCodeFloatingPointDataProcessing1Source)context.CurrentOpCode;

            Operand dat = LoadIntFromVector(context,opCode.rn,opCode.type + 2);

            int Size = (32 + (32 * opCode.type)) - 1;

            dat = context.Xor(dat, context.Const(1UL << Size));

            ClearVector(context,opCode.rd);
            InsertIntToVector(context, opCode.rd,dat,opCode.type + 2);
        }

        public static void Fabs(InstructionEmitContext context)
        {
            OpCodeFloatingPointDataProcessing1Source opCode = (OpCodeFloatingPointDataProcessing1Source)context.CurrentOpCode;

            Operand dat = LoadIntFromVector(context, opCode.rn, opCode.type + 2);

            int Size = (32 + (32 * opCode.type)) - 1;

            dat = context.And(dat, context.Const((1UL << Size) - 1));

            ClearVector(context, opCode.rd);
            InsertIntToVector(context, opCode.rd, dat, opCode.type + 2);
        }

        public static void Fsqrt(InstructionEmitContext context)
        {
            OpCodeFloatingPointDataProcessing1Source opCode = (OpCodeFloatingPointDataProcessing1Source)context.CurrentOpCode;

            Operand dat = LoadIntFromVector(context, opCode.rn, opCode.type + 2);

            ulong Args = (uint)opCode.rd;
            Args |= ((ulong)opCode.rn) << 5;
            Args |= ((ulong)opCode.type) << 15;

            context.CallFunctionFromPointer(context.GetDelegate(new _Void_U_U(Fsqrt_FB)), context.GetContextPointer(),context.Const(Args));
        }

        public static unsafe void Fsqrt_FB(ulong Block, ulong Args)
        {
            ulong rd = Args & 0x1f;
            ulong rn = ((Args >> 5) & 0x1f);
            ulong Size = ((Args >> 15) & 1);

            ContextBlock* block = (ContextBlock*)Block;

            if (Size == 1)
            {
                double dat = GetDouble(block,(int)rn);

                SetDouble(block,(int)rd,Math.Sqrt(dat));
            }
            else
            {
                float dat = GetFloat(block, (int)rn);

                SetFloat(block, (int)rd, MathF.Sqrt(dat));
            }
        }

        public static unsafe void Fcmp_FB(ulong Back, ulong Args)
        {
            ulong rn = (Args >> 5) & 0x1f;
            ulong rm = (Args >> 10) & 0x1f;

            ulong Size = ((Args >> 20) & 1);
            ulong opc = ((Args >> 30) & 0b1);

            ContextBlock* block = (ContextBlock*)Back;

            if (Size == 0)
            {
                FCMP32(block, (int)opc,GetFloat(block,(int)rn), GetFloat(block, (int)rm));
            }
            else if (Size == 1)
            {
                FCMP64(block, (int)opc, GetDouble(block, (int)rn), GetDouble(block, (int)rm));
            }
            else
            {
                throw new Exception();
            }
        }

        public static unsafe void Fccmp_GB(ulong Back, ulong Args)
        {
            ulong rn = (Args >> 5) & 0x1f;
            ulong rm = (Args >> 10) & 0x1f;

            ulong Size = ((Args >> 20) & 1);

            ContextBlock* block = (ContextBlock*)Back;

            if (Size == 0)
            {
                FCCMP32(block, GetFloat(block, (int)rn), GetFloat(block, (int)rm));
            }
            else if (Size == 1)
            {
                FCCMP64(block, GetDouble(block, (int)rn), GetDouble(block, (int)rm));
            }
            else
            {
                throw new Exception();
            }
        }

        public static unsafe void FCMP32(ContextBlock* context, int opc, float rn, float rm)
        {
            float __macro_fcmp_a = rn;
            float __macro_fcmp_b = (((byte)(((opc) == (0x1)) ? 1U : 0U) != 0) ? ((float)((float)(0x0))) : (rm));

            uint res = (uint)((uint)((long)(((long)(((byte)((((byte)((byte)(float.IsNaN(__macro_fcmp_a) ? 1U : 0U))) | ((byte)((byte)(float.IsNaN(__macro_fcmp_b) ? 1U : 0U))))) != 0) ? (0x3) : ((long)(((byte)(((__macro_fcmp_a) == (__macro_fcmp_b)) ? 1U : 0U) != 0) ? (0x6) : ((long)(((byte)(((__macro_fcmp_a) < (__macro_fcmp_b)) ? 1U : 0U) != 0) ? (0x8) : (0x2))))))) << (int)(0x1C))));

            context->SetFlagsIMM(res >> 28);
        }

        public static unsafe void FCMP64(ContextBlock* context, int opc, double rn, double rm)
        {
            double __macro_fcmp_a = rn;
            double __macro_fcmp_b = (((byte)(((opc) == (0x1)) ? 1U : 0U) != 0) ? ((double)((double)(0x0))) : (rm));

            uint res = (uint)((uint)((long)(((long)(((byte)((((byte)((byte)(double.IsNaN(__macro_fcmp_a) ? 1U : 0U))) | ((byte)((byte)(double.IsNaN(__macro_fcmp_b) ? 1U : 0U))))) != 0) ? (0x3) : ((long)(((byte)(((__macro_fcmp_a) == (__macro_fcmp_b)) ? 1U : 0U) != 0) ? (0x6) : ((long)(((byte)(((__macro_fcmp_a) < (__macro_fcmp_b)) ? 1U : 0U) != 0) ? (0x8) : (0x2))))))) << (int)(0x1C))));

            context->SetFlagsIMM(res >> 28);
        }

        public static unsafe void FCCMP32(ContextBlock* context, float rn, float rm)
        {
            float __macro_fcmp_a = rn;
            float __macro_fcmp_b = rm;

            uint res = (uint)((uint)((long)(((long)(((byte)((((byte)((byte)(float.IsNaN(__macro_fcmp_a) ? 1U : 0U))) | ((byte)((byte)(float.IsNaN(__macro_fcmp_b) ? 1U : 0U))))) != 0) ? (0x3) : ((long)(((byte)(((__macro_fcmp_a) == (__macro_fcmp_b)) ? 1U : 0U) != 0) ? (0x6) : ((long)(((byte)(((__macro_fcmp_a) < (__macro_fcmp_b)) ? 1U : 0U) != 0) ? (0x8) : (0x2))))))) << (int)(0x1C))));

            context->SetFlagsIMM(res >> 28);
        }

        public static unsafe void FCCMP64(ContextBlock* context, double rn, double rm)
        {
            double __macro_fcmp_a = rn;
            double __macro_fcmp_b = rm;

            uint res = (uint)((uint)((long)(((long)(((byte)((((byte)((byte)(double.IsNaN(__macro_fcmp_a) ? 1U : 0U))) | ((byte)((byte)(double.IsNaN(__macro_fcmp_b) ? 1U : 0U))))) != 0) ? (0x3) : ((long)(((byte)(((__macro_fcmp_a) == (__macro_fcmp_b)) ? 1U : 0U) != 0) ? (0x6) : ((long)(((byte)(((__macro_fcmp_a) < (__macro_fcmp_b)) ? 1U : 0U) != 0) ? (0x8) : (0x2))))))) << (int)(0x1C))));

            context->SetFlagsIMM(res >> 28);
        }

        public static void CVTF_TwoRegMisc(InstructionEmitContext context, bool signed)
        {
            OpCodeAdvsimdScalarTwoRegMisc opCode = (OpCodeAdvsimdScalarTwoRegMisc)context.CurrentOpCode;

            int size = 2 + (opCode.size & 1);

            if (signed)
            {
                context.CallFunctionFromPointer(context.GetDelegate(new _Void_U_U_U(SCVTFVectorInt)), GetVectorAddress(context, opCode.rd), GetVectorAddress(context, opCode.rn), context.Const((ulong)size));
            }
            else
            {
                context.CallFunctionFromPointer(context.GetDelegate(new _Void_U_U_U(UCVTFVectorInt)), GetVectorAddress(context, opCode.rd), GetVectorAddress(context, opCode.rn), context.Const((ulong)size));
            }
        }

        public static unsafe void UCVTFVectorInt(ulong rd, ulong rn, ulong size)
        {
            Vector128<float>* d = (Vector128<float>*)rd;
            Vector128<float>* n = (Vector128<float>*)rn;

            if (size == 2)
            {
                *d = new Vector128<float>().WithElement(0, (float)((float)((uint)(Bitcast<float, uint>((float)((*n).GetElement(0)))))));
            }
            else if (size == 3)
            {
                *d = new Vector128<double>().WithElement(0, (double)((double)((ulong)(Bitcast<double, ulong>((double)((*n).As<float, double>().GetElement(0))))))).As<double, float>();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static unsafe void SCVTFVectorInt(ulong rd, ulong rn, ulong size)
        {
            Vector128<float>* d = (Vector128<float>*)rd;
            Vector128<float>* n = (Vector128<float>*)rn;

            if (size == 2)
            {
                *d = new Vector128<float>().WithElement(0, (float)((float)((int)(Bitcast<float, int>((float)((*n).GetElement(0)))))));
            }
            else if (size == 3)
            {
                *d = new Vector128<double>().WithElement(0, (double)((double)((long)(Bitcast<double, long>((double)((*n).As<float, double>().GetElement(0))))))).As<double, float>();
            }
            else
            {
                throw new NotImplementedException();
            }
        }


        static unsafe OutT Bitcast<InT, OutT>(InT value)
        {
            var ov = Activator.CreateInstance<OutT>();
            switch (value)
            {
                case int v:
                    switch (ov)
                    {
                        case float _: return (OutT)(object)*(float*)&v;
                        default: throw new NotImplementedException();
                    }
                case uint v:
                    switch (ov)
                    {
                        case float _: return (OutT)(object)*(float*)&v;
                        default: throw new NotImplementedException();
                    }
                case long v:
                    switch (ov)
                    {
                        case double _: return (OutT)(object)*(double*)&v;
                        default: throw new NotImplementedException();
                    }
                case ulong v:
                    switch (ov)
                    {
                        case double _: return (OutT)(object)*(double*)&v;
                        default: throw new NotImplementedException();
                    }
                case float v:
                    switch (ov)
                    {
                        case uint _: return (OutT)(object)*(uint*)&v;
                        case int _: return (OutT)(object)*(int*)&v;
                        default: throw new NotImplementedException();
                    }
                case double v:
                    switch (ov)
                    {
                        case ulong _: return (OutT)(object)*(ulong*)&v;
                        case long _: return (OutT)(object)*(long*)&v;
                        default: throw new NotImplementedException();
                    }
                default: throw new NotImplementedException();
            }
        }
    }
}
