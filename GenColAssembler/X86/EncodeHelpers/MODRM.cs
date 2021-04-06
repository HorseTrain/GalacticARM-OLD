using System;
using System.Collections.Generic;
using System.Text;

namespace GenColAssembler.X86.EncodeHelpers
{
    public class MODRM : BitEncoder
    {
        public int Reg0 { get => GetInt(0, 3); set => SetInt(0, 3, value); }
        public int Reg1 { get => GetInt(3, 3); set => SetInt(3, 3, value); }
        public int Reg2 { get => GetInt(6, 2); set => SetInt(6, 2, value); }

        public MODRM(int r0, int r1)
        {
            Reg2 = r0;
            Reg1 = r1;
        }

        MODRM(byte b)
        {
            raw = b;
        }

        public MODRM Copy() => new MODRM(raw);
    }
}
