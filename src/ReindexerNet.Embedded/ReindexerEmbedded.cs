#pragma warning disable S1135 // Track uses of "TODO" tags
#pragma warning disable S4136 // Method overloads should be grouped together
using ReindexerNet.Embedded.Internal.Helpers;
using ReindexerNet.Embedded.Internal;
using ReindexerNet.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;
using System.Threading;
using System.Linq.Expressions;

namespace ReindexerNet.Embedded;

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

    /// <summary>
    /// Item serializer
    /// </summary>
    protected IReindexerSerializer Serializer { get; }

    /// <summary>
    /// Creates a new embedded Reindexer database.
    /// </summary>
    /// <param name="dbPath">Database path</param>
    /// <param name="serializer">Custom serializer for item serializations. default(Json serializer)</param>        
    public ReindexerEmbedded(string dbPath, IReindexerSerializer serializer = null)
    {
        _connectionString = new ReindexerConnectionString { DatabaseName = dbPath };
        Serializer = serializer ?? new ReindexerJsonSerializer();
        Rx = ReindexerBinding.init_reindexer();
    }

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly ReindexerJsonSerializer _defaultJsonSerializer = new();

    private static byte[] InternalSerializeJson<T>(T obj)
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
    public void AddIndex(string nsName, Index indexDefinition)
    {
        using var nsNameRx = nsName.GetStringHandle();
        if (indexDefinition.JsonPaths == null || indexDefinition.JsonPaths.Count == 0)
            indexDefinition.JsonPaths = [indexDefinition.Name];
        using var jsonRx = ReindexerEmbedded.InternalSerializeJson(indexDefinition).GetStringHandle();
        Assert.ThrowIfError(() =>
                ReindexerBinding.reindexer_add_index(Rx, nsNameRx, jsonRx, _ctxInfo)
        );
    }

    /// <inheritdoc/>
    public Task AddIndexAsync(string nsName, Index indexDefinition, CancellationToken cancellationToken = default)
    {
        AddIndex(nsName, indexDefinition);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void CloseNamespace(string nsName)
    {
        using var nsNameRx = nsName.GetStringHandle();
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

        using var dsn = $"builtin://{_connectionString.DatabaseName}".GetStringHandle();
        using var version = ReindexerBinding.ReindexerVersion.GetStringHandle();
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
    public void DropIndex(string nsName, string indexName)
    {
        using var nsNameRx = nsName.GetStringHandle();
        using var inameRx = indexName.GetStringHandle();
        Assert.ThrowIfError(() =>
            ReindexerBinding.reindexer_drop_index(Rx, nsNameRx, inameRx, _ctxInfo)
        );
    }

    /// <inheritdoc/>
    public Task DropIndexAsync(string nsName, string indexName, CancellationToken cancellationToken = default)
    {
        DropIndex(nsName, indexName);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void DropNamespace(string nsName)
    {
        using var nsNameRx = nsName.GetStringHandle();
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
        using (var nsNameRx = nsName.GetStringHandle())
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
        using var oldNameRx = oldName.GetStringHandle();
        using var newNameRx = newName.GetStringHandle();
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
        using (var nsNameRx = nsName.GetStringHandle())
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
        using var nsNameRx = nsName.GetStringHandle();
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
    public void UpdateIndex(string nsName, Index indexDefinition)
    {
        using var nsNameRx = nsName.GetStringHandle();
        if (indexDefinition.JsonPaths == null || indexDefinition.JsonPaths.Count == 0)
            indexDefinition.JsonPaths = [indexDefinition.Name];
        using var jsonRx = ReindexerEmbedded.InternalSerializeJson(indexDefinition).GetStringHandle();
        Assert.ThrowIfError(() =>
            ReindexerBinding.reindexer_update_index(Rx, nsNameRx, jsonRx, _ctxInfo)
        );

    }

    /// <inheritdoc/>
    public Task UpdateIndexAsync(string nsName, Index indexDefinition, CancellationToken cancellationToken = default)
    {
        UpdateIndex(nsName, indexDefinition);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public int ModifyItem(string nsName, ItemModifyMode mode, ReadOnlySpan<byte> itemBytes, SerializerType dataEncoding, string[] precepts = null)
    {
        var result = 0;
        precepts = precepts ?? [];
        using (var writer = new CJsonWriter())
        {
            writer.PutVString(nsName);
            writer.PutVarCUInt((int)dataEncoding);//format
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
            result += ModifyItem(nsName, mode, Serializer.Serialize(item), Serializer.Type, precepts);
        }

        return result;
    }

    /// <inheritdoc/>
    public int ModifyItems(string nsName, ItemModifyMode mode, IEnumerable<byte[]> itemDatas, SerializerType dataEncoding, string[] precepts = null)
    {
        var result = 0;
        foreach (var itemData in itemDatas)
        {
            result += ModifyItem(nsName, mode, itemData, dataEncoding, precepts);
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
    public Task<int> ModifyItemsAsync(string nsName, ItemModifyMode mode, IEnumerable<byte[]> itemDatas, SerializerType dataEncoding, string[] precepts = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ModifyItems(nsName, mode, itemDatas, dataEncoding, precepts));
    }

    /// <inheritdoc/>
    public QueryItemsOf<T> Execute<T>(string @namespace, Action<IQueryBuilder> query)
    {
        using var builder = new CJsonQueryBuilder(_defaultJsonSerializer, @namespace);
        query(builder);
        var buffer = builder.CloseQuery();

        var result = new QueryItemsOf<T>
        {
            Items = []
        };

        reindexer_buffer.PinBufferFor(buffer, queryRx =>
        {
            var rsp = Assert.ThrowIfError(() => ReindexerBinding.reindexer_select_query(Rx, queryRx, 1, [], 0, _ctxInfo));
            try
            {
                GetItemsFromReindexerResult(result, rsp);
            }
            finally
            {
                rsp.@out.Free();
            }
        });

        return result;
    }

    /// <inheritdoc/>
    public Task<QueryItemsOf<TItem>> ExecuteAsync<TItem>(string @namespace, Action<IQueryBuilder> query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Execute<TItem>(@namespace, query));
    }

    /// <inheritdoc/>
    public QueryItemsOf<T> Execute<T>(byte[] query, SerializerType queryEncoding)
    {
        var result = new QueryItemsOf<T>
        {
            Items = []
        };

        using var queryRx = query.GetHandle();
        var rsp = Assert.ThrowIfError(() => ReindexerBinding.reindexer_select_query(Rx, queryRx, 1, [], 0, _ctxInfo));
        try
        {
            return GetItemsFromReindexerResult(result, rsp);
        }
        finally
        {
            rsp.@out.Free();
        }
    }

    /// <inheritdoc/>
    public QueryItemsOf<T> ExecuteSql<T>(string sql)
    {
        var result = new QueryItemsOf<T>
        {
            Items = []
        };

        using var sqlRx = sql.GetStringHandle();
        var rsp = Assert.ThrowIfError(() => ReindexerBinding.reindexer_select(Rx, sqlRx, 1, [], 0, _ctxInfo));
        try
        {
            return GetItemsFromReindexerResult(result, rsp);
        }
        finally
        {
            rsp.@out.Free();
        }
    }

    private QueryItemsOf<T> GetItemsFromReindexerResult<T>(QueryItemsOf<T> result, reindexer_ret rsp)
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

        result.Aggregations = [];
        foreach (var aggResult in rawQueryParams.aggResults)
        {
            result.Aggregations.Add(JsonSerializer.Deserialize<AggregationResDef>(aggResult));
        }

        if ((rawQueryParams.flags & CJsonReader.ResultsWithJoined) != 0 && reader.GetVarUInt() != 0)
        {
            throw new NotImplementedException("Sorry, not implemented: Can't return join query results as json");
        }

        return result;
    }

    /// <inheritdoc/>
    public Task<QueryItemsOf<T>> ExecuteAsync<T>(byte[] query, SerializerType queryEncoding, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Execute<T>(query, queryEncoding));
    }

    /// <inheritdoc/>
    public Task<QueryItemsOf<T>> ExecuteSqlAsync<T>(string sql, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ExecuteSql<T>(sql));
    }

    /// <inheritdoc/>
    public QueryItemsOf<object> Execute(byte[] query, SerializerType queryEncoding)
    {
        return Execute<object>(query, queryEncoding);
    }

    /// <inheritdoc/>
    public QueryItemsOf<object> ExecuteSql(string sql)
    {
        return ExecuteSql<object>(sql);
    }

    /// <inheritdoc/>
    public Task<QueryItemsOf<object>> ExecuteAsync(byte[] query, SerializerType queryEncoding, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Execute<object>(query, queryEncoding));
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

    /// <inheritdoc/>
    public void CreateDatabase(string dbName)
    {
        var newRx = ReindexerBinding.init_reindexer();
        using (var dsn = $"builtin://{dbName}".GetStringHandle())
        using (var version = ReindexerBinding.ReindexerVersion.GetStringHandle())
            Assert.ThrowIfError(() =>
               ReindexerBinding.reindexer_connect(newRx,
                   dsn,
                   new ConnectionOptions(),
                   version)
           );
        ReindexerBinding.destroy_reindexer(newRx);
    }

    /// <inheritdoc/>
    public Task CreateDatabaseAsync(string dbName, CancellationToken cancellationToken = default)
    {
        CreateDatabase(dbName);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public IEnumerable<Database> EnumDatabases()
    {
        var dbPath = _connectionString.DatabaseName;

        return Directory.GetParent(dbPath).EnumerateDirectories().Select(d => new Database { Name = d.Name });
    }

    /// <inheritdoc/>
    public Task<IEnumerable<Database>> EnumDatabasesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(EnumDatabases());
    }

    /// <inheritdoc/>
    public IEnumerable<Namespace> EnumNamespaces(string name = null, bool onlyNames = false,
        bool hideSystems = true, bool withClosed = false)
    {
        var filters = new List<string>();
        if (hideSystems)
            filters.Add("NOT(name LIKE '#%')");
        if (name != null)
            filters.Add($"name LIKE '{name}'");
        var query = new StringBuilder($"select {(onlyNames ? "name" : "*")} FROM #namespaces");
        if (filters.Any())
        {
            query.AppendFormat(" WHERE {0}", string.Join(" AND ", filters));
        }

        return ExecuteSql<Namespace>(query.ToString()).Items
            .Where(ns => ns.Storage == null || withClosed || ns.Storage.Enabled == true);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<Namespace>> EnumNamespacesAsync(string name = null, bool onlyNames = false,
        bool hideSystems = true, bool withClosed = false, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(EnumNamespaces(name, onlyNames, hideSystems, withClosed));
    }

    /// <inheritdoc/>
    public void SetSchema(string nsName, string jsonSchema)
    {
        using var nsNameRx = nsName.GetStringHandle();
        using var jsonSchemaRx = jsonSchema.GetStringHandle();

        Assert.ThrowIfError(() =>
            ReindexerBinding.reindexer_set_schema(Rx, nsNameRx, jsonSchemaRx, _ctxInfo));
    }

    /// <inheritdoc/>
    public Task SetSchemaAsync(string nsName, string jsonSchema, CancellationToken cancellationToken = default)
    {
        SetSchema(nsName, jsonSchema);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public string GetMeta(string nsName, MetaInfo metadata)
    {
        using var nsNameRx = nsName.GetStringHandle();
        using var keyRx = metadata.Key.GetStringHandle();
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

    /// <inheritdoc/>
    public Task<string> GetMetaAsync(string nsName, MetaInfo metadata, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetMeta(nsName, metadata));
    }

    /// <inheritdoc/>
    public void PutMeta(string nsName, MetaInfo metadata)
    {
        using var nsNameRx = nsName.GetStringHandle();
        using var keyRx = metadata.Key.GetStringHandle();
        using var dataRx = metadata.Value.GetStringHandle();
        Assert.ThrowIfError(() => ReindexerBinding.reindexer_put_meta(Rx, nsNameRx, keyRx, dataRx, _ctxInfo));
    }

    /// <inheritdoc/>
    public Task PutMetaAsync(string nsName, MetaInfo metadata, CancellationToken cancellationToken = default)
    {
        PutMeta(nsName, metadata);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public IEnumerable<string> EnumMeta(string nsName)
    {
        throw new NotImplementedException();//TODO: c binding doesn't support this, get via sql script
    }

    /// <inheritdoc/>
    public Task<IEnumerable<string>> EnumMetaAsync(string nsName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(EnumMeta(nsName));
    }
}
#pragma warning restore S4136 // Method overloads should be grouped together
#pragma warning restore S1135 // Track uses of "TODO" tags