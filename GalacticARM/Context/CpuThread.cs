using GalacticARM.CodeGen;
using GalacticARM.CodeGen.AEmit;
using GalacticARM.CodeGen.Assembler.Msil;
using GalacticARM.CodeGen.Assembler.X86;
using GalacticARM.CodeGen.Intermediate;
using GalacticARM.CodeGen.Translation;
using GalacticARM.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.Context
{
    public delegate void Hook(int id);

    public unsafe class CpuThread
    {
        public unsafe ContextBlock NativeContext;
        public unsafe ContextBlock* NativeContextPointer;

        public ulong GetX(int Index) => NativeContext.RegisterBuffer[Index];
        public void SetX(int Index, ulong Value) => NativeContext.RegisterBuffer[Index] = Value;

        public ulong GetMisc(MiscRegister reg) => NativeContext.RegisterBuffer[InstructionEmitContext.MiscStart + (ulong)reg];
        public void SetMisc(MiscRegister reg, ulong value) => NativeContext.RegisterBuffer[InstructionEmitContext.MiscStart + (ulong)reg] = value;

        public static bool UseX86 = true;

        public Vector128<float> GetQ(int Index)
        {
            fixed (ContextBlock* nc = &NativeContext)
            {
                Vector128<float>* dat = (Vector128<float>*) ((byte*)nc + (InstructionEmitContext.GuestRegisterCount * 8));

                return dat[Index];
            }
        }

        public void SetQ(int Index, Vector128<float> Value)
        {
            fixed (ContextBlock* nc = &NativeContext)
            {
                Vector128<float>* dat = (Vector128<float>*) ((byte*)nc + (InstructionEmitContext.GuestRegisterCount * 8));

                dat[Index] = Value;
            }
        }

        public void AddFunctionPointer(Delegate func, MiscRegister register)
        {
            SetMisc(register,(ulong)DelegateCache.GetOrAdd(func));
        }

        internal CpuThread(ulong ID)
        {
            AddFunctionPointer(new _Void_U(OnSvc) ,MiscRegister.SvcHook);
            AddFunctionPointer(new _Void_U(DebugStep), MiscRegister.DebugHook);

            SetMisc(MiscRegister.dczid_el0,0x4);

            SetMisc(MiscRegister.ID,ID);
        }

        public ulong Entry;

        public unsafe void Execute(ulong Entry, bool Once = false)
        {
            Console.WriteLine($"Started Thread {GetMisc(MiscRegister.ID)}");

            SetMisc(MiscRegister.PageTablePointer, (ulong)MemoryManager.PageTable.Buffer);

            fixed (ContextBlock* nc = &NativeContext)
            {
                NativeContextPointer = nc;

                SetMisc(MiscRegister.IsExecuting,1);

                if (UseX86)
                {
                    while (true)
                    {
                        NativeFunction block = X86Translator.GetOrTranslate(Entry);

                        this.Entry = Entry;

                        Entry = block.Execute(nc);

                        //Console.WriteLine(Entry);

                        if (!DebugMode)
                            step += block.ArmSize;

                        if (Once || GetMisc(MiscRegister.IsExecuting) == 0)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    while (true)
                    {
                        InstructionEmitContext block = InterpreterTranslator.GetOrTranslate(Entry);

                        //Console.WriteLine(block.Block);

                        if (!DebugMode)
                            step += block.ArmSize;

                        this.Entry = Entry;

                        Entry = block.MsilFunc(nc);

                        if (Once || GetMisc(MiscRegister.IsExecuting) == 0)
                        {
                            break;
                        }
                    }
                }
            }

            Console.WriteLine($"Ended Thread {GetMisc(MiscRegister.ID)}");
        }

        public Hook svc;

        public void OnSvc(ulong Arg)
        {
            //Console.WriteLine($"Hello World");

            svc((int)Arg);

            //Console.WriteLine(Arg);

            //Console.WriteLine($"Hello World");
        }

        //Debug Stuff
        StreamWriter writer;

        public ulong step = 0;

        public static ulong DebugStart;
        public static ulong DebugEnd;
        public static bool DebugMode;

        public void DebugStep(ulong Arg)
        {
            //519455596
            //519455596
            //519455596

            //Console.WriteLine(Arg);

            /*
            if (step >= DebugStart && GetMisc(MiscRegister.IsExecuting) == 1)
            {
                if (writer == null)
                {
                    writer = new StreamWriter(@"D:\Debug\GSteps.txt");
                }

                writer.WriteLine($"step: {step} {MemoryManager.GetOpHex(Arg)}");

                for (int i = 0; i < 32; i++)
                {
                    writer.WriteLine($"{i:d2}: {GetX(i):x16}");
                }
            }

            if (step == DebugEnd - 1)
            {
                writer.Close();

                SetMisc(MiscRegister.IsExecuting, 0);
            }
            */
            
            //step++;
        }
    }
}
