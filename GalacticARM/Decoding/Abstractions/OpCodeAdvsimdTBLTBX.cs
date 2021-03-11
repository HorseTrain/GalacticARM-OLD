using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeAdvsimdTBLTBX : AOpCode 
    {
        public int q        {get; private set;}
        public int op2      {get; private set;}
        public int rm       {get; private set;}
        public int len      {get; private set;}
        public int op       {get; private set;}
        public int rn       {get; private set;}
        public int rd       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeAdvsimdTBLTBX Out = new OpCodeAdvsimdTBLTBX();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            q = (RawOpCode >> 30) & 0x1;
            op2 = (RawOpCode >> 22) & 0x3;
            rm = (RawOpCode >> 16) & 0x1F;
            len = (RawOpCode >> 13) & 0x3;
            op = (RawOpCode >> 12) & 0x1;
            rn = (RawOpCode >> 5) & 0x1F;
            rd = (RawOpCode >> 0) & 0x1F;
        }
    }
}
