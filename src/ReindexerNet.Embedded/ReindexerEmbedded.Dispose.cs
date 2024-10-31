using ReindexerNet.Embedded.Internal;
using ReindexerNet.Embedded.Internal.Helpers;
using System;
using System.Threading.Tasks;

namespace ReindexerNet.Embedded
{
    public partial class ReindexerEmbedded
    {
        private bool disposedValue; // To detect redundant calls      
        /// <summary>
        /// Query for getting namespaces.
        /// </summary>
        protected internal const string GetNamespacesQuery = "select name FROM #namespaces where storage.enabled = true and not name like '#%'";
        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {            
            if (!disposedValue)
            {
                if (disposing && Rx != default)
                {
                    DebugHelper.Log("Stopping reindexer...");
                    Parallel.ForEach(ExecuteSql<Namespace>(GetNamespacesQuery).Items, ns =>
                    {
                        CloseNamespace(ns.Name);
                    });

                    if (Rx != default)
                        ReindexerBinding.destroy_reindexer(Rx);
                    DebugHelper.Log("Reindexer stopped...");
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

        public async ValueTask DisposeAsync()
        {
            // Perform async cleanup.
            await DisposeAsyncCore().ConfigureAwait(false);

            // Dispose of unmanaged resources.
            Dispose(false);

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
            // Suppress finalization.
            GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
        }

#if !NET5_0_OR_GREATER
        private static readonly ValueTask _completedTask = new();
#endif
        protected virtual ValueTask DisposeAsyncCore()
        {
#if NET5_0_OR_GREATER
            return ValueTask.CompletedTask;
#else            
            return _completedTask;
#endif
        }
    }
}
