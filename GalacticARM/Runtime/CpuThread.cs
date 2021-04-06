using GalacticARM.CodeGen.Translation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.Runtime
{
    public unsafe delegate void SVC(int id);

    public unsafe class CpuThread
    {
        public static bool DebugComp { get; set; }

        public static bool InDebugMode = false;

        public static ulong Start = 136436001;
        public static ulong End = ulong.MaxValue;

        public ExecutionContext Context;
        ExecutionContext* LoadedContext;

        internal CpuThread(int Handle)
        {
            Context.ID = (ulong)Handle;

            Console.WriteLine($"Created Thread {Handle}");
        }

        static HashSet<int> Handles = new HashSet<int>();
        static Dictionary<int, CpuThread> Threads = new Dictionary<int, CpuThread>();

        public static CpuThread CreateThread()
        {
            int handle = 0;

            while (true)
            {
                if (!Handles.Contains(handle))
                {
                    Handles.Add(handle);

                    Threads.Add(handle, new CpuThread(handle));

                    return Threads[handle];
                }

                handle++;
            }
        }

        public static ulong pp;

        public void Execute(ulong Entry, bool Once = false)
        {
            //Console.WriteLine($"Started Thread {ThreadContext.ID}");

            Context.MemoryPointer = (ulong)VirtualMemoryManager.PageMap;

            fixed (ExecutionContext* context = &Context)
            {
                Context.IsExecuting = 1;

                context->MyPointer = (ulong)context;

                ulong Last = 0;

                LoadedContext = context;

                while (true)
                {
                    GuestFunction function = Translator.GetOrTranslateFunction(Entry);

                    if (DebugComp)
                    {
                        //Console.WriteLine(function.IR);
                        Console.WriteLine(function);
                    }

                    Last = Entry;

                    Entry = function.Execute(context);

                    if (Entry == 0)
                    {
                        Console.WriteLine(Last);
                    }

                    if (Once || Context.IsExecuting == 0)
                    {
                        break;
                    }
                }
            }

            //Console.WriteLine($"Ended Thread {ThreadContext.ID}");
        }

        public SVC svc;

        public static void CallSVC(ulong ContextPointer, ulong id)
        {
            ExecutionContext* context = (ExecutionContext*)ContextPointer;

            Threads[(int)context->ID].svc((int)id);
        }

        public static void DebugStep(ulong ContextPointer, ulong Address)
        {
            ExecutionContext* context = (ExecutionContext*)ContextPointer;

            Threads[(int)context->ID].DebugStep(Address);

            context->ExecutedInstructions++;
        }

        StreamWriter writer;//= new StreamWriter(@"D:\Debug\GSteps.txt");

        void DebugStep(ulong Address)
        {
            //136436001

            if (Context.ExecutedInstructions >= Start && Context.ExecutedInstructions < End)
            {
                writer.WriteLine($"Dat: {Context.ExecutedInstructions} {GalacticARM.Runtime.VirtualMemoryManager.GetOpHex(Address)}");

                for (int i = 0; i < 32; i++)
                {
                    writer.WriteLine(i + " " + Context.GetX(i));
                }
            }

            //157245156

            if (Context.ExecutedInstructions == End)
            {
                writer.Close();

                Console.WriteLine("Done");
            }
        }

        public void EndExecution()
        {
            LoadedContext->IsExecuting = 0;
        }
    }
}
