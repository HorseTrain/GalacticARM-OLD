using GalacticARM.CodeGen.Assembler.X86;
using Iced.Intel;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace GalacticARM.Context
{
    public delegate ulong func(ulong OperationPointer);

    public unsafe class NativeFunction
    {
        public ulong GuestAddress   { get; set; }
        public byte[] Buffer        { get; set; }
        public void* Entry          { get; set; }
        public func _func           { get; set; }

        public NativeFunction(byte[] Buffer)
        {
            this.Buffer = Buffer;

            Entry = JitAllocator.WriteBlock(Buffer);

            _func = Marshal.GetDelegateForFunctionPointer<func>((IntPtr)Entry);
        }

        public ulong Execute(void* OperationPointer) => _func((ulong)OperationPointer);
    }
}
