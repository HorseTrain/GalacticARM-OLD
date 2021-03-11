using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeLoadStoreRegisterUnsignedImmediate : AOpCode 
    {
        public int size     {get; private set;}
        public int v        {get; private set;}
        public int opc      {get; private set;}
        public int imm12    {get; private set;}
        public int rn       {get; private set;}
        public int rt       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeLoadStoreRegisterUnsignedImmediate Out = new OpCodeLoadStoreRegisterUnsignedImmediate();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            size = (RawOpCode >> 30) & 0x3;
            v = (RawOpCode >> 26) & 0x1;
            opc = (RawOpCode >> 22) & 0x3;
            imm12 = (RawOpCode >> 10) & 0xFFF;
            rn = (RawOpCode >> 5) & 0x1F;
            rt = (RawOpCode >> 0) & 0x1F;
        }
    }
}
