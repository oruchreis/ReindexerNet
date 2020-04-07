using ReindexerNet.Embedded.Helpers;
using ReindexerNet.Embedded.Internal;
using System;
using System.Threading.Tasks;
using Utf8Json;
using System.Text;
using Utf8Json.Resolvers;
using System.Collections.Generic;
using System.IO;

namespace ReindexerNet.Embedded
{
    public partial class ReindexerEmbedded : IReindexerClient
    {
        public static ReindexerEmbeddedServer Server { get; } = new ReindexerEmbeddedServer();

        protected UIntPtr Rx;
        private reindexer_ctx_info _ctxInfo = new reindexer_ctx_info { ctx_id = 0, exec_timeout = -1 }; //TODO: Implement async/await logic.

        public ReindexerEmbedded()
        {
            Rx = ReindexerBinding.init_reindexer();
        }

        private byte[] SerializeJson<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, StandardResolver.ExcludeNull);
        }

        private readonly object _logWriterLocker = new object();
        private LogWriterAction _logWriter; //we must pin the delegate before informing to reindexer, so we keep a reference to it, so gc won't collect it.

        public void EnableLogger(LogWriterAction logWriterAction)
        {
            lock (_logWriterLocker)
            {
                ReindexerBinding.reindexer_disable_logger(); //if we free previous delegate before disabling, gc may collect before enabling.
                _logWriter = logWriterAction;
                ReindexerBinding.reindexer_enable_logger(_logWriter);
            }
        }

        public void DisableLogger()
        {
            lock (_logWriterLocker)
            {
                ReindexerBinding.reindexer_disable_logger();//if we free previous delegate before disabling, gc may collect before enabling.
                _logWriter = null;
            }
        }

        public void AddIndex(string nsName, params Index[] indexDefinitions)
        {
            foreach (var index in indexDefinitions)
            {
                if (index.JsonPaths == null)
                    index.JsonPaths = new List<string> { index.Name };
                Assert.ThrowIfError(() =>
                    ReindexerBinding.reindexer_add_index(Rx, nsName, SerializeJson(index), _ctxInfo)
                );
            }
        }

        public Task AddIndexAsync(string nsName, params Index[] indexDefinitions)
        {
            AddIndex(nsName, indexDefinitions);
            return Task.CompletedTask;
        }

        public void CloseNamespace(string nsName)
        {
            Assert.ThrowIfError(() =>
            ReindexerBinding.reindexer_close_namespace(Rx, nsName, _ctxInfo));
        }

        public Task CloseNamespaceAsync(string nsName)
        {
            CloseNamespace(nsName);
            return Task.CompletedTask;
        }

        public virtual void Connect(string connectionString, ConnectionOptions options = null)
        {
            if (!Directory.Exists(connectionString))
            {
                Directory.CreateDirectory(connectionString); //reindexer sometimes throws permission exception from c++ mkdir func. so we try to crate directory before.
                ReindexerBinding.reindexer_init_system_namespaces(Rx);
            }
            Assert.ThrowIfError(() =>
               ReindexerBinding.reindexer_connect(Rx,
                   $"builtin://{connectionString}",
                   options ?? new ConnectionOptions(), ReindexerBinding.ReindexerVersion)
           );
        }

        public Task ConnectAsync(string connectionString, ConnectionOptions options = null)
        {
            Connect(connectionString, options);
            return Task.CompletedTask;
        }

        public void DropIndex(string nsName, params string[] indexName)
        {
            foreach (var iname in indexName)
            {
                Assert.ThrowIfError(() =>
                    ReindexerBinding.reindexer_drop_index(Rx, nsName, iname, _ctxInfo)
                );
            }
        }

        public Task DropIndexAsync(string nsName, params string[] indexName)
        {
            DropIndex(nsName, indexName);
            return Task.CompletedTask;
        }

        public void DropNamespace(string nsName)
        {
            Assert.ThrowIfError(() =>
                ReindexerBinding.reindexer_drop_namespace(Rx, nsName, _ctxInfo)
            );
        }

        public Task DropNamespaceAsync(string nsName)
        {
            DropNamespace(nsName);
            return Task.CompletedTask;
        }

        public void OpenNamespace(string nsName, NamespaceOptions options = null)
        {
            Assert.ThrowIfError(() =>
                ReindexerBinding.reindexer_open_namespace(Rx, nsName, options ?? new NamespaceOptions(), _ctxInfo)
            );
        }

        public Task OpenNamespaceAsync(string nsName, NamespaceOptions options = null)
        {
            OpenNamespace(nsName, options);
            return Task.CompletedTask;
        }

        public void Ping()
        {
            Assert.ThrowIfError(() =>
                ReindexerBinding.reindexer_ping(Rx));
        }

        public Task PingAsync()
        {
            Ping();
            return Task.CompletedTask;
        }

        public void RenameNamespace(string oldName, string newName)
        {
            Assert.ThrowIfError(() =>
                ReindexerBinding.reindexer_rename_namespace(Rx, oldName, newName, _ctxInfo));
        }

        public Task RenameNamespaceAsync(string oldName, string newName)
        {
            RenameNamespace(oldName, newName);
            return Task.CompletedTask;
        }

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

        public Task<ReindexerTransaction> StartTransactionAsync(string nsName)
        {
            return Task.FromResult(StartTransaction(nsName));
        }

        public void TruncateNamespace(string nsName)
        {
            Assert.ThrowIfError(() =>
                ReindexerBinding.reindexer_truncate_namespace(Rx, nsName, _ctxInfo)
            );
        }

        public Task TruncateNamespaceAsync(string nsName)
        {
            TruncateNamespace(nsName);
            return Task.CompletedTask;
        }

        public void UpdateIndex(string nsName, params Index[] indexDefinitions)
        {
            foreach (var index in indexDefinitions)
            {
                Assert.ThrowIfError(() =>
                    ReindexerBinding.reindexer_update_index(Rx, nsName, SerializeJson(index), _ctxInfo)
                );
            }
        }

        public Task UpdateIndexAsync(string nsName, params Index[] indexDefinitions)
        {
            UpdateIndex(nsName, indexDefinitions);
            return Task.CompletedTask;
        }

        public int ModifyItem(string nsName, ItemModifyMode mode, byte[] itemJson, params string[] precepts)
        {
            var result = 0;

            using (var writer = new CJsonWriter())
            {
                writer.PutVString(nsName);
                writer.PutVarCUInt((int)DataFormat.FormatJson);//format;
                writer.PutVarCUInt((int)mode);//mode;
                writer.PutVarCUInt(0);//stateToken;

                writer.PutVarCUInt(precepts.Length);//len(precepts);
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

        public int ModifyItem(string nsName, ItemModifyMode mode, string itemJson, params string[] precepts)
        {
            return ModifyItem(nsName, mode, Encoding.UTF8.GetBytes(itemJson), precepts);
        }

        public Task<int> ModifyItemAsync(string nsName, ItemModifyMode mode, string itemJson, params string[] precepts)
        {
            return Task.FromResult(ModifyItem(nsName, mode, itemJson, precepts));
        }

        private QueryItemsOf<T> ExecuteSql<T>(string sql, Func<byte[], T> serializeFunc)
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
                        result.Items.Add(serializeFunc(item.data.ToArray())); //todo: use span when utf8json supports it.
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

        public QueryItemsOf<T> ExecuteSql<T>(string sql)
        {
            return ExecuteSql(sql, JsonSerializer.Deserialize<T>);
        }

        public Task<QueryItemsOf<T>> ExecuteSqlAsync<T>(string sql)
        {
            return Task.FromResult(ExecuteSql<T>(sql));
        }

        public QueryItemsOf<byte[]> ExecuteSql(string sql)
        {
            return ExecuteSql(sql, data => data);
        }

        public Task<QueryItemsOf<byte[]>> ExecuteSqlAsync(string sql)
        {
            return Task.FromResult(ExecuteSql(sql));
        }

        public int Insert<T>(string nsName, T item, params string[] precepts)
        {
            return ModifyItem(nsName, ItemModifyMode.ModeInsert, SerializeJson(item), precepts);
        }

        public int Update<T>(string nsName, T item, params string[] precepts)
        {
            return ModifyItem(nsName, ItemModifyMode.ModeUpdate, SerializeJson(item), precepts);
        }

        public int Upsert<T>(string nsName, T item, params string[] precepts)
        {
            return ModifyItem(nsName, ItemModifyMode.ModeUpsert, SerializeJson(item), precepts);
        }

        public int Delete<T>(string nsName, T item, params string[] precepts)
        {
            return ModifyItem(nsName, ItemModifyMode.ModeDelete, SerializeJson(item), precepts);
        }

        public Task<int> InsertAsync<T>(string nsName, T item, params string[] precepts)
        {
            return Task.FromResult(Insert<T>(nsName, item, precepts));
        }

        public Task<int> UpdateAsync<T>(string nsName, T item, params string[] precepts)
        {
            return Task.FromResult(Update<T>(nsName, item, precepts));
        }

        public Task<int> UpsertAsync<T>(string nsName, T item, params string[] precepts)
        {
            return Task.FromResult(Upsert<T>(nsName, item, precepts));
        }

        public Task<int> DeleteAsync<T>(string nsName, T item, params string[] precepts)
        {
            return Task.FromResult(Delete<T>(nsName, item, precepts));
        }
    }
}
