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

        public OperationSize CurrentSize { get; set; }

        public int ReturnLocal { get; set; }
        public int PointerStore { get; set; }

        public int FloatStore               { get; set; }
        public int DoubleStore              { get; set; }

        public Operation CurrentOperation   { get; set; }
        public OperationBlock CurrentBlock  { get; set; }

        public Dictionary<int,int> LocalMap { get; set; }

        public unsafe Generator(string name)
        {
            method = new DynamicMethod(name, typeof(ulong), new Type[] { typeof(ContextBlock*) });

            il = method.GetILGenerator();

            ReturnLocal = il.DeclareLocal(typeof(ulong)).LocalIndex;
            PointerStore = il.DeclareLocal(typeof(ulong)).LocalIndex;

            FloatStore = il.DeclareLocal(typeof(float)).LocalIndex;
            DoubleStore = il.DeclareLocal(typeof(double)).LocalIndex;

            Lables = new List<System.Reflection.Emit.Label>();
        }

        public void LoadFloat(int index, int size)
        {
            LoadRegRaw(index);

            if (size == 0)
            {          
                il.Emit(OpCodes.Call,typeof(Generator).GetMethod(nameof(ToFloat)));
            }
            else
            {
                il.Emit(OpCodes.Call, typeof(Generator).GetMethod(nameof(ToDouble)));
            }
        }

        public void StoreFloat(int index, int size)
        {
            if (size == 0)
            {
                il.Emit(OpCodes.Call,typeof(Generator).GetMethod(nameof(ToUint)));

                il.Emit(OpCodes.Conv_U4);
            }
            else
            {
                il.Emit(OpCodes.Call, typeof(Generator).GetMethod(nameof(ToUlong)));
            }

            StoreDataRaw(index);
        }

        public void LoadRegisterPointer(int index)
        {
            //Load Register Pointer
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4, (long)(index * 8));
            il.Emit(OpCodes.Add);
        }

        public void LoadRegFromMem(int index)
        {
            LoadRegisterPointer(index);

            il.Emit(OpCodes.Ldind_I8);

            il.Emit(OpCodes.Conv_U8);

            if (CurrentSize == OperationSize.Int32)
                il.Emit(OpCodes.Conv_U4);
        }

        public void LoadData(int index)
        {
            Operand op = CurrentOperation.Arguments[index];

            if (op.Type == Intermediate.OperandType.Register)
            {
                LoadRegRaw(op.Reg);
            }
            else
            {
                if (CurrentOperation.Size == OperationSize.Int32)
                {
                    il.Emit(OpCodes.Ldc_I4, (int)(uint)op.Imm);
                    il.Emit(OpCodes.Conv_U4);
                }
                else
                {
                    il.Emit(OpCodes.Ldc_I8, (long)op.Imm);
                    il.Emit(OpCodes.Conv_U8);
                }
            }
        }

        public void StoreData(int index)
        {
            Operand op = CurrentOperation.Arguments[index];

            StoreDataRaw(op.Reg);
        }

        public void StoreRegToMem(int index)
        {
            il.Emit(OpCodes.Conv_U8);
            il.Emit(OpCodes.Stloc, PointerStore);

            LoadRegisterPointer(index);

            il.Emit(OpCodes.Ldloc, PointerStore);

            il.Emit(OpCodes.Stind_I8);
        }

        public void StoreDataRaw(int index)
        {
            StoreRegToMem(index);
        }

        public void LoadRegRaw(int index)
        {
            LoadRegFromMem(index);
        }

        public static CompiledFunction CompileIL(string Name,OperationBlock block)
        {
            Generator generator = new Generator(Name);

            generator.CurrentBlock = block;

            for (int i = 0; i < block.Operations.Count; i++)
            {
                generator.Lables.Add(generator.il.DefineLabel());
            }

            for (int i = 0; i < block.Operations.Count; i++)
            {
                generator.il.MarkLabel(generator.Lables[i]);

                generator.CurrentSize = block.Operations[i].Size;

                generator.CurrentOperation = block.Operations[i];

                InterpreterFunctions.Generation[(int)block.Operations[i].Name](block.Operations[i],generator);
            }

            generator.il.Emit(OpCodes.Ldloc,generator.ReturnLocal);
            generator.il.Emit(OpCodes.Ret);

            return (CompiledFunction)generator.method.CreateDelegate(typeof(CompiledFunction));
        }

        public static unsafe float ToFloat(uint i)
        {
            return *(float*)&i;
        }

        public static unsafe double ToDouble(ulong i)
        {
            return *(double*)&i;
        }

        public static unsafe uint ToUint(float i)
        {
            return *(uint*)&i;
        }

        public static unsafe ulong ToUlong(double i)
        {
            return *(ulong*)&i;
        }
    }
}
