using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen
{
    public enum MiscRegister
    {
        N, 
        Z, 
        C, 
        V,
        SvcHook,
        DebugHook,
        IsExecuting,
        CallArgument,
        tpidrro_el0,
        dczid_el0,
        tpidr,
        fpsr,
        fpcr,
        ID,
        PageTablePointer,
    }
}
