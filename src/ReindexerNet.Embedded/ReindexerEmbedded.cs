using ReindexerNet.Embedded.Helpers;
using ReindexerNet.Embedded.Internal;
using System;
using System.Threading.Tasks;
using Utf8Json;
using System.Text;
using Utf8Json.Resolvers;
using System.Collections.Generic;

namespace ReindexerNet.Embedded
{
    public partial class ReindexerEmbedded : IReindexerClient, IDisposable
    {
        private readonly UIntPtr _rx;
        private reindexer_ctx_info _ctxInfo = new reindexer_ctx_info { ctx_id = 0, exec_timeout = -1 }; //TODO: Implement async/await logic.

        public ReindexerEmbedded()
        {
            _rx = ReindexerBinding.init_reindexer();
        }

        private byte[] SerializeJson<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, Utf8Json.Resolvers.StandardResolver.ExcludeNull);
        }

        public void AddIndex(string nsName, params Index[] indexDefinitions)
        {
            foreach (var index in indexDefinitions)
            {
                Assert.ThrowIfError(() =>
                    ReindexerBinding.reindexer_add_index(_rx, nsName, SerializeJson(index), _ctxInfo)
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
            ReindexerBinding.reindexer_close_namespace(_rx, nsName, _ctxInfo));
        }

        public Task CloseNamespaceAsync(string nsName)
        {
            CloseNamespace(nsName);
            return Task.CompletedTask;
        }

        public void Connect(string connectionString, ConnectionOptions options)
        {
            Assert.ThrowIfError(() =>
               ReindexerBinding.reindexer_connect(_rx,
                   $"builtin://{connectionString}",
                   options, ReindexerBinding.ReindexerVersion)
           );
        }

        public Task ConnectAsync(string connectionString, ConnectionOptions options)
        {
            Connect(connectionString, options);
            return Task.CompletedTask;
        }

        public void DropIndex(string nsName, params string[] indexName)
        {
            foreach (var iname in indexName)
            {
                Assert.ThrowIfError(() =>
                    ReindexerBinding.reindexer_drop_index(_rx, nsName, iname, _ctxInfo)
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
                ReindexerBinding.reindexer_drop_namespace(_rx, nsName, _ctxInfo)
            );
        }

        public Task DropNamespaceAsync(string nsName)
        {
            DropNamespace(nsName);
            return Task.CompletedTask;
        }

        public void OpenNamespace(string nsName, NamespaceOptions options)
        {
            Assert.ThrowIfError(() =>
                ReindexerBinding.reindexer_open_namespace(_rx, nsName, options, _ctxInfo)
            );
        }

        public Task OpenNamespaceAsync(string nsName, NamespaceOptions options)
        {
            OpenNamespace(nsName, options);
            return Task.CompletedTask;
        }

        public void Ping()
        {
            Assert.ThrowIfError(() =>
                ReindexerBinding.reindexer_ping(_rx));
        }

        public Task PingAsync()
        {
            Ping();
            return Task.CompletedTask;
        }

        public void RenameNamespace(string oldName, string newName)
        {
            Assert.ThrowIfError(() =>
                ReindexerBinding.reindexer_rename_namespace(_rx, oldName, newName, _ctxInfo));
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
                var rsp = ReindexerBinding.reindexer_start_transaction(_rx, nsName);
                tr = rsp.tx_id;
                return rsp.err;
            });
            return new ReindexerTransaction(new EmbeddedTransactionInvoker(_rx, tr, _ctxInfo));
        }

        public Task<ReindexerTransaction> StartTransactionAsync(string nsName)
        {
            return Task.FromResult(StartTransaction(nsName));
        }

        public void TruncateNamespace(string nsName)
        {
            Assert.ThrowIfError(() =>
                ReindexerBinding.reindexer_truncate_namespace(_rx, nsName, _ctxInfo)
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
                    ReindexerBinding.reindexer_update_index(_rx, nsName, SerializeJson(index), _ctxInfo)
                );
            }
        }

        public Task UpdateIndexAsync(string nsName, params Index[] indexDefinitions)
        {
            UpdateIndex(nsName, indexDefinitions);
            return Task.CompletedTask;
        }

        public int ModifyItem(string nsName, ItemModifyMode mode, string itemJson, params string[] precepts)
        {
            var result = 0;
            Assert.ThrowIfError(() =>
            {
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

                    var error = new reindexer_error();
                    reindexer_buffer.PinBufferFor(writer.CurrentBuffer, args =>
                    {
                        using (var data = reindexer_buffer.From(Encoding.UTF8.GetBytes(itemJson ?? $"{{\"Id\":1, \"Guid\":\"{Guid.NewGuid()}\"}}")))
                        {
                            var rsp = ReindexerBinding.reindexer_modify_item_packed(_rx, args, data.Buffer, _ctxInfo);
                            if (rsp.err_code != 0)
                            {
                                error.code = rsp.err_code;
                                error.what = rsp.@out;
                                return;
                            }

                            var reader = new CJsonReader(rsp.@out);
                            var rawQueryParams = reader.ReadRawQueryParams(null);

                            result = rawQueryParams.count;
                        }
                    });
                    return error;
                }
            });

            return result;
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
            Assert.ThrowIfError(() =>
            {
                var rsp = ReindexerBinding.reindexer_select(_rx, sql, 1, new int[0], 0, _ctxInfo);
                var error = new reindexer_error();

                if (rsp.err_code != 0)
                {
                    error.code = rsp.err_code;
                    error.what = rsp.@out;
                    return error;
                }

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
                return error;
            });
            return result;
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
    }
}
