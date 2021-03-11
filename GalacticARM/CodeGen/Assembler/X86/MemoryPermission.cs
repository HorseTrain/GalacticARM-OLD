using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Assembler.X86
{
    public enum MemoryPermission
    {
        None =              0,
        Read =              1 << 0,
        Write =             1 << 1,
        Execute =           1 << 2,
        ReadAndWrite =      Read | Write,
        ReadAndExecute =    Read | Execute,
        ReadWriteExecute =  Read | Write | Execute
    }
}
