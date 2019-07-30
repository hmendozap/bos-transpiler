using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BosTranspiler.LSP
{
    class SymbolResolver
    {
        public SymbolInformationOrDocumentSymbolContainer ParseFile(string documentUri, string bosFile)
        {
            ICharStream inputStream = CharStreams.fromstring(bosFile);
            //var inputStream = new AntlrInputStream(bosFile);
            BosLexer lexer = new BosLexer(inputStream);
            var tokenStreams = new CommonTokenStream(lexer);
            BosParser parser = new BosParser(tokenStreams);

            // remove standard listener to add custom
            parser.RemoveErrorListeners();
            lexer.RemoveErrorListeners();

            //    Stopwatch sw = Stopwatch.StartNew();
            var firstPassErrors = new List<Error>();
            BosErrorListener errorListener = new BosErrorListener(ref firstPassErrors);
            lexer.AddErrorListener(errorListener);
            parser.AddErrorListener(errorListener);

            var parsedTree = parser.startRule();

            // first processing (pass) of transpilation
            //FixerListener listener = new FixerListener(tokenStreams, documentUri);
            BosListener listener = new BosListener(tokenStreams, documentUri);
            ParseTreeWalker.Default.Walk(listener, parsedTree);

            //symbolContainer = listener.documentSymbols;

            return new SymbolInformationOrDocumentSymbolContainer(listener.documentSymbols);
            //return FixerListener.SymbolNam;
        }

        private static List<IToken> CollectTokens(CommonTokenStream cts)
        {
            IList<IToken> lTokenList = cts.Get(0, cts.Size);
            List<IToken> tokens = new List<IToken>();
            foreach (IToken it in lTokenList)
            {
                if (it.Channel == 2)
                    tokens.Add(it);
            }
            return tokens;
        }
    }
}
