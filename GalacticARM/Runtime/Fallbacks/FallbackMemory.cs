using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.Runtime.Fallbacks
{
    public static unsafe class FallbackMemory
    {
        public static ulong GetPhysicalAddress(ulong VirtualAddress) => (ulong)VirtualMemoryManager.ReqeustPhysicalAddress(VirtualAddress);

        public static void SetExclusive_fb(ulong context, ulong address) => ExclusiveMonitors.SetExclusive((ExecutionContext*)context, address);

        public static ulong TestExclusive_fb(ulong context, ulong address) => ExclusiveMonitors.TestExclusive((ExecutionContext*)context, address);

        public static void Clrex_fb(ulong context) => ExclusiveMonitors.Clrex((ExecutionContext*)context);
    }
}
