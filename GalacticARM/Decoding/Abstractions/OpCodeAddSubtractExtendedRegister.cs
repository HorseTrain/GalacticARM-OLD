using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeAddSubtractExtendedRegister : AOpCode 
    {
        public int sf       {get; private set;}
        public int op       {get; private set;}
        public int s        {get; private set;}
        public int opt      {get; private set;}
        public int rm       {get; private set;}
        public int option   {get; private set;}
        public int imm3     {get; private set;}
        public int rn       {get; private set;}
        public int rd       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeAddSubtractExtendedRegister Out = new OpCodeAddSubtractExtendedRegister();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            sf = (RawOpCode >> 31) & 0x1;
            op = (RawOpCode >> 30) & 0x1;
            s = (RawOpCode >> 29) & 0x1;
            opt = (RawOpCode >> 22) & 0x3;
            rm = (RawOpCode >> 16) & 0x1F;
            option = (RawOpCode >> 13) & 0x7;
            imm3 = (RawOpCode >> 10) & 0x7;
            rn = (RawOpCode >> 5) & 0x1F;
            rd = (RawOpCode >> 0) & 0x1F;
        }
    }
}
