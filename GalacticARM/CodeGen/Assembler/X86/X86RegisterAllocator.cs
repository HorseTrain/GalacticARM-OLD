using Iced.Intel;
using System;
using static Iced.Intel.AssemblerRegisters;

namespace GalacticARM.CodeGen.Assembler.X86
{
    public struct Register
    {
        public bool Locked;

        public int HostIndex;
        public int GuestIndex;

        public bool Loaded => GuestIndex != -1;
    }

    public partial class X86Assembler
    {
        public Register[] Registers     { get; set; }
        public int HostCount;

        public void InitAllocater()
        {
            HostCount = B.Length;

            Registers = new Register[HostCount];

            for (int i = 0; i < Registers.Length; i++)
            {
                Registers[i].HostIndex = i;
                Registers[i].GuestIndex = -1;
            }
        }

        //Note, These are the only registers allowed to be used by ir.
        public static AssemblerRegister8[] B = new AssemblerRegister8[]
        {
            al,
            dl,
            sil,
            dil,

            r8b,
            r9b,
            r10b,
            r11b,
            r12b,
            r13b,
            r14b
        };

        public static AssemblerRegister32[] W = new AssemblerRegister32[]
        {
            eax,
            edx,
            esi,
            edi,

            r8d,
            r9d,
            r10d,
            r11d,
            r12d,
            r13d,
            r14d
        };

        public static AssemblerRegister64[] D = new AssemblerRegister64[] 
        { 
            rax,
            rdx,
            rsi,
            rdi,

            r8,
            r9,
            r10,
            r11,
            r12,
            r13,
            r14
        };

        public void LoadRegister(int Guest, int Host)
        {
            UnloadRegister(Host);

            c.mov(D[Host], __[r15 + (8 * Guest)]);

            Registers[Host].GuestIndex = Guest;
            Registers[Host].Locked = true;
        }

        public void UnloadRegister(int Host)
        {
            if (Registers[Host].Loaded)
            {
                c.mov(__[r15 + (8 * Registers[Host].GuestIndex)],D[Host]);

                Registers[Host].GuestIndex = -1;

                Registers[Host].Locked = false;
            }
        }

        public void UnloadAllRegisters()
        {
            for (int i = 0; i < HostCount; i++)
            {
                UnloadRegister(i);
            }
        }

        int spill = 0;

        public int AllocateOrGetRegister(int guest)
        {
            for (int i = 0; i < HostCount; i++)
            {
                if (Registers[i].GuestIndex == guest)
                {
                    Registers[i].Locked = true;

                    return i;
                }

                if (!Registers[i].Loaded)
                {
                    LoadRegister(guest,i);

                    return i;
                }
            }

            int full = 0;

            while (true)
            {
                if (full == HostCount)
                    throw new OutOfMemoryException();

                if (!Registers[spill].Locked)
                {
                    break;
                }

                spill++;
                full++;

                if (spill >= HostCount - 1)
                    spill = 0;
            }

            LoadRegister(guest,spill);

            return spill;
        }

        public dynamic GetRegRaw(int guest, bool Force = false, int Size = 0)
        {
            int reg = AllocateOrGetRegister(guest);

            if (!Force)
            {
                if (CurrentSize == RegisterSize._32)
                    return W[reg];

                return D[reg];
            }
            else
            {
                if (Size == 0)
                    return B[reg];
                else if (Size == 2)
                    return W[reg];
                else if (Size == 3)
                    return D[reg];
                else
                    throw new Exception();
            }
        }

        public dynamic RegFromMem(int guest)
        {
            return __[r15 + (CurrentOperation.Arguments[guest].Reg * 8)];
        }

        public void UnlockAllRegisters()
        {
            for (int i = 0; i < HostCount; i++)
            {
                Registers[i].Locked = false;
            }
        }
    }
}
