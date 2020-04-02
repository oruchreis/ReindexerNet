using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNet
{
    public class ReindexerTransaction : IDisposable
    {
        private readonly ITransactionInvoker _invoker;
        public ReindexerTransaction(ITransactionInvoker invoker)
        {
            _invoker = invoker;
        }

        private bool _isTransactionSuccess;
        private async Task CheckOperationAsync(Func<Task> actionAsync)
        {
            try
            {
                await actionAsync().ConfigureAwait(false);
                _isTransactionSuccess = true;
            }
            catch (Exception e)
            {
                _isTransactionSuccess = false;
                ExceptionDispatchInfo.Capture(e).Throw();
            }            
        }

        public async Task CommitAsync()
        {
            await CheckOperationAsync(async () => await _invoker.CommitAsync().ConfigureAwait(false)).ConfigureAwait(false);            
        }

        public async Task RollbackAsync()
        {
            await CheckOperationAsync(async () => await _invoker.RollbackAsync().ConfigureAwait(false)).ConfigureAwait(false);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls        

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing && !_isTransactionSuccess)
                {
                    _invoker.Rollback();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ReindexerTransaction()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
