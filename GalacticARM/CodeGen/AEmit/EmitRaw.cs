using GalacticARM.CodeGen.Intermediate;
using GalacticARM.Decoding;
using GalacticARM.Decoding.Abstractions;
using GalacticARM.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.AEmit
{
    public static class EmitRaw
    {
        public static void EmitCompareBranchImmediate(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.CBZ: EmitControlFlow.Cbz(context, false); break;
                case InstructionMnemonic.CBNZ: EmitControlFlow.Cbz(context, true); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitTestBitAndBranchImmediate(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.TBZ: EmitControlFlow.Tbz(context, true); break;
                case InstructionMnemonic.TBNZ: EmitControlFlow.Tbz(context, false); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitConditionalBranchImmediate(InstructionEmitContext context)
        {
            EmitControlFlow.B_Cond(context);
        }

        public static void EmitExceptionGeneration(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.SVC: EmitSys.Svc(context); break;
                default: context.ThrowUnknown(); break;
            }

            context.AdvancePC();
        }

        public static unsafe void EmitSystem(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.MSR: EmitSys.Msr(context); break;
                case InstructionMnemonic.MRS: EmitSys.Mrs(context); break;
                case InstructionMnemonic.HINT: /*No Op*/ break;
                case InstructionMnemonic.DMB: /*No Op*/ break;
                case InstructionMnemonic.DSB: /*No Op*/ break;
                case InstructionMnemonic.CLREX: context.CallFunctionFromPointer(context.Const((ulong)DelegateCache.GetOrAdd(new _Void_U(ExclusiveMonitors.Clrex))), context.GetContextPointer()); break;
                case InstructionMnemonic.SYS: /*No Op*/ break;
                default: context.ThrowUnknown(); break;
            }

            context.AdvancePC();
        }

        public static void EmitUnconditionalBranchRegister(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.RET: EmitControlFlow.Ret(context); break;
                case InstructionMnemonic.BR: EmitControlFlow.Br(context,false); break;
                case InstructionMnemonic.BLR: EmitControlFlow.Br(context,true); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitUnconditionalBranchImmediate(InstructionEmitContext context)
        {
            OpCodeUnconditionalBranchImmediate opCode = (OpCodeUnconditionalBranchImmediate)context.CurrentOpCode;

            ulong NewAddress = opCode.Address + (ulong)(((long)opCode.imm26 << 38) >> 36);

            if (opCode.Name == InstructionMnemonic.BL)
            {
                context.SetRegister(30,context.Const(opCode.Address + 4));
            }

            context.ReturnWithPC(context.Const(NewAddress));
        }

        public static void EmitLoadStoreExclusive(InstructionEmitContext context)
        {
            EmitMemory.LoadStoreExclusive(context);
        }

        public static void EmitLoadRegisterLiteral(InstructionEmitContext context)
        {
            context.ThrowUnknown();
        }

        public static void EmitLoadStoreNoAllocatePairOffset(InstructionEmitContext context)
        {
            context.ThrowUnknown();
        }

        public static void EmitLoadStoreRegisterPairPostIndexed(InstructionEmitContext context)
        {
            EmitMemory.LoadStorePair(context);
        }

        public static void EmitLoadStoreRegisterPairOffset(InstructionEmitContext context)
        {
            EmitMemory.LoadStorePair(context);
        }

        public static void EmitLoadStoreRegisterPairPreIndexed(InstructionEmitContext context)
        {
            EmitMemory.LoadStorePair(context);
        }

        public static void EmitLoadStoreRegisterUnscaledImmediate(InstructionEmitContext context)
        {
            EmitMemory.LoadStore(context);
        }

        public static void EmitLoadStoreRegisterImmediatePostIndexed(InstructionEmitContext context)
        {
            EmitMemory.LoadStore(context);
        }

        public static void EmitLoadStoreRegisterUnprivileged(InstructionEmitContext context)
        {
            context.ThrowUnknown();
        }

        public static void EmitLoadStoreRegisterImmediatePreIndexed(InstructionEmitContext context)
        {
            EmitMemory.LoadStore(context);
        }

        public static void EmitLoadStoreRegisterRegisterOffset(InstructionEmitContext context)
        {
            EmitMemory.LoadStore(context);
        }

        public static void EmitLoadStoreRegisterUnsignedImmediate(InstructionEmitContext context)
        {
            EmitMemory.LoadStore(context);
        }

        public static void EmitPCRelAddressing(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.ADR: EmitALU.Adr(context,false); break;
                case InstructionMnemonic.ADRP: EmitALU.Adr(context,true); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitAddSubtractImmediate(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.ADD: EmitALU.Add(context,true); break;
                case InstructionMnemonic.ADDS:EmitALU.Adds(context); break;
                case InstructionMnemonic.SUB: EmitALU.Sub(context,true); break;
                case InstructionMnemonic.SUBS:EmitALU.Subs(context); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitLogicalImmediate(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.AND: EmitALU.And(context,false,true); break;
                case InstructionMnemonic.ORR: EmitALU.Orr(context, false, true); break;
                case InstructionMnemonic.EOR: EmitALU.Eor(context, false, true); break;
                case InstructionMnemonic.ANDS: EmitALU.Ands(context,false); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitMoveWideImmediate(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.MOVZ: EmitALU.Mov(context,false); break;
                case InstructionMnemonic.MOVN: EmitALU.Mov(context,true); break;
                case InstructionMnemonic.MOVK: EmitALU.MovK(context); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitBitfield(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.BFM: EmitALU.Bfm(context); break;
                case InstructionMnemonic.SBFM: EmitALU.Sbfm(context); break;
                case InstructionMnemonic.UBFM: EmitALU.Ubfm(context); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitExtract(InstructionEmitContext context)
        {
            OpCodeExtract opCode = (OpCodeExtract)context.CurrentOpCode;

            Operand res = context.GetRegister(opCode.rm);

            if (opCode.imms != 0)
            {
                if (opCode.rn == opCode.rm)
                {
                    res = context.RotateRight(res,context.Const(opCode.imms));
                }
                else
                {
                    res = context.ShiftRight(res,context.Const(opCode.imms));

                    Operand n = context.GetRegister(opCode.rn);

                    int invShift = (context.Block.Size == OperationSize.Int32 ? 32 : 64) - opCode.imms;

                    res = context.Or(res,context.ShiftLeft(n,context.Const(invShift)));
                }
            }
            else
            {
                context.ThrowUnknown();
            }

            context.SetRegister(opCode.rd,res);
        }

        public static void EmitLogicalShiftedRegister(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.AND: EmitALU.And(context,false,false); break;
                case InstructionMnemonic.BIC: EmitALU.And(context,true,false); break;

                case InstructionMnemonic.ORR: EmitALU.Orr(context, false, false); break;
                case InstructionMnemonic.ORN: EmitALU.Orr(context, true, false); break;

                case InstructionMnemonic.EOR: EmitALU.Eor(context, false, false); break;
                case InstructionMnemonic.EON: EmitALU.Eor(context, true, false); break;

                case InstructionMnemonic.ANDS: EmitALU.Ands(context,false); break;
                case InstructionMnemonic.BICS: EmitALU.Ands(context,true);  break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitAddSubtractShiftedRegister(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.ADD: EmitALU.Add(context,false); break;
                case InstructionMnemonic.ADDS: EmitALU.Adds(context); break;
                case InstructionMnemonic.SUB: EmitALU.Sub(context, false); break;
                case InstructionMnemonic.SUBS: EmitALU.Subs(context); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitAddSubtractExtendedRegister(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.ADD: EmitALU.Add(context, true); break;
                case InstructionMnemonic.ADDS: EmitALU.Adds(context); break;
                case InstructionMnemonic.SUB: EmitALU.Sub(context, true); break;
                case InstructionMnemonic.SUBS: EmitALU.Subs(context); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitAddSubtractWithCarry(InstructionEmitContext context)
        {
            context.ThrowUnknown();
        }

        public static void EmitConditionalCompareRegister(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.CCMN: EmitControlFlow.Ccm(context, true); break;
                case InstructionMnemonic.CCMP: EmitControlFlow.Ccm(context,false); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitConditionalCompareImmediate(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.CCMN: EmitControlFlow.Ccm(context, true); break;
                case InstructionMnemonic.CCMP: EmitControlFlow.Ccm(context, false); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitConditionalSelect(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.CSEL: EmitControlFlow.Csel(context,false,false); break;
                case InstructionMnemonic.CSINC: EmitControlFlow.Csel(context,false,true); break;
                case InstructionMnemonic.CSINV: EmitControlFlow.Csel(context,true,false); break;
                case InstructionMnemonic.CSNEG: EmitControlFlow.Csel(context,true,true); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitDataProcessing3Source(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.MADD: EmitALU.Mul(context, true); break;
                case InstructionMnemonic.MSUB: EmitALU.Mul(context, false); break;
                case InstructionMnemonic.SMADDL: EmitALU.Mull(context,true,true); break;
                case InstructionMnemonic.SMSUBL: EmitALU.Mull(context,false,true); break;
                case InstructionMnemonic.UMADDL: EmitALU.Mull(context, true, false); break;
                case InstructionMnemonic.UMSUBL: EmitALU.Mull(context, false, false); break;
                case InstructionMnemonic.UMULH: EmitALU.Mulh(context, false); break;
                case InstructionMnemonic.SMULH: EmitALU.Mulh(context, true); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitDataProcessing2Source(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.UDIV: EmitALU.Div(context, false); break;
                case InstructionMnemonic.SDIV: EmitALU.Div(context,true); break;

                case InstructionMnemonic.LSLV: EmitALU.ShiftV(context, ShiftType.LSL); break;
                case InstructionMnemonic.LSRV: EmitALU.ShiftV(context, ShiftType.LSR); break;
                case InstructionMnemonic.ASRV: EmitALU.ShiftV(context, ShiftType.ASR); break;
                case InstructionMnemonic.RORV: EmitALU.ShiftV(context, ShiftType.ROR); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitDataProcessing1Source(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.RBIT: EmitALU.Rbit(context); break;
                case InstructionMnemonic.CLZ: EmitALU.Clz(context); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitFloatingPointFixedPointConversions(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.FCVTZS: EmitFloatingPoint.Fcvtz(context); break;
                case InstructionMnemonic.FCVTZU: EmitFloatingPoint.Fcvtz(context); break; //Watch out 
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitFloatingPointConditionalCompare(InstructionEmitContext context)
        {
            /*
             *            int opc = ((opCode.opcode2 >> 3) & 1);

            ulong Args = 0;
            Args |= ((ulong)opCode.rn) << 5;
            Args |= ((ulong)opCode.rm) << 10;
            Args |= ((ulong)(opCode.opcode2 >> 3) & 1) << 30;

            if (opCode.type == 0 && (opCode.opcode2 == 0 || opCode.opcode2 == 8))
            {
                //Args |= 1UL << 20;
            }
            else if (opCode.type == 1 && (opCode.opcode2 == 0 || opCode.opcode2 == 8))
            {
                Args |= 1UL << 20;
            }
            else
            {
                context.ThrowUnknown();
            }

            context.CallFunctionFromPointer(context.GetDelegate(new _Void_U_U(EmitFloatingPoint.Fcmp_FB)), context.GetContextPointer(), context.Const(Args));
             */

            OpCodeFloatingPointConditionalCompare opCode = (OpCodeFloatingPointConditionalCompare)context.CurrentOpCode;

            if (opCode.Name == InstructionMnemonic.FCCMP)
            {
                int size = opCode.type + 2;

                EmitControlFlow.EmitIF(context,
                    
                    EmitControlFlow.ConditionHolds(context,(Condition)opCode.cond),

                    delegate()
                    {
                        ulong Args = 0;
                        Args |= ((ulong)opCode.rn) << 5;
                        Args |= ((ulong)opCode.rm) << 10;

                        if (opCode.Info == InstructionInfo.Double_precision)
                            Args |= 1UL << 20;

                        context.CallFunctionFromPointer(context.GetDelegate(new _Void_U_U(EmitFloatingPoint.Fccmp_GB)), context.GetContextPointer(), context.Const(Args));
                    },

                    delegate()
                    {
                        context.SetFlags(context.Const(opCode.nzcv));
                    }
                    
                    );

                //Console.WriteLine(context.Block);
            }
            else
            {
                context.ThrowUnknown();
            }
        }

        public static void EmitFloatingPointDataProcessing2Source(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.FADD: EmitFloatingPoint.F_Operation_Scalar(context, ILInstruction.F_Add,false); break;
                case InstructionMnemonic.FSUB: EmitFloatingPoint.F_Operation_Scalar(context, ILInstruction.F_Sub,false); break;
                case InstructionMnemonic.FMUL: EmitFloatingPoint.F_Operation_Scalar(context, ILInstruction.F_Mul,false); break;
                case InstructionMnemonic.FDIV: EmitFloatingPoint.F_Operation_Scalar(context, ILInstruction.F_Div,false); break;

                case InstructionMnemonic.FNMUL: EmitFloatingPoint.F_Operation_Scalar(context, ILInstruction.F_Mul, true); break;

                case InstructionMnemonic.FMAXNM: EmitFloatingPoint.F_Compare(context,true); break;
                case InstructionMnemonic.FMINNM: EmitFloatingPoint.F_Compare(context,false); break;
                case InstructionMnemonic.FMAX: EmitFloatingPoint.F_Compare(context, true); break;
                case InstructionMnemonic.FMIN: EmitFloatingPoint.F_Compare(context, false); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitFloatingPointConditionalSelect(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.FCSEL: EmitFloatingPoint.Fcsel(context); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitFloatingPointImmediate(InstructionEmitContext context)
        {
            OpCodeFloatingPointImmediate opCode = (OpCodeFloatingPointImmediate)context.CurrentOpCode;

            ulong imm = (ulong)opCode.imm8;

            EmitVector.ClearVector(context,opCode.rd);

            if (opCode.type == 0)
            {
                uint dat = ((uint)((uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(((uint)((uint)((uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(uint)(((uint)(((uint)((byte)((byte)(0x0)))) << 0)) | ((uint)(((uint)((byte)((byte)(0x0)))) << 1)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 2)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 3)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 4)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 5)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 6)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 7)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 8)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 9)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 10)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 11)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 12)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 13)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 14)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 15)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 16)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 17)))) | ((uint)(((uint)((byte)((byte)(0x0)))) << 18)))))) << 0)) | ((uint)(((uint)((byte)((byte)((byte)((((ulong)(imm)) & ((ulong)(0xF)))))))) << 19)))) | ((uint)(((uint)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x4)))) & ((ulong)(0x3)))))))) << 23)))) | ((uint)(((uint)((byte)((byte)(((byte)(byte)(((byte)(byte)(((byte)(byte)(((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 0)) | ((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 1)))) | ((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 2)))) | ((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 3)))) | ((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 4)))))) << 25)))) | ((uint)(((uint)((byte)(((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1))))) != 0 ? 0U : 1U))) << 30)))) | ((uint)(((uint)((byte)((byte)((byte)((imm) >> (int)(0x7)))))) << 31)))));

                EmitVector.InsertIntToVector(context,opCode.rd,context.Const(dat),2);
            }
            else if (opCode.type == 1)
            {
                ulong dat = ((ulong)((ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(((ulong)((ulong)((ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(ulong)(((ulong)(((ulong)((byte)((byte)(0x0)))) << 0)) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 1)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 2)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 3)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 4)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 5)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 6)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 7)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 8)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 9)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 10)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 11)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 12)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 13)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 14)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 15)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 16)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 17)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 18)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 19)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 20)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 21)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 22)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 23)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 24)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 25)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 26)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 27)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 28)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 29)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 30)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 31)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 32)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 33)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 34)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 35)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 36)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 37)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 38)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 39)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 40)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 41)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 42)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 43)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 44)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 45)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 46)))) | ((ulong)(((ulong)((byte)((byte)(0x0)))) << 47)))))) << 0)) | ((ulong)(((ulong)((byte)((byte)((byte)((((ulong)(imm)) & ((ulong)(0xF)))))))) << 48)))) | ((ulong)(((ulong)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x4)))) & ((ulong)(0x3)))))))) << 52)))) | ((ulong)(((ulong)((byte)((byte)(((byte)(byte)(((byte)(byte)(((byte)(byte)(((byte)(byte)(((byte)(byte)(((byte)(byte)(((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 0)) | ((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 1)))) | ((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 2)))) | ((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 3)))) | ((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 4)))) | ((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 5)))) | ((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 6)))) | ((byte)(((byte)((byte)((byte)((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1)))))))) << 7)))))) << 54)))) | ((ulong)(((ulong)((byte)(((byte)((((ulong)((byte)((imm) >> (int)(0x6)))) & ((ulong)(0x1))))) != 0 ? 0U : 1U))) << 62)))) | ((ulong)(((ulong)((byte)((byte)((byte)((imm) >> (int)(0x7)))))) << 63)))));

                EmitVector.InsertIntToVector(context,opCode.rd,context.Const(dat),3);
            }
            else
            {
                context.ThrowUnknown();
            }
        }

        public static void EmitFloatingPointCompare(InstructionEmitContext context)
        {
            OpCodeFloatingPointCompare opCode = (OpCodeFloatingPointCompare)context.CurrentOpCode;

            int opc = ((opCode.opcode2 >> 3) & 1);

            ulong Args = 0;
            Args |= ((ulong)opCode.rn) << 5;
            Args |= ((ulong)opCode.rm) << 10;
            Args |= ((ulong)(opCode.opcode2 >> 3) & 1) << 30;

            if (opCode.type == 0 && (opCode.opcode2 == 0 || opCode.opcode2 == 8))
            {
                //Args |= 1UL << 20;
            }
            else if (opCode.type == 1 && (opCode.opcode2 == 0 || opCode.opcode2 == 8))
            {
                Args |= 1UL << 20;
            }
            else
            {
                context.ThrowUnknown();
            }

            context.CallFunctionFromPointer(context.GetDelegate(new _Void_U_U(EmitFloatingPoint.Fcmp_FB)), context.GetContextPointer(), context.Const(Args));
        }

        public static void EmitFloatingPointDataProcessing1Source(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.FCVT: EmitFloatingPoint.Fcvt(context); break;
                case InstructionMnemonic.FNEG: EmitFloatingPoint.Fneg(context); break;
                case InstructionMnemonic.FABS: EmitFloatingPoint.Fabs(context); break;
                case InstructionMnemonic.FSQRT: EmitFloatingPoint.Fsqrt(context); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitFloatingPointIntegerConversions(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.FMOV: EmitFloatingPoint.Fmov(context); break;

                case InstructionMnemonic.SCVTF: EmitFloatingPoint.CVTF_Scalar(context,true); break;
                case InstructionMnemonic.UCVTF: EmitFloatingPoint.CVTF_Scalar(context, false); break;

                case InstructionMnemonic.FCVTZS: EmitFloatingPoint.FCVTZ_Scalar(context,true); break;
                case InstructionMnemonic.FCVTZU: EmitFloatingPoint.FCVTZ_Scalar(context, false); break;

                case InstructionMnemonic.FCVTMS: EmitFloatingPoint.FCVT_TO(context,false,true); break;
                case InstructionMnemonic.FCVTPS: EmitFloatingPoint.FCVT_TO(context, true,true); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitFloatingPointDataProcessing3Source(InstructionEmitContext context)
        {
            context.ThrowUnknown();
        }

        public static void EmitAdvsimdScalarThreeSame(InstructionEmitContext context)
        {
            context.ThrowUnknown();
        }

        public static void EmitAdvsimdScalarThreeDifferent(InstructionEmitContext context)
        {
            context.ThrowUnknown();
        }

        public static void EmitAdvsimdScalarTwoRegMisc(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.UCVTF: EmitFloatingPoint.CVTF_TwoRegMisc(context, false); break;
                case InstructionMnemonic.SCVTF: EmitFloatingPoint.CVTF_TwoRegMisc(context, true); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitAdvsimdScalarPairwise(InstructionEmitContext context)
        {
            context.ThrowUnknown();
        }

        public static void EmitAdvsimdScalarCopy(InstructionEmitContext context)
        {
            context.ThrowUnknown();
        }

        public static void EmitAdvsimdScalarXIndexedElement(InstructionEmitContext context)
        {
            context.ThrowUnknown();
        }

        public static void EmitAdvsimdScalarShiftByImmediate(InstructionEmitContext context)
        {
            context.ThrowUnknown();
        }

        public static void EmitCryptoThreeRegSHA(InstructionEmitContext context)
        {
            context.ThrowUnknown();
        }

        public static void EmitCryptoTwoRegSHA(InstructionEmitContext context)
        {
            context.ThrowUnknown();
        }

        public static void EmitCryptoAES(InstructionEmitContext context)
        {
            context.ThrowUnknown();
        }

        public static void EmitAdvsimdThreeSame(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.ORR: EmitVector.Vector_3_Same(context,ILInstruction.Or,false); break;
                case InstructionMnemonic.ORN: EmitVector.Vector_3_Same(context, ILInstruction.Or, true); break;

                case InstructionMnemonic.AND: EmitVector.Vector_3_Same(context, ILInstruction.And, false); break;
                case InstructionMnemonic.BIC: EmitVector.Vector_3_Same(context, ILInstruction.And, true); break;

                case InstructionMnemonic.EOR: EmitVector.Vector_3_Same(context, ILInstruction.Xor, false); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitAdvsimdThreeDifferent(InstructionEmitContext context)
        {
            context.ThrowUnknown();
        }

        public static void EmitAdvsimdTwoRegMisc(InstructionEmitContext context)
        {
            OpCodeAdvsimdTwoRegMisc opCode = (OpCodeAdvsimdTwoRegMisc)context.CurrentOpCode;

            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.CNT: EmitVector.Cnt(context); break;
                default: context.ThrowUnknown(); break;
            }

            if (opCode.q == 0)
            {
                EmitVector.InsertIntToVector(context,opCode.rd,context.Const(0),3,1);
            }
        }

        public static void EmitAdvsimdAcrossLanes(InstructionEmitContext context)
        {
            OpCodeAdvsimdAcrossLanes opCode = (OpCodeAdvsimdAcrossLanes)context.CurrentOpCode;

            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.UADDLV: EmitVector.Uaddlv(context); break;
                default: context.ThrowUnknown(); break;
            }    
        }

        public static void EmitAdvsimdCopy(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.DUP: EmitVector.Dup(context); break;
                case InstructionMnemonic.INS: EmitVector.Ins(context); break;
                default: context.ThrowUnknown();break;
            }
        }

        public static void EmitAdvsimdVectorXIndexedElement(InstructionEmitContext context)
        {
            context.ThrowUnknown();
        }

        public static void EmitAdvsimdModifiedImmediate(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Name)
            {
                case InstructionMnemonic.MOVI: EmitVector.Movi(context); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void EmitAdvsimdShiftByImmediate(InstructionEmitContext context)
        {
            context.ThrowUnknown();
        }

        public static void EmitAdvsimdTBLTBX(InstructionEmitContext context)
        {
            context.ThrowUnknown();
        }

        public static void EmitAdvsimdZIPUZPTRN(InstructionEmitContext context)
        {
            context.ThrowUnknown();
        }

        public static void EmitAdvsimdEXT(InstructionEmitContext context)
        {
            context.ThrowUnknown();
        }

        public static void EmitAdvsimdLoadStoreMultipleStructures(InstructionEmitContext context)
        {
            context.ThrowUnknown();
        }

        public static void EmitAdvsimdLoadStoreMultipleStructuresPostIndexed(InstructionEmitContext context)
        {
            context.ThrowUnknown();
        }

        public static void EmitAdvsimdLoadStoreSingleStructure(InstructionEmitContext context)
        {
            context.ThrowUnknown();
        }

        public static void EmitAdvsimdLoadStoreSingleStructurePostIndexed(InstructionEmitContext context)
        {
            context.ThrowUnknown();
        }
    }
}