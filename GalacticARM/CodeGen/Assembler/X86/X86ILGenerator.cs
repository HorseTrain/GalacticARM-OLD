using GalacticARM.CodeGen.Intermediate;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static Iced.Intel.AssemblerRegisters;

namespace GalacticARM.CodeGen.Assembler.X86
{
    public delegate void ILAssembler(X86Compiler assembler);

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
            GenerateNop,
        };

        public static void GenerateAdd(X86Compiler assembler)
        {
            assembler.c.add(assembler.GetOperand(0),assembler.GetOperand(1));
        }

        public static void GenerateAnd(X86Compiler assembler)
        {
            assembler.c.and(assembler.GetOperand(0), assembler.GetOperand(1));
        }

        public static void GenerateCall(X86Compiler assembler)
        {
            assembler.UnloadAllRegisters();

            int Count = (int)assembler.CurrentOperation.Arguments[0].Imm;

            //Console.WriteLine(assembler.CurrentOperation);

            assembler.c.push(rbx);
            assembler.c.push(rbp);
            assembler.c.push(rsi);
            assembler.c.push(r15);

            assembler.c.mov(r14,assembler.GetOperand(1,3));

            assembler.c.sub(rsp, 0x58);

            switch (Count)
            {
                case 1: assembler.c.mov(rcx,assembler.GetRawPointerOrImm(2)); break;

                case 2:

                    assembler.c.mov(rcx, assembler.GetRawPointerOrImm(2));
                    assembler.c.mov(rdx, assembler.GetRawPointerOrImm(3));

                    break;

                case 3:

                    assembler.c.mov(rcx, assembler.GetRawPointerOrImm(2));
                    assembler.c.mov(rdx, assembler.GetRawPointerOrImm(3));
                    assembler.c.mov(r8, assembler.GetRawPointerOrImm(4));

                    break;

                case 4:

                    assembler.c.mov(rcx, assembler.GetRawPointerOrImm(2));
                    assembler.c.mov(rdx, assembler.GetRawPointerOrImm(3));
                    assembler.c.mov(r8, assembler.GetRawPointerOrImm(4));
                    assembler.c.mov(r9, assembler.GetRawPointerOrImm(5));


                    break;
                default: throw new Exception();
            }

            assembler.c.call(r14);

            assembler.c.add(rsp, 0x58);

            assembler.c.pop(r15);
            assembler.c.pop(rsi);
            assembler.c.pop(rbp);
            assembler.c.pop(rbx);

            //throw new NotImplementedException();
        }

        public static void GenerateCompareEqual(X86Compiler assembler)
        {
            assembler.c.cmp(assembler.GetOperand(0), assembler.GetOperand(1));

            assembler.c.sete(assembler.GetOperand(0,0));
            assembler.c.and(assembler.GetOperand(0),1);
        }

        public static void GenerateCompareGreaterThan(X86Compiler assembler)
        {
            assembler.c.cmp(assembler.GetOperand(0), assembler.GetOperand(1));

            assembler.c.setg(assembler.GetOperand(0, 0));
            assembler.c.and(assembler.GetOperand(0), 1);
        }

        public static void GenerateCompareGreaterThanUnsigned(X86Compiler assembler)
        {
            assembler.c.cmp(assembler.GetOperand(0), assembler.GetOperand(1));

            assembler.c.seta(assembler.GetOperand(0, 0));
            assembler.c.and(assembler.GetOperand(0), 1);
        }

        public static void GenerateCompareLessThan(X86Compiler assembler)
        {
            assembler.c.cmp(assembler.GetOperand(0), assembler.GetOperand(1));

            assembler.c.setl(assembler.GetOperand(0, 0));
            assembler.c.and(assembler.GetOperand(0), 1);
        }

        public static void GenerateCompareLessThanUnsigned(X86Compiler assembler)
        {
            assembler.c.cmp(assembler.GetOperand(0), assembler.GetOperand(1));

            assembler.c.setb(assembler.GetOperand(0, 0));
            assembler.c.and(assembler.GetOperand(0), 1);
        }

        public static void GenerateCopy(X86Compiler assembler)
        {
            assembler.c.mov(assembler.GetOperand(0,-1,RequestType.Write),assembler.GetOperand(1));
        }

        public static void GenerateDivide(X86Compiler assembler)
        {
            assembler.UnloadAllRegisters();

            if (assembler.CurrentOperation.Size == OperationSize.Int32)
            {
                assembler.c.mov(eax, assembler.GetRawPointerOrImm(0));
                assembler.c.cdq();

                assembler.c.mov(ecx, assembler.GetRawPointerOrImm(1));

                assembler.c.idiv(ecx);
            }
            else
            {
                assembler.c.mov(rax, assembler.GetRawPointerOrImm(0));
                assembler.c.cqo();

                assembler.c.mov(rcx, assembler.GetRawPointerOrImm(1));

                assembler.c.idiv(rcx);
            }

            assembler.c.mov(assembler.GetRawPointerOrImm(0), rax);
        }

        public static void GenerateDivide_Un(X86Compiler assembler)
        {
            assembler.UnloadAllRegisters();

            if (assembler.CurrentOperation.Size == OperationSize.Int32)
            {
                assembler.c.mov(eax, assembler.GetRawPointerOrImm(0));
                assembler.c.mov(edx, 0);

                assembler.c.mov(ecx, assembler.GetRawPointerOrImm(1));

                assembler.c.div(ecx);
            }
            else
            {
                assembler.c.mov(rax, assembler.GetRawPointerOrImm(0));
                assembler.c.mov(rdx, 0);

                assembler.c.mov(rcx, assembler.GetRawPointerOrImm(1));

                assembler.c.div(rcx);
            }

            assembler.c.mov(assembler.GetRawPointerOrImm(0), rax);
        }

        public static void GenerateF_Add(X86Compiler assembler)
        {
            Operation operation = assembler.CurrentOperation;

            assembler.c.movq(xmm0, assembler.GetOperand(0));
            assembler.c.movq(xmm1, assembler.GetOperand(1));

            if (operation.Arguments[2].Imm == 0)
            {
                assembler.c.addss(xmm0, xmm1);
            }
            else if (operation.Arguments[2].Imm == 1)
            {
                assembler.c.addsd(xmm0, xmm1);
            }

            assembler.c.movq(assembler.GetOperand(0, 3), xmm0);
        }

        public static void GenerateF_ConvertPrecision(X86Compiler assembler)
        {
            Operation operation = assembler.CurrentOperation;

            //Console.WriteLine(operation);

            Operand[] args = operation.Arguments;

            assembler.c.movq(xmm0, assembler.GetOperand(0));

            if (args[1].Imm == 1 && args[2].Imm == 0) // float to double
            {
                assembler.c.cvtss2sd(xmm0,xmm0);
            }
            else if (args[1].Imm == 0 && args[2].Imm == 1) // double to float
            {
                assembler.c.cvtsd2ss(xmm0, xmm0);
            }
            else
            {
                throw new NotImplementedException();
            }

            assembler.c.movq(assembler.GetOperand(0), xmm0);
        }

        public static void GenerateF_Div(X86Compiler assembler)
        {
            Operation operation = assembler.CurrentOperation;

            assembler.c.movq(xmm0, assembler.GetOperand(0));
            assembler.c.movq(xmm1, assembler.GetOperand(1));

            if (operation.Arguments[2].Imm == 0)
            {
                assembler.c.divss(xmm0, xmm1);
            }
            else if (operation.Arguments[2].Imm == 1)
            {
                assembler.c.divsd(xmm0, xmm1);
            }

            assembler.c.movq(assembler.GetOperand(0, 3), xmm0);
        }

        public static void GenerateF_FloatConvertToInt(X86Compiler assembler)
        {
            Operation operation = assembler.CurrentOperation;

            //Console.WriteLine(operation);

            Operand[] args = operation.Arguments;

            bool Signed = args[3].Imm == 1;

            assembler.c.movq(xmm0, assembler.GetOperand(0, 3));

            if (Signed)
            {
                if (args[1].Imm == 0 && args[2].Imm == 0) //float to int
                {
                    assembler.c.cvttss2si(assembler.GetOperand(0, 3),xmm0);
                }
                else if (args[1].Imm == 0 && args[2].Imm == 1) //double to int
                {
                    assembler.c.cvttsd2si(assembler.GetOperand(0, 3), xmm0);
                }
                else
                {
                    throw new Exception();
                }
            }
            else
            {
                throw new Exception();
            }

            //throw new Exception();
        }

        public static void GenerateF_GreaterThan(X86Compiler assembler)
        {
            throw new NotImplementedException();
        }

        public static void GenerateF_IntConvertToFloat(X86Compiler assembler)
        {
            Operation operation = assembler.CurrentOperation;

            //Console.WriteLine(operation);

            Operand[] args = operation.Arguments;

            bool Signed = args[3].Imm == 1;

            if (Signed)
            {
                if (args[1].Imm == 0 && args[2].Imm == 0) //int to float
                {
                    assembler.c.cvtsi2ss(xmm0,assembler.GetOperand(0,2));               
                }
                else if (args[1].Imm == 1 && args[2].Imm == 0) //int to double
                {
                    assembler.c.cvtsi2sd(xmm0, assembler.GetOperand(0, 2));
                }
                else
                {
                    throw new Exception();
                }
            }
            else
            {
                if (args[1].Imm == 0 && args[2].Imm == 0) //uint to float
                {
                    assembler.c.cvtsi2ss(xmm0, assembler.GetOperand(0, 3));
                }
                else if (args[1].Imm == 1 && args[2].Imm == 0) //ulong to float
                {
                    assembler.c.cvtsi2ss(xmm0, assembler.GetOperand(0, 3));
                }
                else
                {
                    throw new Exception();
                }
            }

            assembler.c.movq(assembler.GetOperand(0, 3), xmm0);

            //throw new NotImplementedException();
        }

        public static void GenerateF_LessThan(X86Compiler assembler)
        {
            throw new NotImplementedException();
        }

        public static void GenerateF_Mul(X86Compiler assembler)
        {
            Operation operation = assembler.CurrentOperation;

            assembler.c.movq(xmm0, assembler.GetOperand(0));
            assembler.c.movq(xmm1, assembler.GetOperand(1));

            if (operation.Arguments[2].Imm == 0)
            {
                assembler.c.mulss(xmm0,xmm1);
            }
            else if (operation.Arguments[2].Imm == 1)
            {
                assembler.c.mulpd(xmm0, xmm1);
            }

            assembler.c.movq(assembler.GetOperand(0, 3), xmm0);
        }

        public static void GenerateF_Sub(X86Compiler assembler)
        {
            Operation operation = assembler.CurrentOperation;

            assembler.c.movq(xmm0, assembler.GetOperand(0));
            assembler.c.movq(xmm1, assembler.GetOperand(1));

            if (operation.Arguments[2].Imm == 0)
            {
                assembler.c.subss(xmm0, xmm1);
            }
            else if (operation.Arguments[2].Imm == 1)
            {
                assembler.c.subsd(xmm0, xmm1);
            }

            assembler.c.movq(assembler.GetOperand(0, 3), xmm0);
        }

        public static void GenerateGetContextPointer(X86Compiler assembler)
        {
            assembler.c.mov(assembler.GetOperand(0),r15);
        }

        public static void GenerateJump(X86Compiler assembler)
        {
            assembler.c.jmp(assembler.GetOperand(0));
        }

        public static void GenerateJumpIf(X86Compiler assembler)
        {
            //Console.WriteLine(assembler.CurrentOperation);

            assembler.c.cmp(assembler.GetOperand(0),1);
            assembler.c.je(assembler.GetOperand(1));
        }

        public static void GenerateLoadImmediate(X86Compiler assembler)
        {
            assembler.c.mov(assembler.GetOperand(0,-1,RequestType.Write), assembler.GetOperand(1));
        }

        public static void GenerateLoadMem(X86Compiler assembler)
        {
            assembler.c.mov(assembler.GetOperand(0,3),__[assembler.GetOperand(1,3)]);
        }

        public static void GenerateMod(X86Compiler assembler)
        {
            assembler.UnloadAllRegisters();

            if (assembler.CurrentOperation.Size == OperationSize.Int32)
            {
                assembler.c.mov(eax, assembler.GetRawPointerOrImm(0));
                assembler.c.cdq();

                assembler.c.mov(ecx, assembler.GetRawPointerOrImm(1));

                assembler.c.idiv(ecx);
            }
            else
            {
                assembler.c.mov(rax, assembler.GetRawPointerOrImm(0));
                assembler.c.cqo();

                assembler.c.mov(rcx, assembler.GetRawPointerOrImm(1));

                assembler.c.idiv(rcx);
            }

            assembler.c.mov(assembler.GetRawPointerOrImm(0), rdx);
        }

        public static void GenerateMultiply(X86Compiler assembler)
        {
            //if (assembler.CurrentOperation.Arguments[1].Type == OperandType.Constant)
                //assembler.c.imul(assembler.GetOperand(0), assembler.GetOperand(0), assembler.GetOperand(1));
            //else
                assembler.c.imul(assembler.GetOperand(0), assembler.GetOperand(1));
        }

        public static void GenerateNot(X86Compiler assembler)
        {
            assembler.c.not(assembler.GetOperand(0));
        }

        public static void GenerateOr(X86Compiler assembler)
        {
            assembler.c.or(assembler.GetOperand(0), assembler.GetOperand(1));
        }

        public static void GenerateReturn(X86Compiler assembler)
        {
            assembler.UnloadAllRegisters();

            assembler.c.mov(rax,assembler.GetOperand(0,3));

            assembler.c.ret();
        }

        public static void GenerateShiftLeft(X86Compiler assembler)
        {
            assembler.c.mov(rcx,assembler.GetOperand(1,3));

            assembler.c.shl(assembler.GetOperand(0),cl);
        }

        public static void GenerateShiftRight(X86Compiler assembler)
        {
            assembler.c.mov(rcx, assembler.GetOperand(1, 3));

            assembler.c.shr(assembler.GetOperand(0), cl);
        }

        public static void GenerateShiftRightSigned(X86Compiler assembler)
        {
            assembler.c.mov(rcx, assembler.GetOperand(1, 3));

            assembler.c.sar(assembler.GetOperand(0), cl);
        }

        public static void GenerateSignExtend16(X86Compiler assembler)
        {
            assembler.c.movsx(assembler.GetOperand(0), assembler.GetOperand(0, 1));
        }

        public static void GenerateSignExtend32(X86Compiler assembler)
        {
            assembler.c.movsxd(assembler.GetOperand(0), assembler.GetOperand(0, 2));
        }

        public static void GenerateSignExtend8(X86Compiler assembler)
        {
            assembler.c.movsx(assembler.GetOperand(0), assembler.GetOperand(0,0));
        }

        public static void GenerateStore16(X86Compiler assembler)
        {
            assembler.c.mov(__[assembler.GetOperand(0,3)],assembler.GetOperand(1,1));
        }

        public static void GenerateStore32(X86Compiler assembler)
        {
            assembler.c.mov(__[assembler.GetOperand(0, 3)], assembler.GetOperand(1, 2));
        }

        public static void GenerateStore64(X86Compiler assembler)
        {
            assembler.c.mov(__[assembler.GetOperand(0, 3)], assembler.GetOperand(1, 3));
        }

        public static void GenerateStore8(X86Compiler assembler)
        {
            assembler.c.mov(__[assembler.GetOperand(0, 3)], assembler.GetOperand(1, 0));
        }

        public static void GenerateSubtract(X86Compiler assembler)
        {
            assembler.c.sub(assembler.GetOperand(0), assembler.GetOperand(1));
        }

        public static void GenerateWriteRegister(X86Compiler assembler)
        {
            throw new NotImplementedException();
        }

        public static void GenerateXor(X86Compiler assembler)
        {
            assembler.c.xor(assembler.GetOperand(0), assembler.GetOperand(1));
        }

        public static void GenerateNop(X86Compiler assembler)
        {
            assembler.c.nop();
        }
    }
}