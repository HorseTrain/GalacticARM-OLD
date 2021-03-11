using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeExceptionGeneration : AOpCode 
    {
        public int imm16    {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeExceptionGeneration Out = new OpCodeExceptionGeneration();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            imm16 = (RawOpCode >> 5) & 0xFFFF;
        }
    }
}
