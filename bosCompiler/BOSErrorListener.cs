using System.IO;
using Antlr4.Runtime;

namespace BosTranspiler
{
    internal class BOSErrorListener : BaseErrorListener, IAntlrErrorListener<int>
    {
        public BOSErrorListener()
        {
        }

        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            throw new System.NotImplementedException();
        }
    }
}