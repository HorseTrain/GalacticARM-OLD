using GalacticARM.IntermediateRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Translation
{
    public static class EmitUniversal
    {
        public delegate void IfTest();

        public static void EmitIf(TranslationContext context, Operand test, IfTest yes = null, IfTest no = null)
        {
            Operand end = context.CreateLabel();
            Operand succ = context.CreateLabel();

            context.JumpIf(test,succ);

            if (no != null)
                no();

            context.Jump(end);

            context.MarkLabel(succ);

            if (yes != null)
                yes();

            context.MarkLabel(end);
        }
    }
}
