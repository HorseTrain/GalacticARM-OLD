using System;
using System.Collections.Generic;
using System.Text;

namespace GenColAssembler.X86.EncodeHelpers
{
    public class BitEncoder
    {
        public byte raw { get; protected set; }

        public void SetBit(int index, bool set) => SetInt(index,1,set ? 1 : 0);
        public bool GetBit(int index) => GetInt(index,1) == 1;

        public void SetInt(int index, int size, int value)
        {
            int mask = ~(CreateMask(size) << index);

            raw = (byte)(raw & mask);

            raw = (byte)(raw | (value << index));
        }

        public int GetInt(int index, int size) => (raw >> index) & CreateMask(size);

        static int CreateMask(int size) => (1 << (size + 1)) - 1;

        public static implicit operator byte(BitEncoder bit) => bit.raw;

        public override string ToString() => ((byte)this).ToString();
    }
}
