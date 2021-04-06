using GalacticARM.CodeGen.Translation;
using GalacticARM.CodeGen.X86;
using GalacticARM.Decoding;
using GalacticARM.IntermediateRepresentation;
using GalacticARM.Runtime;
using GalacticARM.Runtime.Fallbacks;
using GalacticARM.Runtime.X86;
using Keystone;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;
using UnicornNET;
using static GalacticARM.IntermediateRepresentation.Operand;

namespace Testing
{
    public unsafe class MemoryBlock : IDisposable
    {
        public void* Buffer { get; set; }
        public ulong Size { get; set; }

        GCHandle pin = new GCHandle();

        public MemoryBlock(ulong Size)
        {
            byte[] buff = new byte[Size];

            pin = GCHandle.Alloc(buff, GCHandleType.Pinned);

            fixed (byte* dat = buff)
            {
                Buffer = dat;
            }

            this.Size = Size;
        }

        public void Dispose()
        {
            pin.Free();
        }
    }

    public unsafe class Tester
    {
        public const ulong Size = 20UL * 1024UL * 1024UL;
        int ProgramSize { get; set; }

        public Arm64Engine uengine { get; set; }
        public CpuThread gengine { get; set; }

        MemoryBlock uram { get; set; }
        MemoryBlock gram { get; set; }

        public Tester()
        {
            uengine = new Arm64Engine();
            gengine = CpuThread.CreateThread();

            uram = new MemoryBlock(Size);
            gram = new MemoryBlock(Size);

            uengine.MapMemory(Size, uram.Buffer);
            VirtualMemoryManager.MapMemory(0, gram.Buffer, Size);
        }

        public void WriteProgram(string str)
        {
            Engine keystone = new Engine(Keystone.Architecture.ARM64, Mode.LITTLE_ENDIAN);

            byte[] buff = keystone.Assemble(str, 0).Buffer;

            Random r = new Random();

            for (uint i = 0; i < Size; i++)
            {
                byte e = (byte)r.Next(256);

                ((byte*)uram.Buffer)[i] = e;
                ((byte*)gram.Buffer)[i] = e;
            }

            for (int i = 0; i < buff.Length; i++)
            {
                ((byte*)uram.Buffer)[i] = buff[i];
                ((byte*)gram.Buffer)[i] = buff[i];
            }

            ProgramSize = buff.Length;
        }

        public void TestProgram()
        {
            gengine.Context.NZCV = 0;
            uengine.NZCV = 0;

            gengine.Execute(0,true);
            uengine.RunUntil((ulong)ProgramSize);

            for (int i = 0; i < 32; i++)
            {
                ulong u = uengine.GetX(i);
                ulong g = gengine.Context.GetX(i);

                Console.WriteLine($"{i:d3} {g:x16} {u:x16} {u == g}");
            }

            Console.WriteLine($"Fl: {gengine.Context.NZCV:x16} {uengine.NZCV:x16} {gengine.Context.NZCV == uengine.NZCV}");

            for (int i = 0; i < 32; i++)
            {
                ulong u0 = uengine.GetVector(i).d0;
                ulong u1 = uengine.GetVector(i).d1;

                ulong g0 = gengine.Context.GetQ(i).AsUInt64().GetElement(0);
                ulong g1 = gengine.Context.GetQ(i).AsUInt64().GetElement(1);

                Console.WriteLine($"{i:d3} {g0:x16} {g1:x16} {u0:x16} {u1:x16} {(u0 == g0) && (g1 == u1)}");
            }

            for (ulong i = 0; i < Size; i++)
            {
                if (((byte*)uram.Buffer)[i] != ((byte*)gram.Buffer)[i])
                {
                    Console.WriteLine($"Bad Memory {i}");
                }
            }
        }
    }

