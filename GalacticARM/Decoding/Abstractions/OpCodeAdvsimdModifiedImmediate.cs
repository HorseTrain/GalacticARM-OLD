using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeAdvsimdModifiedImmediate : AOpCode 
    {
        public int q        {get; private set;}
        public int op       {get; private set;}
        public int a        {get; private set;}
        public int b        {get; private set;}
        public int c        {get; private set;}
        public int cmode    {get; private set;}
        public int o2       {get; private set;}
        public int d        {get; private set;}
        public int e        {get; private set;}
        public int f        {get; private set;}
        public int g        {get; private set;}
        public int h        {get; private set;}
        public int rd       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeAdvsimdModifiedImmediate Out = new OpCodeAdvsimdModifiedImmediate();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            q = (RawOpCode >> 30) & 0x1;
            op = (RawOpCode >> 29) & 0x1;
            a = (RawOpCode >> 18) & 0x1;
            b = (RawOpCode >> 17) & 0x1;
            c = (RawOpCode >> 16) & 0x1;
            cmode = (RawOpCode >> 12) & 0xF;
            o2 = (RawOpCode >> 11) & 0x1;
            d = (RawOpCode >> 9) & 0x1;
            e = (RawOpCode >> 8) & 0x1;
            f = (RawOpCode >> 7) & 0x1;
            g = (RawOpCode >> 6) & 0x1;
            h = (RawOpCode >> 5) & 0x1;
            rd = (RawOpCode >> 0) & 0x1F;
        }
    }
}
