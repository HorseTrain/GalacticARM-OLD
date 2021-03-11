using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeFloatingPointDataProcessing1Source : AOpCode 
    {
        public int m        {get; private set;}
        public int s        {get; private set;}
        public int type     {get; private set;}
        public int opcode   {get; private set;}
        public int rn       {get; private set;}
        public int rd       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeFloatingPointDataProcessing1Source Out = new OpCodeFloatingPointDataProcessing1Source();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            m = (RawOpCode >> 31) & 0x1;
            s = (RawOpCode >> 29) & 0x1;
            type = (RawOpCode >> 22) & 0x3;
            opcode = (RawOpCode >> 15) & 0x3F;
            rn = (RawOpCode >> 5) & 0x1F;
            rd = (RawOpCode >> 0) & 0x1F;
        }
    }
}
