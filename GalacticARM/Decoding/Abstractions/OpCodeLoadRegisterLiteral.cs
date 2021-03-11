using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeLoadRegisterLiteral : AOpCode 
    {
        public int imm19    {get; private set;}
        public int rt       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeLoadRegisterLiteral Out = new OpCodeLoadRegisterLiteral();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            imm19 = (RawOpCode >> 5) & 0x7FFFF;
            rt = (RawOpCode >> 0) & 0x1F;
        }
    }
}
