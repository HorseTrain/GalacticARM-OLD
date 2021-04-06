using System;
using System.Collections.Generic;
using System.Text;

namespace GenColAssembler.Intermediate
{
    public class Operand
    {
        public OperandType Type     { get; set; }
        public OperandSize Size     { get; set; }
        public ulong Raw            { get; set; }

        public Operand[] Children   { get; set; }

        public static Operand Reg8(int imm) => new Operand()    { Type = OperandType.Register, Size = OperandSize.Int8, Raw = (uint)imm };
        public static Operand Reg16(int imm) => new Operand()   { Type = OperandType.Register, Size = OperandSize.Int16, Raw = (uint)imm };
        public static Operand Reg32(int imm) => new Operand()   { Type = OperandType.Register, Size = OperandSize.Int32, Raw = (uint)imm };
        public static Operand Reg64(int imm) => new Operand()   { Type = OperandType.Register, Size = OperandSize.Int64, Raw = (uint)imm };
        public static Operand Const(ulong Imm) => new Operand()        { Type = OperandType.Constant, Raw = Imm};
        public static Operand Condition(int Cond) => new Operand()     { Type = OperandType.Condition, Raw = (ulong)Cond };

        public OperandSize GetConstSize()
        {
            if ((Raw >> 8) == 0)
                return OperandSize.Int8;

            if ((Raw >> 16) == 0)
                return OperandSize.Int16;

            if ((Raw >> 32) == 0)
                return OperandSize.Int32;

            return OperandSize.Int64;
        }
    }
}
