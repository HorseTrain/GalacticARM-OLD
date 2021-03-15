using GalacticARM.CodeGen.AEmit;
using GalacticARM.CodeGen.Translation;
using GalacticARM.Context;
using System;
using System.Collections.Generic;
using System.IO;
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

            X86Compiler assembler = new X86Compiler();

            InstructionEmitContext il = InterpreterTranslator.GetIRBlock(Address);

            assembler.Compile(il.Block);

            Out = assembler.GetNativeFunction();

            Out.ArmSize = il.ArmSize;

            lock (Functions)
            {
                Functions.Add(Address, Out);
            }

            //Console.WriteLine(X86Decoder.DecodeBlock(Out.Buffer));

            return Out;
        }
    }
}
