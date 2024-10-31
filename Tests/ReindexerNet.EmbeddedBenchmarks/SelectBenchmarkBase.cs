using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using Cachalot.Linq;
using Client.Interface;
using LiteDB;
using Realms;
using ReindexerNet;
using ReindexerNet.Embedded;
using Server;
using System.Buffers;
using System.Linq.Expressions;
using System.Reflection;
using Index = ReindexerNet.Index;
using IndexType = ReindexerNet.IndexType;

namespace ReindexerNetBenchmark.EmbeddedBenchmarks;

[Config(typeof(SelectBenchmarkConfig))]
public abstract class SelectBenchmarkBase
{
    protected class SelectBenchmarkConfig: ManualConfig
    {
        public SelectBenchmarkConfig()
        {
            AddJob(Job.Default);
            AddDiagnoser(new MemoryDiagnoser(new MemoryDiagnoserConfig(displayGenColumns: true)));
            AddColumn(StatisticColumn.Min, StatisticColumn.Max);
            AddLogicalGroupRules(BenchmarkLogicalGroupRule.ByParams);
            CategoryDiscoverer = new CustomCategoryDiscoverer();
            Orderer = new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest);
        }
    }

    private class CustomCategoryDiscoverer : DefaultCategoryDiscoverer
    {
        public override string[] GetCategories(MethodInfo method) =>
            method.Name.Split('_').Reverse().ToArray();
    }

    protected string DataPath;
    protected BenchmarkEntity[] Data;

    [Params(500,2_000)]
    public int N;

    #region Setups

    protected ReindexerEmbedded? RxClient;
    protected ReindexerEmbedded? RxClientSpanJson;
    protected ReindexerEmbedded? RxClientSql;
    protected Connector? CaConnector;
    protected Connector? CaConnectorMemory;
    //protected Connector? CaConnectorCompressed;
    protected DataSource<BenchmarkEntity> CaDS;
    protected DataSource<BenchmarkEntity> CaDSMemory;
    //protected DataSource<BenchmarkEntity> CaDSCompressed;
    private LiteDatabase _liteDb;
    protected ILiteCollection<BenchmarkEntity> LiteColl;
    private LiteDatabase _liteDbMemory;
    protected ILiteCollection<BenchmarkEntity> LiteCollMemory;
    protected Realm RealmCli;
    protected Expression<Func<BenchmarkEntity, bool>> StrAllQuery;
    protected Expression<Func<BenchmarkEntity, bool>> StrAnyQuery;
    protected Expression<Func<BenchmarkEntity, bool>> IntAllQuery;
    protected Expression<Func<BenchmarkEntity, bool>> IntAnyQuery;
    protected int[] SearchItemsInt;
    protected string[] SearchItemsStr;
    protected string SearchItemsIntJoined;
    protected string SearchItemsStrJoined;
    protected string?[] SearchStringProperties;
    protected string SearchStringPropertiesJoined;
    protected Guid[] SearchIds;
    protected string SearchIdsJoined;

    private static void GetArrayQueryExpressions(int[] searchItemsInt, string[] searchItemsStr,
    out Expression<Func<BenchmarkEntity, bool>> intAnyQuery, out Expression<Func<BenchmarkEntity, bool>> intAllQuery,
    out Expression<Func<BenchmarkEntity, bool>> strAnyQuery, out Expression<Func<BenchmarkEntity, bool>> strAllQuery)
    {
        var firstIntValue = searchItemsInt[0];
        intAnyQuery = e => e.IntArray.Contains(firstIntValue);
        intAllQuery = e => e.IntArray.Contains(firstIntValue);
        foreach (var item in searchItemsInt.Skip(1))
        {
            intAnyQuery = intAnyQuery.OrAlso(e => e.IntArray.Contains(item));
            intAllQuery = intAllQuery.AndAlso(e => e.IntArray.Contains(item));
        }

        var firstStrValue = searchItemsStr[0];
        strAnyQuery = e => e.StrArray.Contains(firstStrValue);
        strAllQuery = e => e.StrArray.Contains(firstStrValue);
        foreach (var item in searchItemsInt.Skip(1))
        {
            strAnyQuery = strAnyQuery.OrAlso(e => e.IntArray.Contains(item));
            strAllQuery = strAllQuery.AndAlso(e => e.IntArray.Contains(item));
        }
    }


    public void Setup()
    {
        DataPath = Directory.CreateTempSubdirectory().FullName;
        Console.WriteLine(DataPath);
        Data = new BenchmarkEntity[N];
        for (int i = 0; i < N; i++)
        {
            Data[i] = new BenchmarkEntity
            {
                Id = Guid.NewGuid(),
                IntProperty = i,
                StringProperty = "ÇŞĞÜÖİöçşğüı" + i,
                CreateDate = DateTime.UtcNow,
                IntArray = Enumerable.Range(0, i).ToArray(),
                StrArray = Enumerable.Range(0, i).Select(i => i.ToString()).ToArray()
            };
        }

        SearchItemsInt = Enumerable.Range(0, 50).Select(i => i * 20).ToArray();
        SearchItemsStr = Enumerable.Range(0, 50).Select(i => i * 20).Select(i => i.ToString()).ToArray();
        SearchItemsIntJoined = string.Join(",", Enumerable.Range(0, 50).Select(i => i * 20));
        SearchItemsStrJoined = "'" + string.Join("','", Enumerable.Range(0, 50).Select(i => i * 20)) + "'";
        SearchStringProperties = Data.Take(N).Select(i => i.StringProperty).ToArray();
        SearchStringPropertiesJoined = "'" + string.Join("','", Data.Take(N).Select(i => i.StringProperty)) + "'";
        SearchIds = Data.Take(N).Select(i => i.Id).ToArray();
        SearchIdsJoined = "'" + string.Join("','", Data.Take(N).Select(i => i.Id)) + "'";
    }

    public void Cleanup()
    {
        Directory.Delete(DataPath, true);
    }

    [GlobalSetup(Targets = new[] {
        "ReindexerNet"
        })]
    public void ReindexerNetSetup()
    {
        Setup();
        var dbPath = Path.Combine(DataPath, "ReindexerEmbedded");
        if (Directory.Exists(dbPath))
            Directory.Delete(dbPath, true);
        RxClient = new ReindexerEmbedded(dbPath);
        RxClient.Connect(new ConnectionOptions { Engine = StorageEngine.LevelDb });
        RxClient.OpenNamespace("Entities");
        RxClient.TruncateNamespace("Entities");
        RxClient.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.Id), IsPk = true, IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = false });
        RxClient.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.IntProperty), IndexType = IndexType.Tree, FieldType = FieldType.Int, IsDense = false });
        RxClient.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.StringProperty), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = false });
        RxClient.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.CreateDate), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = false });
        RxClient.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.IntArray), IndexType = IndexType.Hash, FieldType = FieldType.Int, IsDense = false, IsArray = true });
        RxClient.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.StrArray), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = false, IsArray = true });
        RxClient!.Insert("Entities", Data);
    }

    [GlobalCleanup(Targets = new[] {
        "ReindexerNet"
        })]
    public void ReindexerNetClean()
    {
        RxClient!.Dispose();
        Cleanup();
    }

    private sealed class SpanJsonSerializer : IReindexerSerializer
    {
        public SerializerType Type => SerializerType.Json;

        public T Deserialize<T>(ReadOnlySpan<byte> bytes)
        {
            return SpanJson.JsonSerializer.Generic.Utf8.Deserialize<T>(bytes);
        }

        public ReadOnlySpan<byte> Serialize<T>(T item)
        {
            return SpanJson.JsonSerializer.Generic.Utf8.Serialize(item);
        }
    }

    [GlobalSetup(Targets = new[] {
        "ReindexerNetSpanJson"
        })]
    public void ReindexerNetSpanJsonSetup()
    {
        Setup();
        var dbPathSpanJson = Path.Combine(DataPath, "ReindexerEmbeddedSpanJson");
        if (Directory.Exists(dbPathSpanJson))
            Directory.Delete(dbPathSpanJson, true);
        RxClientSpanJson = new ReindexerEmbedded(dbPathSpanJson, new SpanJsonSerializer());
        RxClientSpanJson.Connect(new ConnectionOptions { Engine = StorageEngine.LevelDb });
        RxClientSpanJson.OpenNamespace("Entities");
        RxClientSpanJson.TruncateNamespace("Entities");
        RxClientSpanJson.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.Id), IsPk = true, IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = true });
        RxClientSpanJson.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.IntProperty), IndexType = IndexType.Tree, FieldType = FieldType.Int, IsDense = true });
        RxClientSpanJson.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.StringProperty), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = true });
        RxClientSpanJson.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.CreateDate), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = true });
        RxClientSpanJson.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.IntArray), IndexType = IndexType.Hash, FieldType = FieldType.Int, IsDense = true, IsArray = true });
        RxClientSpanJson.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.StrArray), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = true, IsArray = true });
        RxClientSpanJson!.Insert("Entities", Data);
    }

    [GlobalCleanup(Targets = new[] {
        "ReindexerNetSpanJson"
        })]
    public void ReindexerNetSpanJsonClean()
    {
        RxClientSpanJson!.Dispose();
        Cleanup();
    }

    [GlobalSetup(Targets = new[] {
        "ReindexerNetSql"
        })]
    public void ReindexerNetSqlSetup()
    {
        Setup();
        var dbPath = Path.Combine(DataPath, "ReindexerEmbeddedSql");
        if (Directory.Exists(dbPath))
            Directory.Delete(dbPath, true);
        RxClientSql = new ReindexerEmbedded(dbPath);
        RxClientSql.Connect(new ConnectionOptions { Engine = StorageEngine.LevelDb });
        RxClientSql.OpenNamespace("Entities");
        RxClientSql.TruncateNamespace("Entities");
        RxClientSql.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.Id), IsPk = true, IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = false });
        RxClientSql.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.IntProperty), IndexType = IndexType.Tree, FieldType = FieldType.Int, IsDense = false });
        RxClientSql.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.StringProperty), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = false });
        RxClientSql.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.CreateDate), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = false });
        RxClientSql.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.IntArray), IndexType = IndexType.Hash, FieldType = FieldType.Int, IsDense = false, IsArray = true });
        RxClientSql.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.StrArray), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = false, IsArray = true });
        RxClientSql!.Insert("Entities", Data);
    }

    [GlobalCleanup(Targets = new[] {
        "ReindexerNetSql"
        })]
    public void ReindexerNetSqlClean()
    {
        RxClientSql!.Dispose();
        Cleanup();
    }

    [GlobalSetup(Targets = new[] {
        "Cachalot"
        })]
    public void CachalotSetup()
    {
        Setup();
        //var server = new Server.Server(new NodeConfig { DataPath = DataPath, IsPersistent = true, ClusterName = "embedded" });
        Directory.SetCurrentDirectory(DataPath);
        CaConnector = new Connector(new ClientConfig { IsPersistent = true, ConnectionPoolCapacity=500, PreloadedConnections = 500  });
        CaConnector.DeclareCollection<BenchmarkEntity>("BenchmarkEntity");
        //CaConnector.GetCollectionSchema("BenchmarkEntity").UseCompression = false;
        CaDS = CaConnector!.DataSource<BenchmarkEntity>("BenchmarkEntity");
        CaDS.PutMany(Data);
        //_caDS.Where(e => e.IntArray.Any(i => searchItemsInt.Contains(i))).ToList(),
        //Doesn't support .Any or All methods right now, so combining queries with OR
        GetArrayQueryExpressions(SearchItemsInt, SearchItemsStr, out IntAnyQuery, out IntAllQuery, out StrAnyQuery, out StrAllQuery);
    }

    [GlobalCleanup(Targets = new[] {
        "Cachalot"
        })]
    public void CachalotClean()
    {
        Directory.SetCurrentDirectory(Directory.GetParent(DataPath).FullName);
        CaDS = null;
        CaConnector!.Dispose();
        Cleanup();
    }

    //[GlobalSetup(Targets = new[] {
    //    "CachalotCompressed"
    //    })]
    //public void CachalotCompressedSetup()
    //{
    //    Setup();
    //    var server = new Server.Server(new NodeConfig { DataPath = DataPath, IsPersistent = true, ClusterName = "embedded" });
    //    CaConnectorCompressed = new Connector(new ClientConfig { IsPersistent = true });
    //    CaConnectorCompressed.DeclareCollection<BenchmarkEntity>("BenchmarkEntity");
    //    CaConnectorCompressed.GetCollectionSchema("BenchmarkEntity").UseCompression = true;
    //    CaDSCompressed = CaConnectorCompressed!.DataSource<BenchmarkEntity>("BenchmarkEntity");
    //    CaDSCompressed.PutMany(Data);

    //    //Doesn't support .Any or All methods right now, so combining queries with OR
    //    GetArrayQueryExpressions(SearchItemsInt, SearchItemsStr, out IntAnyQuery, out IntAllQuery, out StrAnyQuery, out StrAllQuery);
    //}

    //[GlobalCleanup(Targets = new[] {
    //    "CachalotCompressed"
    //    })]
    //public void CachalotCompressedClean()
    //{
    //    CaConnectorCompressed!.Dispose();
    //    Cleanup();
    //}

    [GlobalSetup(Targets = new[] {
        "CachalotMemory"
        })]
    public void CachalotMemorySetup()
    {
        Setup();
        //var server = new Server.Server(new NodeConfig { DataPath = Path.Combine(DataPath, "CachalotMemory"), IsPersistent = false, ClusterName = "embeddedMemory" });
        Directory.SetCurrentDirectory(DataPath);
        CaConnectorMemory = new Connector(new ClientConfig { IsPersistent = false, ConnectionPoolCapacity=500, PreloadedConnections = 500 });
        CaConnectorMemory.DeclareCollection<BenchmarkEntity>("BenchmarkEntityMemory");
        //CaConnectorMemory.GetCollectionSchema("BenchmarkEntity").UseCompression = false;
        CaDSMemory = CaConnectorMemory!.DataSource<BenchmarkEntity>("BenchmarkEntityMemory");
        CaDSMemory.PutMany(Data);
        //Doesn't support .Any or All methods right now, so combining queries with OR
        GetArrayQueryExpressions(SearchItemsInt, SearchItemsStr, out IntAnyQuery, out IntAllQuery, out StrAnyQuery, out StrAllQuery);
    }

    [GlobalCleanup(Targets = new[] {
        "CachalotMemory"
        })]
    public void CachalotMemoryClean()
    {
        Directory.SetCurrentDirectory(Directory.GetParent(DataPath).FullName);
        CaDSMemory = null;
        CaConnectorMemory!.Dispose();
        Cleanup();
    }

    [GlobalSetup(Targets = new[] {
        "LiteDb"
        })]
    public void LiteDBSetup()
    {
        Setup();
        _liteDb = new LiteDatabase(Path.Combine(DataPath, "LiteDB"));
        LiteColl = _liteDb.GetCollection<BenchmarkEntity>("Entities");
        LiteColl.EnsureIndex(e => e.Id, true);
        LiteColl.EnsureIndex(e => e.IntProperty);
        LiteColl.EnsureIndex(e => e.StringProperty);
        LiteColl.EnsureIndex(e => e.CreateDate);
        LiteColl.EnsureIndex(e => e.IntArray);
        LiteColl.EnsureIndex(e => e.StrArray);
        LiteColl.Upsert(Data);

        //Doesn't support .Any or All methods right now, so combining queries with OR
        GetArrayQueryExpressions(SearchItemsInt, SearchItemsStr, out IntAnyQuery, out IntAllQuery, out StrAnyQuery, out StrAllQuery);
    }

    [GlobalCleanup(Targets = new[] {
        "LiteDb"
        })]
    public void LiteDBClean()
    {
        _liteDb!.Dispose();
        Cleanup();
    }

    [GlobalSetup(Targets = new[] {
        "LiteDbMemory"
        })]
    public void LiteDbMemorySetup()
    {
        Setup();
        _liteDbMemory = new LiteDatabase(":memory:");
        LiteCollMemory = _liteDbMemory.GetCollection<BenchmarkEntity>("Entities");
        LiteCollMemory.EnsureIndex(e => e.Id, true);
        LiteCollMemory.EnsureIndex(e => e.IntProperty);
        LiteCollMemory.EnsureIndex(e => e.StringProperty);
        LiteCollMemory.EnsureIndex(e => e.CreateDate);
        LiteCollMemory.EnsureIndex(e => e.IntArray);
        LiteCollMemory.EnsureIndex(e => e.StrArray);
        LiteCollMemory.Upsert(Data);

        //Doesn't support .Any or All methods right now, so combining queries with OR
        GetArrayQueryExpressions(SearchItemsInt, SearchItemsStr, out IntAnyQuery, out IntAllQuery, out StrAnyQuery, out StrAllQuery);
    }

    [GlobalCleanup(Targets = new[] {
        "LiteDbMemory"
        })]
    public void LiteDbMemoryClean()
    {
        _liteDbMemory!.Dispose();
        Cleanup();
    }

    [GlobalSetup(Targets = new[] {
        "Realm"
        })]
    public async Task RealmSetupAsync()
    {
        Setup();
        RealmCli = await Realm.GetInstanceAsync(new RealmConfiguration(Path.Combine(DataPath, "Realms")));
        await RealmCli.WriteAsync(() =>
        {
            RealmCli.Add(Data.Select(e => (BenchmarkRealmEntity)e), update: false);
        });
    }

    [GlobalCleanup(Targets = new[] {
        "Realm"
        })]
    public void RealmClean()
    {
        RealmCli.Dispose();
        Realm.DeleteRealm(new RealmConfiguration(Path.Combine(DataPath, "Realm")));
        Cleanup();
    }
    #endregion

}