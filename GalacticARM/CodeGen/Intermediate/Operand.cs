using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Intermediate
{
    public class Operand
    {
        public ulong Imm            { get; private set; }
        public int Reg              { get; private set; }
        public Label label          { get; private set; }

        public OperandType Type     { get; private set; }

        public Operand()
        {

        }

        public static Operand Register(int Reg)
        {
            Operand Out = new Operand();

            Out.Type = OperandType.Register;
            Out.Reg = Reg;

            return Out;
        }

        public static Operand Const(ulong Imm)
        {
            Operand Out = new Operand();

            Out.Type = OperandType.Constant;
            Out.Imm = Imm;

            return Out;
        }

        public static Operand Label(Label label)
        {
            Operand Out = new Operand();

            Out.Type = OperandType.Label;
            Out.label = label;

            return Out;
        }

        public static Operand Create(OperandType Type, ulong Dat)
        {
            switch (Type)
            {
                case OperandType.Constant: return Const(Dat);
                case OperandType.Label: return Label(new Label() { Address = Dat});
                case OperandType.Register: return Register((int)Dat);
                default: throw new Exception();
            }
        }

        public ulong GetDat()
        {
            switch (Type)
            {
                case OperandType.Constant: return Imm;
                case OperandType.Register: return (ulong)Reg;
                case OperandType.Label: return label.Address;
                default: throw new Exception();
            }
        }

        public override string ToString()
        {
            switch (Type)
            {
                case OperandType.Constant: return $"#{Imm}";
                case OperandType.Register: return $"r{Reg}";
                case OperandType.Label: return $"pc: {label.Address}";
                default: throw new NotImplementedException();
            }
        }
    }
}
