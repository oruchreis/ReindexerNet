using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace ReindexerNet
{
    /// <summary>
    /// Represents a Reindexer transaction.
    /// </summary>
    public sealed class ReindexerTransaction : ITransactionInvoker, IDisposable
    {
        private readonly ITransactionInvoker _invoker;
        /// <summary>
        /// Creates a reindexer transacrtion.
        /// </summary>
        /// <param name="invoker"></param>
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

        /// <inheritdoc/>
        public int Commit()
        {
            return CheckOperation(() => _invoker.Commit());
        }

        /// <inheritdoc/>
        public async Task<int> CommitAsync()
        {
            return await CheckOperationAsync(async () => await _invoker.CommitAsync().ConfigureAwait(false)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void Rollback()
        {
            CheckOperation(() => _invoker.Rollback());
        }

        /// <inheritdoc/>
        public async Task RollbackAsync()
        {
            await CheckOperationAsync(async () => await _invoker.RollbackAsync().ConfigureAwait(false)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void ModifyItem(ItemModifyMode mode, byte[] itemJson, params string[] precepts)
        {
            CheckOperation(() => _invoker.ModifyItem(mode, itemJson, precepts));
        }

        /// <inheritdoc/>
        public async Task ModifyItemAsync(ItemModifyMode mode, byte[] itemJson, params string[] precepts)
        {
            await CheckOperationAsync(async () => await _invoker.ModifyItemAsync(mode, itemJson, precepts).ConfigureAwait(false)).ConfigureAwait(false);
        }

        /// <summary>
        /// Ends transaction. If the transaction is not commited, it will be roll back.
        /// </summary>
        public void Dispose()
        {
            if (!_isTransactionSuccess)
            {
                _invoker.Rollback();
            }
        }
    }
}
