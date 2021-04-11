using GalacticARM.CodeGen.Translation.aarch64;
using GalacticARM.Decoding;
using GalacticARM.IntermediateRepresentation;
using GalacticARM.Runtime;
using GalacticARM.Runtime.Fallbacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Translation
{
    public static class Translator
    {
        public static bool CompileByFunction { get; set; } = true;

        public static Dictionary<ulong,GuestFunction> Functions     { get; set; }
        static Dictionary<ulong,ABasicBlock> BasicBlocks            { get; set; }

        static Translator()
        {
            Functions = new Dictionary<ulong, GuestFunction>();
            BasicBlocks = new Dictionary<ulong, ABasicBlock>();
        }

        public static GuestFunction GetOrTranslateFunction(ulong Address, Optimizations optimizations = Optimizations.None)
        {
            GuestFunction Out;

            if (Functions.TryGetValue(Address, out Out))
            {
                return Out;
            }

            Out = TranslateFunction(Address,optimizations);

            lock (Functions)
            {
                if (!Functions.ContainsKey(Address))

                Functions.Add(Address,Out);
            }

            Out.optimizations = optimizations;

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

        static GuestFunction TranslateFunction(ulong Address, Optimizations optimizations)
        {
            TranslationContext context = new TranslationContext();

            TranslateFunction(context,Address,optimizations);

            return context.CompileFunction();
        }

        static void TranslateFunction(TranslationContext context, ulong Address, Optimizations optimizations)
        {
            if (context.Blocks.ContainsKey(Address))
                return;

            ABasicBlock block = GetOrTranslateBasicBlock(Address);

            context.CurrentBlock = block;

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

                    EmitUniversal.EmitIf(context,
                        
                        context.Ceq(context.GetRegRaw(nameof(ExecutionContext.Return)),0),
                        
                        delegate()
                        {
                            context.Return(opCode.Address + 4);
                        }
                        
                        );
                }
            }

            Operand CurrentReturn = context.GetRegRaw(nameof(ExecutionContext.Return));

            if (CompileByFunction)
            {
                foreach (Operand kr in context.KnwonReturns)
                {
                    context.CurrentSize = IntSize.Int64;

                    EmitUniversal.EmitIf(context,

                        context.Ceq(context.Const(kr.Data), CurrentReturn),

                        delegate ()
                        {
                            if (!context.Blocks.ContainsKey(kr.Data))
                            {
                                TranslateFunction(context, kr.Data, optimizations);
                            }
                            else
                            {
                                context.Jump(context.Blocks[kr.Data]);
                            }
                        }

                        );
                }
            }

            context.Return(CurrentReturn);
        }
    }
}
