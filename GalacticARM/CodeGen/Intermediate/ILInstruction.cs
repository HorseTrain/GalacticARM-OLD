using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Intermediate
{
    public enum ILInstruction : byte
    {
        Add,
        And,
        Call,
        CompareEqual,
        CompareGreaterThan,
        CompareGreaterThanUnsigned,
        CompareLessThan,
        CompareLessThanUnsigned,
        Copy,
        Divide,
        Divide_Un,
        F_Add,
        F_ConvertPrecision,
        F_Div,
        F_FloatConvertToInt,
        F_GreaterThan,
        F_IntConvertToFloat,
        F_LessThan,
        F_Mul,
        F_Sub,
        GetContextPointer,
        Jump,
        JumpIf,
        LoadImmediate,
        LoadMem,
        Mod,
        Multiply,
        Not,
        Or,
        Return,
        ShiftLeft,
        ShiftRight,
        ShiftRightSigned,
        SignExtend16,
        SignExtend32,
        SignExtend8,
        Store16,
        Store32,
        Store64,
        Store8,
        Subtract,
        WriteRegister,
        Xor,
    }
}
