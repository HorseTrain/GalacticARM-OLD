using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.Decoding
{
    public class BasicBlock
    {
        public List<AOpCode> OpCodes { get; set; }

        public BasicBlock()
        {
            OpCodes = new List<AOpCode>();
        }

        public void AddOpCode(AOpCode opCode)
        {
            OpCodes.Add(opCode);
        }
    }
}
