using Microsoft.CodeAnalysis.VisualBasic;
using System.Collections.Generic;

namespace BosTranspiler
{
    internal class BOSRewriter : VisualBasicSyntaxRewriter
    {
        private List<string> structuresWithInit;

        public BOSRewriter(List<string> structuresWithInit)
        {
            this.structuresWithInit = structuresWithInit;
        }

    }
}