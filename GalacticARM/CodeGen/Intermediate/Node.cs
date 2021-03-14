using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Intermediate
{
    public class Node
    {
        public Node         BranchingNodes    { get; set; }

        public List<Operation> Instructions { get; set; }

        public Node()
        {
            Instructions = new List<Operation>();
        }
    }
}
