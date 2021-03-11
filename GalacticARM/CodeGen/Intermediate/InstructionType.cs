using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Intermediate
{
    public enum InstructionType
    {
        NULL,
        Operation,
        Immediate,
        GetRegister
    }
}
