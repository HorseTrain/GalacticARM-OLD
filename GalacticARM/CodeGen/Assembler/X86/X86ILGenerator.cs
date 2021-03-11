using GalacticARM.CodeGen.Intermediate;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static Iced.Intel.AssemblerRegisters;

namespace GalacticARM.CodeGen.Assembler.X86
{
    public delegate void ILAssembler(X86Assembler assembler);

    public static class X86ILGenerator
    {
        public static ILAssembler[] Funcs = new ILAssembler[]
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
        };

        public static void GenerateAdd(X86Assembler assembler)
        {
            assembler.c.add(assembler.GetReg(0), assembler.GetReg(1));
        }

        public static void GenerateAnd(X86Assembler assembler)
        {
            assembler.c.and(assembler.GetReg(0), assembler.GetReg(1));
        }

        public static void GenerateCall(X86Assembler assembler)
        {
            assembler.UnloadAllRegisters();

            assembler.c.mov(r14,assembler.RegFromMem(1));

            assembler.c.jmp(r14);

            assembler.c.call(r14);

            assembler.c.mov(rcx,300);
            assembler.c.ret();
        }

        public static void GenerateCompareEqual(X86Assembler assembler)
        {
            assembler.c.cmp(assembler.GetReg(0), assembler.GetReg(1));

            assembler.c.sete(assembler.GetReg(0,0));
            assembler.c.and(assembler.GetReg(0),1);
        }

        public static void GenerateCompareGreaterThan(X86Assembler assembler)
        {
            throw new NotImplementedException();
        }

        public static void GenerateCompareGreaterThanUnsigned(X86Assembler assembler)
        {
            throw new NotImplementedException();
        }

        public static void GenerateCompareLessThan(X86Assembler assembler)
        {
            assembler.c.cmp(assembler.GetReg(0), assembler.GetReg(1));

            assembler.c.setl(assembler.GetReg(0, 0));
            assembler.c.and(assembler.GetReg(0), 1);
        }

        public static void GenerateCompareLessThanUnsigned(X86Assembler assembler)
        {
            assembler.c.cmp(assembler.GetReg(0), assembler.GetReg(1));

            assembler.c.setb(assembler.GetReg(0, 0));
            assembler.c.and(assembler.GetReg(0), 1);
        }

        public static void GenerateCopy(X86Assembler assembler)
        {
            assembler.c.mov(assembler.GetReg(0), assembler.GetReg(1));
        }

        public static void GenerateDivide(X86Assembler assembler)
        {
            throw new NotImplementedException();
        }

        public static void GenerateDivide_Un(X86Assembler assembler)
        {
            throw new NotImplementedException();
        }

        public static void GenerateF_Add(X86Assembler assembler)
        {
            throw new NotImplementedException();
        }

        public static void GenerateF_ConvertPrecision(X86Assembler assembler)
        {
            throw new NotImplementedException();
        }

        public static void GenerateF_Div(X86Assembler assembler)
        {
            throw new NotImplementedException();
        }

        public static void GenerateF_FloatConvertToInt(X86Assembler assembler)
        {
            throw new NotImplementedException();
        }

        public static void GenerateF_GreaterThan(X86Assembler assembler)
        {
            throw new NotImplementedException();
        }

        public static void GenerateF_IntConvertToFloat(X86Assembler assembler)
        {
            throw new NotImplementedException();
        }

        public static void GenerateF_LessThan(X86Assembler assembler)
        {
            throw new NotImplementedException();
        }

        public static void GenerateF_Mul(X86Assembler assembler)
        {
            throw new NotImplementedException();
        }

        public static void GenerateF_Sub(X86Assembler assembler)
        {
            throw new NotImplementedException();
        }

        public static void GenerateGetContextPointer(X86Assembler assembler)
        {
            assembler.c.mov(assembler.GetReg(0),r15);
        }

        public static void GenerateJump(X86Assembler assembler)
        {
            assembler.c.jmp(assembler.Lables[assembler.CurrentOperation.Arguments[0].label.Address]);
        }

        public static void GenerateJumpIf(X86Assembler assembler)
        {
            assembler.c.cmp(assembler.GetReg(0),1);

            assembler.c.je(assembler.Lables[assembler.CurrentOperation.Arguments[1].label.Address]);
        }

        public static void GenerateLoadImmediate(X86Assembler assembler)
        {
            assembler.c.mov(assembler.GetReg(0), assembler.GetImm(1));
        }

        public static void GenerateLoadMem(X86Assembler assembler)
        {
            assembler.c.mov(assembler.GetReg(0),__[assembler.GetReg(1)]);
        }

        public static void GenerateMod(X86Assembler assembler)
        {
            throw new NotImplementedException();
        }

        public static void GenerateMultiply(X86Assembler assembler)
        {
            assembler.c.imul(assembler.GetReg(0), assembler.GetReg(1));
        }

        public static void GenerateNot(X86Assembler assembler)
        {
            assembler.c.not(assembler.GetReg(0));
        }

        public static void GenerateOr(X86Assembler assembler)
        {
            assembler.c.or(assembler.GetReg(0), assembler.GetReg(1));
        }

        public static void GenerateReturn(X86Assembler assembler)
        {
            assembler.UnloadAllRegisters();

            assembler.c.mov(rcx,assembler.GetReg(0));

            assembler.c.ret();
        }

        public static void GenerateShiftLeft(X86Assembler assembler)
        {
            assembler.c.mov(assembler.GetRCX(), assembler.GetReg(1));

            assembler.c.shl(assembler.GetReg(0),cl);
        }

        public static void GenerateShiftRight(X86Assembler assembler)
        {
            assembler.c.mov(assembler.GetRCX(), assembler.GetReg(1));

            assembler.c.shr(assembler.GetReg(0), cl);
        }

        public static void GenerateShiftRightSigned(X86Assembler assembler)
        {
            assembler.c.mov(assembler.GetRCX(), assembler.GetReg(1));

            assembler.c.sar(assembler.GetReg(0), cl);
        }

        public static void GenerateSignExtend16(X86Assembler assembler)
        {
            throw new NotImplementedException();
        }

        public static void GenerateSignExtend32(X86Assembler assembler)
        {
            assembler.c.movsxd(assembler.GetReg(0), assembler.GetReg(0,2));
        }

        public static void GenerateSignExtend8(X86Assembler assembler)
        {
            assembler.c.movsx(assembler.GetReg(0), assembler.GetReg(0, 0));
        }

        public static void GenerateStore16(X86Assembler assembler)
        {
            throw new NotImplementedException();
        }

        public static void GenerateStore32(X86Assembler assembler)
        {
            assembler.c.mov(__[assembler.GetReg(0)], assembler.GetReg(1,2));
        }

        public static void GenerateStore64(X86Assembler assembler)
        {
            assembler.c.mov(__[assembler.GetReg(0)], assembler.GetReg(1));
        }

        public static void GenerateStore8(X86Assembler assembler)
        {
            assembler.c.mov(__[assembler.GetReg(0)],assembler.GetReg(1,0));
        }

        public static void GenerateSubtract(X86Assembler assembler)
        {
            assembler.c.sub(assembler.GetReg(0), assembler.GetReg(1));
        }

        public static void GenerateWriteRegister(X86Assembler assembler)
        {
            throw new NotImplementedException();
        }

        public static void GenerateXor(X86Assembler assembler)
        {
            assembler.c.xor(assembler.GetReg(0), assembler.GetReg(1));
        }
    }
}