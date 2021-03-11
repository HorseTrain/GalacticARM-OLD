using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeFloatingPointDataProcessing3Source : AOpCode 
    {
        public int m        {get; private set;}
        public int s        {get; private set;}
        public int type     {get; private set;}
        public int o1       {get; private set;}
        public int rm       {get; private set;}
        public int o0       {get; private set;}
        public int ra       {get; private set;}
        public int rn       {get; private set;}
        public int rd       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeFloatingPointDataProcessing3Source Out = new OpCodeFloatingPointDataProcessing3Source();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            m = (RawOpCode >> 31) & 0x1;
            s = (RawOpCode >> 29) & 0x1;
            type = (RawOpCode >> 22) & 0x3;
            o1 = (RawOpCode >> 21) & 0x1;
            rm = (RawOpCode >> 16) & 0x1F;
            o0 = (RawOpCode >> 15) & 0x1;
            ra = (RawOpCode >> 10) & 0x1F;
            rn = (RawOpCode >> 5) & 0x1F;
            rd = (RawOpCode >> 0) & 0x1F;
        }
    }
}
