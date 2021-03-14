using GalacticARM.CodeGen.Intermediate;
using GalacticARM.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Assembler.Msil
{
    public unsafe delegate void Gen(Operation operation,Generator generator);

    public static unsafe class InterpreterFunctions
    {
        public static Gen[] Generation = new Gen[]
        {
            GenerateAdd,
            GenerateAnd,
            GenerateCall,
            GenerateCompareEqual,
            GenerateCompareGreaterThan,
            GenerateCompareGreaterThanUnsigned,
            GenerateCompareLessThan,
            GenerateCompareLessThanUnsigned,
            GenerateCopy,
            GenerateDivide,
            GenerateDivide_Un,
            GenerateF_Add,
            GenerateF_ConvertPrecision,
            GenerateF_Div,
            GenerateF_FloatConvertToInt,
            GenerateF_GreaterThan,
            GenerateF_IntConvertToFloat,
            GenerateF_LessThan,
            GenerateF_Mul,
            GenerateF_Sub,
            GenerateGetContextPointer,
            GenerateJump,
            GenerateJumpIf,
            GenerateLoadImmediate,
            GenerateLoadMem,
            GenerateMod,
            GenerateMultiply,
            GenerateNot,
            GenerateOr,
            GenerateReturn,
            GenerateShiftLeft,
            GenerateShiftRight,
            GenerateShiftRightSigned,
            GenerateSignExtend16,
            GenerateSignExtend32,
            GenerateSignExtend8,
            GenerateStore16,
            GenerateStore32,
            GenerateStore64,
            GenerateStore8,
            GenerateSubtract,
            GenerateWriteRegister,
            GenerateXor,
            GenerateNop
        };

        //Intrinsics
        public static void GenerateF_LessThan(Operation operation, Generator generator)
        {
            throw new Exception();
        }
        public static void GenerateF_GreaterThan(Operation operation, Generator generator)
        {
            throw new Exception();
        }

        public static void GenerateF_ConvertPrecision(Operation operation, Generator generator)
        {
            int DesReg = operation.GetReg(0);
            int DesSize = (int)operation.GetImm(1);
            int SourceSize = (int)operation.GetImm(2);

            generator.LoadFloat(DesReg,SourceSize);

            if (DesSize == 0)
            {
                generator.il.Emit(OpCodes.Conv_R8);
            }
            else
            {
                generator.il.Emit(OpCodes.Conv_R4);
            }

            generator.StoreFloat(DesReg,DesSize);
        }

        public static void GenerateF_FloatConvertToInt(Operation operation, Generator generator)
        {
            int DesReg = (int)operation.GetReg(0);
            int DesSize = (int)operation.GetImm(1);
            int SourceSize = (int)operation.GetImm(2);
            bool Signed = operation.GetImm(3) == 1;

            generator.LoadFloat(DesReg, SourceSize);

            if (!Signed)
                throw new Exception();

            if (DesSize == 0)
            {
                if (Signed)
                    generator.il.Emit(OpCodes.Conv_I4);
                else
                    generator.il.Emit(OpCodes.Conv_U4);

                generator.il.Emit(OpCodes.Conv_U4);
            }
            else
            {
                if (Signed)
                    generator.il.Emit(OpCodes.Conv_I8);
                else
                    generator.il.Emit(OpCodes.Conv_U8);
            }

            generator.il.Emit(OpCodes.Conv_U8);

            generator.StoreData(0);
        }

        public static void GenerateF_IntConvertToFloat(Operation operation, Generator generator)
        {
            int DesReg = (int)operation.GetReg(0);
            int DesSize = (int)operation.GetImm(1);
            int SourceSize = (int)operation.GetImm(2);
            bool Signed = operation.GetImm(3) == 1;

            generator.LoadData(0);

            if (Signed)
            {
                if (SourceSize == 0)
                    generator.il.Emit(OpCodes.Conv_I4);
                else
                    generator.il.Emit(OpCodes.Conv_I8);
            }
            else
            {
                if (SourceSize == 0)
                    generator.il.Emit(OpCodes.Conv_U4);
                else
                    generator.il.Emit(OpCodes.Conv_U8);

                generator.il.Emit(OpCodes.Conv_R_Un);
            }

            if (DesSize == 0)
            {
                generator.il.Emit(OpCodes.Conv_R4);
            }
            else
            {
                generator.il.Emit(OpCodes.Conv_R8);
            }

            generator.StoreFloat(DesReg,DesSize);
        }

        public static void GenerateF_Add(Operation operation, Generator generator)
        {
            int DesReg = (int)operation.GetReg(0);
            int SourceReg = (int)operation.GetReg(1);
            int Size = (int)operation.GetImm(2);

            generator.LoadFloat(DesReg,Size);
            generator.LoadFloat(SourceReg,Size);

            generator.il.Emit(OpCodes.Add);

            generator.StoreFloat(DesReg,Size);
        }

        public static void GenerateF_Div(Operation operation, Generator generator)
        {
            int DesReg = (int)operation.GetReg(0);
            int SourceReg = (int)operation.GetReg(1);
            int Size = (int)operation.GetImm(2);

            generator.LoadFloat(DesReg, Size);
            generator.LoadFloat(SourceReg, Size);

            generator.il.Emit(OpCodes.Div);

            generator.StoreFloat(DesReg, Size);
        }

        public static void GenerateF_Mul(Operation operation, Generator generator)
        {
            int DesReg = (int)operation.GetReg(0);
            int SourceReg = (int)operation.GetReg(1);
            int Size = (int)operation.GetImm(2);

            generator.LoadFloat(DesReg, Size);
            generator.LoadFloat(SourceReg, Size);

            generator.il.Emit(OpCodes.Mul);

            generator.StoreFloat(DesReg, Size);
        }

        public static void GenerateF_Sub(Operation operation, Generator generator)
        {
            int DesReg = (int)operation.GetReg(0);
            int SourceReg = (int)operation.GetReg(1);
            int Size = (int)operation.GetImm(2);

            generator.LoadFloat(DesReg, Size);
            generator.LoadFloat(SourceReg, Size);

            generator.il.Emit(OpCodes.Sub);

            generator.StoreFloat(DesReg, Size);
        }

        //Normal
        public static void GenerateAdd(Operation operation, Generator generator)
        {
            generator.LoadData(0);
            generator.LoadData(1);

            generator.il.Emit(OpCodes.Add);

            generator.StoreData(0);
        }

        public static void GenerateAnd(Operation operation, Generator generator)
        {
            generator.LoadData(0);
            generator.LoadData(1);

            generator.il.Emit(OpCodes.And);

            generator.StoreData(0);
        }

        public static void GenerateCall(Operation operation, Generator generator)
        {
            generator.LoadData(1);

            for (int i = 0; i < (int)operation.GetImm(0); i++)
            {
                generator.LoadData(2 + i);
            }

            switch (operation.GetImm(0))
            {
                case 1: generator.il.Emit(OpCodes.Call, typeof(InterpreterFunctions).GetMethod(nameof(Call_Void_U))); break;
                case 2: generator.il.Emit(OpCodes.Call, typeof(InterpreterFunctions).GetMethod(nameof(Call_Void_U_U))); break;
                case 3: generator.il.Emit(OpCodes.Call, typeof(InterpreterFunctions).GetMethod(nameof(Call_Void_U_U_U))); break;
                case 4: generator.il.Emit(OpCodes.Call, typeof(InterpreterFunctions).GetMethod(nameof(Call_Void_U_U_U_U))); break;
                default: throw new Exception();
            }
        }

        public static void Call_Void_U(ulong Func, ulong Arg0) => Marshal.GetDelegateForFunctionPointer<_Void_U>((IntPtr)Func)(Arg0);
        public static void Call_Void_U_U(ulong Func, ulong Arg0, ulong Arg1) => Marshal.GetDelegateForFunctionPointer<_Void_U_U>((IntPtr)Func)(Arg0,Arg1);
        public static void Call_Void_U_U_U(ulong Func, ulong Arg0, ulong Arg1, ulong Arg2) => Marshal.GetDelegateForFunctionPointer<_Void_U_U_U>((IntPtr)Func)(Arg0,Arg1,Arg2);
        public static void Call_Void_U_U_U_U(ulong Func, ulong Arg0, ulong Arg1, ulong Arg2, ulong Arg3) => Marshal.GetDelegateForFunctionPointer<_Void_U_U_U_U>((IntPtr)Func)(Arg0,Arg1,Arg2,Arg3);

        public static void GenerateCompareEqual(Operation operation, Generator generator)
        {
            generator.LoadData(0);
            generator.LoadData(1);

            generator.il.Emit(OpCodes.Ceq);

            generator.StoreData(0);
        }

        public static void GenerateCompareGreaterThan(Operation operation, Generator generator)
        {
            generator.LoadData(0);
            generator.LoadData(1);

            generator.il.Emit(OpCodes.Cgt);

            generator.StoreData(0);
        }

        public static void GenerateCompareGreaterThanUnsigned(Operation operation, Generator generator)
        {
            generator.LoadData(0);
            generator.LoadData(1);

            generator.il.Emit(OpCodes.Cgt_Un);

            generator.StoreData(0);
        }

        public static void GenerateCompareLessThan(Operation operation, Generator generator)
        {
            generator.LoadData(0);
            generator.LoadData(1);

            generator.il.Emit(OpCodes.Clt);

            generator.StoreData(0);
        }

        public static void GenerateCompareLessThanUnsigned(Operation operation, Generator generator)
        {
            generator.LoadData(0);
            generator.LoadData(1);

            generator.il.Emit(OpCodes.Clt_Un);

            generator.StoreData(0);
        }

        public static void GenerateCopy(Operation operation, Generator generator)
        {
            generator.LoadData(1);

            generator.StoreData(0);
        }

        public static void GenerateDivide(Operation operation, Generator generator)
        {
            generator.LoadData(0);
            generator.LoadData(1);

            generator.il.Emit(OpCodes.Div);

            generator.StoreData(0);
        }

        public static void GenerateDivide_Un(Operation operation, Generator generator)
        {
            generator.LoadData(0);
            generator.LoadData(1);

            generator.il.Emit(OpCodes.Div_Un);

            generator.StoreData(0);
        }

        public static void GenerateGetContextPointer(Operation operation, Generator generator)
        {
            generator.il.Emit(OpCodes.Ldarg_0);

            generator.StoreData(0);
        }

        public static void GenerateJump(Operation operation, Generator generator)
        {
            generator.il.Emit(OpCodes.Br, generator.Lables[(int)operation.Arguments[0].label.Address]);
        }

        public static void GenerateJumpIf(Operation operation, Generator generator)
        {
            generator.LoadData(0);

            generator.il.Emit(OpCodes.Brtrue, generator.Lables[(int)operation.Arguments[1].label.Address]);
        }

        public static void GenerateLoadImmediate(Operation operation, Generator generator)
        {
            /*
            if (operation.Size == OperationSize.Int32)
            {
                generator.il.Emit(OpCodes.Ldc_I4, (int)(uint)operation.GetImm(1));
            }
            else
            {
                generator.il.Emit(OpCodes.Ldc_I8, (long)operation.GetImm(1));
            }
            */

            generator.LoadData(1);

            generator.il.Emit(OpCodes.Conv_U8);

            generator.StoreData(0);
        }

        public static void GenerateLoadMem(Operation operation, Generator generator)
        {
            generator.LoadData(1);

            generator.il.Emit(OpCodes.Ldind_I8);

            generator.StoreData(0);
        }

        public static void GenerateMod(Operation operation, Generator generator)
        {
            generator.LoadData(0);
            generator.LoadData(1);

            generator.il.Emit(OpCodes.Rem);

            generator.StoreData(0);
        }

        public static void GenerateMultiply(Operation operation, Generator generator)
        {
            generator.LoadData(0);
            generator.LoadData(1);

            generator.il.Emit(OpCodes.Mul);

            generator.StoreData(0);
        }

        public static void GenerateNot(Operation operation, Generator generator)
        {
            generator.LoadData(0);

            generator.il.Emit(OpCodes.Not);

            generator.StoreData(0);
        }

        public static void GenerateOr(Operation operation, Generator generator)
        {
            generator.LoadData(0);
            generator.LoadData(1);

            generator.il.Emit(OpCodes.Or);

            generator.StoreData(0);
        }

        public static void GenerateReturn(Operation operation, Generator generator)
        {
            generator.LoadData(0);

            generator.il.Emit(OpCodes.Stloc,generator.ReturnLocal);
        }

        public static void GenerateShiftLeft(Operation operation, Generator generator)
        {
            generator.LoadData(0);
            generator.LoadData(1);

            generator.il.Emit(OpCodes.Shl);

            generator.StoreData(0);
        }

        public static void GenerateShiftRight(Operation operation, Generator generator)
        {
            generator.LoadData(0);
            generator.LoadData(1);

            generator.il.Emit(OpCodes.Shr_Un);

            generator.StoreData(0);
        }

        public static void GenerateShiftRightSigned(Operation operation, Generator generator)
        {
            generator.LoadData(0);
            generator.LoadData(1);

            generator.il.Emit(OpCodes.Shr);

            generator.StoreData(0);
        }

        public static void GenerateSignExtend16(Operation operation, Generator generator)
        {
            generator.LoadData(0);

            generator.il.Emit(OpCodes.Conv_I2);
            generator.il.Emit(OpCodes.Conv_I8);

            generator.StoreData(0);
        }

        public static void GenerateSignExtend32(Operation operation, Generator generator)
        {
            generator.LoadData(0);

            generator.il.Emit(OpCodes.Conv_I4);
            generator.il.Emit(OpCodes.Conv_I8);

            generator.StoreData(0);
        }

        public static void GenerateSignExtend8(Operation operation, Generator generator)
        {
            generator.LoadData(0);

            generator.il.Emit(OpCodes.Conv_I1);
            generator.il.Emit(OpCodes.Conv_I8);

            generator.StoreData(0);
        }

        public static void GenerateStore16(Operation operation, Generator generator)
        {
            generator.LoadData(0);
            generator.LoadData(1);

            generator.il.Emit(OpCodes.Stind_I2);
        }

        public static void GenerateStore32(Operation operation, Generator generator)
        {
            generator.LoadData(0);
            generator.LoadData(1);

            generator.il.Emit(OpCodes.Stind_I4);
        }

        public static void GenerateStore64(Operation operation, Generator generator)
        {
            generator.LoadData(0);
            generator.LoadData(1);

            generator.il.Emit(OpCodes.Stind_I8);
        }

        public static void GenerateStore8(Operation operation, Generator generator)
        {
            generator.LoadData(0);
            generator.LoadData(1);

            generator.il.Emit(OpCodes.Stind_I1);
        }

        public static void GenerateSubtract(Operation operation, Generator generator)
        {
            generator.LoadData(0);
            generator.LoadData(1);

            generator.il.Emit(OpCodes.Sub);

            generator.StoreData(0);
        }

        public static void GenerateWriteRegister(Operation operation, Generator generator)
        {
            throw new NotImplementedException();
        }

        public static void GenerateXor(Operation operation, Generator generator)
        {
            generator.LoadData(0);
            generator.LoadData(1);

            generator.il.Emit(OpCodes.Xor);

            generator.StoreData(0);
        }

        public static void GenerateNop(Operation operation, Generator generator)
        {
            generator.il.Emit(OpCodes.Nop);
        }
    }
}
