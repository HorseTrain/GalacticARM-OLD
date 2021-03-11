using GalacticARM.CodeGen;

namespace GalacticARM.Decoding.Abstractions
{
    class OpCodeAdvsimdLoadStoreSingleStructurePostIndexed : AOpCode 
    {
        public int q        {get; private set;}
        public int l        {get; private set;}
        public int r        {get; private set;}
        public int rm       {get; private set;}
        public int opcode   {get; private set;}
        public int s        {get; private set;}
        public int size     {get; private set;}
        public int rn       {get; private set;}
        public int rt       {get; private set;}

        public static AOpCode Create(int RawOpCode, ulong Address,InstructionMnemonic Name, InstructionInfo Info, ILEmit Emit)
        {
            OpCodeAdvsimdLoadStoreSingleStructurePostIndexed Out = new OpCodeAdvsimdLoadStoreSingleStructurePostIndexed();
            
            Out.InitData(RawOpCode,Address,Name,Info,Emit);

            return Out;
        }

        protected override void Load()
        {
            q = (RawOpCode >> 30) & 0x1;
            l = (RawOpCode >> 22) & 0x1;
            r = (RawOpCode >> 21) & 0x1;
            rm = (RawOpCode >> 16) & 0x1F;
            opcode = (RawOpCode >> 13) & 0x7;
            s = (RawOpCode >> 12) & 0x1;
            size = (RawOpCode >> 10) & 0x3;
            rn = (RawOpCode >> 5) & 0x1F;
            rt = (RawOpCode >> 0) & 0x1F;
        }
    }
}
