using GalacticARM.CodeGen.AEmit;
using GalacticARM.CodeGen.Assembler.Msil;
using GalacticARM.CodeGen.Assembler.X86;
using GalacticARM.CodeGen.Intermediate;
using GalacticARM.Context;
using GalacticARM.Decoding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Translation
{
    public static class InterpreterTranslator
    {
        public static bool InDebugMode = false;

        public static Dictionary<ulong, InstructionEmitContext> TranslatedFunctions  { get; set; }

        static InterpreterTranslator()
        {
            TranslatedFunctions = new Dictionary<ulong, InstructionEmitContext>();
        }

        public static InstructionEmitContext GetOrTranslate(ulong Address)
        {
            InstructionEmitContext Out;

            if (TranslatedFunctions.TryGetValue(Address, out Out))
            {
                return Out;
            }

            Out = GetIRBlock(Address);

            if (Optimization.UsePasses && Out.Block.Storeable())
            {
                Out.Block = Optimization.Optimize(Out.Block);
            }

            lock (TranslatedFunctions)
            {
                TranslatedFunctions.Add(Address,Out);
            }

            Out.MsilFunc = Generator.CompileIL($"FUNC_{Address:x16}",Out.Block);

            return Out;
        }

        public static InstructionEmitContext GetIRBlock(ulong Address)
        {
            BasicBlock block = OpCodeTable.DecodeBlock(Address);

            InstructionEmitContext translator = new InstructionEmitContext();

            translator.ArmSize = (ulong)block.OpCodes.Count;

            for (int i = 0; i < block.OpCodes.Count; i++)
            {
                AOpCode opCode = block.OpCodes[i];

                translator.SetSize(opCode.Info);

                translator.CurrentOpCode = opCode;

                opCode.Emit(translator);

                if (CpuThread.DebugMode)
                {
                    translator.SetSize(InstructionInfo.X);

                    translator.CallFunctionFromPointer(translator.GetMisc(MiscRegister.DebugHook), translator.Const(opCode.Address));
                }

                translator.Block.ClearLocals();
            }

            translator.Block.Add(new Operation(ILInstruction.Return, translator.ReturnArgument));

            translator.ArmSize = (ulong)block.OpCodes.Count;

            return translator;
        }
    }
}
