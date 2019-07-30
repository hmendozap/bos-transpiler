using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace BosTranspiler.LSP
{
    /// <summary>
    /// Class to handle Document Symbol definitions
    /// </summary>
    class SymbolHandler : IDocumentSymbolHandler
    {
        private readonly ILanguageServer _router;
        private readonly BufferManager _bufferManager;
        private readonly SymbolResolver _resolver;

        private readonly DocumentSelector _documentSelector = new DocumentSelector(
            new DocumentFilter()
            {
                Pattern = "**/*.bos"
            }
        );

        private DocumentSymbolCapability _capability;

        public SymbolHandler(ILanguageServer router, BufferManager buffMgr, SymbolResolver resolver)
        {
            _router = router;
            _bufferManager = buffMgr;
            _resolver = resolver;
        }

        public TextDocumentRegistrationOptions GetRegistrationOptions()
        {
            return new TextDocumentRegistrationOptions()
            {
                DocumentSelector = _documentSelector
            };
        }

        public async Task<SymbolInformationOrDocumentSymbolContainer> Handle(DocumentSymbolParams request, CancellationToken cancellationToken)
        {
            var documentPath = request.TextDocument.Uri.ToString();
            var buffer = _bufferManager.GetBuffer(documentPath);

            if (buffer == null)
            {
                DocumentSymbol symbol = new DocumentSymbol();
                return new SymbolInformationOrDocumentSymbolContainer(symbol);
            }
            _router.Window.LogMessage(new LogMessageParams()
            {
                Type = MessageType.Log,
                Message = "Igualdad entre las razas"
            });

            //List<string> uselessList = new List<string>();
            //var reus = _resolver.CallingResolver();

            var symbolContainer = new SymbolInformationOrDocumentSymbolContainer();

            try
            {
                symbolContainer = _resolver.ParseFile(documentPath, buffer);
            }
            catch (Exception ex)
            {
                _router.Window.LogError($"Error in buffer: {ex}");
            }
            
            foreach (var item in symbolContainer)
            {
                _router.Window.LogMessage(new LogMessageParams()
                {
                    Type = MessageType.Log,
                    Message = $"Symbol: {item.DocumentSymbol}"
                });
            }

            return symbolContainer;
        }

        private SymbolInformationOrDocumentSymbolContainer CreateEmptyDocumentSymbol()
        {
            DocumentSymbol symbol = new DocumentSymbol();
            return new SymbolInformationOrDocumentSymbolContainer(symbol);
        }

        public void SetCapability(DocumentSymbolCapability capability)
        {
            _capability = capability;
        }
    }
}
