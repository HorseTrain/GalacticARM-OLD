using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeFloatingPointIntegerConversions : AOpCode 
    {
        public int sf       {get; private set;}
        public int s        {get; private set;}
        public int type     {get; private set;}
        public int rmode    {get; private set;}
        public int opcode   {get; private set;}
        public int rn       {get; private set;}
        public int rd       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeFloatingPointIntegerConversions Out = new OpCodeFloatingPointIntegerConversions();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            sf = (RawOpCode >> 31) & 0x1;
            s = (RawOpCode >> 29) & 0x1;
            type = (RawOpCode >> 22) & 0x3;
            rmode = (RawOpCode >> 19) & 0x3;
            opcode = (RawOpCode >> 16) & 0x7;
            rn = (RawOpCode >> 5) & 0x1F;
            rd = (RawOpCode >> 0) & 0x1F;
        }
    }
}
