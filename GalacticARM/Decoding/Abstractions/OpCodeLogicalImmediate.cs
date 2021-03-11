using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeLogicalImmediate : AOpCode 
    {
        public int sf       {get; private set;}
        public int opc      {get; private set;}
        public int n        {get; private set;}
        public int immr     {get; private set;}
        public int imms     {get; private set;}
        public int rn       {get; private set;}
        public int rd       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeLogicalImmediate Out = new OpCodeLogicalImmediate();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            sf = (RawOpCode >> 31) & 0x1;
            opc = (RawOpCode >> 29) & 0x3;
            n = (RawOpCode >> 22) & 0x1;
            immr = (RawOpCode >> 16) & 0x3F;
            imms = (RawOpCode >> 10) & 0x3F;
            rn = (RawOpCode >> 5) & 0x1F;
            rd = (RawOpCode >> 0) & 0x1F;
        }
    }
}
