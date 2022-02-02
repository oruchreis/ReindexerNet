#pragma warning disable S1135 // Track uses of "TODO" tags
#pragma warning disable S4136 // Method overloads should be grouped together
using ReindexerNet.Embedded.Internal.Helpers;
using ReindexerNet.Embedded.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;
using System.Threading;

namespace ReindexerNet.Embedded
{
    /// <summary>
    /// Reindexer Embedded database mode. It creates a new embedded Reindexer native database and disposes on <see cref="IDisposable.Dispose"/> method.
    /// It is thread-safe, so you can use in multiple threads. If your database will be long lived, you don't have to dispose.
    /// </summary>
    public partial class ReindexerEmbedded : IReindexerClient
    {
        /// <summary>
        /// Reindexer native object pointer.
        /// </summary>
        protected UIntPtr Rx;
        private reindexer_ctx_info _ctxInfo = new reindexer_ctx_info { ctx_id = 0, exec_timeout = -1 }; //TODO: Implement async/await logic.

        private readonly ReindexerConnectionString _connectionString;

        protected IReindexerSerializer Serializer { get; }

        /// <summary>
        /// Creates a new embedded Reindexer database.
        /// </summary>
        /// <param name="dbPath">Database path</param>        
        public ReindexerEmbedded(string dbPath, IReindexerSerializer serializer = null)
        {
            _connectionString = new ReindexerConnectionString { DatabaseName = dbPath };
            Serializer = serializer ?? new ReindexerJsonSerializer();
            Rx = ReindexerBinding.init_reindexer();
        }

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
#if NET5_0_OR_GREATER
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
#else
            IgnoreNullValues = true,
#endif
        };

        private byte[] InternalSerializeJson<T>(T obj)
        {
            return JsonSerializer.SerializeToUtf8Bytes(obj, _jsonSerializerOptions);
        }

        private static readonly object _logWriterLocker = new object();
        private static LogWriterAction _logWriter; //we must pin the delegate before informing to reindexer, so we keep a reference to it, so gc won't collect it.

        /// <summary>
        /// Enables logger and send internal reindexer logs to <paramref name="logWriterAction"/>.
        /// </summary>
        /// <param name="logWriterAction">Action to send logs</param>
        public static void EnableLogger(LogWriterAction logWriterAction)
        {
            lock (_logWriterLocker)
            {
                ReindexerBinding.reindexer_disable_logger(); //if we free previous delegate before disabling, gc may collect before enabling.
                _logWriter = logWriterAction;
                ReindexerBinding.reindexer_enable_logger(_logWriter);
            }
        }

        /// <summary>
        /// Disables logger.
        /// </summary>
        public static void DisableLogger()
        {
            lock (_logWriterLocker)
            {
                ReindexerBinding.reindexer_disable_logger();//if we free previous delegate before disabling, gc may collect before enabling.
                _logWriter = null;
            }
        }

        /// <inheritdoc/>
        public void AddIndex(string nsName, Index[] indexDefinitions)
        {
            using (var nsNameRx = nsName.GetHandle())
                foreach (var index in indexDefinitions)
                {
                    if (index.JsonPaths == null || index.JsonPaths.Count == 0)
                        index.JsonPaths = new List<string> { index.Name };
                    using (var jsonRx = InternalSerializeJson(index).GetStringHandle())
                        Assert.ThrowIfError(() =>
                                ReindexerBinding.reindexer_add_index(Rx, nsNameRx, jsonRx, _ctxInfo)
                        );
                }
        }

