using GalacticARM.CodeGen.Translation;
using GalacticARM.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Assembler.X86
{
    public static class X86Translator
    {
        public static Dictionary<ulong, NativeFunction> Functions   { get; set; }

        static X86Translator()
        {
            Functions = new Dictionary<ulong, NativeFunction>();
        }

        public static NativeFunction GetOrTranslate(ulong Address)
        {
            NativeFunction Out;

            if (Functions.TryGetValue(Address, out Out))
            {
                return Out;
            }

            X86Assembler assembler = new X86Assembler();

            assembler.Compile(InterpreterTranslator.GetIRBlock(Address).Block);

            Out = assembler.GetNativeFunction();

            lock (Functions)
            {
                Functions.Add(Address, Out);
            }

            return Out;
        }
    }
}
