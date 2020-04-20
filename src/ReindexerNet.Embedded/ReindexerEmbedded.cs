#pragma warning disable S1135 // Track uses of "TODO" tags
#pragma warning disable S4136 // Method overloads should be grouped together
using ReindexerNet.Embedded.Helpers;
using ReindexerNet.Embedded.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Utf8Json;
using Utf8Json.Resolvers;

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

        /// <summary>
        /// Creates a new embedded Reindexer database.
        /// </summary>
        public ReindexerEmbedded()
        {
            Rx = ReindexerBinding.init_reindexer();
        }

        private byte[] SerializeJson<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, StandardResolver.ExcludeNull);
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
        public void AddIndex(string nsName, params Index[] indexDefinitions)
        {
            foreach (var index in indexDefinitions)
            {
                if (index.JsonPaths == null || index.JsonPaths.Count == 0)
                    index.JsonPaths = new List<string> { index.Name };
                Assert.ThrowIfError(() =>
                    ReindexerBinding.reindexer_add_index(Rx, nsName, SerializeJson(index), _ctxInfo)
                );
            }
        }

        /// <inheritdoc/>
        public Task AddIndexAsync(string nsName, params Index[] indexDefinitions)
        {
            AddIndex(nsName, indexDefinitions);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void CloseNamespace(string nsName)
        {
            Assert.ThrowIfError(() =>
            ReindexerBinding.reindexer_close_namespace(Rx, nsName, _ctxInfo));
        }

        /// <inheritdoc/>
        public Task CloseNamespaceAsync(string nsName)
        {
            CloseNamespace(nsName);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual void Connect(string connectionString, ConnectionOptions options = null)
        {
            if (!Directory.Exists(connectionString))
            {
                Directory.CreateDirectory(connectionString); //reindexer sometimes throws permission exception from c++ mkdir func. so we try to crate directory before.
            }
            Assert.ThrowIfError(() =>
               ReindexerBinding.reindexer_connect(Rx,
                   $"builtin://{connectionString}",
                   options ?? new ConnectionOptions(), ReindexerBinding.ReindexerVersion)
           );
        }

        /// <inheritdoc/>
        public Task ConnectAsync(string connectionString, ConnectionOptions options = null)
        {
            Connect(connectionString, options);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void DropIndex(string nsName, params string[] indexName)
        {
            foreach (var iname in indexName)
            {
                Assert.ThrowIfError(() =>
                    ReindexerBinding.reindexer_drop_index(Rx, nsName, iname, _ctxInfo)
                );
            }
        }

        /// <inheritdoc/>
        public Task DropIndexAsync(string nsName, params string[] indexName)
        {
            DropIndex(nsName, indexName);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void DropNamespace(string nsName)
        {
            Assert.ThrowIfError(() =>
                ReindexerBinding.reindexer_drop_namespace(Rx, nsName, _ctxInfo)
            );
        }

        /// <inheritdoc/>
        public Task DropNamespaceAsync(string nsName)
        {
            DropNamespace(nsName);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void OpenNamespace(string nsName, NamespaceOptions options = null)
        {
            Assert.ThrowIfError(() =>
            {
                reindexer_error rsp = default;
                for (int retry = 0; retry < 2; retry++)
                {
                    rsp = ReindexerBinding.reindexer_open_namespace(Rx, nsName, options ?? new NamespaceOptions(), _ctxInfo);
                    if (rsp.code != 0)
                    {
                        ReindexerBinding.reindexer_close_namespace(Rx, nsName, _ctxInfo);
                    }
                }
                return rsp;
            });
        }

        /// <inheritdoc/>
        public Task OpenNamespaceAsync(string nsName, NamespaceOptions options = null)
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
        public Task PingAsync()
        {
            Ping();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void RenameNamespace(string oldName, string newName)
        {
            Assert.ThrowIfError(() =>
                ReindexerBinding.reindexer_rename_namespace(Rx, oldName, newName, _ctxInfo));
        }

        /// <inheritdoc/>
        public Task RenameNamespaceAsync(string oldName, string newName)
        {
            RenameNamespace(oldName, newName);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public ReindexerTransaction StartTransaction(string nsName)
        {
            UIntPtr tr = UIntPtr.Zero;
            Assert.ThrowIfError(() =>
            {
                var rsp = ReindexerBinding.reindexer_start_transaction(Rx, nsName);
                tr = rsp.tx_id;
                return rsp.err;
            });
            return new ReindexerTransaction(new EmbeddedTransactionInvoker(Rx, tr, _ctxInfo));
        }

        /// <inheritdoc/>
        public Task<ReindexerTransaction> StartTransactionAsync(string nsName)
        {
            return Task.FromResult(StartTransaction(nsName));
        }

        /// <inheritdoc/>
        public void TruncateNamespace(string nsName)
        {
            Assert.ThrowIfError(() =>
                ReindexerBinding.reindexer_truncate_namespace(Rx, nsName, _ctxInfo)
            );
        }

        /// <inheritdoc/>
        public Task TruncateNamespaceAsync(string nsName)
        {
            TruncateNamespace(nsName);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void UpdateIndex(string nsName, params Index[] indexDefinitions)
        {
            foreach (var index in indexDefinitions)
            {
                if (index.JsonPaths == null || index.JsonPaths.Count == 0)
                    index.JsonPaths = new List<string> { index.Name };
                Assert.ThrowIfError(() =>
                    ReindexerBinding.reindexer_update_index(Rx, nsName, SerializeJson(index), _ctxInfo)
                );
            }
        }

        /// <inheritdoc/>
        public Task UpdateIndexAsync(string nsName, params Index[] indexDefinitions)
        {
            UpdateIndex(nsName, indexDefinitions);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public int ModifyItem(string nsName, ItemModifyMode mode, byte[] itemJson, params string[] precepts)
        {
            var result = 0;

            using (var writer = new CJsonWriter())
            {
                writer.PutVString(nsName);
                writer.PutVarCUInt((int)DataFormat.FormatJson);//format
                writer.PutVarCUInt((int)mode);//mode
                writer.PutVarCUInt(0);//stateToken

                writer.PutVarCUInt(precepts.Length);//len(precepts)
                foreach (var precept in precepts)
                {
                    writer.PutVString(precept);
                }

                reindexer_buffer.PinBufferFor(writer.CurrentBuffer, args =>
                {
                    using (var data = reindexer_buffer.From(itemJson))
                    {
                        var rsp = Assert.ThrowIfError(() => ReindexerBinding.reindexer_modify_item_packed(Rx, args, data.Buffer, _ctxInfo));
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
                    }
                });
            }

            return result;
        }

        /// <inheritdoc/>
        public int ModifyItem(string nsName, ItemModifyMode mode, string itemJson, params string[] precepts)
        {
            return ModifyItem(nsName, mode, Encoding.UTF8.GetBytes(itemJson), precepts);
        }

        /// <inheritdoc/>
        public Task<int> ModifyItemAsync(string nsName, ItemModifyMode mode, string itemJson, params string[] precepts)
        {
            return Task.FromResult(ModifyItem(nsName, mode, itemJson, precepts));
        }

        /// <inheritdoc/>
        public QueryItemsOf<T> ExecuteSql<T>(string sql, Func<byte[], T> deserializeItem)
        {
            var result = new QueryItemsOf<T>
            {
                Items = new List<T>()
            };

            var rsp = Assert.ThrowIfError(() => ReindexerBinding.reindexer_select(Rx, sql, 1, new int[0], 0, _ctxInfo));
            try
            {
                var reader = new CJsonReader(rsp.@out);
                var rawQueryParams = reader.ReadRawQueryParams();
                var explain = rawQueryParams.explainResults;

                result.QueryTotalItems = rawQueryParams.totalcount != 0 ? rawQueryParams.totalcount : rawQueryParams.count;
                if (explain.Length > 0)
                {
                    result.Explain = JsonSerializer.Deserialize<ExplainDef>(explain.ToArray(), //todo: use span when utf8json supports it.
                        StandardResolver.ExcludeNull);
                }

                for (var i = 0; i < rawQueryParams.count; i++)
                {
                    var item = reader.ReadRawItemParams();
                    if (item.data.Length > 0)
                        result.Items.Add(deserializeItem(item.data.ToArray())); //todo: use span when utf8json supports it.
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

        /// <inheritdoc/>
        public QueryItemsOf<T> ExecuteSql<T>(string sql)
        {
            return ExecuteSql(sql, JsonSerializer.Deserialize<T>);
        }

        /// <inheritdoc/>
        public Task<QueryItemsOf<T>> ExecuteSqlAsync<T>(string sql, Func<byte[], T> deserializeItem)
        {
            return Task.FromResult(ExecuteSql(sql, deserializeItem));
        }

        /// <inheritdoc/>
        public Task<QueryItemsOf<T>> ExecuteSqlAsync<T>(string sql)
        {
            return Task.FromResult(ExecuteSql<T>(sql));
        }

        /// <inheritdoc/>
        public QueryItemsOf<byte[]> ExecuteSql(string sql)
        {
            return ExecuteSql(sql, data => data);
        }

        /// <inheritdoc/>
        public Task<QueryItemsOf<byte[]>> ExecuteSqlAsync(string sql)
        {
            return Task.FromResult(ExecuteSql(sql));
        }

        /// <inheritdoc/>
        public int Insert<T>(string nsName, T item, params string[] precepts)
        {
            return ModifyItem(nsName, ItemModifyMode.Insert, SerializeJson(item), precepts);
        }

        /// <inheritdoc/>
        public int Update<T>(string nsName, T item, params string[] precepts)
        {
            return ModifyItem(nsName, ItemModifyMode.Update, SerializeJson(item), precepts);
        }

        /// <inheritdoc/>
        public int Upsert<T>(string nsName, T item, params string[] precepts)
        {
            return ModifyItem(nsName, ItemModifyMode.Upsert, SerializeJson(item), precepts);
        }

        /// <inheritdoc/>
        public int Delete<T>(string nsName, T item, params string[] precepts)
        {
            return ModifyItem(nsName, ItemModifyMode.Delete, SerializeJson(item), precepts);
        }

        /// <inheritdoc/>
        public Task<int> InsertAsync<T>(string nsName, T item, params string[] precepts)
        {
            return Task.FromResult(Insert<T>(nsName, item, precepts));
        }

        /// <inheritdoc/>
        public Task<int> UpdateAsync<T>(string nsName, T item, params string[] precepts)
        {
            return Task.FromResult(Update<T>(nsName, item, precepts));
        }

        /// <inheritdoc/>
        public Task<int> UpsertAsync<T>(string nsName, T item, params string[] precepts)
        {
            return Task.FromResult(Upsert<T>(nsName, item, precepts));
        }

        /// <inheritdoc/>
        public Task<int> DeleteAsync<T>(string nsName, T item, params string[] precepts)
        {
            return Task.FromResult(Delete<T>(nsName, item, precepts));
        }
    }
}
#pragma warning restore S4136 // Method overloads should be grouped together
#pragma warning restore S1135 // Track uses of "TODO" tags