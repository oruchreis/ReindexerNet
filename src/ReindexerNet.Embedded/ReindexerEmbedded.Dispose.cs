using ReindexerNet.Embedded.Internal;
using System;

namespace ReindexerNet.Embedded
{
    public partial class ReindexerEmbedded
    {
        private bool disposedValue; // To detect redundant calls      
        /// <summary>
        /// Query for getting namespaces.
        /// </summary>
        protected internal const string GetNamespacesQuery = "select name FROM #namespaces";
        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing && Rx != default)
                {
                    foreach (var ns in ExecuteSql<Namespace>(GetNamespacesQuery).Items)
                        CloseNamespace(ns.Name);

                    ReindexerBinding.destroy_reindexer(Rx);
                }

                Rx = default;

                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        ~ReindexerEmbedded()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes Reindexer native object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
