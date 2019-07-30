using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace BosTranspiler.LSP
{
    /// <summary>
    /// Class to manage the latest version of a document. In order to maintain
    /// other buffers available to other handlers.
    /// </summary>
    class BufferManager
    {
        private ConcurrentDictionary<string, string> _buffers = new ConcurrentDictionary<string, string>();

        public void UpdateBuffer(string documentUri, string buffer)
        {
            _buffers.AddOrUpdate(documentUri, buffer, (k, v) => buffer);
        }

        public string GetBuffer(string documentUri)
        {
            return _buffers.TryGetValue(documentUri, out var buffer) ? buffer : null;
        }
    }
}
