using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeConditionalSelect : AOpCode 
    {
        public int sf       {get; private set;}
        public int op       {get; private set;}
        public int s        {get; private set;}
        public int rm       {get; private set;}
        public int cond     {get; private set;}
        public int op2      {get; private set;}
        public int rn       {get; private set;}
        public int rd       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeConditionalSelect Out = new OpCodeConditionalSelect();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            sf = (RawOpCode >> 31) & 0x1;
            op = (RawOpCode >> 30) & 0x1;
            s = (RawOpCode >> 29) & 0x1;
            rm = (RawOpCode >> 16) & 0x1F;
            cond = (RawOpCode >> 12) & 0xF;
            op2 = (RawOpCode >> 10) & 0x3;
            rn = (RawOpCode >> 5) & 0x1F;
            rd = (RawOpCode >> 0) & 0x1F;
        }
    }
}
