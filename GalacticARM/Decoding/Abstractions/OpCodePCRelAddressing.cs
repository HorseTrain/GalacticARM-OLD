using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodePCRelAddressing : AOpCode 
    {
        public int op       {get; private set;}
        public int immlo    {get; private set;}
        public int immhi    {get; private set;}
        public int rd       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodePCRelAddressing Out = new OpCodePCRelAddressing();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            op = (RawOpCode >> 31) & 0x1;
            immlo = (RawOpCode >> 29) & 0x3;
            immhi = (RawOpCode >> 5) & 0x7FFFF;
            rd = (RawOpCode >> 0) & 0x1F;
        }
    }
}
