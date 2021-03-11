using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Intermediate
{
    public class OperationBlock
    {
        public int LocalStart                       { get; private set; }
        public int LocalPlace                       { get; private set; }

        public void SetLocalOffset(int Offset)
        {
            LocalStart = Offset;

            ClearLocals();
        }

        public List<Operation> Operations           { get; set; }
        public OperationSize Size                   { get; set; } = OperationSize.Int64;
        public Dictionary<Label, ulong> Lables      { get; set; }

        public OperationBlock()
        {
            Operations = new List<Operation>();
            Lables = new Dictionary<Label, ulong>();
        }
        
        public void ClearLocals()
        {
            LocalPlace = LocalStart;
        }

        public Operand Register(int reg) => Operand.Register(reg);

        public Operand Local()
        {
            LocalPlace++;

            return Operand.Register(LocalPlace - 1);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            int i = 0;

            foreach (Operation o in Operations)
            {
                builder.AppendLine( $"{i:d3} {o}");

                i++;
            }

            return builder.ToString();
        }

        public void Assert(bool Cond)
        {
            if (!Cond)
            {
                throw new Exception();
            }
        }

        public void AssertIsRegister(Operand test) => Assert(test.Type == OperandType.Register);

        public Operand Const(ulong Imm)
        {
            Operand loc = Local();

            Add(new Operation(ILInstruction.LoadImmediate,loc,Operand.Const(Imm)));

            return loc;//Operand.Const(Imm);
        }

        public void Add(Operation o)
        {
            o.Size = Size;
            o.Address = (ulong)Operations.Count;

            Operations.Add(o);
        }

        public Operand AddOperation(ILInstruction instruction, Operand n, Operand m)
        {
            Operand loc = Local();

            AssertIsRegister(n);

            Add(new Operation(ILInstruction.Copy, loc, n));
            Add(new Operation(instruction, loc, m));

            return loc;
        }

        public Operand AddOperation(ILInstruction instruction, Operand n)
        {
            Add(new Operation(instruction,n));

            return n;
        }

        public void WriteRegister(int des, Operand value)
        {
            Add(new Operation(ILInstruction.Copy,Register(des),value));
        }

        public Label CreateLable()
        {
            Label Out = new Label();

            Lables.Add(Out, ulong.MaxValue);

            return Out;
        }

        public void MarkLabel(ref Label label)
        {
            label.Address = (ulong)Operations.Count;
        }

        public void Jump(Label label)
        {
            Add(new Operation(ILInstruction.Jump,Operand.Label(label)));
        }

        public void JumpIf(Label label,Operand arg)
        {
            Add(new Operation(ILInstruction.JumpIf,arg, Operand.Label(label)));
        }

        public bool Storeable()
        {
            foreach (Operation o in Operations)
            {
                if (o.Name == ILInstruction.Call)
                    return false;
            }

            return true;
        }

        public byte[] GetBuffer()
        {
            List<byte> Final = new List<byte>();

            foreach (Operation operation in Operations)
            {
                AddOperationToBuffer(ref Final,operation);
            }

            return Final.ToArray();
        }

        public static unsafe void AddOperationToBuffer(ref List<byte> buffer, Operation operation)
        {
            buffer.Add((byte)operation.Arguments.Length);

            buffer.Add((byte)operation.Name);

            buffer.Add((byte)operation.Size);

            for (int i = 0; i < operation.Arguments.Length; i++)
            {
                Operand arg = operation.Arguments[i];

                buffer.Add((byte)arg.Type);

                ulong dat = arg.GetDat();

                byte* buf = (byte*)&dat;

                for (int b = 0; b < 8; b++)
                {
                    buffer.Add(buf[b]);
                }
            }
        }

        public unsafe void LoadOperationBlock(byte[] Buffer)
        {
            int Location = 0;

            fixed (byte* loc = Buffer)
            {
                byte ReadByte()
                {
                    Location++;

                    return Buffer[Location - 1];
                }

                while (Location < Buffer.Length)
                {
                    byte Length = ReadByte();
                    ILInstruction instruction = (ILInstruction)ReadByte();
                    OperationSize size = (OperationSize)ReadByte();

                    List<Operand> O = new List<Operand>();

                    for (int i = 0; i < Length; i++)
                    {
                        OperandType type = (OperandType)ReadByte();

                        ulong Imm = *(ulong*)&loc[Location];

                        O.Add(Operand.Create(type,Imm));

                        Location += 8;
                    }

                    Operation Out = new Operation(instruction, O.ToArray());

                    Out.Size = size;

                    Operations.Add(Out);
                }
            }
        }
    }
}
