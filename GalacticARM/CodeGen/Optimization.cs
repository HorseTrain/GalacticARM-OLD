using GalacticARM.CodeGen.Intermediate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen
{
    public static class Optimization
    {
        public static bool UsePasses = false;

        public static OperationBlock Optimize(OperationBlock source)
        {
            OperationBlock Out = source;

            Console.WriteLine(source);

            return Out;
        }
    }
}
