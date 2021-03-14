using GalacticARM.CodeGen.Assembler.X86;
using GalacticARM.CodeGen.Intermediate;
using System;
using static Iced.Intel.AssemblerRegisters;
using GalacticARM.CodeGen.AEmit;
using GalacticARM.Context;
using GalacticARM.Memory;
using UnicornNET;
using Keystone;
using System.Text;
using GalacticARM.CodeGen.Assembler.Msil;
using GalacticARM.Decoding;
using System.Runtime.Intrinsics;
using GalacticARM.CodeGen;

namespace Tester
{
    public unsafe class Tester
    {
        public const ulong Size = 20UL * 1024UL * 1024UL;
        int ProgramSize { get; set; }

        public Arm64Engine uengine  { get; set; }
        public CpuThread gengine    { get; set; }

        MemoryBlock uram { get; set; }
        MemoryBlock gram { get; set; }

        public Tester()
        {
            uengine = new Arm64Engine();
            gengine = ThreadManager.CreateThread();

            uram = new MemoryBlock(Size);
            gram = new MemoryBlock(Size);

            uengine.MapMemory(Size,uram.Buffer);
            MemoryManager.MapMemory(0,gram.Buffer,Size);
        }

        public void WriteProgram(string str)
        {
            Engine keystone = new Engine(Architecture.ARM64,Mode.LITTLE_ENDIAN);

            byte[] buff = keystone.Assemble(str,0).Buffer;

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
            gengine.Execute(0,true);
            uengine.RunUntil((ulong)ProgramSize);

            for (int i = 0; i < 32; i++)
            {
                ulong u = uengine.GetX(i);
                ulong g = gengine.GetX(i);

                Console.WriteLine($"{g:x16} {u:x16} {u == g}");
            }

            for (int i = 0; i < 32; i++)
            {
                ulong u0 = uengine.GetVector(i).d0;
                ulong u1 = uengine.GetVector(i).d1;

                ulong g0 = gengine.GetQ(i).AsUInt64().GetElement(0);
                ulong g1 = gengine.GetQ(i).AsUInt64().GetElement(1);

                Console.WriteLine($"{g0:x16} {g1:x16} {u0:x16} {u1:x16} {(u0 == g0) && (g1 == u1)}");
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

    class Program
    {
        static void TestCall(ulong i)
        {
            //Console.WriteLine("Nigga");
        }

        static unsafe void Test()
        {
            /*
            var values = Enum.GetValues(typeof(ILInstruction));

            foreach (ILInstruction instruction in values)
            {
                //Console.WriteLine(instruction);

                Console.WriteLine("Generate" + instruction + ",");

                //Console.WriteLine(@"        public static void Generate" + instruction + "(X86Assembler assembler)\n        {\n            throw new NotImplementedException();\n        }\n");
            }
            */

            Random r = new Random();

            InstructionEmitContext test = new InstructionEmitContext();

            int regcap = 30;

            //test.SetSize(InstructionInfo.W);

            for (int i = 0; i < 10;i++)
            {
                test.Block.Add(new Operation(ILInstruction.Subtract, Operand.Register(r.Next(regcap)), Operand.Const((ulong)r.Next() | ((ulong)r.Next() << 32))));

                Label label = test.CreateLabel();

                Operand b = Operand.Register(r.Next(regcap));

                test.Block.Add(new Operation(ILInstruction.CompareLessThan, b, Operand.Const((ulong)r.Next() | ((ulong)r.Next() << 32))));
                test.Block.JumpIf(label, b);

                test.Block.Add(new Operation(ILInstruction.Add, Operand.Register(r.Next(regcap)), Operand.Const((ulong)r.Next() | ((ulong)r.Next() << 32))));
                test.Block.Add(new Operation(ILInstruction.Subtract, Operand.Register(r.Next(regcap)), Operand.Const((ulong)r.Next() | ((ulong)r.Next() << 32))));
                test.Block.Add(new Operation(ILInstruction.Multiply, Operand.Register(r.Next(regcap)), Operand.Const((ulong)r.Next() | ((ulong)r.Next() << 32))));
                test.Block.Add(new Operation(ILInstruction.Divide_Un, Operand.Register(r.Next(regcap)), Operand.Const((ulong)r.Next() | ((ulong)r.Next() << 32))));

                test.MarkLabel(label);
            }

            test.Block.Add(new Operation(ILInstruction.Return, Operand.Register(0)));

            ContextBlock msil = new ContextBlock();
            ContextBlock x86 = new ContextBlock();

            test.MsilFunc = Generator.CompileIL("", test.Block);

            test.MsilFunc(&msil);

            X86Compiler compiler = new X86Compiler();
            compiler.Compile(test.Block);

            NativeFunction X86func = compiler.GetNativeFunction();

            Console.WriteLine(X86Decoder.DecodeBlock(X86func.Buffer));

            X86func.Execute(&x86);

            for (int i = 0; i < regcap; i++)
            {
                Console.WriteLine($"{msil.RegisterBuffer[i]:x16} {x86.RegisterBuffer[i]:x16} {msil.RegisterBuffer[i] == x86.RegisterBuffer[i]}");
            }
        }

        static void TestRand()
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

            void Random()
            {
                for (int i = 0; i < 1; i++)
                {
                    builder.AppendLine($"movz {reg(false, true)}, {r.Next(ushort.MaxValue)}, lsl #{r.Next(4) * 16}");
                    builder.AppendLine($"movn {reg(false, true)}, {r.Next(ushort.MaxValue)}, lsl #{r.Next(4) * 16}");
                    builder.AppendLine($"movk {reg(false, true)}, {r.Next(ushort.MaxValue)}, lsl #{r.Next(4) * 16}");

                    builder.AppendLine($"add {reg(true)}, {reg(true)}, {r.Next(4095)}");
                    builder.AppendLine($"sub {reg(true)}, {reg(true)}, {r.Next(4095)}, lsl #12");                   
                    builder.AppendLine($"add {reg(true)}, {reg(true)}, {r.Next(4095)}");
                    builder.AppendLine($"subs {reg(false)}, {reg(true)}, {r.Next(4095)}, lsl #12");                   
                    builder.AppendLine($"csinv {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");                   
                    builder.AppendLine($"subs {reg(false)}, {reg(true)}, {r.Next(4095)}");                  
                    builder.AppendLine($"subs {reg(false)}, {reg(true)}, {r.Next(4095)}, lsl #12");                  
                    builder.AppendLine($"adds {reg(false)}, {reg(false)}, {reg(false)}, lsl {r.Next(31)}");                   
                    builder.AppendLine($"subs {reg(false)}, {reg(false)}, {reg(false)}, lsr {r.Next(31)}");                   
                    builder.AppendLine($"bic {reg(false)}, {reg(false)}, {reg(false)}, lsr {r.Next(31)}");
                    builder.AppendLine($"orr {reg(false)}, {reg(false)}, {reg(false)}, asr {r.Next(31)}");
                    builder.AppendLine($"orn {reg(false)}, {reg(false)}, {reg(false)}, lsr {r.Next(31)}");
                    builder.AppendLine($"eor {reg(false)}, {reg(false)}, {reg(false)}, ror {r.Next(31)}");
                    builder.AppendLine($"eon {reg(false)}, {reg(false)}, {reg(false)}, lsl {r.Next(31)}");                   
                    builder.AppendLine($"ccmp {reg(false)}, {r.Next(31)}, {r.Next(15)},{(Condition)r.Next(15)}");
                    builder.AppendLine($"ccmn {reg(false)}, {r.Next(31)}, {r.Next(15)},{(Condition)r.Next(15)}");                  
                    builder.AppendLine($"csneg {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");                  
                    builder.AppendLine($"adr {reg(false, true)}, {r.Next(4096)}");
                    builder.AppendLine($"adrp {reg(false, true)}, {4096 * -r.Next(4096)}");
                    builder.AppendLine($"adrp {reg(false, true)}, {4096 * r.Next(4096)}");                   
                    builder.AppendLine($"sub {reg(true, true)},{reg(true, true)}, {reg(false, false)}, sxtb");
                    builder.AppendLine($"sub {reg(true, true)},{reg(true, true)}, {reg(false, false)}, sxth");
                    builder.AppendLine($"add {reg(true, true)},{reg(true, true)}, {reg(false, false)}, sxtw");
                    builder.AppendLine($"subs {reg(false, true)},{reg(true, true)}, {reg(false, false)}, sxtb");
                    builder.AppendLine($"subs {reg(false, true)},{reg(true, true)}, {reg(false, false)}, sxth");
                    builder.AppendLine($"adds {reg(false, true)},{reg(true, true)}, {reg(false, false)}, sxtw");
                    //builder.AppendLine($"sbfm {reg(false)}, {reg(false)}, {r.Next(31)}, {r.Next(31)}");
                    //builder.AppendLine($"ubfm {reg(false)}, {reg(false)}, {r.Next(31)}, {r.Next(31)}");
                    //builder.AppendLine($"bfm {reg(false)}, {reg(false)}, {r.Next(31)}, {r.Next(31)}");                 
                    //builder.AppendLine($"udiv {reg(false)}, {reg(false)}, {reg(false)}");
                    //builder.AppendLine($"sdiv {reg(false)}, {reg(false)}, {reg(false)}");                  
                   // builder.AppendLine($"LSLV {reg(false)}, {reg(false)}, {reg(false)}");
                   // builder.AppendLine($"LSRV {reg(false)}, {reg(false)}, {reg(false)}");
                   // builder.AppendLine($"ASRV {reg(false)}, {reg(false)}, {reg(false)}");
                   // builder.AppendLine($"RORV {reg(false)}, {reg(false)}, {reg(false)}");                
                   // builder.AppendLine($"madd {reg(false)}, {reg(false)},{reg(false)},{reg(false)}");
                   // builder.AppendLine($"msub {reg(false)}, {reg(false)},{reg(false)},{reg(false)}");
                   // builder.AppendLine($"dup v{r.Next(32)}.8b,{reg(false, false)}");
                   // builder.AppendLine($"dup v{r.Next(32)}.16b,{reg(false, false)}");
                   // builder.AppendLine($"adds {reg(false)}, {reg(false)}, {reg(false)}, lsl {r.Next(31)}");
                   // builder.AppendLine($"ccmp {reg(false)}, {reg(false)}, {r.Next(15)},{(Condition)r.Next(15)}");
                   // builder.AppendLine($"ccmn {reg(false)}, {reg(false)}, {r.Next(15)},{(Condition)r.Next(15)}");
                   // builder.AppendLine($"csneg {reg(false)}, {reg(false)},{reg(false)},{(Condition)r.Next(15)} ");
                   // builder.AppendLine($"rbit {reg(false,false)}, {reg(false,false)}");
                   // builder.AppendLine($"rbit {reg(false)}, {reg(false)}");
                   // builder.AppendLine($"clz {reg(false, false)}, {reg(false, false)}");
                   // builder.AppendLine($"clz {reg(false)}, {reg(false)}");
                   // builder.AppendLine($"fmov d{r.Next(32)}, {reg(false, true)}");
                   // builder.AppendLine($"orr v{r.Next(32)}.16b, v{r.Next(32)}.16b, v{r.Next(32)}.16b");
                   // builder.AppendLine($"orn v{r.Next(32)}.8b, v{r.Next(32)}.8b, v{r.Next(32)}.8b");
                   // builder.AppendLine($"and v{r.Next(32)}.16b, v{r.Next(32)}.16b, v{r.Next(32)}.16b");
                   // builder.AppendLine($"bic v{r.Next(32)}.8b, v{r.Next(32)}.8b, v{r.Next(32)}.8b");
                   // builder.AppendLine($"eor v{r.Next(32)}.16b, v{r.Next(32)}.16b, v{r.Next(32)}.16b");
                    //builder.AppendLine($"cnt v{r.Next(32)}.8b, v{r.Next(32)}.8b");

                    builder.AppendLine($"");
                }
            }

            void Generate()
            {
                builder.AppendLine(@$"

subs x0, x0, #100

");
            }

            //Generate();
            Random();


            builder.AppendLine("b end");
            builder.AppendLine("end:");

            test.WriteProgram(builder.ToString());

            test.TestProgram();

        }

        static unsafe void Main(string[] args)
        {
            //Test();

            TestRand();
        }
    }
}
