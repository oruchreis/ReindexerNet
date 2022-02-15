using System;
using System.Threading.Tasks;
using Reindexer.Grpc;
using ReindexerGrpc = Reindexer.Grpc.Reindexer;
using Grpc.Core;
using Google.Protobuf;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Net;
using Grpc.Core.Interceptors;
#if LEGACY_GRPC_CORE
using System.Text.RegularExpressions;
using GrpcChannel = Grpc.Core.Channel;
#else
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
#endif

namespace ReindexerNet.Remote.Grpc
{
    /// <summary>
    /// Reindexer grpc client for remote Reindexer servers. Reindexer server must be started with grpc=true argument.
    /// </summary>
    public sealed class ReindexerGrpcClient : IAsyncReindexerClient
    {
        private ReindexerConnectionString _connectionString;
        private readonly IReindexerSerializer _serializer;
        private GrpcChannel _channel;
        private ReindexerGrpc.ReindexerClient _grpcClient;
        private readonly OutputFlags _outputFlags;

        /// <param name="connectionString">Connection string for the implementation.</param>
        /// <param name="serializer"></param>
        /// <param name="maxSendMessageSize"></param>
        /// <param name="maxReceiveMessageSize"></param>
        /// <param name="configureChannelOptions">Experimental parameter, maybe removed in future</param>
        /// <param name="grpcInterceptor">Experimental parameter, maybe removed in future</param>
        public ReindexerGrpcClient(ReindexerConnectionString connectionString, IReindexerSerializer serializer = null,
            int? maxSendMessageSize = null, int? maxReceiveMessageSize = null
#if LEGACY_GRPC_CORE
            , Action<IList<ChannelOption>> configureChannelOptions = null
#else
            , Action<GrpcChannelOptions> configureChannelOptions = null
#endif
            , Interceptor grpcInterceptor = null
            )
        {
            _connectionString = connectionString;
            _serializer = serializer ?? new ReindexerJsonSerializer();
            _outputFlags = new OutputFlags
            {
                EncodingType = _serializer.Type switch
                {
                    SerializerType.Json => EncodingType.Json,
                    SerializerType.Msgpack => EncodingType.Msgpack,
                    SerializerType.Cjson => EncodingType.Cjson,
                    SerializerType.Protobuf => EncodingType.Protobuf,
                    _ => throw new NotImplementedException(),
                }
            };

#if NET472 || NETSTANDARD2_0_OR_GREATER
        https://docs.microsoft.com/en-us/aspnet/core/grpc/troubleshoot?view=aspnetcore-3.0#call-insecure-grpc-services-with-net-core-client
            // This switch must be set before creating the GrpcChannel/HttpClient.
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
#endif

            if (!_connectionString.GrpcAddress.StartsWith("http://") || !_connectionString.GrpcAddress.StartsWith("https://"))
                _connectionString.GrpcAddress = "http://" + _connectionString.GrpcAddress;
            var ipEndpoint = new Uri(_connectionString.GrpcAddress, UriKind.Absolute);
#if LEGACY_GRPC_CORE
            var channelOptions = new List<ChannelOption>();
            channelOptions.Add(new ChannelOption(ChannelOptions.MaxReceiveMessageLength, maxReceiveMessageSize ?? -1));
            channelOptions.Add(new ChannelOption(ChannelOptions.MaxSendMessageLength, maxSendMessageSize ?? -1));
            configureChannelOptions?.Invoke(channelOptions);

            _channel = new GrpcChannel(ipEndpoint.Host, ipEndpoint.Port //doesn't support subdirectories.
                , ChannelCredentials.Insecure, channelOptions);
#else
            var grpcMethodConfigs = new[]{ new MethodConfig
            {
                Names = { MethodName.Default },
                RetryPolicy = new RetryPolicy
                {
                    MaxAttempts = 5,
                    InitialBackoff = TimeSpan.FromSeconds(1),
                    MaxBackoff = TimeSpan.FromSeconds(5),
                    BackoffMultiplier = 1.5,
                    RetryableStatusCodes = { StatusCode.Unavailable }
                }
            }};
            var channelOptions = new GrpcChannelOptions
            {
                ServiceConfig = new ServiceConfig { }                
            };

            channelOptions.MaxReceiveMessageSize = maxReceiveMessageSize;
            channelOptions.MaxSendMessageSize = maxSendMessageSize;

            foreach (var methodConfig in grpcMethodConfigs)
            {
                channelOptions.ServiceConfig.MethodConfigs.Add(methodConfig);
            }

            configureChannelOptions?.Invoke(channelOptions);

            _channel = GrpcChannel.ForAddress(_connectionString.GrpcAddress, channelOptions);
#endif            
            if (grpcInterceptor == null)
                _grpcClient = new ReindexerGrpc.ReindexerClient(_channel);
            else
                _grpcClient = new ReindexerGrpc.ReindexerClient(_channel.Intercept(grpcInterceptor));
        }

