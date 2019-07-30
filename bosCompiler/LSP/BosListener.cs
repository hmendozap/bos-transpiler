using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BosTranspiler.LSP
{
    internal class BosListener : BosBaseListener
    {
        #region "Private Props"
        private CommonTokenStream TokenStream {get; set; }
        private TokenStreamRewriter StreamRewriter { get; set; }
        #endregion
        #region "Public Props"
        public string Filename { get; set; }
        //public Dictionary<string, DocumentSymbol> documentSymbols { get; set; } = new Dictionary<string, DocumentSymbol>();
        public List<SymbolInformationOrDocumentSymbol> documentSymbols { get; set; } = new List<SymbolInformationOrDocumentSymbol>();
        #endregion

        public BosListener(CommonTokenStream tokenStream, string fName)
        {
            TokenStream = tokenStream;
            Filename = fName;
            StreamRewriter = new TokenStreamRewriter(tokenStream);
        }
        public override void EnterFunctionStmt([NotNull] BosParser.FunctionStmtContext context)
        {
            AddDocumentSymbol(context.ambiguousIdentifier());
        }

        private void AddDocumentSymbol(BosParser.AmbiguousIdentifierContext context)
        {
            var name = context.GetText();
            var range = new Range(new Position
            {
                Line = context.Start.Line-1,
                Character = context.Start.Column
            }, new Position
            {
                Line = context.Start.Line-1,
                Character = context.Start.Column
            });
            var symbol = new DocumentSymbol
            {
                Name = name,
                Kind = SymbolKind.Function,
                Detail = $"Function: {name}",
                SelectionRange = range,
                Children = null,
                Range = range
            };
            documentSymbols.Add(symbol);
        }

        public override void EnterSubStmt([NotNull] BosParser.SubStmtContext context)
        {
            base.EnterSubStmt(context);
        }

    }
}
