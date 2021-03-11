using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeExtract : AOpCode 
    {
        public int sf       {get; private set;}
        public int op21     {get; private set;}
        public int n        {get; private set;}
        public int o0       {get; private set;}
        public int rm       {get; private set;}
        public int imms     {get; private set;}
        public int rn       {get; private set;}
        public int rd       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeExtract Out = new OpCodeExtract();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            sf = (RawOpCode >> 31) & 0x1;
            op21 = (RawOpCode >> 29) & 0x3;
            n = (RawOpCode >> 22) & 0x1;
            o0 = (RawOpCode >> 21) & 0x1;
            rm = (RawOpCode >> 16) & 0x1F;
            imms = (RawOpCode >> 10) & 0x3F;
            rn = (RawOpCode >> 5) & 0x1F;
            rd = (RawOpCode >> 0) & 0x1F;
        }
    }
}
