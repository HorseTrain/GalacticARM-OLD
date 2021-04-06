using GenColAssembler.Intermediate;
using GenColAssembler.X86.EncodeHelpers;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace GenColAssembler.X86
{
    public class Assembler
    {
        public List<Instruction> instructions { get; set; }

        public Assembler()
        {
            instructions = new List<Instruction>();
        }

        void EnsureBool(bool Test)
        {
            if (!Test)
            {
                throw new Exception();
            }
        }

        void WriteByte(byte source)
        {
            Out.Add(source);
        }

        void WriteByte(BitEncoder encoder) => WriteByte(encoder.raw);

        void WriteByte(int source) => WriteByte((byte)source);
        
        void WriteInt32(uint source)
        {
            WriteByte((byte)source);
            WriteByte((byte)(source >> 8));
            WriteByte((byte)(source >> 16));
            WriteByte((byte)(source >> 24));
        }

        void WriteInt64(ulong source)
        {
            WriteInt32((uint)source);
            WriteInt32((uint)(source >> 32));
        }

        void WriteEncoding(int source)
        {
            EnsureValidEncoding(source);

            for (int i = 3; i > -1; i--)
            {
                int test = source >> (i * 8);

                byte t = (byte)test;

                if (test != 0)
                {
                    WriteByte(t);
                }
            }
        }

        int GetReg(Operand Source)
        {
            EnsureIsRegister(Source);

            EnsureBool(Source.Raw >= 0 && Source.Raw <= 15);

            return (int)(Source.Raw & 0b111);
        }

        void EnsureIsRegister(Operand Source) => EnsureBool(Source.Type == OperandType.Register);
        void EnsureISconstant(Operand Source) => EnsureBool(Source.Type == OperandType.Constant);
        void EnsureIsCond(Operand Source) => EnsureBool(Source.Type == OperandType.Condition);
        void EnsureSameSize(Operand Source0, Operand Source1) => EnsureBool(Source0.Size == Source1.Size);
        void EnsureValidEncoding(int test) => EnsureBool(test != -1);
        void EnsureInstructionSlotOpen(InstructionTable table) => EnsureBool(table.Valid);
        void EnsureValidSize(Operand Test) => EnsureBool(Test.Size == OperandSize.Int64 || Test.Size == OperandSize.Int32);


        public void AddInstruction(X86Instruction instruction, params Operand[] Operands) => instructions.Add(new Instruction((int)instruction,Operands));

        public void WriteInstruction(Instruction instruction)
        {
            Operand[] operands = instruction.Operands;

            InstructionTable table = InstructionTable.Tables[instruction.instruction];

            EnsureInstructionSlotOpen(table);

            Operand des = operands[0];

            REX rex = new REX();

            rex.W = des.Size == OperandSize.Int64;

            if (operands.Length == 2)
            {
                Operand source = operands[1];

                if ((X86Instruction)instruction.instruction == X86Instruction.Mov)
                {
                    if (des.Type == OperandType.Register)
                    {
                        if (source.Type == OperandType.Constant)
                        {
                            OperandSize size = source.GetConstSize();

                            if ((int)size < 2)
                            {
                                size = OperandSize.Int32;
                            }

                            rex.W = size == OperandSize.Int64;

                            rex.B = des.Raw >= 8;

                            if (rex.Needed)
                            {
                                WriteByte(rex);
                            }

                            switch (size)
                            {
                                case OperandSize.Int32:

                                    WriteEncoding(table.R_I32 | GetReg(des));

                                    WriteInt32((uint)source.Raw);

                                    break;

                                case OperandSize.Int64:

                                    WriteEncoding(table.R_I64 | GetReg(des));

                                    WriteInt64(source.Raw);

                                    break;

                                default: throw new NotImplementedException();
                            }
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (table.Flags == InstructionFlag.IsCond)
                {
                    if ((X86Instruction)instruction.instruction == X86Instruction.Setcc)
                    {
                        //setcc des

                        EnsureIsCond(source);

                        rex.B = des.Raw >= 8;

                        if (rex.Needed)
                        {
                            WriteByte(rex);
                        }

                        WriteEncoding(table.R_RM);

                        WriteByte(0b10010000 | (int)source.Raw);

                        WriteByte(0b11000000 | GetReg(des));
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (table.Flags.HasFlag(InstructionFlag.IsShift) && table.Flags.HasFlag(InstructionFlag.IsALU) && operands[1].Type == OperandType.Register)
                {
                    //Shift des, cl

                    EnsureIsRegister(operands[0]);

                    rex.B = des.Raw > 8;

                    EnsureBool(source.Raw == 1);

                    if (rex.Needed)
                    {
                        WriteByte(rex);
                    }

                    WriteEncoding(table.R_RM);

                    int mod = table.Modrm;

                    mod |= GetReg(des);

                    WriteByte(mod);
                }    
                else if (table.Flags.HasFlag(InstructionFlag.IsALU))
                {
                    //instruction des, source

                    EnsureIsRegister(operands[0]);

                    int mod = table.Modrm;

                    if (operands[1].Type == OperandType.Constant)
                    {
                        rex.B = des.Raw >= 8;

                        mod |= GetReg(des);

                        if (rex.Needed)
                        {
                            WriteByte(rex);
                        }

                        OperandSize size = source.GetConstSize();

                        if (size == OperandSize.Int16)
                            size = OperandSize.Int32;

                        switch (size)
                        {
                            case OperandSize.Int8:

                                WriteEncoding(table.R_I8);

                                WriteByte(mod);

                                WriteByte((byte)source.Raw);

                                break;

                            case OperandSize.Int32:

                                WriteEncoding(table.R_I32);

                                WriteByte(mod);

                                WriteInt32((uint)source.Raw);

                                break;


                            default: throw new NotImplementedException();
                        }
                    }
                    else if (operands[1].Type == OperandType.Register)
                    {
                        EnsureIsRegister(operands[0]);

                        EnsureSameSize(des, source);
                        EnsureValidSize(des);

                        rex.B = source.Raw >= 8;
                        rex.R = des.Raw >= 8;

                        mod |= GetReg(des) << 3;
                        mod |= GetReg(source);

                        if (rex.Needed)
                        {
                            WriteByte(rex);
                        }

                        WriteEncoding(table.R_RM);

                        WriteByte(mod);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (operands.Length == 1)
            {
                if (table.Flags == InstructionFlag.PushPop)
                {
                    EnsureIsRegister(operands[0]);

                    EnsureBool(des.Size == OperandSize.Int64);

                    rex.W = false;
                    rex.B = des.Raw >= 8;
                    
                    if (rex.Needed)
                    {
                        WriteByte(rex);
                    }

                    WriteEncoding(table.R_RM | GetReg(des));
                }
                else
                {
                    EnsureIsRegister(operands[0]);

                    rex.B = des.Raw >= 8;

                    if (rex.Needed)
                    {
                        WriteByte(rex);
                    }

                    WriteEncoding(table.R_RM);

                    int mod = table.Modrm;

                    mod |= GetReg(des);

                    WriteByte(mod);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        List<byte> Out;

        public byte[] GetBuffer()
        {
            Out = new List<byte>();

            foreach (Instruction instruction in instructions)
            {
                WriteInstruction(instruction);
            }

            return Out.ToArray();
        }

        //Frontend
        public void Add(Operand Des, Operand Source) => AddInstruction(X86Instruction.Add, Des, Source);
        public void And(Operand Des, Operand Source) => AddInstruction(X86Instruction.And, Des, Source);
        public void Call(Operand Des) => AddInstruction(X86Instruction.Call, Des);
        public void Cmp(Operand Des, Operand Source) => AddInstruction(X86Instruction.Cmp, Des, Source);
        public void Jcc(Operand Des, X86Condition condition) => AddInstruction(X86Instruction.Jcc, Des, Operand.Condition((int)condition));
        public void Jmp(Operand Des, Operand Source) => AddInstruction(X86Instruction.Jmp, Des, Source);
        public void Mov(Operand Des, Operand Source) => AddInstruction(X86Instruction.Mov, Des, Source);
        public void Movsx(Operand Des, Operand Source) => AddInstruction(X86Instruction.Movsx, Des, Source);
        public void Movsxd(Operand Des, Operand Source) => AddInstruction(X86Instruction.Movsxd, Des, Source);
        public void Not(Operand Des) => AddInstruction(X86Instruction.Not, Des);
        public void Or(Operand Des, Operand Source) => AddInstruction(X86Instruction.Or, Des, Source);
        public void Sar(Operand Des, Operand Source) => AddInstruction(X86Instruction.Sar, Des, Source);
        public void Setcc(Operand Des,X86Condition condition) => AddInstruction(X86Instruction.Setcc, Des, Operand.Condition((int)condition));
        public void Shl(Operand Des, Operand Source) => AddInstruction(X86Instruction.Shl, Des, Source);
        public void Shr(Operand Des, Operand Source) => AddInstruction(X86Instruction.Shr, Des, Source);
        public void Sub(Operand Des, Operand Source) => AddInstruction(X86Instruction.Sub, Des, Source);
        public void Xor(Operand Des, Operand Source) => AddInstruction(X86Instruction.Xor, Des, Source);
        public void Push(Operand Des) => AddInstruction(X86Instruction.Push,Des);
        public void Pop(Operand Des) => AddInstruction(X86Instruction.Pop,Des);
    }
}
