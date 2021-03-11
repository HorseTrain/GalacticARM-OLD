using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeDataProcessing3Source : AOpCode 
    {
        public int sf       {get; private set;}
        public int op54     {get; private set;}
        public int op31     {get; private set;}
        public int rm       {get; private set;}
        public int o0       {get; private set;}
        public int ra       {get; private set;}
        public int rn       {get; private set;}
        public int rd       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeDataProcessing3Source Out = new OpCodeDataProcessing3Source();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            sf = (RawOpCode >> 31) & 0x1;
            op54 = (RawOpCode >> 29) & 0x3;
            op31 = (RawOpCode >> 21) & 0x7;
            rm = (RawOpCode >> 16) & 0x1F;
            o0 = (RawOpCode >> 15) & 0x1;
            ra = (RawOpCode >> 10) & 0x1F;
            rn = (RawOpCode >> 5) & 0x1F;
            rd = (RawOpCode >> 0) & 0x1F;
        }
    }
}
