using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReindexerGrpc = Reindexer.Grpc.Reindexer;
using Reindexer.Grpc;
using Google.Protobuf;
using System.Threading;
using System.Linq;

namespace ReindexerNet.Remote.Grpc
{
    internal class GrpcTransactionInvoker : ITransactionInvoker
    {
        private readonly ReindexerGrpc.ReindexerClient _grpcClient;
        private readonly long _tranId;
        private readonly IReindexerSerializer _serializer;

        internal GrpcTransactionInvoker(ReindexerGrpc.ReindexerClient reindexerClient, long tranId, IReindexerSerializer serializer)
        {
            _grpcClient = reindexerClient;
            _tranId = tranId;
            _serializer = serializer;
        }

        public int Commit()
        {
            _grpcClient.CommitTransaction(new CommitTransactionRequest
            {
                Id = _tranId,
            }).HandleErrorResponse();
            return 0;
        }

        public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            (await _grpcClient.CommitTransactionAsync(new CommitTransactionRequest
            {
                Id = _tranId,
            }, cancellationToken: cancellationToken)).HandleErrorResponse();
            return 0;
        }

        public int ModifyItems<TItem>(ItemModifyMode mode, IEnumerable<TItem> items, string[] precepts = null)
        {
            throw new NotSupportedException("Reindexer grpc client doesn't support sync method, use ModifyItemAsync instead.");
        }

        public int ModifyItems(ItemModifyMode mode, IEnumerable<byte[]> itemDatas, SerializerType dataEncoding, string[] precepts = null)
        {
            throw new NotSupportedException("Reindexer grpc client doesn't support sync method, use ModifyItemAsync instead.");
        }

        private async Task<int> ModifyItemsAsync(ItemModifyMode mode, IEnumerable<ByteString> itemDatas, SerializerType dataEncoding,
            string[] precepts = null, CancellationToken cancellationToken = default)
        {
            using var asyncReq = _grpcClient.AddTxItem();

            var handleRsp = asyncReq.ResponseStream.HandleErrorResponseAsync(cancellationToken: cancellationToken);
            foreach (var itemData in itemDatas)
            {
                await asyncReq.RequestStream.WriteAsync(new AddTxItemRequest
                {
                    Id = _tranId,
                    Mode = mode.ToModifyMode(),
                    Data = itemData,
                    EncodingType = dataEncoding switch
                    {
                        SerializerType.Json => EncodingType.Json,
                        SerializerType.Msgpack => EncodingType.Msgpack,
                        SerializerType.Cjson => EncodingType.Cjson,
                        SerializerType.Protobuf => EncodingType.Protobuf,
                        _ => throw new NotImplementedException(),
                    }
                });
                if (cancellationToken.IsCancellationRequested)
                    break;
            }

            await asyncReq.RequestStream.CompleteAsync();
            return await handleRsp;
        }


        public Task<int> ModifyItemsAsync<TItem>(ItemModifyMode mode, IEnumerable<TItem> items,
            string[] precepts = null, CancellationToken cancellationToken = default)
        {
            return ModifyItemsAsync(mode, items.Select(item => ByteString.CopyFrom(_serializer.Serialize(item))), 
                _serializer.Type, precepts, cancellationToken);
        }

        public Task<int> ModifyItemsAsync(ItemModifyMode mode, IEnumerable<byte[]> itemDatas, SerializerType dataEncoding,
            string[] precepts = null, CancellationToken cancellationToken = default)
        {
            return ModifyItemsAsync(mode, itemDatas.Select(itemData => ByteString.CopyFrom(itemData)), 
                dataEncoding, precepts, cancellationToken);
        }

        public void Rollback()
        {
            _grpcClient.RollbackTransaction(new RollbackTransactionRequest
            {
                Id = _tranId,
            }).HandleErrorResponse();
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            (await _grpcClient.RollbackTransactionAsync(new RollbackTransactionRequest
            {
                Id = _tranId,
            }, cancellationToken: cancellationToken)).HandleErrorResponse();
        }
    }
}
