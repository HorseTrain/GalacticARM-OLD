using GalacticARM.CodeGen;
using GalacticARM.CodeGen.AEmit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.Context
{
    public unsafe struct ContextBlock
    {
        public fixed ulong RegisterBuffer[InstructionEmitContext.GuestRegisterCount + InstructionEmitContext.GuestSimdRegisterCount + InstructionEmitContext.GuestMiscCount];
        public fixed ulong OperationRegisters[10000];

        public ulong GetMisc(MiscRegister reg) => RegisterBuffer[InstructionEmitContext.MiscStart + (ulong)reg];
        public void SetMisc(MiscRegister reg, ulong value) => RegisterBuffer[InstructionEmitContext.MiscStart + (ulong)reg] = value;

        public void SetFlagsIMM(ulong imm)
        {
            SetMisc(MiscRegister.N, (imm >> 3) & 1);
            SetMisc(MiscRegister.Z, (imm >> 2) & 1);
            SetMisc(MiscRegister.C, (imm >> 1) & 1);
            SetMisc(MiscRegister.V, (imm >> 0) & 1);
        }
    }
}
