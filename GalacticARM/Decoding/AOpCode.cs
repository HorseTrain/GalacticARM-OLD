using GalacticARM.CodeGen;
using System;


namespace GalacticARM.Decoding
{
    public class AOpCode
    {
        public int RawOpCode                { get; set; }
        public ulong Address                { get; set; }
        public InstructionMnemonic Name     { get; set; }
        public InstructionInfo Info         { get; set; }
        public ILEmit Emit                  { get; set; }

        public void InitData(int RawOpCode, ulong Address, InstructionMnemonic Name, InstructionInfo Info, ILEmit emit)
        {
            this.RawOpCode = RawOpCode;
            this.Address = Address;
            this.Name = Name;
            this.Info = Info;
            this.Emit = emit;

            Load();
        }

        protected virtual void Load()
        {
            throw new NotImplementedException();
        }
    }
}
