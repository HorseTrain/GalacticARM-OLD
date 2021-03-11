using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeMoveWideImmediate : AOpCode 
    {
        public int sf       {get; private set;}
        public int opc      {get; private set;}
        public int hw       {get; private set;}
        public int imm16    {get; private set;}
        public int rd       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeMoveWideImmediate Out = new OpCodeMoveWideImmediate();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            sf = (RawOpCode >> 31) & 0x1;
            opc = (RawOpCode >> 29) & 0x3;
            hw = (RawOpCode >> 21) & 0x3;
            imm16 = (RawOpCode >> 5) & 0xFFFF;
            rd = (RawOpCode >> 0) & 0x1F;
        }
    }
}
