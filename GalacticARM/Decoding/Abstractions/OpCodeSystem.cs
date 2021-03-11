using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    public class OpCodeSystem : AOpCode 
    {
        public int Rt { get; private set; }
        public int Op2 { get; private set; }
        public int CRm { get; private set; }
        public int CRn { get; private set; }
        public int Op1 { get; private set; }
        public int Op0 { get; private set; }

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeSystem Out = new OpCodeSystem();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            Rt = (RawOpCode >> 0) & 0x1f;
            Op2 = (RawOpCode >> 5) & 0x7;
            CRm = (RawOpCode >> 8) & 0xf;
            CRn = (RawOpCode >> 12) & 0xf;
            Op1 = (RawOpCode >> 16) & 0x7;
            Op0 = ((RawOpCode >> 19) & 0x1) | 2;
        }
    }
}
