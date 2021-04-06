﻿using GalacticARM.CodeGen.X86;
using GalacticARM.Runtime;
using GalacticARM.Runtime.Fallbacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Translation
{
    public delegate void _Void__();
    public delegate void _Void___Ulong(ulong Arg0);
    public delegate void _Void___Ulong_Ulong(ulong Arg0, ulong Arg1);
    public delegate void _Void___Ulong_Ulong_Ulong(ulong Arg0, ulong Arg1, ulong Arg2);
    public delegate void _Void___Ulong_Ulong_Ulong_Ulong(ulong Arg0, ulong Arg1, ulong Arg2, ulong Arg3);
    public delegate void _Void___Ulong_Ulong_Ulong_Ulong_Ulong(ulong Arg0, ulong Arg1, ulong Arg2, ulong Arg3, ulong Arg4); 
    public delegate void _Void___Ulong_Ulong_Ulong_Ulong_Ulong_Ulong(ulong Arg0, ulong Arg1, ulong Arg2, ulong Arg3, ulong Arg4, ulong Arg5);
    public delegate void _Void___Ulong_Ulong_Ulong_Ulong_Ulong_Ulong_Ulong(ulong Arg0, ulong Arg1, ulong Arg2, ulong Arg3, ulong Arg4, ulong Arg5, ulong Arg6);

    public delegate ulong _Ulong__();
    public delegate ulong _Ulong___Ulong(ulong Arg0);
    public delegate ulong _Ulong___Ulong_Ulong(ulong Arg0, ulong Arg1);
    public delegate ulong _Ulong___Ulong_Ulong_Ulong(ulong Arg0, ulong Arg1, ulong Arg2);
    public delegate ulong _Ulong___Ulong_Ulong_Ulong_Ulong(ulong Arg0, ulong Arg1, ulong Arg2, ulong Arg3);
    public delegate ulong _Ulong___Ulong_Ulong_Ulong_Ulong_Ulong(ulong Arg0, ulong Arg1, ulong Arg2, ulong Arg3, ulong Arg4);
    public delegate ulong _Ulong___Ulong_Ulong_Ulong_Ulong_Ulong_Ulong(ulong Arg0, ulong Arg1, ulong Arg2, ulong Arg3, ulong Arg4, ulong Arg5);
    public delegate ulong _Ulong___Ulong_Ulong_Ulong_Ulong_Ulong_Ulong_Ulong(ulong Arg0, ulong Arg1, ulong Arg2, ulong Arg3, ulong Arg4, ulong Arg5, ulong Arg6);

    public static class DelegateCache
    {
        static Dictionary<string, Delegate> Methoddic = new Dictionary<string, Delegate>();

        static Delegate[] Methods = new Delegate[]
        {
            new _Void___Ulong_Ulong(CpuThread.CallSVC),
            new _Void___Ulong_Ulong(CpuThread.DebugStep),
            new _Ulong___Ulong_Ulong(Fallbackbits.CountLeadingZeros),
            new _Ulong___Ulong_Ulong_Ulong(Fallbackbits.MulH),

            new _Ulong___Ulong_Ulong(FallbackMemory.TestExclusive_fb),
            new _Void___Ulong_Ulong(FallbackMemory.SetExclusive_fb),
            new _Void___Ulong(FallbackMemory.Clrex_fb),
            new _Void___Ulong(FallbackFloat.FB_Fcvtz_Scalar_Fixed),
            new _Void___Ulong(FallbackFloat.ConvertPerc),
            new _Void___Ulong_Ulong_Ulong_Ulong(FallbackFloat.Fsqrt),
            new _Void___Ulong(FallbackFloat.Fcmp),
            new _Void___Ulong_Ulong_Ulong(Fallbackbits.Cnt),
            new _Ulong___Ulong(FallbackFloat.FCompare)
        };

        static DelegateCache()
        {
            foreach (Delegate d in Methods)
            {
                Methoddic.Add(d.Method.Name,d);
            }
        }

        public static ulong GetFunctionPointer(string Name)
        {
            if (Methoddic.ContainsKey(Name))
            {
                Delegate d = Methoddic[Name];

                return (ulong)Marshal.GetFunctionPointerForDelegate(d);
            }

            throw new Exception();
        }
    }
}
