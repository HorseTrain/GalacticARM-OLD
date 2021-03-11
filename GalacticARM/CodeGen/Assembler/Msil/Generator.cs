using GalacticARM.CodeGen.AEmit;
using GalacticARM.CodeGen.Intermediate;
using GalacticARM.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Assembler.Msil
{
    public class Generator
    {
        DynamicMethod method;

        public ILGenerator il;

        const int RegSize = InstructionEmitContext.GuestRegisterCount + InstructionEmitContext.GuestSimdRegisterCount + InstructionEmitContext.GuestMiscCount + 10000;

        public List<System.Reflection.Emit.Label> Lables { get; set; }

        public OperationSize CurrentSize                { get; set; }

        public int ReturnLocal                          { get; set; }
        public int PointerStore                         { get; set; }

        public int FloatStore                           { get; set; }
        public int DoubleStore                          { get; set; }

        public unsafe Generator(string name)
        {
            method = new DynamicMethod(name,typeof(ulong),new Type[] { typeof(ContextBlock*) });

            il = method.GetILGenerator();

            ReturnLocal = il.DeclareLocal(typeof(ulong)).LocalIndex;
            PointerStore = il.DeclareLocal(typeof(ulong)).LocalIndex;

            FloatStore = il.DeclareLocal(typeof(float)).LocalIndex;
            DoubleStore = il.DeclareLocal(typeof(float)).LocalIndex;

            Lables = new List<System.Reflection.Emit.Label>();
        }

        public void LoadFloat(int index, int size)
        {
            LoadRegisterPointer(index);

            if (size == 0)
            {
                il.Emit(OpCodes.Ldind_R4);
            }
            else
            {
                il.Emit(OpCodes.Ldind_R8);
            }
        }

        public void StoreFloat(int index, int size)
        {         
            if (size == 0)
            {
                il.Emit(OpCodes.Stloc,FloatStore);
            }
            else
            {
                il.Emit(OpCodes.Stloc, DoubleStore);
            }

            LoadRegisterPointer(index);

            if (size == 0)
            {
                il.Emit(OpCodes.Ldloc, FloatStore);
                il.Emit(OpCodes.Stind_R4);

                /*
                LoadRegisterPointer(index);
                il.Emit(OpCodes.Ldc_I8,0L);
                il.Emit(OpCodes.Stind_I8);
                */
            }
            else
            {
                il.Emit(OpCodes.Ldloc, DoubleStore);
                il.Emit(OpCodes.Stind_R8);
            }
        }

        public void LoadRegisterPointer(int index)
        {
            //Load Register Pointer
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4, (long)(index * 8));
            il.Emit(OpCodes.Add);
        }

        public void LoadReg(int index)
        {
            LoadRegisterPointer(index);

            il.Emit(OpCodes.Ldind_I8);

            il.Emit(OpCodes.Conv_U8);

            if (CurrentSize == OperationSize.Int32)
                il.Emit(OpCodes.Conv_U4);
        }

        public void LoadReg(int index, int size)
        {
            LoadReg(index);

            if (size == 0)
            {
                il.Emit(OpCodes.Conv_U4);
            }
            else
            {
                il.Emit(OpCodes.Conv_I8);
            }
        }

        public void StoreReg(int index)
        {
            il.Emit(OpCodes.Conv_U8);
            il.Emit(OpCodes.Stloc, PointerStore);

            LoadRegisterPointer(index);

            il.Emit(OpCodes.Ldloc, PointerStore);

            il.Emit(OpCodes.Stind_I8);
        }

        public static CompiledFunction CompileIL(string Name,OperationBlock block)
        {
            Generator generator = new Generator(Name);

            for (int i = 0; i < block.Operations.Count; i++)
            {
                generator.Lables.Add(generator.il.DefineLabel());
            }

            for (int i = 0; i < block.Operations.Count; i++)
            {
                generator.il.MarkLabel(generator.Lables[i]);

                generator.CurrentSize = block.Operations[i].Size;

                InterpreterFunctions.Generation[(int)block.Operations[i].Name](block.Operations[i],generator);
            }

            /*
            generator.il.Emit(OpCodes.Ldc_I4,100);

            generator.StoreReg(0);

            generator.LoadReg(0);

            generator.StoreReg(10);
            */

            generator.il.Emit(OpCodes.Ldloc,generator.ReturnLocal);
            generator.il.Emit(OpCodes.Ret);

            return (CompiledFunction)generator.method.CreateDelegate(typeof(CompiledFunction));
        }
    }
}
