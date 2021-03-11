using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeFloatingPointConditionalSelect : AOpCode 
    {
        public int m        {get; private set;}
        public int s        {get; private set;}
        public int type     {get; private set;}
        public int rm       {get; private set;}
        public int cond     {get; private set;}
        public int rn       {get; private set;}
        public int rd       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeFloatingPointConditionalSelect Out = new OpCodeFloatingPointConditionalSelect();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            m = (RawOpCode >> 31) & 0x1;
            s = (RawOpCode >> 29) & 0x1;
            type = (RawOpCode >> 22) & 0x3;
            rm = (RawOpCode >> 16) & 0x1F;
            cond = (RawOpCode >> 12) & 0xF;
            rn = (RawOpCode >> 5) & 0x1F;
            rd = (RawOpCode >> 0) & 0x1F;
        }
    }
}
