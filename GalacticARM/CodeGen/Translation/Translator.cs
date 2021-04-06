using GalacticARM.CodeGen.Translation.aarch64;
using GalacticARM.Decoding;
using GalacticARM.IntermediateRepresentation;
using GalacticARM.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Translation
{
    public static class Translator
    {
        public static bool CompileByFunction { get; set; } = true;

        static Dictionary<ulong,GuestFunction> Functions    { get; set; }
        static Dictionary<ulong,ABasicBlock> BasicBlocks     { get; set; }

        static Translator()
        {
            Functions = new Dictionary<ulong, GuestFunction>();
            BasicBlocks = new Dictionary<ulong, ABasicBlock>();
        }

        public static GuestFunction GetOrTranslateFunction(ulong Address)
        {
            GuestFunction Out;

            if (Functions.TryGetValue(Address, out Out))
            {
                return Out;
            }

            Out = TranslateFunction(Address);

            lock (Functions)
            {
                if (!Functions.ContainsKey(Address))

                Functions.Add(Address,Out);
            }

            return Out;
        }

        static ABasicBlock GetOrTranslateBasicBlock(ulong Address)
        {
            ABasicBlock block;

            if (BasicBlocks.TryGetValue(Address,out block))
            {
                return block;
            }

            block = new ABasicBlock(Address);

            lock (BasicBlocks)
            {
                if (!BasicBlocks.ContainsKey(Address))

                BasicBlocks.Add(Address,block);
            }

            return block;
        }

        static GuestFunction TranslateFunction(ulong Address)
        {
            TranslationContext context = new TranslationContext();

            TranslateFunction(context,Address);

            return context.CompileFunction();
        }

        static void TranslateFunction(TranslationContext context, ulong Address)
        {
            if (context.Blocks.ContainsKey(Address))
                return;

            ABasicBlock block = GetOrTranslateBasicBlock(Address);

            Operand Label = context.CreateLabel();

            context.MarkLabel(Label);
            context.Blocks.Add(Address,Label);

            context.KnwonReturns = new List<Operand>();

            foreach (AOpCode opCode in block.Instructions)
            {
                //Emit
                context.CurrentSize = IntSize.Int64;
                context.CurrentOpCode = opCode;

                context.Advance();

                opCode.emit(context);

                //Other
                context.CurrentSize = IntSize.Int64;

                if (CpuThread.InDebugMode)
                {
                    context.Call(nameof(CpuThread.DebugStep),context.ContextPointer(),opCode.Address);
                }
            }

            Operand CurrentReturn = context.GetRegRaw(nameof(ExecutionContext.Return));

            foreach (Operand kr in context.KnwonReturns)
            {
                context.CurrentSize = IntSize.Int64;

                EmitUniversal.EmitIf(context,

                    context.Ceq(context.Const(kr.Data),CurrentReturn),

                    delegate()
                    {
                        if (!context.Blocks.ContainsKey(kr.Data))
                        {
                            TranslateFunction(context,kr.Data);
                        }
                        else
                        {
                            context.Jump(context.Blocks[kr.Data]);
                        }
                    }
                    
                    );
            }

            context.Return(CurrentReturn);
        }
    }
}
