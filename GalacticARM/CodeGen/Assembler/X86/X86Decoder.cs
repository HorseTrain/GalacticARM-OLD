using Iced.Intel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Assembler.X86
{
    public static class X86Decoder
    {
        public static string DecodeBlock(byte[] Buffer)
        {
            SharpDisasm.Disassembler disassembler = new SharpDisasm.Disassembler(Buffer, SharpDisasm.ArchitectureMode.x86_64);

            StringBuilder Out = new StringBuilder();

            foreach (var ins in disassembler.Disassemble())
            {
                Out.AppendLine($"0x{ins.Offset:x4} " + ins.ToString());
            }

            return Out.ToString();
        }
    }
}
