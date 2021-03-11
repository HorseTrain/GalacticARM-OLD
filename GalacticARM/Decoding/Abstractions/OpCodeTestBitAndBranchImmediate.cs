using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeTestBitAndBranchImmediate : AOpCode 
    {
        public int b5       {get; private set;}
        public int b40      {get; private set;}
        public int imm14    {get; private set;}
        public int rt       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeTestBitAndBranchImmediate Out = new OpCodeTestBitAndBranchImmediate();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            b5 = (RawOpCode >> 31) & 0x1;
            b40 = (RawOpCode >> 19) & 0x1F;
            imm14 = (RawOpCode >> 5) & 0x3FFF;
            rt = (RawOpCode >> 0) & 0x1F;
        }
    }
}
