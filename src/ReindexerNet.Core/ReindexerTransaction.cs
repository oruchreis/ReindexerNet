using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNet
{
    public class ReindexerTransaction : ITransactionInvoker, IDisposable
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
                _isTransactionSuccess = true;
                await actionAsync().ConfigureAwait(false);                
            }
            catch (Exception e)
            {
                _isTransactionSuccess = false;
                ExceptionDispatchInfo.Capture(e).Throw();
                throw;
            }
        }

        private async Task<T> CheckOperationAsync<T>(Func<Task<T>> funcAsync)
        {
            try
            {
                _isTransactionSuccess = true;
                return await funcAsync().ConfigureAwait(false);                
            }
            catch (Exception e)
            {
                _isTransactionSuccess = false;
                ExceptionDispatchInfo.Capture(e).Throw();
                throw;
            }
        }

        private void CheckOperation(Action action)
        {
            try
            {
                _isTransactionSuccess = true;
                action();
            }
            catch (Exception e)
            {
                _isTransactionSuccess = false;
                ExceptionDispatchInfo.Capture(e).Throw();
                throw;
            }
        }

        private T CheckOperation<T>(Func<T> func)
        {
            try
            {
                _isTransactionSuccess = true;
                return func();
            }
            catch (Exception e)
            {
                _isTransactionSuccess = false;
                ExceptionDispatchInfo.Capture(e).Throw();
                throw;
            }
        }

        public int Commit()
        {
            return CheckOperation(() => _invoker.Commit());
        }

        public async Task<int> CommitAsync()
        {
            return await CheckOperationAsync(async () => await _invoker.CommitAsync().ConfigureAwait(false)).ConfigureAwait(false);
        }

        public void Rollback()
        {
            CheckOperation(() => _invoker.Rollback());
        }

        public async Task RollbackAsync()
        {
            await CheckOperationAsync(async () => await _invoker.RollbackAsync().ConfigureAwait(false)).ConfigureAwait(false);
        }
        
        public void ModifyItem(ItemModifyMode mode, byte[] itemJson, params string[] precepts)
        {
            CheckOperation(() => _invoker.ModifyItem(mode, itemJson, precepts));
        }

        public async Task ModifyItemAsync(ItemModifyMode mode, byte[] itemJson, params string[] precepts)
        {
            await CheckOperationAsync(async () => await _invoker.ModifyItemAsync(mode, itemJson, precepts).ConfigureAwait(false)).ConfigureAwait(false);
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
