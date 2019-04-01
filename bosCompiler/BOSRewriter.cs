using System.Collections.Generic;

namespace BosTranspiler
{
    internal class BOSRewriter
    {
        private List<string> structuresWithInit;

        public BOSRewriter(List<string> structuresWithInit)
        {
            this.structuresWithInit = structuresWithInit;
        }
    }
}