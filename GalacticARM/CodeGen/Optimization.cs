using GalacticARM.CodeGen.Intermediate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen
{
    public static class Optimization
    {
        public static bool UsePasses = true;

        //Function calls are vert problematic.
        public static OperationBlock Optimize(OperationBlock source)
        {
            OperationBlock Out = source;

            source = RunImmediateOptimizationPass(source);
            source = RunKnownResultPass(source);

            //Console.WriteLine(source);

            return Out;
        }

        public static OperationBlock RunImmediateOptimizationPass(OperationBlock source)
        {
            //Console.WriteLine(source);

            Dictionary<int, ulong> StoredImmediates = new Dictionary<int, ulong>();

            for (int o = 0; o < source.Operations.Count; o++)
            {
                Operation cop = source.Operations[o];

                if (cop.Name == ILInstruction.Nop)
                    continue;

                if (cop.Name == ILInstruction.LoadImmediate)
                {
                    if (StoredImmediates.ContainsKey(cop.GetReg(0)))
                    {
                        StoredImmediates.Remove(cop.GetReg(0));
                    }

                    StoredImmediates.Add(cop.GetReg(0), cop.GetImm(1));
                }
                else
                {
                    if (cop.GetType(0) == OperandType.Register)
                    {
                        if (StoredImmediates.ContainsKey(cop.GetReg(0)))
                        {
                            StoredImmediates.Remove(cop.GetReg(0));
                        }
                        else
                        {
                            for (int r = 0; r < cop.Arguments.Length; r++)
                            {
                                if (cop.Arguments[r].Type == OperandType.Register && StoredImmediates.ContainsKey(cop.GetReg(r)))
                                {
                                    cop.Arguments[r] = Operand.Const(StoredImmediates[cop.GetReg(r)]);
                                }
                            }
                        }
                    }
                }
            }

            for (int o = 0; o < source.Operations.Count; o++)
            {
                Operation cop = source.Operations[o];

                if (cop.Name == ILInstruction.LoadImmediate)
                {
                    int arg = cop.GetReg(0);

                    bool Safe = false;

                    for (int i = o; i < source.Operations.Count; i++)
                    {
                        Operation check = source.Operations[i];

                        for (int c = 1; c < check.Arguments.Length; c++)
                        {
                            if ((check.Arguments[c].Type == OperandType.Register && check.GetReg(c) == arg))
                            {
                                Safe = true;

                                break;
                            }
                        }                        
                    }

                    if (!Safe)
                    {
                        source.Operations[o] = new Operation(ILInstruction.Nop);
                    }
                }
            }

            return source;
        }

        public static OperationBlock RunKnownResultPass(OperationBlock source)
        {
            for (int o = 0; o < source.Operations.Count; o++)
            {
                Operation cop = source.Operations[o];

                if ((cop.Name == ILInstruction.Subtract || cop.Name == ILInstruction.Add || cop.Name == ILInstruction.ShiftLeft || cop.Name == ILInstruction.ShiftRight) && cop.Arguments[1].Type == OperandType.Constant)
                {
                    if (cop.GetImm(1) == 0)
                    {
                        source.Operations[o] = new Operation(ILInstruction.Nop);
                    }
                }

                if (cop.Name == ILInstruction.Copy)
                {
                    if (cop.Arguments[1].Type == OperandType.Constant)
                    {
                        source.Operations[o].Name = ILInstruction.LoadImmediate;
                    }

                    if (cop.Arguments[1].Type == OperandType.Register)
                    {
                        if (cop.Arguments[1].Reg == cop.Arguments[0].Reg)
                        {
                            source.Operations[o] = new Operation(ILInstruction.Nop);
                        }
                    }
                }

            }

            return source;
        }
    }
}