        /// <inheritdoc/>
        public Task AddIndexAsync(string nsName, Index[] indexDefinitions, CancellationToken cancellationToken = default)
        {
            AddIndex(nsName, indexDefinitions);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void CloseNamespace(string nsName)
        {
            using (var nsNameRx = nsName.GetHandle())
                Assert.ThrowIfError(() =>
                    ReindexerBinding.reindexer_close_namespace(Rx, nsNameRx, _ctxInfo));
        }

        /// <inheritdoc/>
        public Task CloseNamespaceAsync(string nsName, CancellationToken cancellationToken = default)
        {
            CloseNamespace(nsName);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual void Connect(ConnectionOptions options = null)
        {
            if (!Directory.Exists(_connectionString.DatabaseName))
            {
                Directory.CreateDirectory(_connectionString.DatabaseName); //reindexer sometimes throws permission exception from c++ mkdir func. so we try to crate directory before.
            }

            using (var dsn = $"builtin://{_connectionString.DatabaseName}".GetHandle())
            using (var version = ReindexerBinding.ReindexerVersion.GetHandle())
                Assert.ThrowIfError(() =>
                   ReindexerBinding.reindexer_connect(Rx,
                       dsn,
                       options ?? new ConnectionOptions(),
                       version)
               );
        }

        /// <inheritdoc/>
        public Task ConnectAsync(ConnectionOptions options = null, CancellationToken cancellationToken = default)
        {
            Connect(options);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void DropIndex(string nsName, string[] indexName)
        {
            using (var nsNameRx = nsName.GetHandle())
                foreach (var iname in indexName)
                {
                    using (var inameRx = iname.GetHandle())
                        Assert.ThrowIfError(() =>
                            ReindexerBinding.reindexer_drop_index(Rx, nsNameRx, inameRx, _ctxInfo)
                        );
                }
        }

        /// <inheritdoc/>
        public Task DropIndexAsync(string nsName, string[] indexName, CancellationToken cancellationToken = default)
        {
            DropIndex(nsName, indexName);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void DropNamespace(string nsName)
        {
            using (var nsNameRx = nsName.GetHandle())
                Assert.ThrowIfError(() =>
                    ReindexerBinding.reindexer_drop_namespace(Rx, nsNameRx, _ctxInfo)
                );
        }

        /// <inheritdoc/>
        public Task DropNamespaceAsync(string nsName, CancellationToken cancellationToken = default)
        {
            DropNamespace(nsName);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void OpenNamespace(string nsName, NamespaceOptions options = null)
        {
            using (var nsNameRx = nsName.GetHandle())
                Assert.ThrowIfError(() =>
                {
                    reindexer_error rsp = default;
                    for (int retry = 0; retry < 2; retry++)
                    {
                        rsp = ReindexerBinding.reindexer_open_namespace(Rx, nsNameRx, options ?? new NamespaceOptions(), _ctxInfo);
                        if (rsp.code != 0)
                        {
                            ReindexerBinding.reindexer_close_namespace(Rx, nsNameRx, _ctxInfo);
                        }
                    }
                    return rsp;
                });
        }

        /// <inheritdoc/>
        public Task OpenNamespaceAsync(string nsName, NamespaceOptions options = null, CancellationToken cancellationToken = default)
        {
            OpenNamespace(nsName, options);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Ping()
        {
            Assert.ThrowIfError(() =>
                ReindexerBinding.reindexer_ping(Rx));
        }

        /// <inheritdoc/>
        public Task PingAsync(CancellationToken cancellationToken = default)
        {
            Ping();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void RenameNamespace(string oldName, string newName)
        {
            using (var oldNameRx = oldName.GetHandle())
            using (var newNameRx = newName.GetHandle())
                Assert.ThrowIfError(() =>
                    ReindexerBinding.reindexer_rename_namespace(Rx, oldNameRx, newNameRx, _ctxInfo));
        }

        /// <inheritdoc/>
        public Task RenameNamespaceAsync(string oldName, string newName, CancellationToken cancellationToken = default)
        {
            RenameNamespace(oldName, newName);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public ReindexerTransaction StartTransaction(string nsName)
        {
            UIntPtr tr = UIntPtr.Zero;
            using (var nsNameRx = nsName.GetHandle())
                Assert.ThrowIfError(() =>
                {
                    var rsp = ReindexerBinding.reindexer_start_transaction(Rx, nsNameRx);
                    tr = rsp.tx_id;
                    return rsp.err;
                });
            return new ReindexerTransaction(new EmbeddedTransactionInvoker(Rx, tr, _ctxInfo, Serializer));
        }

        /// <inheritdoc/>
        public Task<ReindexerTransaction> StartTransactionAsync(string nsName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(StartTransaction(nsName));
        }

        /// <inheritdoc/>
        public void TruncateNamespace(string nsName)
        {
            using (var nsNameRx = nsName.GetHandle())
                Assert.ThrowIfError(() =>
                    ReindexerBinding.reindexer_truncate_namespace(Rx, nsNameRx, _ctxInfo)
                );
        }

        /// <inheritdoc/>
        public Task TruncateNamespaceAsync(string nsName, CancellationToken cancellationToken = default)
        {
            TruncateNamespace(nsName);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void UpdateIndex(string nsName, Index[] indexDefinitions)
        {
            using (var nsNameRx = nsName.GetHandle())
                foreach (var index in indexDefinitions)
                {
                    if (index.JsonPaths == null || index.JsonPaths.Count == 0)
                        index.JsonPaths = new List<string> { index.Name };
                    using (var jsonRx = InternalSerializeJson(index).GetStringHandle())
                        Assert.ThrowIfError(() =>
                            ReindexerBinding.reindexer_update_index(Rx, nsNameRx, jsonRx, _ctxInfo)
                        );
                }
        }

        /// <inheritdoc/>
        public Task UpdateIndexAsync(string nsName, Index[] indexDefinitions, CancellationToken cancellationToken = default)
        {
            UpdateIndex(nsName, indexDefinitions);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public int ModifyItem(string nsName, ItemModifyMode mode, ReadOnlySpan<byte> itemBytes, string[] precepts = null)
        {
            var result = 0;
            precepts = precepts ?? new string[0];
            using (var writer = new CJsonWriter())
            {
                writer.PutVString(nsName);
                writer.PutVarCUInt((int)Serializer.Type);//format
                writer.PutVarCUInt((int)mode);//mode
                writer.PutVarCUInt(0);//stateToken

                writer.PutVarCUInt(precepts.Length);//len(precepts)
                foreach (var precept in precepts)
                {
                    writer.PutVString(precept);
                }

                reindexer_buffer.PinBufferFor(writer.CurrentBuffer, itemBytes, (args, data) =>
                {
                    var rsp = Assert.ThrowIfError(() => ReindexerBinding.reindexer_modify_item_packed(Rx, args, data, _ctxInfo));
                    try
                    {
                        var reader = new CJsonReader(rsp.@out);
                        var rawQueryParams = reader.ReadRawQueryParams();

                        result = rawQueryParams.count;
                    }
                    finally
                    {
                        rsp.@out.Free();
                    }
                });
            }

            return result;
        }

        /// <inheritdoc/>
        public int ModifyItems<TItem>(string nsName, ItemModifyMode mode, IEnumerable<TItem> items, string[] precepts = null)
        {
            var result = 0;
            foreach (var item in items)
            {
                result += ModifyItem(nsName, mode, Serializer.Serialize(item), precepts);
            }

            return result;
        }

        /// <inheritdoc/>
        public Task<int> ModifyItemsAsync<TItem>(string nsName, ItemModifyMode mode, IEnumerable<TItem> items,
            string[] precepts = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ModifyItems(nsName, mode, items, precepts));
        }

        /// <inheritdoc/>
        public QueryItemsOf<T> ExecuteSql<T>(string sql)
        {
            var result = new QueryItemsOf<T>
            {
                Items = new List<T>()
            };

            using (var sqlRx = sql.GetHandle())
            {
                var rsp = Assert.ThrowIfError(() => ReindexerBinding.reindexer_select(Rx, sqlRx, 1, new int[0], 0, _ctxInfo));
                try
                {
                    var reader = new CJsonReader(rsp.@out);
                    var rawQueryParams = reader.ReadRawQueryParams();
                    var explain = rawQueryParams.explainResults;

                    result.QueryTotalItems = rawQueryParams.totalcount != 0 ? rawQueryParams.totalcount : rawQueryParams.count;
                    if (explain.Length > 0)
                    {
                        result.Explain = JsonSerializer.Deserialize<ExplainDef>(explain, _jsonSerializerOptions);
                    }

                    for (var i = 0; i < rawQueryParams.count; i++)
                    {
                        var item = reader.ReadRawItemParams();
                        if (item.data.Length > 0)
                            result.Items.Add(Serializer.Deserialize<T>(item.data));
                    }

                    if ((rawQueryParams.flags & CJsonReader.ResultsWithJoined) != 0 && reader.GetVarUInt() != 0)
                    {
                        throw new NotImplementedException("Sorry, not implemented: Can't return join query results as json");
                    }

                    return result;
                }
                finally
                {
                    rsp.@out.Free();
                }
            }
        }

        /// <inheritdoc/>
        public Task<QueryItemsOf<T>> ExecuteSqlAsync<T>(string sql, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ExecuteSql<T>(sql));
        }

        /// <inheritdoc/>
        public QueryItemsOf<object> ExecuteSql(string sql)
        {
            return ExecuteSql<object>(sql);
        }

        /// <inheritdoc/>
        public Task<QueryItemsOf<object>> ExecuteSqlAsync(string sql, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ExecuteSql<object>(sql));
        }

        /// <inheritdoc/>
        public int Insert<T>(string nsName, IEnumerable<T> items, string[] precepts = null)
        {
            return ModifyItems(nsName, ItemModifyMode.Insert, items, precepts);
        }

        /// <inheritdoc/>
        public int Update<T>(string nsName, IEnumerable<T> items, string[] precepts = null)
        {
            return ModifyItems(nsName, ItemModifyMode.Update, items, precepts);
        }

        /// <inheritdoc/>
        public int Upsert<T>(string nsName, IEnumerable<T> items, string[] precepts = null)
        {
            return ModifyItems(nsName, ItemModifyMode.Upsert, items, precepts);
        }

        /// <inheritdoc/>
        public int Delete<T>(string nsName, IEnumerable<T> items, string[] precepts = null)
        {
            return ModifyItems(nsName, ItemModifyMode.Delete, items, precepts);
        }

        /// <inheritdoc/>
        public Task<int> InsertAsync<T>(string nsName, IEnumerable<T> items, string[] precepts = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Insert<T>(nsName, items, precepts));
        }

        /// <inheritdoc/>
        public Task<int> UpdateAsync<T>(string nsName, IEnumerable<T> items, string[] precepts = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Update<T>(nsName, items, precepts));
        }

        /// <inheritdoc/>
        public Task<int> UpsertAsync<T>(string nsName, IEnumerable<T> items, string[] precepts = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Upsert<T>(nsName, items, precepts));
        }

        /// <inheritdoc/>
        public Task<int> DeleteAsync<T>(string nsName, IEnumerable<T> items, string[] precepts = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Delete<T>(nsName, items, precepts));
        }

        public void CreateDatabase(string dbName)
        {
            var newRx = ReindexerBinding.init_reindexer();
            using (var dsn = $"builtin://{dbName}".GetHandle())
            using (var version = ReindexerBinding.ReindexerVersion.GetHandle())
                Assert.ThrowIfError(() =>
                   ReindexerBinding.reindexer_connect(newRx,
                       dsn,
                       new ConnectionOptions(),
                       version)
               );
            ReindexerBinding.destroy_reindexer(newRx);
        }

        public Task CreateDatabaseAsync(string dbName, CancellationToken cancellationToken = default)
        {
            CreateDatabase(dbName);
            return Task.CompletedTask;
        }

        public IEnumerable<Database> EnumDatabases()
        {
            var dbPath = _connectionString.DatabaseName;

            return Directory.GetParent(dbPath).EnumerateDirectories().Select(d => new Database { Name = d.Name });
        }

        public Task<IEnumerable<Database>> EnumDatabasesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(EnumDatabases());
        }

        public IEnumerable<Namespace> EnumNamespaces()
        {
            return ExecuteSql<Namespace>(GetNamespacesQuery).Items;
        }

        public Task<IEnumerable<Namespace>> EnumNamespacesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(EnumNamespaces());
        }

        public void SetSchema(string nsName, string jsonSchema)
        {
            using var nsNameRx = nsName.GetHandle();
            using var jsonSchemaRx = jsonSchema.GetHandle();

            Assert.ThrowIfError(() =>
                ReindexerBinding.reindexer_set_schema(Rx, nsNameRx, jsonSchemaRx, _ctxInfo));
        }

        public Task SetSchemaAsync(string nsName, string jsonSchema, CancellationToken cancellationToken = default)
        {
            SetSchema(nsName, jsonSchema);
            return Task.CompletedTask;
        }

        public string GetMeta(string nsName, MetaInfo metadata)
        {
            using var nsNameRx = nsName.GetHandle();
            using var keyRx = metadata.Key.GetHandle();
            var rsp = Assert.ThrowIfError(() => ReindexerBinding.reindexer_get_meta(Rx, nsNameRx, keyRx, _ctxInfo));
            try
            {
                return rsp.@out;
            }
            finally
            {
                rsp.@out.Free();
            }
        }

        public Task<string> GetMetaAsync(string nsName, MetaInfo metadata, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GetMeta(nsName, metadata));
        }

        public void PutMeta(string nsName, MetaInfo metadata)
        {
            using var nsNameRx = nsName.GetHandle();
            using var keyRx = metadata.Key.GetHandle();
            using var dataRx = metadata.Value.GetHandle();
            Assert.ThrowIfError(() => ReindexerBinding.reindexer_put_meta(Rx, nsNameRx, keyRx, dataRx, _ctxInfo));
        }

        public Task PutMetaAsync(string nsName, MetaInfo metadata, CancellationToken cancellationToken = default)
        {
            PutMeta(nsName, metadata);
            return Task.CompletedTask;
        }

        public IEnumerable<string> EnumMeta(string nsName)
        {
            throw new NotImplementedException();//TODO: c binding doesn't support this, get via sql script
        }

        public Task<IEnumerable<string>> EnumMetaAsync(string nsName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(EnumMeta(nsName));
        }
    }
}
#pragma warning restore S4136 // Method overloads should be grouped together
#pragma warning restore S1135 // Track uses of "TODO" tags