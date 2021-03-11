using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeAdvsimdScalarXIndexedElement : AOpCode 
    {
        public int u        {get; private set;}
        public int size     {get; private set;}
        public int l        {get; private set;}
        public int m        {get; private set;}
        public int rm       {get; private set;}
        public int opcode   {get; private set;}
        public int h        {get; private set;}
        public int rn       {get; private set;}
        public int rd       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeAdvsimdScalarXIndexedElement Out = new OpCodeAdvsimdScalarXIndexedElement();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            u = (RawOpCode >> 29) & 0x1;
            size = (RawOpCode >> 22) & 0x3;
            l = (RawOpCode >> 21) & 0x1;
            m = (RawOpCode >> 20) & 0x1;
            rm = (RawOpCode >> 16) & 0xF;
            opcode = (RawOpCode >> 12) & 0xF;
            h = (RawOpCode >> 11) & 0x1;
            rn = (RawOpCode >> 5) & 0x1F;
            rd = (RawOpCode >> 0) & 0x1F;
        }
    }
}
