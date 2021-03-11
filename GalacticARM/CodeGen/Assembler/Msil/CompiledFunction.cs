using GalacticARM.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Assembler.Msil
{
    public unsafe delegate ulong CompiledFunction(ContextBlock* block);
}
