using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeFloatingPointCompare : AOpCode 
    {
        public int m        {get; private set;}
        public int s        {get; private set;}
        public int type     {get; private set;}
        public int rm       {get; private set;}
        public int op       {get; private set;}
        public int rn       {get; private set;}
        public int opcode2  {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeFloatingPointCompare Out = new OpCodeFloatingPointCompare();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            m = (RawOpCode >> 31) & 0x1;
            s = (RawOpCode >> 29) & 0x1;
            type = (RawOpCode >> 22) & 0x3;
            rm = (RawOpCode >> 16) & 0x1F;
            op = (RawOpCode >> 14) & 0x3;
            rn = (RawOpCode >> 5) & 0x1F;
            opcode2 = (RawOpCode >> 0) & 0x1F;
        }
    }
}
