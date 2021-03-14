using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Intermediate
{
    public class ControlFlowGraph
    {
        public Node EntryNode   { get; set; }

        public ControlFlowGraph(OperationBlock block)
        {
            EntryNode = new Node();

            EntryNode = CreateNode(block,0);
        }

        public static Node CreateNode(OperationBlock block, int Entry)
        {
            Node Out = new Node();

            for (int i = Entry; i < block.Operations.Count; i++)
            {
                Operation CurrentOperation = block.Operations[i];

                Out.Instructions.Add(CurrentOperation);

                if (CurrentOperation.Name == ILInstruction.Jump)
                {
                    Out.BranchingNodes = CreateNode(block, (int)CurrentOperation.Arguments[0].label.Address);
                }
                else if (CurrentOperation.Name == ILInstruction.JumpIf)
                {
                    Out.BranchingNodes = CreateNode(block, (int)CurrentOperation.Arguments[1].label.Address);
                }
            }

            return Out;
        }
    }
}
