using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.Decoding
{
    public class AOpCode
    {
        public ulong Address                                        { get; set; }
        public int RawOpCode                                        { get; set; }
        public Emit emit                                            { get; set; }
        public Dictionary<string, InstructionInfo> InstructionData  { get; set; }
    }
}