        /// <inheritdoc/>
        public async Task ConnectAsync(ConnectionOptions options = null, CancellationToken cancellationToken = default)
        {
            if (options == null)
                options = new ConnectionOptions();

            (await _grpcClient.ConnectAsync(new ConnectRequest
            {
                DbName = _connectionString.DatabaseName,
                Login = _connectionString.Username,
                Password = _connectionString.Password,
                ConnectOpts = new ConnectOptions
                {
                    AllowNamespaceErrors = options.AllowNamespaceErrors,
                    Autorepair = options.AutoRepair,
                    DisableReplication = options.DisableReplication,
                    ExpectedClusterID = options.ExpectedClusterId,
                    OpenNamespaces = options.OpenNamespaces,
                    StorageType = (StorageType)(int)options.Engine
                }
            }, cancellationToken: cancellationToken)).HandleErrorResponse();
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await _channel.ShutdownAsync();
#if !LEGACY_GRPC_CORE
            _channel?.Dispose();
#endif
        }

        /// <inheritdoc/>
        public async Task CloseNamespaceAsync(string nsName, CancellationToken cancellationToken = default)
        {
            (await _grpcClient.CloseNamespaceAsync(new CloseNamespaceRequest
            {
                DbName = _connectionString.DatabaseName,
                NsName = nsName
            }, cancellationToken: cancellationToken)).HandleErrorResponse();
        }

        /// <inheritdoc/>
        public Task PingAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task OpenNamespaceAsync(string nsName, NamespaceOptions options = null, CancellationToken cancellationToken = default)
        {
            (await _grpcClient.OpenNamespaceAsync(new OpenNamespaceRequest
            {
                DbName = _connectionString.DatabaseName,
                StorageOptions = new StorageOptions
                {
                    Autorepair = options.AutoRepair,
                    CreateIfMissing = options.CreateIfMissing,
                    DropOnFileFormatError = options.DropOnFileFormatError,
                    Enabled = options.EnableStorage,
                    FillCache = options.FillCache,
                    NsName = nsName,
                    SlaveMode = options.SlaveMode,
                    Sync = options.Sync,
                    VerifyChecksums = options.VerifyChecksums
                }
            }, cancellationToken: cancellationToken)).HandleErrorResponse();
        }

        /// <inheritdoc/>
        public async Task DropNamespaceAsync(string nsName, CancellationToken cancellationToken = default)
        {
            (await _grpcClient.DropNamespaceAsync(new DropNamespaceRequest
            {
                DbName = _connectionString.DatabaseName,
                NsName = nsName
            }, cancellationToken: cancellationToken)).HandleErrorResponse();
        }

        /// <inheritdoc/>
        public async Task TruncateNamespaceAsync(string nsName, CancellationToken cancellationToken = default)
        {
            (await _grpcClient.TruncateNamespaceAsync(new TruncateNamespaceRequest
            {
                DbName = _connectionString.DatabaseName,
                NsName = nsName
            }, cancellationToken: cancellationToken)).HandleErrorResponse();
        }

        /// <summary>
        /// Not supported by Reindexer grpc server.
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public Task RenameNamespaceAsync(string oldName, string newName, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("Not supported by Reindexer grpc server");
        }

