using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReindexerGrpc = Reindexer.Grpc.Reindexer;
using Reindexer.Grpc;
using Google.Protobuf;
using System.Threading;

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

        public async Task<int> ModifyItemsAsync<TItem>(ItemModifyMode mode, IEnumerable<TItem> items,
            string[] precepts = null, CancellationToken cancellationToken = default)
        {
            using var asyncReq = _grpcClient.AddTxItem();

            var handleRsp = asyncReq.ResponseStream.HandleErrorResponseAsync(cancellationToken: cancellationToken);
            foreach (var item in items)
            {
                await asyncReq.RequestStream.WriteAsync(new AddTxItemRequest
                {
                    Id = _tranId,
                    Mode = mode.ToModifyMode(),
                    Data = ByteString.CopyFrom(_serializer.Serialize(item)),
                    EncodingType = EncodingType.Json
                });
                if (cancellationToken.IsCancellationRequested)
                    break;
            }

            await asyncReq.RequestStream.CompleteAsync();
            return await handleRsp;
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
