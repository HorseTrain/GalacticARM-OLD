using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.Memory
{
    public unsafe class MemoryBlock : IDisposable
    {
        public void* Buffer { get; set; }
        public ulong Size   { get; set; }

        GCHandle pin = new GCHandle();

        public MemoryBlock(ulong Size)
        {
            byte[] buff = new byte[Size];

            pin = GCHandle.Alloc(buff,GCHandleType.Pinned);

            fixed (byte* dat = buff)
            {
                Buffer = dat;
            }

            this.Size = Size;
        }

        public void Dispose()
        {
            pin.Free();
        }
    }
}
