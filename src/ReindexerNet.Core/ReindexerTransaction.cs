using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
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
        public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            return await CheckOperationAsync(async () => await _invoker.CommitAsync(cancellationToken).ConfigureAwait(false)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void Rollback()
        {
            CheckOperation(() => _invoker.Rollback());
        }

        /// <inheritdoc/>
        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            await CheckOperationAsync(async () => await _invoker.RollbackAsync(cancellationToken).ConfigureAwait(false)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public int ModifyItems<TItem>(ItemModifyMode mode, IEnumerable<TItem> items, string[] precepts = null)
        {
            return CheckOperation(() => _invoker.ModifyItems(mode, items, precepts));
        }

        /// <inheritdoc/>
        public async Task<int> ModifyItemsAsync<TItem>(ItemModifyMode mode, IEnumerable<TItem> items, string[] precepts = null, CancellationToken cancellationToken = default)
        {
            return await CheckOperationAsync(async () => await _invoker.ModifyItemsAsync(mode, items, precepts, cancellationToken).ConfigureAwait(false)).ConfigureAwait(false);
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
