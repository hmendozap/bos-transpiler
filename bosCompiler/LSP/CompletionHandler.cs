using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BosTranspiler.LSP
{
    /// <summary>
    /// Class to react to the `textDocument/completion` request
    /// </summary>
    class CompletionHandler : ICompletionHandler
    {
        private readonly ILanguageServer _router;
        private readonly BufferManager _bufferManager;
        private readonly DocumentSelector _documentSelector = new DocumentSelector(
            new DocumentFilter()
            {
                Pattern = "**/*.bos"
            }
        );

        private CompletionCapability _capability;

        public CompletionHandler(ILanguageServer router, BufferManager buffMgr)
        {
            _router = router;
            _bufferManager = buffMgr;
        }

        public CompletionRegistrationOptions GetRegistrationOptions()
        {
            return new CompletionRegistrationOptions
            {
                DocumentSelector = _documentSelector,
                TriggerCharacters = new string[] { "." },
                ResolveProvider = false
            };
        }

        public async Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
        {
            var documentPath = request.TextDocument.Uri.ToString();
            var buffer = _bufferManager.GetBuffer(documentPath);

            if (buffer == null)
            {
                CompletionList completionList = new CompletionList();
                return completionList;
            }

            var position = GetPosition(buffer, (int)request.Position.Line,
                (int)request.Position.Character);

            var tsCompletionItem = new CompletionItem()
            {
                Label = "AhoraSi",
                Kind = CompletionItemKind.Text,
                Data = 1
            };

            var jsCompletionItem = new CompletionItem()
            {
                Label = "YaChingue",
                Kind = CompletionItemKind.Text,
                Data = 2
            };

            return new CompletionList(tsCompletionItem, jsCompletionItem);
        }

        private static int GetPosition(string buffer, int line, int character)
        {
            var position = 0;
            for (int i = 0; i < line; i++)
            {
                position = buffer.IndexOf('\n', position) + 1;
            }
            return position + character;
        }

        public void SetCapability(CompletionCapability capability)
        {
            _capability = capability;
        }
    }
}
