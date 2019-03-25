using System;
using System.IO;
using Antlr4.Runtime;

namespace bosCompiler
{
    internal class ThrowingErrorListener<Tsymbol> : IAntlrErrorListener<Tsymbol>
    {
        //ErrorListener implmentation
        public void SyntaxError(TextWriter output, IRecognizer recognizer,
            Tsymbol offendingSymbol, int line, int charPositionInLine, string msg,
            RecognitionException e)
        {
            throw new Exception($"line: {line} : {charPositionInLine} {msg}");
        }
    }
}