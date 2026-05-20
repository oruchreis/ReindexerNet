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
    private const ulong ContextIdMask = 0x1FFF_FFFF_FFFF_FFFF;
    private static long _lastContextId;

    /// <summary>
    /// Reindexer native object pointer.
    /// </summary>
    protected UIntPtr Rx;
    private readonly reindexer_ctx_info _ctxInfo = new reindexer_ctx_info { ctx_id = 0, exec_timeout = -1 };
    private readonly EmbeddedNativeScheduler _nativeScheduler;

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
    /// <param name="options">Embedded client options.</param>
    public ReindexerEmbedded(string dbPath, IReindexerSerializer serializer = null, ReindexerEmbeddedOptions options = null)
    {
        _connectionString = new ReindexerConnectionString { DatabaseName = dbPath };
        Serializer = serializer ?? new ReindexerJsonSerializer();
        _nativeScheduler = new EmbeddedNativeScheduler(options);
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

    private static reindexer_ctx_info CreateOperationContext()
    {
        var ctxId = (ulong)Interlocked.Increment(ref _lastContextId) & ContextIdMask;
        if (ctxId == 0)
        {
            ctxId = (ulong)Interlocked.Increment(ref _lastContextId) & ContextIdMask;
        }

        return new reindexer_ctx_info { ctx_id = ctxId, exec_timeout = -1 };
    }

    internal Task RunNativeAsync(Action<reindexer_ctx_info> action, CancellationToken cancellationToken)
    {
        return _nativeScheduler.Run(() => ExecuteNative(action, cancellationToken), cancellationToken);
    }

    internal Task<T> RunNativeAsync<T>(Func<reindexer_ctx_info, T> action, CancellationToken cancellationToken)
    {
        return _nativeScheduler.Run(() => ExecuteNative(action, cancellationToken), cancellationToken);
    }

    internal Task RunAsync(Action action, CancellationToken cancellationToken)
    {
        return _nativeScheduler.Run(action, cancellationToken);
    }

    internal Task<T> RunAsync<T>(Func<T> action, CancellationToken cancellationToken)
    {
        return _nativeScheduler.Run(action, cancellationToken);
    }

    internal static void ExecuteNative(Action<reindexer_ctx_info> action, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        var ctxInfo = CreateOperationContext();
        using var registration = cancellationToken.Register(static state =>
        {
            try
            {
                ReindexerBinding.reindexer_cancel_context((reindexer_ctx_info)state, ctx_cancel_type.cancel_expilicitly);
            }
            catch
            {
            }
        }, ctxInfo);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            action(ctxInfo);
        }
        catch (ReindexerException e) when (cancellationToken.IsCancellationRequested && e.ErrorCode == ReindexerErrorCode.Canceled)
        {
            throw new OperationCanceledException(cancellationToken);
        }
    }

    internal static T ExecuteNative<T>(Func<reindexer_ctx_info, T> action, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        var ctxInfo = CreateOperationContext();
        using var registration = cancellationToken.Register(static state =>
        {
            try
            {
                ReindexerBinding.reindexer_cancel_context((reindexer_ctx_info)state, ctx_cancel_type.cancel_expilicitly);
            }
            catch
            {
            }
        }, ctxInfo);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return action(ctxInfo);
        }
        catch (ReindexerException e) when (cancellationToken.IsCancellationRequested && e.ErrorCode == ReindexerErrorCode.Canceled)
        {
            throw new OperationCanceledException(cancellationToken);
        }
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
        AddIndex(nsName, indexDefinition, _ctxInfo);
    }

    private void AddIndex(string nsName, Index indexDefinition, reindexer_ctx_info ctxInfo)
    {
        using var nsNameRx = nsName.GetStringHandle();
        if (indexDefinition.JsonPaths == null || indexDefinition.JsonPaths.Count == 0)
            indexDefinition.JsonPaths = [indexDefinition.Name];
        using var jsonRx = ReindexerEmbedded.InternalSerializeJson(indexDefinition).GetStringHandle();
        Assert.ThrowIfError(() =>
                ReindexerBinding.reindexer_add_index(Rx, nsNameRx, jsonRx, ctxInfo)
        );
    }

    /// <inheritdoc/>
    public Task AddIndexAsync(string nsName, Index indexDefinition, CancellationToken cancellationToken = default)
    {
        return RunNativeAsync(ctxInfo => AddIndex(nsName, indexDefinition, ctxInfo), cancellationToken);
    }

    /// <inheritdoc/>
    public void CloseNamespace(string nsName)
    {
        CloseNamespace(nsName, _ctxInfo);
    }

    private void CloseNamespace(string nsName, reindexer_ctx_info ctxInfo)
    {
        using var nsNameRx = nsName.GetStringHandle();
        Assert.ThrowIfError(() =>
            ReindexerBinding.reindexer_close_namespace(Rx, nsNameRx, ctxInfo));
    }

    /// <inheritdoc/>
    public Task CloseNamespaceAsync(string nsName, CancellationToken cancellationToken = default)
    {
        return RunNativeAsync(ctxInfo => CloseNamespace(nsName, ctxInfo), cancellationToken);
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
               version,
               new BindingCapabilities())
       );
    }

    /// <inheritdoc/>
    public Task ConnectAsync(ConnectionOptions options = null, CancellationToken cancellationToken = default)
    {
        return RunAsync(() => Connect(options), cancellationToken);
    }

    /// <inheritdoc/>
    public void DropIndex(string nsName, string indexName)
    {
        DropIndex(nsName, indexName, _ctxInfo);
    }

    private void DropIndex(string nsName, string indexName, reindexer_ctx_info ctxInfo)
    {
        using var nsNameRx = nsName.GetStringHandle();
        using var inameRx = indexName.GetStringHandle();
        Assert.ThrowIfError(() =>
            ReindexerBinding.reindexer_drop_index(Rx, nsNameRx, inameRx, ctxInfo)
        );
    }

    /// <inheritdoc/>
    public Task DropIndexAsync(string nsName, string indexName, CancellationToken cancellationToken = default)
    {
        return RunNativeAsync(ctxInfo => DropIndex(nsName, indexName, ctxInfo), cancellationToken);
    }

    /// <inheritdoc/>
    public void DropNamespace(string nsName)
    {
        DropNamespace(nsName, _ctxInfo);
    }

    private void DropNamespace(string nsName, reindexer_ctx_info ctxInfo)
    {
        using var nsNameRx = nsName.GetStringHandle();
        Assert.ThrowIfError(() =>
            ReindexerBinding.reindexer_drop_namespace(Rx, nsNameRx, ctxInfo)
        );
    }

    /// <inheritdoc/>
    public Task DropNamespaceAsync(string nsName, CancellationToken cancellationToken = default)
    {
        return RunNativeAsync(ctxInfo => DropNamespace(nsName, ctxInfo), cancellationToken);
    }

    /// <inheritdoc/>
    public void OpenNamespace(string nsName, NamespaceOptions options = null)
    {
        OpenNamespace(nsName, options, _ctxInfo);
    }

    private void OpenNamespace(string nsName, NamespaceOptions options, reindexer_ctx_info ctxInfo)
    {
        using (var nsNameRx = nsName.GetStringHandle())
            Assert.ThrowIfError(() =>
            {
                reindexer_error rsp = default;
                for (int retry = 0; retry < 2; retry++)
                {
                    rsp = ReindexerBinding.reindexer_open_namespace(Rx, nsNameRx, options ?? new NamespaceOptions(), ctxInfo);
                    if (rsp.code != 0)
                    {
                        ReindexerBinding.reindexer_close_namespace(Rx, nsNameRx, ctxInfo);
                    }
                }
                return rsp;
            });
    }

    /// <inheritdoc/>
    public Task OpenNamespaceAsync(string nsName, NamespaceOptions options = null, CancellationToken cancellationToken = default)
    {
        return RunNativeAsync(ctxInfo => OpenNamespace(nsName, options, ctxInfo), cancellationToken);
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
        return RunAsync(Ping, cancellationToken);
    }

    /// <inheritdoc/>
    public void RenameNamespace(string oldName, string newName)
    {
        RenameNamespace(oldName, newName, _ctxInfo);
    }

    private void RenameNamespace(string oldName, string newName, reindexer_ctx_info ctxInfo)
    {
        using var oldNameRx = oldName.GetStringHandle();
        using var newNameRx = newName.GetStringHandle();
        Assert.ThrowIfError(() =>
            ReindexerBinding.reindexer_rename_namespace(Rx, oldNameRx, newNameRx, ctxInfo));
    }

    /// <inheritdoc/>
    public Task RenameNamespaceAsync(string oldName, string newName, CancellationToken cancellationToken = default)
    {
        return RunNativeAsync(ctxInfo => RenameNamespace(oldName, newName, ctxInfo), cancellationToken);
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
        return new ReindexerTransaction(new EmbeddedTransactionInvoker(Rx, tr, _ctxInfo, Serializer, _nativeScheduler));
    }

    /// <inheritdoc/>
    public Task<ReindexerTransaction> StartTransactionAsync(string nsName, CancellationToken cancellationToken = default)
    {
        return RunAsync(() => StartTransaction(nsName), cancellationToken);
    }

    /// <inheritdoc/>
    public void TruncateNamespace(string nsName)
    {
        TruncateNamespace(nsName, _ctxInfo);
    }

    private void TruncateNamespace(string nsName, reindexer_ctx_info ctxInfo)
    {
        using var nsNameRx = nsName.GetStringHandle();
        Assert.ThrowIfError(() =>
            ReindexerBinding.reindexer_truncate_namespace(Rx, nsNameRx, ctxInfo)
        );
    }

    /// <inheritdoc/>
    public Task TruncateNamespaceAsync(string nsName, CancellationToken cancellationToken = default)
    {
        return RunNativeAsync(ctxInfo => TruncateNamespace(nsName, ctxInfo), cancellationToken);
    }

    /// <inheritdoc/>
    public void UpdateIndex(string nsName, Index indexDefinition)
    {
        UpdateIndex(nsName, indexDefinition, _ctxInfo);
    }

    private void UpdateIndex(string nsName, Index indexDefinition, reindexer_ctx_info ctxInfo)
    {
        using var nsNameRx = nsName.GetStringHandle();
        if (indexDefinition.JsonPaths == null || indexDefinition.JsonPaths.Count == 0)
            indexDefinition.JsonPaths = [indexDefinition.Name];
        using var jsonRx = ReindexerEmbedded.InternalSerializeJson(indexDefinition).GetStringHandle();
        Assert.ThrowIfError(() =>
            ReindexerBinding.reindexer_update_index(Rx, nsNameRx, jsonRx, ctxInfo)
        );

    }

    /// <inheritdoc/>
    public Task UpdateIndexAsync(string nsName, Index indexDefinition, CancellationToken cancellationToken = default)
    {
        return RunNativeAsync(ctxInfo => UpdateIndex(nsName, indexDefinition, ctxInfo), cancellationToken);
    }

    /// <inheritdoc/>
    public int ModifyItem(string nsName, ItemModifyMode mode, ReadOnlySpan<byte> itemBytes, SerializerType dataEncoding, string[] precepts = null)
    {
        return ModifyItem(nsName, mode, itemBytes, dataEncoding, precepts, _ctxInfo);
    }

    private int ModifyItem(string nsName, ItemModifyMode mode, ReadOnlySpan<byte> itemBytes, SerializerType dataEncoding, string[] precepts, reindexer_ctx_info ctxInfo)
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
                var rsp = Assert.ThrowIfError(() => ReindexerBinding.reindexer_modify_item_packed(Rx, args, data, ctxInfo));
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
        return ModifyItems(nsName, mode, items, precepts, _ctxInfo);
    }

    private int ModifyItems<TItem>(string nsName, ItemModifyMode mode, IEnumerable<TItem> items, string[] precepts, reindexer_ctx_info ctxInfo)
    {
        var result = 0;
        foreach (var item in items)
        {
            result += ModifyItem(nsName, mode, Serializer.Serialize(item), Serializer.Type, precepts, ctxInfo);
        }

        return result;
    }

    /// <inheritdoc/>
    public int ModifyItems(string nsName, ItemModifyMode mode, IEnumerable<byte[]> itemDatas, SerializerType dataEncoding, string[] precepts = null)
    {
        return ModifyItems(nsName, mode, itemDatas, dataEncoding, precepts, _ctxInfo);
    }

    private int ModifyItems(string nsName, ItemModifyMode mode, IEnumerable<byte[]> itemDatas, SerializerType dataEncoding, string[] precepts, reindexer_ctx_info ctxInfo)
    {
        var result = 0;
        foreach (var itemData in itemDatas)
        {
            result += ModifyItem(nsName, mode, itemData, dataEncoding, precepts, ctxInfo);
        }

        return result;
    }

    /// <inheritdoc/>
    public Task<int> ModifyItemsAsync<TItem>(string nsName, ItemModifyMode mode, IEnumerable<TItem> items,
        string[] precepts = null, CancellationToken cancellationToken = default)
    {
        return RunNativeAsync(ctxInfo => ModifyItems(nsName, mode, items, precepts, ctxInfo), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<int> ModifyItemsAsync(string nsName, ItemModifyMode mode, IEnumerable<byte[]> itemDatas, SerializerType dataEncoding, string[] precepts = null,
        CancellationToken cancellationToken = default)
    {
        return RunNativeAsync(ctxInfo => ModifyItems(nsName, mode, itemDatas, dataEncoding, precepts, ctxInfo), cancellationToken);
    }

    /// <inheritdoc/>
    public QueryItemsOf<T> Execute<T>(string @namespace, Action<IQueryBuilder> query)
    {
        return Execute<T>(@namespace, query, _ctxInfo);
    }

    private QueryItemsOf<T> Execute<T>(string @namespace, Action<IQueryBuilder> query, reindexer_ctx_info ctxInfo)
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
            var rsp = Assert.ThrowIfError(() => ReindexerBinding.reindexer_select_query(Rx, queryRx, 1, [], 0, ctxInfo));
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
        return RunNativeAsync(ctxInfo => Execute<TItem>(@namespace, query, ctxInfo), cancellationToken);
    }

    /// <inheritdoc/>
    public QueryItemsOf<T> Execute<T>(byte[] query, SerializerType queryEncoding)
    {
        return Execute<T>(query, queryEncoding, _ctxInfo);
    }

    private QueryItemsOf<T> Execute<T>(byte[] query, SerializerType queryEncoding, reindexer_ctx_info ctxInfo)
    {
        var result = new QueryItemsOf<T>
        {
            Items = []
        };

        using var queryRx = query.GetHandle();
        var rsp = Assert.ThrowIfError(() => ReindexerBinding.reindexer_select_query(Rx, queryRx, 1, [], 0, ctxInfo));
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
        return ExecuteSql<T>(sql, _ctxInfo);
    }

    private QueryItemsOf<T> ExecuteSql<T>(string sql, reindexer_ctx_info ctxInfo)
    {
        var result = new QueryItemsOf<T>
        {
            Items = []
        };

        using var sqlRx = sql.GetStringHandle();
        var rsp = Assert.ThrowIfError(() => ReindexerBinding.reindexer_select(Rx, sqlRx, 1, [], 0, ctxInfo));
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
        return RunNativeAsync(ctxInfo => Execute<T>(query, queryEncoding, ctxInfo), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<QueryItemsOf<T>> ExecuteSqlAsync<T>(string sql, CancellationToken cancellationToken = default)
    {
        return RunNativeAsync(ctxInfo => ExecuteSql<T>(sql, ctxInfo), cancellationToken);
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
        return RunNativeAsync(ctxInfo => Execute<object>(query, queryEncoding, ctxInfo), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<QueryItemsOf<object>> ExecuteSqlAsync(string sql, CancellationToken cancellationToken = default)
    {
        return RunNativeAsync(ctxInfo => ExecuteSql<object>(sql, ctxInfo), cancellationToken);
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
        return RunNativeAsync(ctxInfo => ModifyItems(nsName, ItemModifyMode.Insert, items, precepts, ctxInfo), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<int> UpdateAsync<T>(string nsName, IEnumerable<T> items, string[] precepts = null, CancellationToken cancellationToken = default)
    {
        return RunNativeAsync(ctxInfo => ModifyItems(nsName, ItemModifyMode.Update, items, precepts, ctxInfo), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<int> UpsertAsync<T>(string nsName, IEnumerable<T> items, string[] precepts = null, CancellationToken cancellationToken = default)
    {
        return RunNativeAsync(ctxInfo => ModifyItems(nsName, ItemModifyMode.Upsert, items, precepts, ctxInfo), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<int> DeleteAsync<T>(string nsName, IEnumerable<T> items, string[] precepts = null, CancellationToken cancellationToken = default)
    {
        return RunNativeAsync(ctxInfo => ModifyItems(nsName, ItemModifyMode.Delete, items, precepts, ctxInfo), cancellationToken);
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
                   version,
                   new BindingCapabilities())
           );
        ReindexerBinding.destroy_reindexer(newRx);
    }

    /// <inheritdoc/>
    public Task CreateDatabaseAsync(string dbName, CancellationToken cancellationToken = default)
    {
        return RunAsync(() => CreateDatabase(dbName), cancellationToken);
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
        return RunAsync(EnumDatabases, cancellationToken);
    }

    /// <inheritdoc/>
    public IEnumerable<Namespace> EnumNamespaces(string name = null, bool onlyNames = false,
        bool hideSystems = true, bool withClosed = false)
    {
        return EnumNamespaces(name, onlyNames, hideSystems, withClosed, _ctxInfo);
    }

    private IEnumerable<Namespace> EnumNamespaces(string name, bool onlyNames, bool hideSystems, bool withClosed, reindexer_ctx_info ctxInfo)
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

        return ExecuteSql<Namespace>(query.ToString(), ctxInfo).Items
            .Where(ns => ns.Storage == null || withClosed || ns.Storage.Enabled == true);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<Namespace>> EnumNamespacesAsync(string name = null, bool onlyNames = false,
        bool hideSystems = true, bool withClosed = false, CancellationToken cancellationToken = default)
    {
        return RunNativeAsync(ctxInfo => EnumNamespaces(name, onlyNames, hideSystems, withClosed, ctxInfo), cancellationToken);
    }

    /// <inheritdoc/>
    public void SetSchema(string nsName, string jsonSchema)
    {
        SetSchema(nsName, jsonSchema, _ctxInfo);
    }

    private void SetSchema(string nsName, string jsonSchema, reindexer_ctx_info ctxInfo)
    {
        using var nsNameRx = nsName.GetStringHandle();
        using var jsonSchemaRx = jsonSchema.GetStringHandle();

        Assert.ThrowIfError(() =>
            ReindexerBinding.reindexer_set_schema(Rx, nsNameRx, jsonSchemaRx, ctxInfo));
    }

    /// <inheritdoc/>
    public Task SetSchemaAsync(string nsName, string jsonSchema, CancellationToken cancellationToken = default)
    {
        return RunNativeAsync(ctxInfo => SetSchema(nsName, jsonSchema, ctxInfo), cancellationToken);
    }

    /// <inheritdoc/>
    public string GetMeta(string nsName, MetaInfo metadata)
    {
        return GetMeta(nsName, metadata, _ctxInfo);
    }

    private string GetMeta(string nsName, MetaInfo metadata, reindexer_ctx_info ctxInfo)
    {
        using var nsNameRx = nsName.GetStringHandle();
        using var keyRx = metadata.Key.GetStringHandle();
        var rsp = Assert.ThrowIfError(() => ReindexerBinding.reindexer_get_meta(Rx, nsNameRx, keyRx, ctxInfo));
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
        return RunNativeAsync(ctxInfo => GetMeta(nsName, metadata, ctxInfo), cancellationToken);
    }

    /// <inheritdoc/>
    public void PutMeta(string nsName, MetaInfo metadata)
    {
        PutMeta(nsName, metadata, _ctxInfo);
    }

    private void PutMeta(string nsName, MetaInfo metadata, reindexer_ctx_info ctxInfo)
    {
        using var nsNameRx = nsName.GetStringHandle();
        using var keyRx = metadata.Key.GetStringHandle();
        using var dataRx = metadata.Value.GetStringHandle();
        Assert.ThrowIfError(() => ReindexerBinding.reindexer_put_meta(Rx, nsNameRx, keyRx, dataRx, ctxInfo));
    }

    /// <inheritdoc/>
    public Task PutMetaAsync(string nsName, MetaInfo metadata, CancellationToken cancellationToken = default)
    {
        return RunNativeAsync(ctxInfo => PutMeta(nsName, metadata, ctxInfo), cancellationToken);
    }

    /// <inheritdoc/>
    public IEnumerable<string> EnumMeta(string nsName)
    {
        throw new NotImplementedException();//TODO: c binding doesn't support this, get via sql script
    }

    /// <inheritdoc/>
    public Task<IEnumerable<string>> EnumMetaAsync(string nsName, CancellationToken cancellationToken = default)
    {
        return RunAsync(() => EnumMeta(nsName), cancellationToken);
    }
}
#pragma warning restore S4136 // Method overloads should be grouped together
#pragma warning restore S1135 // Track uses of "TODO" tags
