using GalacticARM.CodeGen;
using GalacticARM.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.Memory
{
    public static unsafe class ExclusiveMonitors
    {
        public const ulong ErgMask = (4 << 4) - 1;

        static ExclusiveMonitors()
        {
            Addresses = new HashSet<ulong>();
            Monitors = new Dictionary<ulong, Monitor>();
        }

        public struct Monitor
        {
            public ulong Address;
            public bool ExState;

            public Monitor(ulong Address, bool ExState)
            {
                this.Address = Address;
                this.ExState = ExState;
            }

            public bool HasExclusiveAccess(ulong Address)
            {
                return this.Address == Address && ExState;
            }

            public void Reset()
            {
                ExState = false;
            }
        }

        public static HashSet<ulong> Addresses { get; set; }

        public static Dictionary<ulong, Monitor> Monitors { get; set; }

        public static void SetExclusive(ulong Context, ulong Address)
        {
            ContextBlock* context = (ContextBlock*)Context;

            Address &= ~ErgMask;

            lock (Monitors)
            {
                if (Monitors.TryGetValue(context->GetMisc(MiscRegister.ID), out Monitor monitor))
                {
                    Addresses.Remove(monitor.Address);
                }

                bool ExState = Addresses.Add(Address);

                Monitor Monitor = new Monitor(Address, ExState);

                if (!Monitors.TryAdd(context->GetMisc(MiscRegister.ID), Monitor))
                {
                    Monitors[context->GetMisc(MiscRegister.ID)] = Monitor;
                }
            }
        }

        public static void TextExclusive(ulong Context, ulong Result, ulong Address)
        {
            ContextBlock* context = (ContextBlock*)Context;

            /*
            Address &= ~ErgMask;
            lock (Monitors)
            {
                if (!Monitors.TryGetValue(context->GetMisc(MiscRegister.ID), out Monitor monitor))
                {
                    context->RegisterBuffer[Result] = 0;
                }

                context->RegisterBuffer[Result] = monitor.HasExclusiveAccess(Address) ? 1 : 0;
            }
            */

            context->RegisterBuffer[Result] = TextExclusive(context->GetMisc(MiscRegister.ID),Address) ? 1 : 0;
        }

        public static bool TextExclusive(ulong ID, ulong Address)
        {
            Address &= ~ErgMask;
            lock (Monitors)
            {
                if (!Monitors.TryGetValue(ID, out Monitor monitor))
                {
                    return false;
                }

                return monitor.HasExclusiveAccess(Address);
            }
        }

        public static void Clrex(ulong Context)
        {
            ContextBlock* context = (ContextBlock*)Context;

            lock (Monitors)
            {
                if (Monitors.TryGetValue(context->GetMisc(MiscRegister.ID), out Monitor monitor))
                {
                    monitor.Reset();
                    Addresses.Remove(monitor.Address);
                }
            }
        }
    }
}