        /// <inheritdoc/>
        public async Task AddIndexAsync(string nsName, Index indexDefinition, CancellationToken cancellationToken = default)
        {
            var req = new AddIndexRequest
            {
                DbName = _connectionString.DatabaseName,
                NsName = nsName,
                Definition = new Reindexer.Grpc.Index
                {
                    Name = indexDefinition.Name,
                    FieldType = indexDefinition.FieldType.ToEnumString(),
                    IndexType = indexDefinition.IndexType.ToEnumString(),
                    JsonPaths = { indexDefinition.JsonPaths ?? new List<string>() { indexDefinition.Name } },
                    Options = new IndexOptions
                    {
                        CollateMode = Enum.TryParse(indexDefinition.CollateMode, true, out IndexOptions.Types.CollateMode collateMode) ? collateMode : IndexOptions.Types.CollateMode.CollateNoneMode,
                        IsArray = indexDefinition.IsArray ?? false,
                        IsDense = indexDefinition.IsDense ?? false,
                        IsPk = indexDefinition.IsPk ?? false,
                        IsSparse = indexDefinition.IsSparse ?? false,
                        SortOrdersTable = indexDefinition.SortOrderLetters ?? "",
                        //Config = indexDef.Config,
                        //RtreeType
                    }
                }
            };

            (await _grpcClient.AddIndexAsync(req, cancellationToken: cancellationToken)).HandleErrorResponse();
        }

        /// <inheritdoc/>
        public async Task UpdateIndexAsync(string nsName, Index indexDefinition, CancellationToken cancellationToken = default)
        {
            var req = new UpdateIndexRequest
            {
                DbName = _connectionString.DatabaseName,
                NsName = nsName,
                Definition = new Reindexer.Grpc.Index
                {
                    Name = indexDefinition.Name,
                    FieldType = indexDefinition.FieldType.ToEnumString(),
                    IndexType = indexDefinition.IndexType.ToEnumString(),
                    JsonPaths = { indexDefinition.JsonPaths ?? new List<string>() { indexDefinition.Name } },
                    Options = new IndexOptions
                    {
                        CollateMode = Enum.TryParse(indexDefinition.CollateMode, true, out IndexOptions.Types.CollateMode collateMode) ?
                            collateMode :
                            IndexOptions.Types.CollateMode.CollateNoneMode,
                        IsArray = indexDefinition.IsArray ?? false,
                        IsDense = indexDefinition.IsDense ?? false,
                        IsPk = indexDefinition.IsPk ?? false,
                        IsSparse = indexDefinition.IsSparse ?? false,
                        SortOrdersTable = indexDefinition.SortOrderLetters,
                        //Config = indexDef.Config,
                        //RtreeType
                    }
                }
            };

            (await _grpcClient.UpdateIndexAsync(req, cancellationToken: cancellationToken)).HandleErrorResponse();
        }

        /// <inheritdoc/>
        public async Task DropIndexAsync(string nsName, string indexName, CancellationToken cancellationToken = default)
        {
            (await _grpcClient.DropIndexAsync(new DropIndexRequest
            {
                DbName = _connectionString.DatabaseName,
                NsName = nsName,
                Definition = new Reindexer.Grpc.Index
                {
                    Name = indexName
                }
            }, cancellationToken: cancellationToken)).HandleErrorResponse();
        }

        /// <inheritdoc/>
        public async Task<ReindexerTransaction> StartTransactionAsync(string nsName, CancellationToken cancellationToken = default)
        {
            var tranRsp = await _grpcClient.BeginTransactionAsync(new BeginTransactionRequest
            {
                DbName = _connectionString.DatabaseName,
                NsName = nsName
            }, cancellationToken: cancellationToken);

            tranRsp.Status.HandleErrorResponse();
            return new ReindexerTransaction(new GrpcTransactionInvoker(_grpcClient, tranRsp.Id, _serializer));
        }

        private async Task<int> ModifyItemAsync(string nsName, ItemModifyMode mode, IEnumerable<ByteString> itemsBytes, EncodingType dataEncoding,
            string[] precepts = null, CancellationToken cancellationToken = default)
        {
            using var asyncReq = _grpcClient.ModifyItem();
            var handleRsp = asyncReq.ResponseStream.HandleErrorResponseAsync(cancellationToken);
            foreach (var itemBytes in itemsBytes)
            {
                await asyncReq.RequestStream.WriteAsync(new ModifyItemRequest
                {
                    DbName = _connectionString.DatabaseName,
                    EncodingType = dataEncoding,
                    Mode = mode.ToModifyMode(),
                    NsName = nsName,
                    Data = itemBytes,
                }).ConfigureAwait(false);
                if (cancellationToken.IsCancellationRequested)
                    break;
            }
            await asyncReq.RequestStream.CompleteAsync();

            return await handleRsp;
        }

