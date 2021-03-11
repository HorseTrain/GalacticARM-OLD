using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeDataProcessing2Source : AOpCode 
    {
        public int sf       {get; private set;}
        public int s        {get; private set;}
        public int rm       {get; private set;}
        public int opcode   {get; private set;}
        public int rn       {get; private set;}
        public int rd       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeDataProcessing2Source Out = new OpCodeDataProcessing2Source();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            sf = (RawOpCode >> 31) & 0x1;
            s = (RawOpCode >> 29) & 0x1;
            rm = (RawOpCode >> 16) & 0x1F;
            opcode = (RawOpCode >> 10) & 0x3F;
            rn = (RawOpCode >> 5) & 0x1F;
            rd = (RawOpCode >> 0) & 0x1F;
        }
    }
}
