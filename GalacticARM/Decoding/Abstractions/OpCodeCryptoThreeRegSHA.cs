using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeCryptoThreeRegSHA : AOpCode 
    {
        public int size     {get; private set;}
        public int rm       {get; private set;}
        public int opcode   {get; private set;}
        public int rn       {get; private set;}
        public int rd       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeCryptoThreeRegSHA Out = new OpCodeCryptoThreeRegSHA();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            size = (RawOpCode >> 22) & 0x3;
            rm = (RawOpCode >> 16) & 0x1F;
            opcode = (RawOpCode >> 12) & 0x7;
            rn = (RawOpCode >> 5) & 0x1F;
            rd = (RawOpCode >> 0) & 0x1F;
        }
    }
}
