using ReindexerNet;
using System.Reflection;
using Index = ReindexerNet.Index;

namespace ReindexerNetBenchmark.EmbeddedBenchmarks;

/// <summary>
/// Typed wrapper for ReindexerNet v3 (NuGet 0.5.0.3310) loaded via V3AssemblyContext.
/// Pre-compiles delegates via Delegate.CreateDelegate to minimize hot-path reflection overhead.
/// Used as the "RxV3" alias in benchmark method variants.
/// </summary>
public sealed class RxV3ReindexerEmbedded : IDisposable
{
    private static readonly V3AssemblyContext _context;
    private static readonly Type _embeddedType;
    private static readonly ConstructorInfo _ctor;
    private static readonly MethodInfo _connectMethod;
    private static readonly MethodInfo _openNamespaceMethod;
    private static readonly MethodInfo _truncateNamespaceMethod;
    private static readonly MethodInfo _addIndexMethod;
    private static readonly MethodInfo _disposeMethod;

    // Pre-specialized MethodInfos for BenchmarkEntity (avoid MakeGenericMethod on hot path)
    private static readonly MethodInfo _insertBEMethod;
    private static readonly MethodInfo _upsertBEMethod;
    private static readonly MethodInfo _executeBEMethod;
    private static readonly MethodInfo _executeSqlBEMethod;

    public static string V3Directory => Path.Combine(
        Path.GetDirectoryName(typeof(RxV3ReindexerEmbedded).Assembly.Location)
            ?? AppContext.BaseDirectory,
        "rxv3");

    static RxV3ReindexerEmbedded()
    {
        var v3Dir = V3Directory;
        if (!Directory.Exists(v3Dir))
            throw new DirectoryNotFoundException(
                $"V3 binaries directory not found: {v3Dir}. Run 'dotnet build' first.");

        _context = new V3AssemblyContext(v3Dir);
        var asm = _context.LoadFromAssemblyPath(Path.Combine(v3Dir, "ReindexerNet.Embedded.dll"));
        _embeddedType = asm.GetType("ReindexerNet.Embedded.ReindexerEmbedded")
            ?? throw new InvalidOperationException("ReindexerEmbedded not found in v3 assembly");

        // Constructor: ReindexerEmbedded(string dbPath, IReindexerSerializer? serializer, options?)
        _ctor = _embeddedType.GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .First();

        var allMethods = _embeddedType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

        _connectMethod = _embeddedType.GetMethod("Connect", new[] { typeof(ConnectionOptions) })!;
        _openNamespaceMethod = _embeddedType.GetMethod("OpenNamespace", new[] { typeof(string), typeof(NamespaceOptions) })!;
        _truncateNamespaceMethod = _embeddedType.GetMethod("TruncateNamespace", new[] { typeof(string) })!;
        _addIndexMethod = _embeddedType.GetMethod("AddIndex", new[] { typeof(string), typeof(Index) })!;
        _disposeMethod = _embeddedType.GetMethod("Dispose", Type.EmptyTypes)!;

        var insertDef = allMethods.First(m =>
            m.Name == "Insert" && m.IsGenericMethodDefinition
            && m.GetParameters() is { Length: 3 } ps && ps[2].ParameterType == typeof(string[]));
        var upsertDef = allMethods.First(m =>
            m.Name == "Upsert" && m.IsGenericMethodDefinition
            && m.GetParameters() is { Length: 3 } ps && ps[2].ParameterType == typeof(string[]));
        var executeDef = allMethods.First(m =>
            m.Name == "Execute" && m.IsGenericMethodDefinition
            && m.GetParameters() is { Length: 2 } ps
            && ps[1].ParameterType == typeof(Action<IQueryBuilder>));
        var executeSqlDef = allMethods.First(m =>
            m.Name == "ExecuteSql" && m.IsGenericMethodDefinition
            && m.GetParameters() is { Length: 1 } ps && ps[0].ParameterType == typeof(string));

        _insertBEMethod = insertDef.MakeGenericMethod(typeof(BenchmarkEntity));
        _upsertBEMethod = upsertDef.MakeGenericMethod(typeof(BenchmarkEntity));
        _executeBEMethod = executeDef.MakeGenericMethod(typeof(BenchmarkEntity));
        _executeSqlBEMethod = executeSqlDef.MakeGenericMethod(typeof(BenchmarkEntity));
    }

