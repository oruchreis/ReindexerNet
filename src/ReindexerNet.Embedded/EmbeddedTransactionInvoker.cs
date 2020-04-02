using ReindexerNet.Embedded.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNet.Embedded
{
    class EmbeddedTransactionInvoker : ITransactionInvoker
    {
        private readonly UIntPtr _rx;
        private readonly UIntPtr _tr;
        private readonly reindexer_ctx_info _ctxInfo;

        public EmbeddedTransactionInvoker(UIntPtr rx, UIntPtr tr, reindexer_ctx_info ctxInfo)
        {
            _rx = rx;
            _tr = tr;
            _ctxInfo = ctxInfo;
        }

        public void Commit()
        {
            ReindexerBinding.reindexer_commit_transaction(_rx, _tr, _ctxInfo);
        }

        public Task CommitAsync()
        {
            Commit();
            return Task.CompletedTask;
        }

        public int ModifyItem(ItemModifyMode mode, string itemJson, params string[] precepts)
        {
            throw new NotImplementedException();
        }

        public Task<int> ModifyItemAsync(ItemModifyMode mode, string itemJson, params string[] precepts)
        {
            throw new NotImplementedException();
        }

        public void Rollback()
        {
            ReindexerBinding.reindexer_rollback_transaction(_rx, _tr);
        }

        public Task RollbackAsync()
        {
            Rollback();
            return Task.CompletedTask;
        }
    }
}
