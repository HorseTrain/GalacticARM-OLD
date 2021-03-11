using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeAddSubtractShiftedRegister : AOpCode 
    {
        public int sf       {get; private set;}
        public int op       {get; private set;}
        public int s        {get; private set;}
        public int shift    {get; private set;}
        public int rm       {get; private set;}
        public int imm6     {get; private set;}
        public int rn       {get; private set;}
        public int rd       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeAddSubtractShiftedRegister Out = new OpCodeAddSubtractShiftedRegister();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            sf = (RawOpCode >> 31) & 0x1;
            op = (RawOpCode >> 30) & 0x1;
            s = (RawOpCode >> 29) & 0x1;
            shift = (RawOpCode >> 22) & 0x3;
            rm = (RawOpCode >> 16) & 0x1F;
            imm6 = (RawOpCode >> 10) & 0x3F;
            rn = (RawOpCode >> 5) & 0x1F;
            rd = (RawOpCode >> 0) & 0x1F;
        }
    }
}