        /// <inheritdoc/>
        public async Task<int> ModifyItemsAsync<TItem>(string nsName, ItemModifyMode mode, IEnumerable<TItem> items,
            string[] precepts = null, CancellationToken cancellationToken = default)
        {
            return await ModifyItemAsync(nsName, mode, items.Select(item => ByteString.CopyFrom(_serializer.Serialize(item))), 
                _outputFlags.EncodingType, precepts, cancellationToken);
        }

        public Task<int> ModifyItemsAsync(string nsName, ItemModifyMode mode, IEnumerable<byte[]> itemDatas, SerializerType dataEncoding, 
            string[] precepts = null, CancellationToken cancellationToken = default)
        {
            return ModifyItemAsync(nsName, mode, itemDatas.Select(itemData => ByteString.CopyFrom(itemData)), 
                dataEncoding switch
                {
                    SerializerType.Json => EncodingType.Json,
                    SerializerType.Msgpack => EncodingType.Msgpack,
                    SerializerType.Cjson => EncodingType.Cjson,
                    SerializerType.Protobuf => EncodingType.Protobuf,
                    _ => throw new NotImplementedException(),
                }, precepts, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<int> InsertAsync<TItem>(string nsName, IEnumerable<TItem> items,
            string[] precepts = null, CancellationToken cancellationToken = default)
        {
            return await ModifyItemsAsync(nsName, ItemModifyMode.Insert, items, precepts, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<int> UpdateAsync<TItem>(string nsName, IEnumerable<TItem> items,
            string[] precepts = null, CancellationToken cancellationToken = default)
        {
            return await ModifyItemsAsync(nsName, ItemModifyMode.Update, items, precepts, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<int> UpsertAsync<TItem>(string nsName, IEnumerable<TItem> items,
            string[] precepts = null, CancellationToken cancellationToken = default)
        {
            return await ModifyItemsAsync(nsName, ItemModifyMode.Upsert, items, precepts, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<int> DeleteAsync<TItem>(string nsName, IEnumerable<TItem> items,
            string[] precepts = null, CancellationToken cancellationToken = default)
        {
            return await ModifyItemsAsync(nsName, ItemModifyMode.Delete, items, precepts, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<QueryItemsOf<TItem>> ExecuteSqlAsync<TItem>(string sql, CancellationToken cancellationToken = default)
        {
            var asyncReq = _grpcClient.SelectSql(new SelectSqlRequest
            {
                DbName = _connectionString.DatabaseName,
                Sql = sql,
                Flags = _outputFlags
            });
            var result = new QueryItemsOf<TItem> { Items = new List<TItem>() };
            var hasResultOptSet = false;
            await foreach (var (queryItems, resultOpt) in asyncReq.HandleResponseAsync<TItem>(_serializer, cancellationToken))
            {
                if (!hasResultOptSet)
                {
                    if (resultOpt != null)
                    {
                        result.QueryTotalItems = resultOpt.TotalItems != 0 ? resultOpt.TotalItems : resultOpt.QueryTotalItems; //why?
                        result.Explain = !string.IsNullOrWhiteSpace(resultOpt.Explain) ? resultOpt.Explain.AsSpan().DeserializeJson<ExplainDef>() : new ExplainDef();
                        result.CacheEnabled = resultOpt.CacheEnabled;
                    }

                    hasResultOptSet = true;
                }
                if (queryItems.Items?.Count > 0)
                    result.Items.AddRange(queryItems.Items);
            }

            return result;
        }

        /// <inheritdoc/>
        public Task<QueryItemsOf<object>> ExecuteSqlAsync(string sql, CancellationToken cancellationToken = default)
        {
            return ExecuteSqlAsync<object>(sql, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task CreateDatabaseAsync(string dbName, CancellationToken cancellationToken = default)
        {
            (await _grpcClient.CreateDatabaseAsync(new CreateDatabaseRequest { DbName = dbName }, cancellationToken: cancellationToken))
                .HandleErrorResponse();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Database>> EnumDatabasesAsync(CancellationToken cancellationToken = default)
        {
            var rsp = await _grpcClient.EnumDatabasesAsync(new EnumDatabasesRequest(), cancellationToken: cancellationToken);
            rsp.ErrorResponse.HandleErrorResponse();
            return rsp.Names.Select(n => new Database { Name = n });
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Namespace>> EnumNamespacesAsync(CancellationToken cancellationToken = default)
        {
            var rsp = await _grpcClient.EnumNamespacesAsync(new EnumNamespacesRequest { DbName = _connectionString.DatabaseName },
                cancellationToken: cancellationToken);
            rsp.ErrorResponse.HandleErrorResponse();
            return rsp.NamespacesDefinitions.Select(ns => new Namespace
            {
                Name = ns.Name,
                Indexes = ns.IndexesDefinitions.Select(id => new Index
                {
                    CollateMode = id.Options.CollateMode switch
                    {
                        IndexOptions.Types.CollateMode.CollateNoneMode => "none",
                        IndexOptions.Types.CollateMode.CollateAsciimode => "ascii",
                        IndexOptions.Types.CollateMode.CollateNumericMode => "numeric",
                        IndexOptions.Types.CollateMode.CollateUtf8Mode => "utf-8",
                        IndexOptions.Types.CollateMode.CollateCustomMode => "custom",
                        _ => throw new NotImplementedException()
                    },
                    IsArray = id.Options.IsArray,
                    IsDense = id.Options.IsDense,
                    IsPk = id.Options.IsPk,
                    IsSparse = id.Options.IsSparse,
                    Name = id.Name,
                    JsonPaths = id.JsonPaths.ToList(),
                    SortOrderLetters = id.Options.SortOrdersTable,
                    IndexType = id.IndexType switch
                    {
                        "text" => IndexType.Text,
                        "tree" => IndexType.Tree,
                        "-" => IndexType.ColumnIndex,
                        "hash" => IndexType.Hash,
                        _ => throw new NotImplementedException()
                    },
                    FieldType = id.FieldType switch
                    {
                        "double" => FieldType.Double,
                        "int" => FieldType.Int,
                        "int64" => FieldType.Int64,
                        "composite" => FieldType.Composite,
                        "string" => FieldType.String,
                        "bool" => FieldType.Bool,
                        _ => throw new NotImplementedException(),
                    },
                    Config = !string.IsNullOrWhiteSpace(id.Options.Config) ? id.Options.Config.AsSpan().DeserializeJson<FulltextConfig>() : null
                }).ToList()
            });
        }

        /// <inheritdoc/>
        public async Task SetSchemaAsync(string nsName, string jsonSchema, CancellationToken cancellationToken = default)
        {
            (await _grpcClient.SetSchemaAsync(new SetSchemaRequest
            {
                DbName = _connectionString.DatabaseName,
                SchemaDefinitionRequest = new SchemaDefinition
                {
                    NsName = nsName,
                    JsonData = jsonSchema
                }
            }, cancellationToken: cancellationToken)).HandleErrorResponse();
        }

        /// <inheritdoc/>
        public async Task<string> GetMetaAsync(string nsName, MetaInfo metadata, CancellationToken cancellationToken = default)
        {
            var rsp = await _grpcClient.GetMetaAsync(new GetMetaRequest
            {
                DbName = _connectionString.DatabaseName,
                Metadata = new Reindexer.Grpc.Metadata
                {
                    NsName = nsName,
                    Key = metadata.Key,
                    Value = metadata.Value
                }
            }, cancellationToken: cancellationToken);
            rsp.ErrorResponse.HandleErrorResponse();
            return rsp.Metadata;
        }

        /// <inheritdoc/>
        public async Task PutMetaAsync(string nsName, MetaInfo metadata, CancellationToken cancellationToken = default)
        {
            (await _grpcClient.PutMetaAsync(new PutMetaRequest
            {
                DbName = _connectionString.DatabaseName,
                Metadata = new Reindexer.Grpc.Metadata
                {
                    NsName = nsName,
                    Key = metadata.Key,
                    Value = metadata.Value
                }
            }, cancellationToken: cancellationToken)).HandleErrorResponse();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> EnumMetaAsync(string nsName, CancellationToken cancellationToken = default)
        {
            var rsp = await _grpcClient.EnumMetaAsync(new EnumMetaRequest
            {
                DbName = _connectionString.DatabaseName,
                NsName = nsName
            }, cancellationToken: cancellationToken);

            rsp.ErrorResponse.HandleErrorResponse();
            return rsp.Keys;
        }
    }
}