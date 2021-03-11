using GalacticARM.CodeGen.Intermediate;
using GalacticARM.Decoding;
using GalacticARM.Decoding.Abstractions;
using GalacticARM.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.AEmit
{
    public static class EmitMemory
    {
        public static void LoadStore(InstructionEmitContext context)
        {
            context.SetSize(InstructionInfo.X);

            if (context.CurrentOpCode.Name == InstructionMnemonic.PRFM)
                return;

            bool IsSimD = ((context.CurrentOpCode.RawOpCode >> 26) & 1) == 1;

            if (!IsSimD)
            {
                if (context.CurrentOpCode.Name.ToString().StartsWith("LD"))
                    Load(context);
                else if (context.CurrentOpCode.Name.ToString().StartsWith("ST"))
                    Store(context);
                else
                    context.ThrowUnknown();
            }
            else
            {
                if (context.CurrentOpCode.Name.ToString().StartsWith("LD"))
                    LoadSimd(context);
                else if (context.CurrentOpCode.Name.ToString().StartsWith("ST"))
                    StoreSimd(context);
                else
                    context.ThrowUnknown();
            }
        }

        public static void LoadStorePair(InstructionEmitContext context)
        {
            context.SetSize(InstructionInfo.X);

            bool IsSimD = ((context.CurrentOpCode.RawOpCode >> 26) & 1) == 1;

            if (!IsSimD)
            {
                if (context.CurrentOpCode.Name.ToString().StartsWith("LD"))
                    LoadPair(context);
                else if (context.CurrentOpCode.Name.ToString().StartsWith("ST"))
                    StorePair(context);
                else
                    context.ThrowUnknown();
            }
            else
            {
                if (context.CurrentOpCode.Name.ToString().StartsWith("LD"))
                    LoadPairSimd(context);
                else if (context.CurrentOpCode.Name.ToString().StartsWith("ST"))
                    StorePairSimd(context);
                else
                    context.ThrowUnknown();
            }
        }

        public static unsafe Operand GetPhysicalAddress(InstructionEmitContext context, Operand VirtualAddress)
        {
            Operand PageTablePointer = context.GetMisc(MiscRegister.PageTablePointer);

            Operand Index = context.ShiftRight(VirtualAddress,context.Const(MemoryManager.PageBit));
            Operand Offset = context.And(VirtualAddress,context.Const(MemoryManager.PageMask));

            return context.Add(context.LoadMem(context.Local(),context.Add(PageTablePointer, context.ShiftLeft(Index, context.Const(3)))),Offset);
        }

        public static int GetSize(InstructionEmitContext context)
        {
            InstructionMnemonic name = (InstructionMnemonic)Enum.Parse(typeof(InstructionMnemonic),context.CurrentOpCode.Name.ToString().Replace("U",""));

            switch (name)
            {
                case InstructionMnemonic.LDRB: return 0;
                case InstructionMnemonic.LDRH: return 1;
                case InstructionMnemonic.LDR:

                    if (context.CurrentOpCode.Info == InstructionInfo.W)
                        return 2;
                    else
                        return 3;

                case InstructionMnemonic.STRB: return 0;
                case InstructionMnemonic.STRH: return 1;
                case InstructionMnemonic.STR:

                    if (context.CurrentOpCode.Info == InstructionInfo.W)
                        return 2;
                    else
                        return 3;

                case InstructionMnemonic.LDRSB: return 0;
                case InstructionMnemonic.LDRSH: return 1;
                case InstructionMnemonic.LDRSW: return 2;

                default: throw new NotImplementedException();
            }
        }

        public static Operand GetT(InstructionEmitContext context)
        {
            return context.GetRegister(GetTIndex(context));
        }

        public static int GetTIndex(InstructionEmitContext context) => context.CurrentOpCode.RawOpCode & 0x1f;
        public static int GetT2Index(InstructionEmitContext context) => (context.CurrentOpCode.RawOpCode >> 10) & 0x1f;

        public static Operand ExtendLoad(InstructionEmitContext context, Operand Load)
        {
            InstructionMnemonic name = (InstructionMnemonic)Enum.Parse(typeof(InstructionMnemonic), context.CurrentOpCode.Name.ToString().Replace("U", ""));

            Operand ExtendSigned(Operand load)
            {
                if (context.CurrentOpCode.Info == InstructionInfo.W)
                    return context.And(load,context.Const(uint.MaxValue));

                return load;
            }

            switch (name)
            {
                case InstructionMnemonic.LDRB: return context.And(Load,context.Const((ulong)byte.MaxValue));
                case InstructionMnemonic.LDRH: return context.And(Load, context.Const((ulong)ushort.MaxValue));
                case InstructionMnemonic.LDR:

                    if (context.CurrentOpCode.Info == InstructionInfo.W)
                        return context.And(Load, context.Const(uint.MaxValue));
                    else
                        return Load;

                case InstructionMnemonic.LDRSB: return ExtendSigned(context.SignExtend8(context.And(Load, context.Const((ulong)byte.MaxValue))));
                case InstructionMnemonic.LDRSH: return ExtendSigned(context.SignExtend16(context.And(Load, context.Const((ulong)ushort.MaxValue))));
                case InstructionMnemonic.LDRSW: return ExtendSigned(context.SignExtend32(context.And(Load, context.Const(uint.MaxValue))));
                default: throw new NotImplementedException();
            }
        }

        public static Operand GetPointerReg(InstructionEmitContext context)
        {
            return context.GetRegister((context.CurrentOpCode.RawOpCode >> 5) & 0x1f,true);
        }

        public static int GetRt(InstructionEmitContext context)
        {
            return context.CurrentOpCode.RawOpCode & 0x1f;
        }

        public static Operand GetAddress(InstructionEmitContext context, bool IsSimd = false)
        {
            Operand Out;

            Operand BasePointer = GetPointerReg(context);

            switch (context.CurrentOpCode)
            {
                case OpCodeLoadStoreRegisterUnsignedImmediate op: 
                    
                    if (!IsSimd)
                    {
                        Out = context.Add(BasePointer, context.Const(op.imm12 << GetSize(context)));
                    }
                    else
                    {
                        Out = context.Add(BasePointer, context.Const(op.imm12 << GetSimdSize(context)));
                    }
                    
                    break;
                case OpCodeLoadStoreRegisterUnscaledImmediate op: Out = context.Add(BasePointer, context.Const((op.imm9 << 55) >> 55)); break;
                case OpCodeLoadStoreRegisterImmediatePreIndexed op:

                    Operand imm = context.Const((op.imm9 << 55) >> 55);

                    BasePointer = context.Add(BasePointer, imm);

                    context.SetRegister(op.rn,BasePointer,true);

                    Out = BasePointer;

                    break;

                case OpCodeLoadStoreRegisterImmediatePostIndexed op:

                    imm = context.Const((op.imm9 << 55) >> 55);

                    context.SetRegister(op.rn, context.Add(BasePointer, imm), true);

                    Out = BasePointer;

                    break;

                case OpCodeLoadStoreRegisterPairOffset op:

                    ulong Imm = 0;
                    
                    if (!IsSimd)
                    {
                        Imm = ((ulong)(((long)op.imm7 << 57) >> 57)) << (2 + (op.opc >> 1));
                    }
                    else
                    {
                        Imm = ((ulong)(((long)op.imm7 << 57) >> 57)) << (2 + op.opc);
                    }

                    Out = context.Add(BasePointer, context.Const(Imm));

                    break;

                case OpCodeLoadStoreRegisterPairPreIndexed op:

                    if (!IsSimd)
                    {
                        Imm = ((ulong)(((long)op.imm7 << 57) >> 57)) << (2 + (op.opc >> 1));
                    }
                    else
                    {
                        Imm = ((ulong)(((long)op.imm7 << 57) >> 57)) << (2 + op.opc);
                    }

                    imm = context.Const(Imm);

                    BasePointer = context.Add(BasePointer, imm);

                    context.SetRegister(op.rn, BasePointer, true);

                    Out = BasePointer;

                    break;

                case OpCodeLoadStoreRegisterPairPostIndexed op:

                    if (!IsSimd)
                    {
                        Imm = ((ulong)(((long)op.imm7 << 57) >> 57)) << (2 + (op.opc >> 1));
                    }
                    else
                    {
                        Imm = ((ulong)(((long)op.imm7 << 57) >> 57)) << (2 + op.opc);
                    }

                    imm = context.Const(Imm);

                    context.SetRegister(op.rn, context.Add(BasePointer, imm), true);

                    Out = BasePointer;

                    break;

                case OpCodeLoadStoreRegisterRegisterOffset op:

                    Operand m = EmitALU.Extend(context,context.GetRegister(op.rm),(IntType)op.option);

                    if (op.s == 1)
                    {
                        if (!IsSimd)
                        {
                            m = context.ShiftLeft(m, context.Const(op.size));
                        }
                        else
                        {
                            m = context.ShiftLeft(m, context.Const(4));
                        }
                    }

                    Out = context.Add(BasePointer,m);

                    break;

                default: return context.ThrowUnknown(); 
            }

            return GetPhysicalAddress(context,Out);
        }

        public static void Load(InstructionEmitContext context)
        {
            Operand load = context.LoadMem(GetAddress(context));

            load = ExtendLoad(context,load);

            context.SetRegister(GetRt(context),load,false);
        }

        public static void Store(InstructionEmitContext context)
        {
            context.StoreMem(GetAddress(context),GetT(context), GetSize(context));
        }

        public static void LoadPair(InstructionEmitContext context)
        {
            int datasize = 8 << (2 + ((context.CurrentOpCode.RawOpCode >> 31) & 1));

            Operand Address = GetAddress(context);
            Operand Address1 = context.Add(Address,context.Const(datasize >> 3));

            void LoadReg(int t,Operand Address)
            {
                Operand load = context.LoadMem(Address);

                if (datasize == 32)
                {
                    load = context.And(load,context.Const(uint.MaxValue));

                    if (context.CurrentOpCode.Name == InstructionMnemonic.LDPSW)
                    {
                        load = context.SignExtend32(load);
                    }
                }

                context.SetRegister(t,load);
            }

            LoadReg(GetTIndex(context),Address);
            LoadReg(GetT2Index(context), Address1);
        }

        public static void StorePair(InstructionEmitContext context)
        {
            int datasize = 8 << (2 + ((context.CurrentOpCode.RawOpCode >> 31) & 1));

            Operand Address = GetAddress(context);
            Operand Address1 = context.Add(Address, context.Const(datasize >> 3));

            void StoreReg(int t, Operand Address)
            {
                Operand store = context.GetRegister(t);

                if (datasize == 32)
                {
                    context.StoreMem(Address,store,2);

                    return;
                }

                context.StoreMem(Address, store, 3);
            }

            StoreReg(GetTIndex(context), Address);
            StoreReg(GetT2Index(context), Address1);
        }

        //Simd
        public static int GetSimdSize(InstructionEmitContext context)
        {
            switch (context.CurrentOpCode.Info)
            {
                case InstructionInfo.B: return 0;
                case InstructionInfo.H: return 1;
                case InstructionInfo.S: return 2;
                case InstructionInfo.D: return 3;
                case InstructionInfo.Q: return 4;
                default: throw new NotImplementedException();
            }
        }

        public static void LoadPairSimd(InstructionEmitContext context)
        {
            int datasize = (8 << (2 + ((context.CurrentOpCode.RawOpCode >> 30) & 0b11))) >> 3;
            Operand Address = GetAddress(context,true);
            Operand Address1 = context.Add(Address,context.Const(datasize));

            int Size = GetSimdSize(context);
            
            void LoadReg(int reg, Operand Address)
            {
                if (Size == 4)
                {
                    EmitVector.InsertIntToVector(context, reg, context.LoadMem(Address),3);
                    EmitVector.InsertIntToVector(context, reg, context.LoadMem(context.Add(Address, context.Const(8))), 3, 1);
                }
                else
                {
                    EmitVector.ClearVector(context, reg);
                    EmitVector.InsertIntToVector(context,reg, context.LoadMem(Address), Size);
                }
            }

            LoadReg(GetRt(context),Address);
            LoadReg(GetT2Index(context), Address1);
        }

        public static void StorePairSimd(InstructionEmitContext context)
        {
            int datasize = (8 << (2 + ((context.CurrentOpCode.RawOpCode >> 30) & 0b11))) >> 3;
            Operand Address = GetAddress(context, true);
            Operand Address1 = context.Add(Address, context.Const(datasize));

            int Size = GetSimdSize(context);

            void StoreReg(int reg, Operand Address)
            {
                if (Size == 4)
                {
                    context.StoreMem(Address, EmitVector.LoadIntFromVector(context, reg, 3), 3);
                    context.StoreMem(context.Add(Address,context.Const(8)), EmitVector.LoadIntFromVector(context, reg, 3,1), 3);
                }
                else
                {
                    context.StoreMem(Address,EmitVector.LoadIntFromVector(context,reg,Size),Size);
                }
            }

            StoreReg(GetRt(context),Address);
            StoreReg(GetT2Index(context), Address1);
        }

        public static void LoadSimd(InstructionEmitContext context)
        {
            Operand Address = GetAddress(context,true);

            int Size = GetSimdSize(context);

            Operand ExtendSimd(Operand source)
            {
                switch (Size)
                {
                    case 0: return context.And(source,context.Const((ulong)(byte.MaxValue)));
                    case 1: return context.And(source, context.Const((ulong)(ushort.MaxValue)));
                    case 2: return context.And(source, context.Const((ulong)(uint.MaxValue)));
                    default: return source;
                }
            }

            if (Size == 4)
            {
                Operand Load0 = context.LoadMem(Address);
                Operand Load1 = context.LoadMem(context.Add(Address,context.Const(8)));

                EmitVector.InsertIntToVector(context, GetRt(context),Load0, 3);
                EmitVector.InsertIntToVector(context, GetRt(context), Load1, 3,1);
            }
            else
            {
                Operand Load = ExtendSimd(context.LoadMem(Address));

                EmitVector.ClearVector(context, GetRt(context));
                EmitVector.InsertIntToVector(context, GetRt(context), Load, Size);
            }
        }

        public static void StoreSimd(InstructionEmitContext context)
        {
            Operand Address = GetAddress(context,true);

            int Size = GetSimdSize(context);

            if (Size == 4)
            {
                context.StoreMem(Address, EmitVector.LoadIntFromVector(context, GetRt(context), 3, 0), 3);
                context.StoreMem(context.Add(Address,context.Const(8)), EmitVector.LoadIntFromVector(context, GetRt(context), 3, 1), 3);
            }
            else
            {
                context.StoreMem(Address,EmitVector.LoadIntFromVector(context,GetRt(context),Size,0),Size);
            }
        }

        public static void LoadStoreExclusive(InstructionEmitContext context)
        {
            context.SetSize(InstructionInfo.X);

            OpCodeLoadStoreExclusive opCode = (OpCodeLoadStoreExclusive)context.CurrentOpCode;

            int size = (opCode.RawOpCode >> 30) & 0x3;

            bool o2 = ((opCode.RawOpCode >> 23) & 1) == 1;
            bool l = ((opCode.RawOpCode >> 22) & 1) == 1;
            bool o1 = ((opCode.RawOpCode >> 21) & 1) == 1;
            bool o0 = ((opCode.RawOpCode >> 15) & 1) == 1;

            Operand address = GetPhysicalAddress(context,context.GetRegister(opCode.rn,true));

            if (!o2 && l && !o1 && o0)
            {
                LoadExclusive(context,address,size,true);
            }
            else if (o2 && l && !o1 && o0)
            {
                LoadExclusive(context, address, size, false);
            }
            else if (!o2 && l && !o1 && !o0)
            {
                LoadExclusive(context, address, size, true);
            }
            else if (!o2 && !l && !o1 && o0)
            {
                StoreExclusive(context,address,size,true);
            }
            else if (o2 && !l && !o1 && o0)
            {
                StoreExclusive(context, address, size, false);
            }
            else if (!o2 && !l && !o1 && !o0)
            {
                StoreExclusive(context, address, size, true);
            }
            else
            {
                context.ThrowUnknown();
            }
        }

        public static unsafe void StoreExclusive(InstructionEmitContext context, Operand Address, int size,bool test)
        {
            OpCodeLoadStoreExclusive opCode = (OpCodeLoadStoreExclusive)context.CurrentOpCode;

            if (test)
            {
                Operand TestResult = context.Local();

                context.CallFunctionFromPointer(context.Const((ulong)DelegateCache.GetOrAdd(new _Void_U_U_U(ExclusiveMonitors.TextExclusive))),context.GetContextPointer(),context.Const(TestResult.Reg),Address);

                EmitControlFlow.EmitIF(context, TestResult,
                    
                    delegate()
                    {
                        context.SetRegister(opCode.rs,context.Const(0));

                        context.StoreMem(Address,context.GetRegister(opCode.rt),size);

                        context.CallFunctionFromPointer(context.Const((ulong)DelegateCache.GetOrAdd(new _Void_U(ExclusiveMonitors.Clrex))),context.GetContextPointer());
                    },

                    delegate()
                    {
                        context.SetRegister(opCode.rs,context.Const(1));
                    }

                    );
            }
            else
            {
                context.StoreMem(Address,context.GetRegister(opCode.rt),size);
            }
        }

        public static unsafe void LoadExclusive(InstructionEmitContext context, Operand Address,int size,bool Test)
        {
            OpCodeLoadStoreExclusive opCode = (OpCodeLoadStoreExclusive)context.CurrentOpCode;

            Operand load = context.LoadMem(Address);

            switch (size)
            {
                case 0: load = context.And(load, context.Const((ulong)byte.MaxValue)); break;
                case 1: load = context.And(load, context.Const((ulong)ushort.MaxValue)); break;
                case 2: load = context.And(load, context.Const((ulong)uint.MaxValue)); break;
            }

            context.SetRegister(opCode.rt,load);

            if (Test)
            {
                Operand SetExclusive = context.Const((ulong)DelegateCache.GetOrAdd(new _Void_U_U(ExclusiveMonitors.SetExclusive)));

                context.CallFunctionFromPointer(SetExclusive,context.GetContextPointer(),Address);
            }
        }
    }
}
