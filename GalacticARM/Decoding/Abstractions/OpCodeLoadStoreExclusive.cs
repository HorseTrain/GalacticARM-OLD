using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeLoadStoreExclusive : AOpCode 
    {
        public int rs       {get; private set;}
        public int rt2      {get; private set;}
        public int rn       {get; private set;}
        public int rt       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeLoadStoreExclusive Out = new OpCodeLoadStoreExclusive();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            rs = (RawOpCode >> 16) & 0x1F;
            rt2 = (RawOpCode >> 10) & 0x1F;
            rn = (RawOpCode >> 5) & 0x1F;
            rt = (RawOpCode >> 0) & 0x1F;
        }
    }
}
