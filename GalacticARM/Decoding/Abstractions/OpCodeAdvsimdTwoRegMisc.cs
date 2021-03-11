using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeAdvsimdTwoRegMisc : AOpCode 
    {
        public int q        {get; private set;}
        public int u        {get; private set;}
        public int size     {get; private set;}
        public int opcode   {get; private set;}
        public int rn       {get; private set;}
        public int rd       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeAdvsimdTwoRegMisc Out = new OpCodeAdvsimdTwoRegMisc();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            q = (RawOpCode >> 30) & 0x1;
            u = (RawOpCode >> 29) & 0x1;
            size = (RawOpCode >> 22) & 0x3;
            opcode = (RawOpCode >> 12) & 0x1F;
            rn = (RawOpCode >> 5) & 0x1F;
            rd = (RawOpCode >> 0) & 0x1F;
        }
    }
}
