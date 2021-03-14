using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Intermediate
{
    public class Operation 
    {
        public ulong Address        { get; set; }
        public ILInstruction Name   { get; set; }
        public OperationSize Size   { get; set; } = OperationSize.Int64;

        public Operand[] Arguments  { get; set; }
        public ulong Imm            { get; set; }

        public Operation(ILInstruction instruction, params Operand[] Arguments)
        {
            Name = instruction;

            this.Arguments = Arguments;
        }

        public override string ToString()
        {
            StringBuilder final = new StringBuilder();

            string Args = "";

            for (int i = 0; i < Arguments.Length; i++)
            {
                Args += $"{Arguments[i]}";

                if (i != Arguments.Length - 1)
                {
                    Args += ", ";
                }
            }

            final.Append($"{Name} {Args}");

            return final.ToString();
        }

        public int GetReg(int i) => Arguments[i].Reg;

        public ulong GetImm(int i) => Arguments[i].Imm;

        public OperandType GetType(int i) => Arguments[i].Type;
    }
}
