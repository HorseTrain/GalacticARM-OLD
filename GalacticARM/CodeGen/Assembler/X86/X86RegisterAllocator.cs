using System;
using static Iced.Intel.AssemblerRegisters;
using Iced.Intel;
using GalacticARM.CodeGen.Intermediate;

namespace GalacticARM.CodeGen.Assembler.X86
{
    public struct RegisterPointer
    {
        public int HostRegister;
        public int GuestRegister;

        public bool Locked;
        public bool Loaded => GuestRegister != -1;

        public void Unload()
        {
            GuestRegister = -1;
            Locked = false;
        }

        public void Load(int guest)
        {
            GuestRegister = guest;
            Locked = true;
        }
    }

    public enum RequestType
    {
        Read,
        Write,
        N
    }

    public partial class X86Compiler
    {
        public static AssemblerRegister8[] B = new AssemblerRegister8[]
        {
            al,
            cl,
            dl,
            bl,
            spl,
            bpl,
            sil,
            dil,
            r8b,
            r9b,
            r10b,
            r11b,
            r12b,
            r13b,
            r14b,
            r15b
        };

        public static AssemblerRegister16[] H = new AssemblerRegister16[]
        {
            ax,
            cx,
            dx,
            bx,
            sp,
            bp,
            si,
            di,
            r8w,
            r9w,
            r10w,
            r11w,
            r12w,
            r13w,
            r14w,
            r15w
        };

        public static AssemblerRegister32[] W = new AssemblerRegister32[]
        {
            eax,
            ecx,
            edx,
            ebx,
            esp,
            ebp,
            esi,
            edi,
            r8d,
            r9d,
            r10d,
            r11d,
            r12d,
            r13d,
            r14d,
            r15d
        };

        public static AssemblerRegister64[] D = new AssemblerRegister64[]
        {
            rax,
            rcx,
            rdx,
            rbx,
            rsp,
            rbp,
            rsi,
            rdi,
            r8,
            r9,
            r10,
            r11,
            r12,
            r13,
            r14,
            r15
        };

        public static dynamic GetRegRaw(int index, int size)
        {
            switch (size)
            {
                case 0: return B[index];
                case 1: return H[index];
                case 2: return W[index];
                case 3: return D[index];
                default: throw new NotImplementedException();
            }
        }

        public static int[] OpenRegisters = new int[]
        {
            0,
            2,
            6,
            7,
            8,
            9,
            10,
            11,
            12,
            13,
            14
        };

        public static dynamic GetReg(int index, int size = 3) => GetRegRaw(OpenRegisters[index], size);

        public static int OpenCount => OpenRegisters.Length;

        public RegisterPointer[] RegisterPointers;

        public void InitAllocator()
        {
            RegisterPointers = new RegisterPointer[OpenCount];

            for (int i = 0; i < RegisterPointers.Length; i++)
            {
                RegisterPointers[i].HostRegister = i;
                RegisterPointers[i].GuestRegister = -1;

                //c.mov(GetReg(i),0);
            }
        }

        public dynamic GetRegPointer(int index) => __[r15 + (index * 8)];

        public void LoadRegister(int guest, int host, RequestType Type)
        {
            UnloadRegister(host);

            if (Type == RequestType.Read || Type == RequestType.N)
            {
                c.mov(GetReg(host), GetRegPointer(guest));
            }

            RegisterPointers[host].Load(guest);
        }

        public void UnloadRegister(int host)
        {
            if (RegisterPointers[host].Loaded)
            {
                int guest = RegisterPointers[host].GuestRegister;

                c.mov(GetRegPointer(guest),GetReg(host));

                RegisterPointers[host].Unload();
            }
        }

        public int GetOrAllocate(int guest, RequestType Type)
        {
            for (int i = 0; i < RegisterPointers.Length; i++)
            {
                if (!RegisterPointers[i].Loaded)
                {
                    LoadRegister(guest,i,Type);

                    return i;
                }
                
                if (RegisterPointers[i].GuestRegister == guest)
                {
                    RegisterPointers[i].Locked = true;

                    return i;
                }
            }

            for (int i = 0; i < OpenCount; i++)
            {
                if (!RegisterPointers[i].Locked)
                {
                    LoadRegister(guest,i,Type);

                    return i;
                }
            }

            throw new Exception();
        }

        public void UnloadAllRegisters()
        {
            for (int i = 0; i < RegisterPointers.Length; i++)
            {
                UnloadRegister(i);
            }
        }

        public void UnlockAllRegisters()
        {
            for (int i = 0; i < RegisterPointers.Length; i++)
            {
                RegisterPointers[i].Locked = false;
            }
        }

        public dynamic GetOperand(int argument, int Size = -1, RequestType Type = RequestType.N)
        {
            Operand Guest = CurrentOperation.Arguments[argument];

            if (Guest.Type == OperandType.Register)
            {
                int host = GetOrAllocate(Guest.Reg, Type);

                if (Size != -1)
                {
                    return GetReg(host,Size);
                }

                if (CurrentOperation.Size == OperationSize.Int32)
                    return GetReg(host,2);

                return GetReg(host, 3);
            }
            else if (Guest.Type == OperandType.Constant)
            {
                return LoadImm(Guest, Guest.Imm);
            }
            else if (Guest.Type == OperandType.Label)
            {
                return Labels[Guest.label.Address];
            }
            else
            {
                throw new Exception();
            }
        }

        public dynamic LoadImm(Operand source,ulong imm)
        {
            if ((uint)source.Imm == source.Imm)
                return (uint)source.Imm;

            c.mov(rcx, imm);

            if (CurrentOperation.Size == OperationSize.Int32)
                return ecx;

            return rcx;
        }

        public dynamic GetRawPointerOrImm(int argument)
        {
            Operand Guest = CurrentOperation.Arguments[argument];

            if (Guest.Type == OperandType.Register)
            {
                return GetRegPointer(Guest.Reg);
            }
            if (Guest.Type == OperandType.Constant)
            {
                return LoadImm(Guest,Guest.Imm);
            }
            else
            {
                throw new Exception();
            }
        }

        public RegisterPointer[] GetRegisterPointerCopy()
        {
            RegisterPointer[] Out = new RegisterPointer[RegisterPointers.Length];

            for (int i = 0; i < Out.Length; i++)
            {
                Out[i] = RegisterPointers[i];
            }

            //Buffer.BlockCopy(Out,0, RegisterPointers, 0,Out.Length);

            return Out;
        }

        public void SetRegisterPointers(RegisterPointer[] Source)
        {
            for (int i = 0; i < Source.Length; i++)
            {
                RegisterPointers[i] = Source[i];
            }
        }
    }
}
