using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Intermediate
{
    public class InstructionNode
    {
        public InstructionNode         BranchingNodes    { get; set; }

        public List<Operation> Instructions { get; set; }

        public InstructionNode()
        {
            Instructions = new List<Operation>();
        }
    }
}
