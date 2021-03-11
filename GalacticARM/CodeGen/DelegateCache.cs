using GalacticARM.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen
{
    public static unsafe class DelegateCache
    {
        static Dictionary<Delegate, ulong> Functions;

        static DelegateCache()
        {
            Functions = new Dictionary<Delegate, ulong>();
        }

        public static void* GetOrAdd(Delegate del)
        {
            if (Functions.ContainsKey(del))
            {
                return (void*)Functions[del];
            }

            Functions.Add(del,(ulong)Marshal.GetFunctionPointerForDelegate(del));

            return GetOrAdd(del);
        }
    }

    public unsafe delegate void _Void();
    public unsafe delegate void _Void_U(ulong arg0);
    public unsafe delegate void _Void_U_U(ulong arg0, ulong arg1);
    public unsafe delegate void _Void_U_U_U(ulong arg0, ulong arg1, ulong arg2);
    public unsafe delegate void _Void_U_U_U_U(ulong arg0, ulong arg1, ulong arg2, ulong Size);
}
