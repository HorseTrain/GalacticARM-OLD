using System;
using System.Collections.Generic;
using System.Text;

namespace GenColAssembler.X86.EncodeHelpers
{
    public class REX : BitEncoder
    {
        public REX()
        {
            raw = 0b01000000;
        }

        public bool W { get => GetBit(3); set => SetBit(3,value); }
        public bool R { get => GetBit(2); set => SetBit(2, value); }
        public bool X { get => GetBit(1); set => SetBit(1, value); }
        public bool B { get => GetBit(0); set => SetBit(0, value); }

        public bool Needed => (raw & 0b1111) != 0;
    }
}
