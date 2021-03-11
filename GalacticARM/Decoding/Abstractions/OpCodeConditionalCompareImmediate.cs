using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeConditionalCompareImmediate : AOpCode 
    {
        public int sf       {get; private set;}
        public int op       {get; private set;}
        public int s        {get; private set;}
        public int imm5     {get; private set;}
        public int cond     {get; private set;}
        public int o2       {get; private set;}
        public int rn       {get; private set;}
        public int o3       {get; private set;}
        public int nzcv     {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeConditionalCompareImmediate Out = new OpCodeConditionalCompareImmediate();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            sf = (RawOpCode >> 31) & 0x1;
            op = (RawOpCode >> 30) & 0x1;
            s = (RawOpCode >> 29) & 0x1;
            imm5 = (RawOpCode >> 16) & 0x1F;
            cond = (RawOpCode >> 12) & 0xF;
            o2 = (RawOpCode >> 10) & 0x1;
            rn = (RawOpCode >> 5) & 0x1F;
            o3 = (RawOpCode >> 4) & 0x1;
            nzcv = (RawOpCode >> 0) & 0xF;
        }
    }
}
