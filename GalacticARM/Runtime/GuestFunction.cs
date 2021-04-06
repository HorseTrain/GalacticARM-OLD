using GalacticARM.IntermediateRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.Runtime
{
    public unsafe delegate ulong _func(ExecutionContext* context);

    public unsafe class GuestFunction
    {
        public OperationBlock IR    { get; set; }
        public byte[] Buffer        { get; set; }
        public _func Func           { get; set; }

        public ulong Execute(ExecutionContext* context) => Func(context);

        public override string ToString()
        {
            SharpDisasm.Disassembler dis = new SharpDisasm.Disassembler(Buffer,SharpDisasm.ArchitectureMode.x86_64);

            StringBuilder Out = new StringBuilder();

            foreach (var ins in dis.Disassemble())
            {
                Out.AppendLine($"0x{ins.Offset:x3} {ins}");
            }

            return Out.ToString();
        }
    }
}
