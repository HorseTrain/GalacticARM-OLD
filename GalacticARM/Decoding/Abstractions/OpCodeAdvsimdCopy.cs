using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeAdvsimdCopy : AOpCode 
    {
        public int q        {get; private set;}
        public int op       {get; private set;}
        public int imm5     {get; private set;}
        public int imm4     {get; private set;}
        public int rn       {get; private set;}
        public int rd       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeAdvsimdCopy Out = new OpCodeAdvsimdCopy();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            q = (RawOpCode >> 30) & 0x1;
            op = (RawOpCode >> 29) & 0x1;
            imm5 = (RawOpCode >> 16) & 0x1F;
            imm4 = (RawOpCode >> 11) & 0xF;
            rn = (RawOpCode >> 5) & 0x1F;
            rd = (RawOpCode >> 0) & 0x1F;
        }
    }
}
