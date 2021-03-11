using GalacticARM.CodeGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.Decoding
{
    public delegate AOpCode OpCodeCreate(int RawOpCode, ulong Address, InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit);
}
