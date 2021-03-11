using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeConditionalBranchImmediate : AOpCode 
    {
        public int imm19    {get; private set;}
        public int cond     {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeConditionalBranchImmediate Out = new OpCodeConditionalBranchImmediate();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            imm19 = (RawOpCode >> 5) & 0x7FFFF;
            cond = (RawOpCode >> 0) & 0xF;
        }
    }
}
