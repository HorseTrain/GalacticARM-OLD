using GenColAssembler.X86.EncodeHelpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenColAssembler.X86
{
    public enum InstructionFlag
    {
        None =              0,
        IsALU =             1 << 0,
        IsShift =           1 << 2,
        IsCond =            1 << 3,
        IsControlFlow =     1 << 4,
        PushPop =           1 << 5,
        Move =              1 << 6
    }

    public struct InstructionTable
    {
        public static InstructionTable[] Tables;

        public int R_I8;
        public int R_I16;
        public int R_I32;
        public int R_I64;
        public int R_RM;
        public InstructionFlag Flags;
        public int Modrm;

        public bool Valid;

        static void Add(X86Instruction instruction,int r_ri8, int r_ri16, int r_ri32, int r_ri64, int r_rm, int modrm, InstructionFlag Flags)
        {
            InstructionTable table = new InstructionTable()
            {
                R_I8 = r_ri8,
                R_I16 = r_ri16,
                R_I32 = r_ri32,
                R_I64 = r_ri64,
                R_RM = r_rm,
                Modrm = modrm,
                Flags = Flags,
                Valid = true
            };

            Tables[(int)instruction] = table;

        }

        static InstructionTable()
        {
            Tables = new InstructionTable[(int)X86Instruction.Count];

            int NULL = -1;
             
            //                          i8    i16   i32   i64   r
            Add(X86Instruction.Add,     0x83, NULL, 0x81, NULL, 0x03, 0b11_000_000, InstructionFlag.IsALU);
            Add(X86Instruction.Sub,     0x83, NULL, 0x81, NULL, 0x2b, 0b11_101_000, InstructionFlag.IsALU);

            Add(X86Instruction.And,     0x83, NULL, 0x81, NULL, 0x23, 0b11_100_000, InstructionFlag.IsALU);
            Add(X86Instruction.Or,      0x83, NULL, 0x81, NULL, 0x0b, 0b11_001_000, InstructionFlag.IsALU);
            Add(X86Instruction.Xor,     0x83, NULL, 0x81, NULL, 0x33, 0b11_110_000, InstructionFlag.IsALU);
            Add(X86Instruction.Cmp,     0x83, NULL, 0x81, NULL, 0x3b, 0b11_111_000, InstructionFlag.IsALU);
            Add(X86Instruction.Not,     NULL, NULL, NULL, NULL, 0xf7, 0b11_010_000, InstructionFlag.IsALU);

            Add(X86Instruction.Shl,     0xc1, NULL, NULL, NULL, 0xd3, 0b11_100_000, InstructionFlag.IsALU | InstructionFlag.IsShift);
            Add(X86Instruction.Shr,     0xc1, NULL, NULL, NULL, 0xd3, 0b11_101_000, InstructionFlag.IsALU | InstructionFlag.IsShift);
            Add(X86Instruction.Sar,     0xc1, NULL, NULL, NULL, 0xd3, 0b11_111_000, InstructionFlag.IsALU | InstructionFlag.IsShift);

            Add(X86Instruction.Setcc,   NULL, NULL, NULL, NULL, 0x0f, 0b11_000_000, InstructionFlag.IsCond);

            Add(X86Instruction.Call,    NULL, NULL, NULL, NULL, 0xff, 0b11_010_000, InstructionFlag.IsControlFlow); //CALL FAR LOL

            Add(X86Instruction.Push,    NULL, NULL, NULL, NULL, 0x50, 0b00_000_000, InstructionFlag.PushPop);
            Add(X86Instruction.Pop,     NULL, NULL, NULL, NULL, 0x58, 0b00_000_000, InstructionFlag.PushPop);

            Add(X86Instruction.Mov,     NULL, NULL, 0xb8, 0xb8, NULL, 0b11_000_000, InstructionFlag.None);
        }
    }
}
