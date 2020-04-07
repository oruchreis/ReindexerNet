using ReindexerNet.Embedded.Internal;
using System;

namespace ReindexerNet.Embedded
{
    public partial class ReindexerEmbedded
    {
        private bool disposedValue; // To detect redundant calls        

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
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
