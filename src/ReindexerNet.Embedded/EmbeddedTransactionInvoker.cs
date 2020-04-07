using ReindexerNet.Embedded.Helpers;
using ReindexerNet.Embedded.Internal;
using System;
using System.Threading.Tasks;

namespace ReindexerNet.Embedded
{
    internal class EmbeddedTransactionInvoker : ITransactionInvoker
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

        public int Commit()
        {
            var rsp = Assert.ThrowIfError(() => ReindexerBinding.reindexer_commit_transaction(_rx, _tr, _ctxInfo));
            try
            {
                var reader = new CJsonReader(rsp.@out);
                var rawQueryParams = reader.ReadRawQueryParams();

                return rawQueryParams.count;
            }
            finally
            {
                rsp.@out.Free();
            }
        }

        public Task<int> CommitAsync()
        {
            return Task.FromResult(Commit());
        }

        public void ModifyItem(ItemModifyMode mode, byte[] itemJson, params string[] precepts)
        {
            using (var writer = new CJsonWriter())
            {
                writer.PutVarCUInt((int)DataFormat.FormatJson); // format
                writer.PutVarCUInt((int)mode);// mode
                writer.PutVarCUInt(0);// stateToken

                writer.PutVarCUInt(precepts.Length);// len(precepts)
                foreach (var precept in precepts)
                {
                    writer.PutVString(precept);
                }

                reindexer_buffer.PinBufferFor(writer.CurrentBuffer, args =>
                {
                    using (var data = reindexer_buffer.From(itemJson))
                    {
                        Assert.ThrowIfError(() => ReindexerBinding.reindexer_modify_item_packed_tx(_rx, _tr, args, data.Buffer));
                    }
                });
            }
        }

        public Task ModifyItemAsync(ItemModifyMode mode, byte[] itemJson, params string[] precepts)
        {
            ModifyItem(mode, itemJson, precepts);
            return Task.CompletedTask;
        }

        public void Rollback()
        {
            Assert.ThrowIfError(() => ReindexerBinding.reindexer_rollback_transaction(_rx, _tr));
        }

        public Task RollbackAsync()
        {
            Rollback();
            return Task.CompletedTask;
        }
    }
}