    private readonly object _client;

    // Bound delegates — essentially zero overhead vs a direct virtual call
    private readonly Func<string, IEnumerable<BenchmarkEntity>, string[]?, int> _insertDelegate;
    private readonly Func<string, IEnumerable<BenchmarkEntity>, string[]?, int> _upsertDelegate;
    private readonly Func<string, Action<IQueryBuilder>, QueryItemsOf<BenchmarkEntity>> _executeDelegate;
    private readonly Func<string, QueryItemsOf<BenchmarkEntity>> _executeSqlDelegate;
    private readonly Action _disposeDelegate;

    public RxV3ReindexerEmbedded(string dbPath, IReindexerSerializer? serializer = null)
    {
        var ctorParams = _ctor.GetParameters();
        var args = new object?[ctorParams.Length];
        args[0] = dbPath;
        if (ctorParams.Length > 1) args[1] = serializer;
        // remaining params (options) stay null

        _client = _ctor.Invoke(args)!;

        _insertDelegate = (Func<string, IEnumerable<BenchmarkEntity>, string[]?, int>)
            Delegate.CreateDelegate(
                typeof(Func<string, IEnumerable<BenchmarkEntity>, string[]?, int>),
                _client, _insertBEMethod);
        _upsertDelegate = (Func<string, IEnumerable<BenchmarkEntity>, string[]?, int>)
            Delegate.CreateDelegate(
                typeof(Func<string, IEnumerable<BenchmarkEntity>, string[]?, int>),
                _client, _upsertBEMethod);
        _executeDelegate = (Func<string, Action<IQueryBuilder>, QueryItemsOf<BenchmarkEntity>>)
            Delegate.CreateDelegate(
                typeof(Func<string, Action<IQueryBuilder>, QueryItemsOf<BenchmarkEntity>>),
                _client, _executeBEMethod);
        _executeSqlDelegate = (Func<string, QueryItemsOf<BenchmarkEntity>>)
            Delegate.CreateDelegate(
                typeof(Func<string, QueryItemsOf<BenchmarkEntity>>),
                _client, _executeSqlBEMethod);
        _disposeDelegate = (Action)Delegate.CreateDelegate(typeof(Action), _client, _disposeMethod);
    }

    public void Connect(ConnectionOptions? options = null)
        => _connectMethod.Invoke(_client, new object?[] { options });

    public void OpenNamespace(string nsName, NamespaceOptions? options = null)
        => _openNamespaceMethod.Invoke(_client, new object?[] { nsName, options });

    public void TruncateNamespace(string nsName)
        => _truncateNamespaceMethod.Invoke(_client, new object[] { nsName });

    public void AddIndex(string nsName, Index indexDefinition)
        => _addIndexMethod.Invoke(_client, new object[] { nsName, indexDefinition });

    public int Insert(string nsName, IEnumerable<BenchmarkEntity> items)
        => _insertDelegate(nsName, items, null);

    public int Upsert(string nsName, IEnumerable<BenchmarkEntity> items)
        => _upsertDelegate(nsName, items, null);

    public QueryItemsOf<BenchmarkEntity> Execute(string @namespace, Action<IQueryBuilder> query)
        => _executeDelegate(@namespace, query);

    public QueryItemsOf<BenchmarkEntity> ExecuteSql(string sql)
        => _executeSqlDelegate(sql);

    public void Dispose()
    {
        _disposeDelegate();
        GC.SuppressFinalize(this);
    }
}
