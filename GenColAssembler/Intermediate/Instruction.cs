using System;
using System.Collections.Generic;
using System.Text;

namespace GenColAssembler.Intermediate
{
    public class Instruction
    {
        public int instruction      { get; set; }
        public Operand[] Operands   { get; set; }
        public object OtherData     { get; set; }

        public Instruction(int instruction, params Operand[] operands)
        {
            this.instruction = instruction;
            this.Operands = operands;
        }
    }
}
