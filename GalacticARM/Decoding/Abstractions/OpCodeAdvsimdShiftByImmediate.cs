using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeAdvsimdShiftByImmediate : AOpCode 
    {
        public int q        {get; private set;}
        public int u        {get; private set;}
        public int immh     {get; private set;}
        public int immb     {get; private set;}
        public int opcode   {get; private set;}
        public int rn       {get; private set;}
        public int rd       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeAdvsimdShiftByImmediate Out = new OpCodeAdvsimdShiftByImmediate();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            q = (RawOpCode >> 30) & 0x1;
            u = (RawOpCode >> 29) & 0x1;
            immh = (RawOpCode >> 19) & 0xF;
            immb = (RawOpCode >> 16) & 0x7;
            opcode = (RawOpCode >> 11) & 0x1F;
            rn = (RawOpCode >> 5) & 0x1F;
            rd = (RawOpCode >> 0) & 0x1F;
        }
    }
}
