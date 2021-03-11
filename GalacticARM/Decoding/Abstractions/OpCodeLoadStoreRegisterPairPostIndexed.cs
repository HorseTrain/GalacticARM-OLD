using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeLoadStoreRegisterPairPostIndexed : AOpCode 
    {
        public int opc      {get; private set;}
        public int v        {get; private set;}
        public int l        {get; private set;}
        public int imm7     {get; private set;}
        public int rt2      {get; private set;}
        public int rn       {get; private set;}
        public int rt       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeLoadStoreRegisterPairPostIndexed Out = new OpCodeLoadStoreRegisterPairPostIndexed();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            opc = (RawOpCode >> 30) & 0x3;
            v = (RawOpCode >> 26) & 0x1;
            l = (RawOpCode >> 22) & 0x1;
            imm7 = (RawOpCode >> 15) & 0x7F;
            rt2 = (RawOpCode >> 10) & 0x1F;
            rn = (RawOpCode >> 5) & 0x1F;
            rt = (RawOpCode >> 0) & 0x1F;
        }
    }
}