    unsafe class Program
    {
        static void Test()
        {
            Tester test = new Tester();

            StringBuilder builder = new StringBuilder();

            Random r = new Random();

            string reg(bool sp, bool x = true)
            {
                int rand = r.Next(32);

                if (x)
                {
                    if (rand == 31)
                    {
                        if (sp)
                            return "sp";

                        return "xzr";
                    }

                    return $"x{rand}";
                }
                else
                {
                    if (rand == 31)
                    {
                        if (sp)
                            return "wsp";

                        return "wzr";
                    }

                    return $"w{rand}";
                }
            }

            CpuThread.DebugComp = false;

            void Random()
            {
                for (int i = 0; i < 50; i++)
                {
                    builder.AppendLine($"movz {reg(false, true)}, {r.Next(ushort.MaxValue)}, lsl #{r.Next(4) * 16}");
                    builder.AppendLine($"movn {reg(false, true)}, {r.Next(ushort.MaxValue)}, lsl #{r.Next(4) * 16}");
                    builder.AppendLine($"movk {reg(false, true)}, {r.Next(ushort.MaxValue)}, lsl #{r.Next(4) * 16}");
                    
                    builder.AppendLine($"add {reg(true)}, {reg(true)}, {r.Next(4095)}");
                    builder.AppendLine($"sub {reg(true)}, {reg(true)}, {r.Next(4095)}, lsl #12");
                    builder.AppendLine($"add {reg(true)}, {reg(true)}, {r.Next(4095)}");
                    
                    builder.AppendLine($"sub {reg(false)}, {reg(false)}, {reg(false)}, lsl {r.Next(32)}");
                    builder.AppendLine($"add {reg(false)}, {reg(false)}, {reg(false)}, lsr {r.Next(32)}");
                    builder.AppendLine($"add {reg(false)}, {reg(false)}, {reg(false)}, asr {r.Next(32)}");
                    
                    builder.AppendLine($"adds {reg(false)}, {reg(true)}, {r.Next(4095)}, lsl #12");
                    builder.AppendLine($"subs {reg(false)}, {reg(true)}, {r.Next(4095)}");
                    builder.AppendLine($"subs {reg(false)}, {reg(true)}, {r.Next(4095)}, lsl #12");
                    builder.AppendLine($"adds {reg(false)}, {reg(false)}, {reg(false)}, lsl {r.Next(31)}");
                    builder.AppendLine($"subs {reg(false)}, {reg(false)}, {reg(false)}, lsr {r.Next(31)}");
                    
                    builder.AppendLine($"sub {reg(true, true)},{reg(true, true)}, {reg(false, false)}, lsl 1");
                    builder.AppendLine($"sub {reg(true, true)},{reg(true, true)}, {reg(false, false)}, lsl 2");
                    builder.AppendLine($"add {reg(true, true)},{reg(true, true)}, {reg(false, false)}, lsl 3");
                    
                    builder.AppendLine($"add {reg(true, true)}, {reg(true, true)}, {reg(false, false)}, sxtb");
                    builder.AppendLine($"add {reg(true, true)}, {reg(true, true)}, {reg(false, false)}, sxth");
                    builder.AppendLine($"add {reg(true,true)}, {reg(true, true)}, {reg(false, false)}, sxtw");
                    
                    builder.AppendLine($"csel {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");
                    builder.AppendLine($"subs {reg(false)}, {reg(false)}, {reg(false)}, lsl {r.Next(32)}");
                    builder.AppendLine($"csinc {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");
                    builder.AppendLine($"adds {reg(false)}, {reg(false)}, {reg(false)}, lsr {r.Next(32)}");
                    builder.AppendLine($"csneg {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");
                    builder.AppendLine($"adds {reg(false)}, {reg(false)}, {reg(false)}, asr {r.Next(32)}");
                    builder.AppendLine($"csinv {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");
                       
                    builder.AppendLine($"csel {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");
                    builder.AppendLine($"subs {reg(false)}, {reg(false)}, {reg(false)}, lsl {r.Next(32)}");
                    builder.AppendLine($"csinc {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");
                    builder.AppendLine($"adds {reg(false)}, {reg(false)}, {reg(false)}, lsr {r.Next(32)}");
                    builder.AppendLine($"csneg {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");
                    builder.AppendLine($"adds {reg(false)}, {reg(false)}, {reg(false)}, asr {r.Next(32)}");
                    builder.AppendLine($"csinv {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");
                    
                    builder.AppendLine($"subs {reg(false, true)},{reg(true, true)}, {reg(false, false)}, sxtb");
                    builder.AppendLine($"subs {reg(false, true)},{reg(true, true)}, {reg(false, false)}, sxth");
                    builder.AppendLine($"adds {reg(false, true)},{reg(true, true)}, {reg(false, false)}, sxtw");
                    
                    builder.AppendLine($"csel {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");
                    builder.AppendLine($"csinc {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");
                    builder.AppendLine($"csneg {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");
                    builder.AppendLine($"csinv {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");
                    
                    builder.AppendLine($"orr {reg(true)},  {reg(false)}, 0xffff");
                    builder.AppendLine($"and {reg(true)},  {reg(false)}, 0xffff");
                    builder.AppendLine($"eor {reg(true)},  {reg(false)}, 0xffff");
                    
                    builder.AppendLine($"ands {reg(false)},  {reg(false)}, 0xffff");
                    
                    builder.AppendLine($"and {reg(false)}, {reg(false)}, {reg(false)}, lsr {r.Next(31)}");
                    builder.AppendLine($"bic {reg(false)}, {reg(false)}, {reg(false)}, lsr {r.Next(31)}");
                    builder.AppendLine($"orr {reg(false)}, {reg(false)}, {reg(false)}, asr {r.Next(31)}");
                    builder.AppendLine($"orn {reg(false)}, {reg(false)}, {reg(false)}, lsr {r.Next(31)}");
                    builder.AppendLine($"eor {reg(false)}, {reg(false)}, {reg(false)}, lsr {r.Next(31)}");
                    builder.AppendLine($"eon {reg(false)}, {reg(false)}, {reg(false)}, lsl {r.Next(31)}");
                    builder.AppendLine($"ands {reg(false)}, {reg(false)}, {reg(false)}, lsl {r.Next(31)}");
                    builder.AppendLine($"ccmp {reg(false)}, {r.Next(31)}, {r.Next(15)},{(Condition)r.Next(15)}");
                    builder.AppendLine($"csinc {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");
                    builder.AppendLine($"ccmp {reg(false)}, {reg(false)}, {r.Next(15)},{(Condition)r.Next(15)}");
                    builder.AppendLine($"csneg {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");
                    builder.AppendLine($"ccmn {reg(false)}, {reg(false)}, {r.Next(15)},{(Condition)r.Next(15)}");
                    builder.AppendLine($"csneg {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");
                    builder.AppendLine($"adr {reg(false, true)}, {r.Next(4096)}");
                    builder.AppendLine($"adrp {reg(false, true)}, {4096 * -r.Next(4096)}");
                    builder.AppendLine($"adrp {reg(false, true)}, {4096 * r.Next(4096)}");
                    builder.AppendLine($"ands {reg(false)},  {reg(false)}, 0xf");
                    builder.AppendLine($"ccmn {reg(false)}, {r.Next(31)}, {r.Next(15)},{(Condition)r.Next(15)}");
                    builder.AppendLine($"csel {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");
                    builder.AppendLine($"csinc {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");
                    builder.AppendLine($"csneg {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");
                    builder.AppendLine($"csinv {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");
                    //
                    builder.AppendLine($"sbfm {reg(false)}, {reg(false)}, {r.Next(31)}, {r.Next(31)}");
                    builder.AppendLine($"ubfm {reg(false)}, {reg(false)}, {r.Next(31)}, {r.Next(31)}");
                    builder.AppendLine($"bfm {reg(false)}, {reg(false)}, {r.Next(31)}, {r.Next(31)}");                 
                    builder.AppendLine($"udiv {reg(false)}, {reg(false)}, {reg(false)}");
                    //builder.AppendLine($"sdiv {reg(false)}, {reg(false)}, {reg(false)}");                  
                    builder.AppendLine($"LSLV {reg(false)}, {reg(false)}, {reg(false)}");
                    builder.AppendLine($"LSRV {reg(false)}, {reg(false)}, {reg(false)}");
                    builder.AppendLine($"ASRV {reg(false)}, {reg(false)}, {reg(false)}");
                    builder.AppendLine($"RORV {reg(false)}, {reg(false)}, {reg(false)}");
                    builder.AppendLine($"madd {reg(false)}, {reg(false)},{reg(false)},{reg(false)}");
                    builder.AppendLine($"msub {reg(false)}, {reg(false)},{reg(false)},{reg(false)}");
                    //builder.AppendLine($"dup v{r.Next(32)}.8b,{reg(false, false)}");
                    //builder.AppendLine($"dup v{r.Next(32)}.16b,{reg(false, false)}");
                    //builder.AppendLine($"adds {reg(false)}, {reg(false)}, {reg(false)}, lsl {r.Next(31)}");
                    //
                    //builder.AppendLine($"fcsel s{r.Next(32)}, s{r.Next(32)}, s{r.Next(32)}, lo");
                    //
                    builder.AppendLine($"SMADDL {reg(false, true)}, {reg(false, false)}, {reg(false, false)}, {reg(false, true)}");
                    builder.AppendLine($"SMSUBL {reg(false, true)}, {reg(false, false)}, {reg(false, false)}, {reg(false, true)}");
                    builder.AppendLine($"UMADDL {reg(false, true)}, {reg(false, false)}, {reg(false, false)}, {reg(false, true)}");
                    builder.AppendLine($"UMSUBL {reg(false, true)}, {reg(false, false)}, {reg(false, false)}, {reg(false, true)}");
                    //
                    //builder.AppendLine($"fcsel d{r.Next(32)}, d{r.Next(32)}, d{r.Next(32)}, {(Condition)r.Next(15)}");
                    //
                    builder.AppendLine($"extr {reg(false, false)},{reg(false, false)},{reg(false, false)}, #8");
                    builder.AppendLine($"extr {reg(false)},{reg(false)},{reg(false)}, #8");
                    ////
                    builder.AppendLine($"ins v{r.Next(32)}.d[{r.Next(2)}], {reg(false, true)}");
                    builder.AppendLine($"ins v{r.Next(32)}.s[{r.Next(4)}], {reg(false, false)}");
                    //builder.AppendLine($"rev {reg(false, false)},{reg(false, false)}");
                    //builder.AppendLine($"rev {reg(false, true)},{reg(false, true)}");
                    //builder.AppendLine($"rev16 {reg(false, false)},{reg(false, false)}");
                    //builder.AppendLine($"rev16 {reg(false, true)},{reg(false, true)}");
                    //
                    //builder.AppendLine($"ccmp {reg(false)}, {reg(false)}, {r.Next(15)},{(Condition)r.Next(15)}");
                    //builder.AppendLine($"ccmn {reg(false)}, {reg(false)}, {r.Next(15)},{(Condition)r.Next(15)}");
                    //builder.AppendLine($"csneg {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");
                    builder.AppendLine($"rbit {reg(false, false)}, {reg(false, false)}");
                    builder.AppendLine($"rbit {reg(false)}, {reg(false)}");
                    builder.AppendLine($"clz {reg(false, false)}, {reg(false, false)}");
                    builder.AppendLine($"clz {reg(false)}, {reg(false)}");
                    //builder.AppendLine($"ext v{r.Next(32)}.16b, v{r.Next(32)}.16b, v{r.Next(32)}.16b, #8");
                    //
                    //builder.AppendLine($"mov d{r.Next(32)}, v3.d[{r.Next(2)}]");
                    //builder.AppendLine($"mov s{r.Next(32)}, v3.s[{r.Next(4)}]");
                    //builder.AppendLine($"mov h{r.Next(32)}, v3.h[{r.Next(8)}]");
                    //builder.AppendLine($"mov b{r.Next(32)}, v3.b[{r.Next(16)}]");

                    //builder.AppendLine($"UMULH {reg(false,true)}, {reg(false, true)},{reg(false, true)}");
                    //builder.AppendLine($"fmov d{r.Next(32)}, {reg(false, true)}");
                    builder.AppendLine($"orr v{r.Next(32)}.16b, v{r.Next(32)}.16b, v{r.Next(32)}.16b");
                    builder.AppendLine($"orn v{r.Next(32)}.8b, v{r.Next(32)}.8b, v{r.Next(32)}.8b");
                    builder.AppendLine($"and v{r.Next(32)}.16b, v{r.Next(32)}.16b, v{r.Next(32)}.16b");
                    builder.AppendLine($"bic v{r.Next(32)}.8b, v{r.Next(32)}.8b, v{r.Next(32)}.8b");
                    builder.AppendLine($"eor v{r.Next(32)}.16b, v{r.Next(32)}.16b, v{r.Next(32)}.16b");
                    builder.AppendLine($"cnt v{r.Next(32)}.8b, v{r.Next(32)}.8b");
                    builder.AppendLine($"uaddlv h{r.Next(32)}, v{r.Next(32)}.16b");
                    //builder.AppendLine($"ucvtf s{r.Next(32)}, s{r.Next(32)}");

                    //builder.AppendLine($"ext v5.16b, v3.16b, v3.16b, #8");
                    //
                    //builder.AppendLine($"add v{r.Next(32)}.2d,v{r.Next(32)}.2d, v{r.Next(32)}.2d");
                    //builder.AppendLine($"sub v{r.Next(32)}.2d,v{r.Next(32)}.2d, v{r.Next(32)}.2d");
                    //
                    //builder.AppendLine($"neg v{r.Next(32)}.16b, v{r.Next(32)}.16b");
                    //builder.AppendLine($"neg v{r.Next(32)}.8h, v{r.Next(32)}.8h");
                    //builder.AppendLine($"neg v{r.Next(32)}.4s, v{r.Next(32)}.4s");
                    //builder.AppendLine($"neg v{r.Next(32)}.2d, v{r.Next(32)}.2d");
                    //
                    //builder.AppendLine($"add v{r.Next(32)}.4s, v{r.Next(32)}.4s, v{r.Next(32)}.4s");
                    //builder.AppendLine($"sub v{r.Next(32)}.4s, v{r.Next(32)}.4s, v{r.Next(32)}.4s");
                    //
                    //builder.AppendLine($"ushl v{r.Next(32)}.2s, v{r.Next(32)}.2s, v{r.Next(32)}.2s");
                    //builder.AppendLine($"ushl v{r.Next(32)}.4s, v{r.Next(32)}.4s, v{r.Next(32)}.4s");
                    //builder.AppendLine($"ushl v{r.Next(32)}.2d, v{r.Next(32)}.2d, v{r.Next(32)}.2d");
                    //builder.AppendLine($"sshll v{r.Next(32)}.2d, v{r.Next(32)}.2s, #{r.Next(5)}");
                    //
                    //builder.AppendLine($"shl v{r.Next(32)}.2d, v{r.Next(32)}.2d, #{r.Next(15)}");
                    //
                    //builder.AppendLine($"mov s{r.Next(32)}, v{r.Next(32)}.s[{r.Next(4)}]");
                    //builder.AppendLine($"csneg {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");
                    //builder.AppendLine($"csneg {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");
                    //

                    //builder.AppendLine($"dup v{r.Next(32)}.2s, v{r.Next(32)}.s[{r.Next(4)}]");
                    //
                    //builder.AppendLine($"dup v{r.Next(32)}.4s, v{r.Next(32)}.s[{r.Next(4)}]");
                    //
                    //builder.AppendLine($"dup v{r.Next(32)}.8b, v{r.Next(32)}.b[3]");

                    //builder.AppendLine($"scvtf d{r.Next(32)},{reg(false, true)}");

                    //
                    //builder.AppendLine($"bsl v{r.Next(32)}.16b, v{r.Next(32)}.16b, v{r.Next(32)}.16b");

                    //builder.AppendLine($"movi v{r.Next(32)}.2d, 255");

                    //builder.AppendLine($"scvtf s{r.Next(32)},{reg(false, true)}");
                    //builder.AppendLine($"scvtf s{r.Next(32)},{reg(false, true)}");

                    //builder.AppendLine($"scvtf s{r.Next(32)},{reg(false)}");
                    //builder.AppendLine($"scvtf s{r.Next(32)},{reg(false)}");
                    //
                    //builder.AppendLine($"fadd s{r.Next(32)},s{r.Next(32)},s{r.Next(32)}");
                    //builder.AppendLine($"fmul s{r.Next(32)},s{r.Next(32)},s{r.Next(32)}");
                    //builder.AppendLine($"fdiv s{r.Next(32)},s{r.Next(32)},s{r.Next(32)}");

                }
            }

            void Generate()
            {
                builder.AppendLine(@$"

mov x0, 300

scvtf d0, x0

mov x0, -400

scvtf d1, x0

fnmul d4, d0, d1


");
            }

            Generate();
            //Random();

            builder.AppendLine("b end");
            builder.AppendLine("end:");

            test.WriteProgram(builder.ToString());

            GuestFunction context = Translator.GetOrTranslateFunction(0);

            Console.WriteLine(context.IR);
            Console.WriteLine(context);

            test.TestProgram();
        }

        static void TestComp()
        {
            TranslationContext context = new TranslationContext();

            //Operand t1 = context.LoadVector(32,3);

            context.SetRegister(0,100);

            context.SetVectorElement(Vec(1), ulong.MaxValue, 1, 3);

            Operand v = context.CreateVector();

            context.SetVectorElement(v,300,0,0);

            Operand d = context.GetVectorElement(v,3,0);

            context.Return(30);

            GAssembler assembler = new GAssembler(context);

            GuestFunction function = assembler.Compile();

            Console.WriteLine(function);

            ExecutionContext executionContext = new ExecutionContext();

            function.Execute(&executionContext);
        }

        static void Main(string[] args)
        {
            CpuThread.DebugComp = true;
            CpuThread.InDebugMode = false;
            Translator.CompileByFunction = false;

            Test();

            //TestComp();

            /*
            foreach (var e in Enum.GetValues(typeof(Instruction)))
            {
                Console.WriteLine($"public Operand {e}(Operand Arg0, Operand Arg1) => MoveWithOperation(Instruction.{e}, Arg0, Arg1);");
            }*/
        }
    }
}
