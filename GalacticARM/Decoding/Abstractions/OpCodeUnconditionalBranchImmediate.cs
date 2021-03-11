using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeUnconditionalBranchImmediate : AOpCode 
    {
        public int imm26    {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeUnconditionalBranchImmediate Out = new OpCodeUnconditionalBranchImmediate();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            imm26 = (RawOpCode >> 0) & 0x3FFFFFF;
        }
    }
}
