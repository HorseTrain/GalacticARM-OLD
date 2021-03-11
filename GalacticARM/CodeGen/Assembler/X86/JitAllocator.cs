using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Assembler.X86
{
    public unsafe class JitAllocator
    {
        public const uint JitSize = 800 * 1024U * 1024U;  //800 mb jit buffer

        [DllImport("kernel32.dll")]
        private static extern void* VirtualAlloc(void* addr, uint size, AllocationType type, MemoryPermission protect);

        [DllImport("kernel32.dll")]
        private static extern bool VirtualProtect(void* addr, uint size, int new_protect, int* old_protect);

        public static byte* JitBase     { get; set; }
        static ulong AddressBase        { get; set; }

        static JitAllocator()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT: 
                    JitBase = (byte*)VirtualAlloc(null, JitSize, AllocationType.Commit, MemoryPermission.Execute);

                    int dummy;

                    VirtualProtect(JitBase, JitSize, 0x40, &dummy);

                    break;
                default: throw new NotImplementedException();
            }
        }

        public static void* WriteBlock(byte[] Buffer)
        {
            if (AddressBase + (ulong)Buffer.Length >= JitSize)
            {
                throw new OutOfMemoryException();
            }

            Marshal.Copy(Buffer,0,(IntPtr)(JitBase + AddressBase),Buffer.Length);

            void* Out = JitBase + AddressBase;

            AddressBase += (ulong)Buffer.Length;

            return Out;
        }
    }
}
