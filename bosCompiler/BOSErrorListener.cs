using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;

namespace BosTranspiler
{
    public class BOSErrorListener : BaseErrorListener, IAntlrErrorListener<int>, IAntlrErrorListener<IToken>
    {
        public List<Error> Errors {get; private set;}
        public BOSErrorListener(ref List<Error> errors)
        {
            Errors = errors;
        }

        public override void SyntaxError(TextWriter output, IRecognizer recognizer,
            IToken offendingSymbol, int line, int charPositionInLine, string msg,
            RecognitionException e)
        {
            Error error = new Error();

            if (recognizer.GetType()  == typeof(BOSParser))
                error.Stack = (recognizer as BOSParser).GetRuleInvocationStackAsString();

            error.File = Path.GetFileName(e.InputStream.SourceName);
            error.Msg = $"{error.File}: {msg} at {line}:{charPositionInLine}";

            if (offendingSymbol != null)
                error.Symbol = offendingSymbol.Text;

            if (e != null)
                error.Exception = e.GetType().ToString();

            this.Errors.Add(error);
        }

        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            SyntaxError(output, recognizer, new CommonToken(offendingSymbol), line, charPositionInLine, msg, e);
        }
    }

}