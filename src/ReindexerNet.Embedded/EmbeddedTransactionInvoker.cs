using ReindexerNet.Embedded.Internal.Helpers;
using ReindexerNet.Embedded.Internal;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace ReindexerNet.Embedded
{
    internal class EmbeddedTransactionInvoker : ITransactionInvoker
    {
        private readonly UIntPtr _rx;
        private readonly UIntPtr _tr;
        private readonly reindexer_ctx_info _ctxInfo;
        private readonly IReindexerSerializer _serializer;

        public EmbeddedTransactionInvoker(UIntPtr rx, UIntPtr tr, reindexer_ctx_info ctxInfo, IReindexerSerializer serializer)
        {
            _rx = rx;
            _tr = tr;
            _ctxInfo = ctxInfo;
            _serializer = serializer;
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

        public Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Commit());
        }

        public void ModifyItem(ItemModifyMode mode, ReadOnlySpan<byte> itemBytes, SerializerType dataEncoding, string[] precepts = null)
        {
            precepts = precepts ?? new string[0];
            using (var writer = new CJsonWriter())
            {
                writer.PutVarCUInt((int)dataEncoding); // format
                writer.PutVarCUInt((int)mode);// mode
                writer.PutVarCUInt(0);// stateToken

                writer.PutVarCUInt(precepts.Length);// len(precepts)
                foreach (var precept in precepts)
                {
                    writer.PutVString(precept);
                }

                reindexer_buffer.PinBufferFor(writer.CurrentBuffer, itemBytes, (args, data) =>
                {
                    Assert.ThrowIfError(() => ReindexerBinding.reindexer_modify_item_packed_tx(_rx, _tr, args, data));
                });
            }
        }


        public int ModifyItems<TItem>(ItemModifyMode mode, IEnumerable<TItem> items, string[] precepts = null)
        {
            var result = 0;
            foreach (var item in items)
            {
                ModifyItem(mode, _serializer.Serialize(item), _serializer.Type, precepts);
                result++;
            }
            return result;
        }

        public int ModifyItems(ItemModifyMode mode, IEnumerable<byte[]> itemDatas, SerializerType dataEncoding, string[] precepts = null)
        {
            var result = 0;
            foreach (var itemData in itemDatas)
            {
                ModifyItem(mode, itemData, dataEncoding, precepts);
                result++;
            }
            return result;
        }

        public Task<int> ModifyItemsAsync<TItem>(ItemModifyMode mode, IEnumerable<TItem> items, string[] precepts = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ModifyItems(mode, items, precepts));
        }

        public Task<int> ModifyItemsAsync(ItemModifyMode mode, IEnumerable<byte[]> itemDatas, SerializerType dataEncoding, string[] precepts = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ModifyItems(mode, itemDatas, dataEncoding, precepts));
        }

        public void Rollback()
        {
            Assert.ThrowIfError(() => ReindexerBinding.reindexer_rollback_transaction(_rx, _tr));
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            Rollback();
            return Task.CompletedTask;
        }
    }
}
