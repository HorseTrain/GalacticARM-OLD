using GalacticARM.CodeGen.Intermediate;
using GalacticARM.Decoding.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.AEmit
{
    public static class EmitSys
    {
        public static void Svc(InstructionEmitContext context)
        {
            OpCodeExceptionGeneration opCode = (OpCodeExceptionGeneration)context.CurrentOpCode;

            context.CallFunctionFromPointer(context.GetMisc(MiscRegister.SvcHook),context.Const(opCode.imm16));
        }

        public static int GetPackedId(OpCodeSystem op)
        {
            int id;

            id = op.Op2 << 0;
            id |= op.CRm << 3;
            id |= op.CRn << 7;
            id |= op.Op1 << 11;
            id |= op.Op0 << 14;

            return id;
        }

        public static void Mrs(InstructionEmitContext context)
        {
            OpCodeSystem opCode = (OpCodeSystem)context.CurrentOpCode;

            Operand t;

            switch (GetPackedId(opCode))
            {
                case 0b11_011_1101_0000_010: t = context.GetMisc(MiscRegister.tpidr); break;
                case 0b11_011_1101_0000_011: t = context.GetMisc(MiscRegister.tpidrro_el0); break;
                case 0b11_011_0000_0000_111: t = context.GetMisc(MiscRegister.dczid_el0); break;
                case 0b11_011_0100_0100_000: t = context.GetMisc(MiscRegister.fpcr); break;
                case 0b11_011_0100_0100_001: t = context.GetMisc(MiscRegister.fpsr); break;
                case 0b11_011_0000_0000_001: t = context.Const(0x8444c004); break;
                case 0b11_011_1110_0000_001: t = context.Const(0); break; //todo
                default: throw new NotImplementedException();
            }

            context.SetRegister(opCode.Rt,t);
        }

        public static void Msr(InstructionEmitContext context)
        {
            OpCodeSystem opCode = (OpCodeSystem)context.CurrentOpCode;

            Operand t = context.GetRegister(opCode.Rt);

            switch (GetPackedId(opCode))
            {
                case 0b11_011_0100_0100_000: context.SetMisc(MiscRegister.fpcr,t); break;
                case 0b11_011_0100_0100_001: context.SetMisc(MiscRegister.fpsr,t); break;
                case 0b11_011_1101_0000_010: context.SetMisc(MiscRegister.tpidr,t); break;
                default: throw new NotImplementedException();
            }
        }
    }
}
