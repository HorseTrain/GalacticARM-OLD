using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeUnconditionalBranchRegister : AOpCode 
    {
        public int opc      {get; private set;}
        public int op2      {get; private set;}
        public int op3      {get; private set;}
        public int rn       {get; private set;}
        public int op4      {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeUnconditionalBranchRegister Out = new OpCodeUnconditionalBranchRegister();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            opc = (RawOpCode >> 21) & 0xF;
            op2 = (RawOpCode >> 16) & 0x1F;
            op3 = (RawOpCode >> 10) & 0x3F;
            rn = (RawOpCode >> 5) & 0x1F;
            op4 = (RawOpCode >> 0) & 0x1F;
        }
    }
}
