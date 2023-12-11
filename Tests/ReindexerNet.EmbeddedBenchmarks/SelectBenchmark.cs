using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Cachalot.Linq;
using Client.Interface;
using LiteDB;
using MessagePack;
using MessagePack.Resolvers;
using Newtonsoft.Json.Linq;
using Realms;
using ReindexerNet;
using ReindexerNet.Embedded;
using Server;
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Index = ReindexerNet.Index;
using IndexType = ReindexerNet.IndexType;

namespace ReindexerNetBenchmark.EmbeddedBenchmarks;

//[Config(typeof(AntiVirusFriendlyConfig))]
[SimpleJob(launchCount: 1, warmupCount:1, iterationCount:5)]
[MemoryDiagnoser()]
[CategoriesColumn]
[CustomCategoryDiscoverer]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory, BenchmarkLogicalGroupRule.ByMethod)]
[PlainExporter]
public class SelectBenchmark
{
    private class CustomCategoryDiscoverer : DefaultCategoryDiscoverer
    {
        public override string[] GetCategories(MethodInfo method) =>
            method.Name.Split('_').Reverse().ToArray();
    }

    [AttributeUsage(AttributeTargets.Class)]
    private class CustomCategoryDiscovererAttribute : Attribute, IConfigSource
    {
        public CustomCategoryDiscovererAttribute()
        {
            Config = ManualConfig.CreateEmpty()
                .WithCategoryDiscoverer(new CustomCategoryDiscoverer());
        }

        public IConfig Config { get; }
    }

    private string _dataPath;
    private BenchmarkEntity[] _data;

    [Params(1_000)]
    public int N;

    #region Setups

    private ReindexerEmbedded? _rxClient;
    private ReindexerEmbedded? _rxClientSpanJson;
    private ReindexerEmbedded? _rxClientSql;
    private Connector? _caConnector;
    private Connector? _caConnectorMemory;
    private Connector? _caConnectorCompressed;
    private DataSource<BenchmarkEntity> _caDS;
    private DataSource<BenchmarkEntity> _caDSMemory;
    private DataSource<BenchmarkEntity> _caDSCompressed;
    private LiteDatabase _liteDb;
    private ILiteCollection<BenchmarkEntity> _liteColl;
    private LiteDatabase _liteDbMemory;
    private ILiteCollection<BenchmarkEntity> _liteCollMemory;
    private Realm _realm;
    private Expression<Func<BenchmarkEntity, bool>> _strAllQuery;
    private Expression<Func<BenchmarkEntity, bool>> _strAnyQuery;
    private Expression<Func<BenchmarkEntity, bool>> _intAllQuery;
    private Expression<Func<BenchmarkEntity, bool>> _intAnyQuery;
    private int[] _searchItemsInt;
    private string[] _searchItemsStr;
    private string _searchItemsIntJoined;
    private string _searchItemsStrJoined;

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
        _dataPath = Directory.CreateTempSubdirectory().FullName;
        _data = new BenchmarkEntity[N];
        for (int i = 0; i < N; i++)
        {
            _data[i] = new BenchmarkEntity
            {
                Id = Guid.NewGuid(),
                IntProperty = i,
                StringProperty = "ÇŞĞÜÖİöçşğüı" + i,
                CreateDate = DateTime.UtcNow,
                IntArray = Enumerable.Range(0, i).ToArray(),
                StrArray = Enumerable.Range(0, i).Select(i => i.ToString()).ToArray()
            };
        }

        _searchItemsInt = Enumerable.Range(0, 50).Select(i => i * 20).ToArray();
        _searchItemsStr = Enumerable.Range(0, 50).Select(i => i * 20).Select(i => i.ToString()).ToArray();
        _searchItemsIntJoined = string.Join(",", Enumerable.Range(0, 50).Select(i => i * 20));
        _searchItemsStrJoined = "'" + string.Join("','", Enumerable.Range(0, 50).Select(i => i * 20)) + "'";
    }

    public void Cleanup()
    {
        Directory.Delete(_dataPath, true);
    }

    [GlobalSetup(Targets = new[] {
        nameof(ReindexerNet_SelectArrayMultiple),
        nameof(ReindexerNet_SelectArraySingle),
        nameof(ReindexerNet_SelectMultipleHash),
        nameof(ReindexerNet_SelectMultiplePK),
        nameof(ReindexerNet_SelectRange),
        nameof(ReindexerNet_SelectSingleHash),
        nameof(ReindexerNet_SelectSingleHashParallel),
        nameof(ReindexerNet_SelectSinglePK)
        })]
    public void ReindexerNetSetup()
    {
        Setup();
        var dbPath = Path.Combine(_dataPath, "ReindexerEmbedded");
        if (Directory.Exists(dbPath))
            Directory.Delete(dbPath, true);
        _rxClient = new ReindexerEmbedded(dbPath);
        _rxClient.Connect(new ConnectionOptions { Engine = StorageEngine.LevelDb });
        _rxClient.OpenNamespace("Entities");
        _rxClient.TruncateNamespace("Entities");
        _rxClient.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.Id), IsPk = true, IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = false });
        _rxClient.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.IntProperty), IndexType = IndexType.Tree, FieldType = FieldType.Int, IsDense = false });
        _rxClient.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.StringProperty), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = false });
        _rxClient.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.CreateDate), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = false });
        _rxClient.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.IntArray), IndexType = IndexType.Hash, FieldType = FieldType.Int, IsDense = false, IsArray = true });
        _rxClient.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.StrArray), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = false, IsArray = true });
        _rxClient!.Insert("Entities", _data);
    }

    [GlobalCleanup(Targets = new[] {
        nameof(ReindexerNet_SelectArrayMultiple),
        nameof(ReindexerNet_SelectArraySingle),
        nameof(ReindexerNet_SelectMultipleHash),
        nameof(ReindexerNet_SelectMultiplePK),
        nameof(ReindexerNet_SelectRange),
        nameof(ReindexerNet_SelectSingleHash),
        nameof(ReindexerNet_SelectSingleHashParallel),
        nameof(ReindexerNet_SelectSinglePK)
        })]
    public void ReindexerNetClean()
    {
        _rxClient!.Dispose();
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
        nameof(ReindexerNetSpanJson_SelectArrayMultiple),
        nameof(ReindexerNetSpanJson_SelectArraySingle),
        nameof(ReindexerNetSpanJson_SelectMultipleHash),
        nameof(ReindexerNetSpanJson_SelectMultiplePK),
        nameof(ReindexerNetSpanJson_SelectRange),
        nameof(ReindexerNetSpanJson_SelectSingleHash),
        nameof(ReindexerNetSpanJson_SelectSingleHashParallel),
        nameof(ReindexerNetSpanJson_SelectSinglePK)
        })]
    public void ReindexerNetSpanJsonSetup()
    {
        Setup();
        var dbPathSpanJson = Path.Combine(_dataPath, "ReindexerEmbeddedSpanJson");
        if (Directory.Exists(dbPathSpanJson))
            Directory.Delete(dbPathSpanJson, true);
        _rxClientSpanJson = new ReindexerEmbedded(dbPathSpanJson, new SpanJsonSerializer());
        _rxClientSpanJson.Connect(new ConnectionOptions { Engine = StorageEngine.LevelDb });
        _rxClientSpanJson.OpenNamespace("Entities");
        _rxClientSpanJson.TruncateNamespace("Entities");
        _rxClientSpanJson.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.Id), IsPk = true, IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = true });
        _rxClientSpanJson.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.IntProperty), IndexType = IndexType.Tree, FieldType = FieldType.Int, IsDense = true });
        _rxClientSpanJson.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.StringProperty), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = true });
        _rxClientSpanJson.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.CreateDate), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = true });
        _rxClientSpanJson.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.IntArray), IndexType = IndexType.Hash, FieldType = FieldType.Int, IsDense = true, IsArray = true });
        _rxClientSpanJson.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.StrArray), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = true, IsArray = true });
        _rxClientSpanJson!.Insert("Entities", _data);
    }

    [GlobalCleanup(Targets = new[] {
        nameof(ReindexerNetSpanJson_SelectArrayMultiple),
nameof(ReindexerNetSpanJson_SelectArraySingle),
        nameof(ReindexerNetSpanJson_SelectMultipleHash),
        nameof(ReindexerNetSpanJson_SelectMultiplePK),
        nameof(ReindexerNetSpanJson_SelectRange),
        nameof(ReindexerNetSpanJson_SelectSingleHash),
        nameof(ReindexerNetSpanJson_SelectSingleHashParallel),
        nameof(ReindexerNetSpanJson_SelectSinglePK)
        })]
    public void ReindexerNetSpanJsonClean()
    {
        _rxClientSpanJson!.Dispose();
        Cleanup();
    }

    [GlobalSetup(Targets = new[] {
        nameof(ReindexerNetSql_SelectArrayMultiple),
        nameof(ReindexerNetSql_SelectArraySingle),
        nameof(ReindexerNetSql_SelectMultipleHash),
        nameof(ReindexerNetSql_SelectMultiplePK),
        nameof(ReindexerNetSql_SelectRange),
        nameof(ReindexerNetSql_SelectSingleHash),
        nameof(ReindexerNetSql_SelectSingleHashParallel),
        nameof(ReindexerNetSql_SelectSinglePK)
        })]
    public void ReindexerNetSqlSetup()
    {
        Setup();
        var dbPath = Path.Combine(_dataPath, "ReindexerEmbeddedSql");
        if (Directory.Exists(dbPath))
            Directory.Delete(dbPath, true);
        _rxClientSql = new ReindexerEmbedded(dbPath);
        _rxClientSql.Connect(new ConnectionOptions { Engine = StorageEngine.LevelDb });
        _rxClientSql.OpenNamespace("Entities");
        _rxClientSql.TruncateNamespace("Entities");
        _rxClientSql.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.Id), IsPk = true, IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = false });
        _rxClientSql.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.IntProperty), IndexType = IndexType.Tree, FieldType = FieldType.Int, IsDense = false });
        _rxClientSql.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.StringProperty), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = false });
        _rxClientSql.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.CreateDate), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = false });
        _rxClientSql.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.IntArray), IndexType = IndexType.Hash, FieldType = FieldType.Int, IsDense = false, IsArray = true });
        _rxClientSql.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.StrArray), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = false, IsArray = true });
        _rxClientSql!.Insert("Entities", _data);
    }

    [GlobalCleanup(Targets = new[] {
        nameof(ReindexerNetSql_SelectArrayMultiple),
        nameof(ReindexerNetSql_SelectArraySingle),
        nameof(ReindexerNetSql_SelectMultipleHash),
        nameof(ReindexerNetSql_SelectMultiplePK),
        nameof(ReindexerNetSql_SelectRange),
        nameof(ReindexerNetSql_SelectSingleHash),
        nameof(ReindexerNetSql_SelectSingleHashParallel),
        nameof(ReindexerNetSql_SelectSinglePK)
        })]
    public void ReindexerNetSqlClean()
    {
        _rxClientSql!.Dispose();
        Cleanup();
    }

    [GlobalSetup(Targets = new[] {
        nameof(Cachalot_SelectArrayMultiple),
        nameof(Cachalot_SelectArraySingle),
        nameof(Cachalot_SelectMultipleHash),
        nameof(Cachalot_SelectMultiplePK),
        nameof(Cachalot_SelectRange),
        nameof(Cachalot_SelectSingleHash),
        nameof(Cachalot_SelectSingleHashParallel),
        nameof(Cachalot_SelectSinglePK)
        })]
    public void CachalotSetup()
    {
        Setup();
        var server = new Server.Server(new NodeConfig { DataPath = _dataPath, IsPersistent = true, ClusterName = "embedded" });
        _caConnector = new Connector(new ClientConfig { IsPersistent = true });
        _caConnector.DeclareCollection<BenchmarkEntity>("BenchmarkEntity");
        _caConnector.GetCollectionSchema("BenchmarkEntity").UseCompression = false;
        _caDS = _caConnector!.DataSource<BenchmarkEntity>("BenchmarkEntity");
        _caDS.PutMany(_data);

        //_caDS.Where(e => e.IntArray.Any(i => searchItemsInt.Contains(i))).ToList(),
        //Doesn't support .Any or All methods right now, so combining queries with OR
        GetArrayQueryExpressions(_searchItemsInt, _searchItemsStr, out _intAnyQuery, out _intAllQuery, out _strAnyQuery, out _strAllQuery);
    }

    [GlobalCleanup(Targets = new[] {
        nameof(Cachalot_SelectArrayMultiple),
        nameof(Cachalot_SelectArraySingle),
        nameof(Cachalot_SelectMultipleHash),
        nameof(Cachalot_SelectMultiplePK),
        nameof(Cachalot_SelectRange),
        nameof(Cachalot_SelectSingleHash),
        nameof(Cachalot_SelectSingleHashParallel),
        nameof(Cachalot_SelectSinglePK)
        })]
    public void CachalotClean()
    {
        _caConnector!.Dispose();
        Cleanup();
    }

    [GlobalSetup(Targets = new[] {
        nameof(CachalotCompressed_SelectArrayMultiple),
        nameof(CachalotCompressed_SelectArraySingle),
        nameof(CachalotCompressed_SelectMultipleHash),
        nameof(CachalotCompressed_SelectMultiplePK),
        nameof(CachalotCompressed_SelectRange),
        nameof(CachalotCompressed_SelectSingleHash),
        nameof(CachalotCompressed_SelectSingleHashParallel),
        nameof(CachalotCompressed_SelectSinglePK)
        })]
    public void CachalotCompressedSetup()
    {
        Setup();
        var server = new Server.Server(new NodeConfig { DataPath = _dataPath, IsPersistent = true, ClusterName = "embedded" });
        _caConnectorCompressed = new Connector(new ClientConfig { IsPersistent = true });
        _caConnectorCompressed.DeclareCollection<BenchmarkEntity>("BenchmarkEntity");
        _caConnectorCompressed.GetCollectionSchema("BenchmarkEntity").UseCompression = true;
        _caDSCompressed = _caConnectorCompressed!.DataSource<BenchmarkEntity>("BenchmarkEntity");
        _caDSCompressed.PutMany(_data);

        //Doesn't support .Any or All methods right now, so combining queries with OR
        GetArrayQueryExpressions(_searchItemsInt, _searchItemsStr, out _intAnyQuery, out _intAllQuery, out _strAnyQuery, out _strAllQuery);
    }

    [GlobalCleanup(Targets = new[] {
        nameof(CachalotCompressed_SelectArrayMultiple),
        nameof(CachalotCompressed_SelectArraySingle),
        nameof(CachalotCompressed_SelectMultipleHash),
        nameof(CachalotCompressed_SelectMultiplePK),
        nameof(CachalotCompressed_SelectRange),
        nameof(CachalotCompressed_SelectSingleHash),
        nameof(CachalotCompressed_SelectSingleHashParallel),
        nameof(CachalotCompressed_SelectSinglePK)
        })]
    public void CachalotCompressedClean()
    {
        _caConnectorCompressed!.Dispose();
        Cleanup();
    }

    [GlobalSetup(Targets = new[] {
        nameof(CachalotMemory_SelectArrayMultiple),
        nameof(CachalotMemory_SelectArraySingle),
        nameof(CachalotMemory_SelectMultipleHash),
        nameof(CachalotMemory_SelectMultiplePK),
        nameof(CachalotMemory_SelectRange),
        nameof(CachalotMemory_SelectSingleHash),
        nameof(CachalotMemory_SelectSingleHashParallel),
        nameof(CachalotMemory_SelectSinglePK)
        })]
    public void CachalotMemorySetup()
    {
        Setup();
        var server = new Server.Server(new NodeConfig { DataPath = Path.Combine(_dataPath, "CachalotMemory"), IsPersistent = false, ClusterName = "embedded" });
        _caConnectorMemory = new Connector(new ClientConfig { IsPersistent = false });
        _caConnectorMemory.DeclareCollection<BenchmarkEntity>("BenchmarkEntity");
        _caConnectorMemory.GetCollectionSchema("BenchmarkEntity").UseCompression = false;
        _caDSMemory = _caConnectorMemory!.DataSource<BenchmarkEntity>("BenchmarkEntity");
        _caDSMemory.PutMany(_data);

        //Doesn't support .Any or All methods right now, so combining queries with OR
        GetArrayQueryExpressions(_searchItemsInt, _searchItemsStr, out _intAnyQuery, out _intAllQuery, out _strAnyQuery, out _strAllQuery);
    }

    [GlobalCleanup(Targets = new[] {
        nameof(CachalotMemory_SelectArrayMultiple),
        nameof(CachalotMemory_SelectArraySingle),
        nameof(CachalotMemory_SelectMultipleHash),
        nameof(CachalotMemory_SelectMultiplePK),
        nameof(CachalotMemory_SelectRange),
        nameof(CachalotMemory_SelectSingleHash),
        nameof(CachalotMemory_SelectSingleHashParallel),
        nameof(CachalotMemory_SelectSinglePK)
        })]
    public void CachalotMemoryClean()
    {
        _caConnectorMemory!.Dispose();
        Cleanup();
    }

    [GlobalSetup(Targets = new[] {
        nameof(LiteDb_SelectArrayMultiple),
        nameof(LiteDb_SelectArraySingle),
        nameof(LiteDb_SelectMultipleHash),
        nameof(LiteDb_SelectMultiplePK),
        nameof(LiteDb_SelectRange),
        nameof(LiteDb_SelectSingleHash),
        nameof(LiteDb_SelectSingleHashParallel),
        nameof(LiteDb_SelectSinglePK)
        })]
    public void LiteDBSetup()
    {
        Setup();
        _liteDb = new LiteDatabase(Path.Combine(_dataPath, "LiteDB"));
        _liteColl = _liteDb.GetCollection<BenchmarkEntity>("Entities");
        _liteColl.EnsureIndex(e => e.Id, true);
        _liteColl.EnsureIndex(e => e.IntProperty);
        _liteColl.EnsureIndex(e => e.StringProperty);
        _liteColl.EnsureIndex(e => e.CreateDate);
        _liteColl.EnsureIndex(e => e.IntArray);
        _liteColl.EnsureIndex(e => e.StrArray);
        _liteColl.Upsert(_data);

        //Doesn't support .Any or All methods right now, so combining queries with OR
        GetArrayQueryExpressions(_searchItemsInt, _searchItemsStr, out _intAnyQuery, out _intAllQuery, out _strAnyQuery, out _strAllQuery);
    }

    [GlobalCleanup(Targets = new[] {
        nameof(LiteDb_SelectArrayMultiple),
        nameof(LiteDb_SelectArraySingle),
        nameof(LiteDb_SelectMultipleHash),
        nameof(LiteDb_SelectMultiplePK),
        nameof(LiteDb_SelectRange),
        nameof(LiteDb_SelectSingleHash),
        nameof(LiteDb_SelectSingleHashParallel),
        nameof(LiteDb_SelectSinglePK)
        })]
    public void LiteDBClean()
    {
        _liteDb!.Dispose();
        Cleanup();
    }

    [GlobalSetup(Targets = new[] {
        nameof(LiteDbMemory_SelectArrayMultiple),
        nameof(LiteDbMemory_SelectArraySingle),
        nameof(LiteDbMemory_SelectMultipleHash),
        nameof(LiteDbMemory_SelectMultiplePK),
        nameof(LiteDbMemory_SelectRange),
        nameof(LiteDbMemory_SelectSingleHash),
        nameof(LiteDbMemory_SelectSingleHashParallel),
        nameof(LiteDbMemory_SelectSinglePK)
        })]
    public void LiteDbMemorySetup()
    {
        Setup();
        _liteDbMemory = new LiteDatabase(":memory:");
        _liteCollMemory = _liteDbMemory.GetCollection<BenchmarkEntity>("Entities");
        _liteCollMemory.EnsureIndex(e => e.Id, true);
        _liteCollMemory.EnsureIndex(e => e.IntProperty);
        _liteCollMemory.EnsureIndex(e => e.StringProperty);
        _liteCollMemory.EnsureIndex(e => e.CreateDate);
        _liteCollMemory.EnsureIndex(e => e.IntArray);
        _liteCollMemory.EnsureIndex(e => e.StrArray);
        _liteCollMemory.Upsert(_data);

        //Doesn't support .Any or All methods right now, so combining queries with OR
        GetArrayQueryExpressions(_searchItemsInt, _searchItemsStr, out _intAnyQuery, out _intAllQuery, out _strAnyQuery, out _strAllQuery);
    }

    [GlobalCleanup(Targets = new[] {
        nameof(LiteDbMemory_SelectArrayMultiple),
        nameof(LiteDbMemory_SelectArraySingle),
        nameof(LiteDbMemory_SelectMultipleHash),
        nameof(LiteDbMemory_SelectMultiplePK),
        nameof(LiteDbMemory_SelectRange),
        nameof(LiteDbMemory_SelectSingleHash),
        nameof(LiteDbMemory_SelectSingleHashParallel),
        nameof(LiteDbMemory_SelectSinglePK)
        })]
    public void LiteDbMemoryClean()
    {
        _liteDbMemory!.Dispose();
        Cleanup();
    }

    [GlobalSetup(Targets = new[] {
        nameof(Realm_SelectArrayMultiple),
        nameof(Realm_SelectArraySingle),
        nameof(Realm_SelectMultipleHash),
        nameof(Realm_SelectMultiplePK),
        nameof(Realm_SelectRange),
        nameof(Realm_SelectSingleHash),
        nameof(Realm_SelectSingleHashParallel),
        nameof(Realm_SelectSinglePK)
        })]
    public void RealmSetup()
    {
        Setup();
        _realm = Realm.GetInstance(new RealmConfiguration(Path.Combine(_dataPath, "Realms")));
        _realm.Write(() =>
        {
            _realm.Add(_data.Select(e => (BenchmarkRealmEntity)e), update: false);
        });
    }

    [GlobalCleanup(Targets = new[] {
        nameof(Realm_SelectArrayMultiple),
        nameof(Realm_SelectArraySingle),
        nameof(Realm_SelectMultipleHash),
        nameof(Realm_SelectMultiplePK),
        nameof(Realm_SelectRange),
        nameof(Realm_SelectSingleHash),
        nameof(Realm_SelectSingleHashParallel),
        nameof(Realm_SelectSinglePK)
        })]
    public void RealmClean()
    {
        _realm.Dispose();
        Realm.DeleteRealm(new RealmConfiguration(Path.Combine(_dataPath, "Realm")));
        Cleanup();
    }
    #endregion

    [Benchmark]
    public IList<object?> ReindexerNet_SelectSinglePK()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            result.Add(_rxClient.Execute<BenchmarkEntity>("Entities", q => q.Limit(1).WhereGuid("Id", Condition.EQ, _data[i].Id)));
        }

        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSpanJson_SelectSinglePK()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            result.Add(_rxClientSpanJson.Execute<BenchmarkEntity>("Entities", q => q.Limit(1).WhereGuid("Id", Condition.EQ, _data[i].Id)));
        }
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSql_SelectSinglePK()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            result.Add(_rxClientSql.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE Id = '{_data[i].Id}' LIMIT 1"));
        }

        return result;
    }

    [Benchmark]
    public IList<object?> Cachalot_SelectSinglePK()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            var id = _data[i].Id;
            result.Add(_caDS[id]);
        }

        return result;
    }

    [Benchmark]
    public IList<object?> CachalotMemory_SelectSinglePK()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            var id = _data[i].Id;
            result.Add(_caDSMemory[id]);
        }

        return result;
    }

    [Benchmark]
    public IList<object?> CachalotCompressed_SelectSinglePK()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            var id = _data[i].Id;
            result.Add(_caDSCompressed[id]);
        }
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDb_SelectSinglePK()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            var id = _data[i].Id;
            result.Add(_liteColl.FindById(id));
        }
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDbMemory_SelectSinglePK()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            var id = _data[i].Id;
            result.Add(_liteCollMemory.FindById(id));
        }
        return result;
    }

    [Benchmark]
    public IList<object?> Realm_SelectSinglePK()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            result.Add(_realm.Find<BenchmarkRealmEntity>(_data[i].Id));
        }
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNet_SelectMultiplePK()
    {
        var result = new List<object?>();
        var ids = _data.Take(N).Select(i => i.Id).ToArray();
        result.Add(_rxClient.Execute<BenchmarkEntity>("Entities", q => q.WhereGuid("Id", Condition.SET, ids)));
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSpanJson_SelectMultiplePK()
    {
        var result = new List<object?>();
        var ids = _data.Take(N).Select(i => i.Id).ToArray();
        result.Add(_rxClientSpanJson.Execute<BenchmarkEntity>("Entities", q => q.WhereGuid("Id", Condition.SET, ids)));
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSql_SelectMultiplePK()
    {
        var result = new List<object?>();
        var ids = string.Join("','", _data.Take(N).Select(i => i.Id));
        result.Add(_rxClientSql.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE Id IN ('{ids}')"));
        return result;
    }

    [Benchmark]
    public IList<object?> Cachalot_SelectMultiplePK()
    {
        var result = new List<object?>();
        var ids = _data.Take(N).Select(i => i.Id).ToList();
        result.Add(_caDS.Where(e => ids.Contains(e.Id)).ToList());
        return result;
    }

    [Benchmark]
    public IList<object?> CachalotMemory_SelectMultiplePK()
    {
        var result = new List<object?>();
        var ids = _data.Take(N).Select(i => i.Id).ToList();
        result.Add(_caDSMemory.Where(e => ids.Contains(e.Id)).ToList());
        return result;
    }

    [Benchmark]
    public IList<object?> CachalotCompressed_SelectMultiplePK()
    {
        var result = new List<object?>();
        var ids = _data.Take(N).Select(i => i.Id).ToList();
        result.Add(_caDSCompressed.Where(e => ids.Contains(e.Id)).ToList());
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDb_SelectMultiplePK()
    {
        var result = new List<object?>();
        var ids = _data.Take(N).Select(i => i.Id).ToList();
        result.Add(_liteColl.Query().Where(e => ids.Contains(e.Id)).ToList());
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDbMemory_SelectMultiplePK()
    {
        var result = new List<object?>();
        var ids = _data.Take(N).Select(i => i.Id).ToList();
        result.Add(_liteCollMemory.Query().Where(e => ids.Contains(e.Id)).ToList());
        return result;
    }

    [Benchmark]
    public IList<object?> Realm_SelectMultiplePK()
    {
        var result = new List<object?>();
        var filter = string.Join(" OR ", _data.Take(N).Select(i => $"(Id == uuid({i.Id}))"));
        result.Add(_realm.All<BenchmarkRealmEntity>().Filter(filter).ToList());
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNet_SelectSingleHash()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            result.Add(_rxClient.Execute<BenchmarkEntity>("Entities", q => q.WhereString("StringProperty", Condition.EQ, _data[i].StringProperty).Limit(1)));
        }

        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSpanJson_SelectSingleHash()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            result.Add(_rxClientSpanJson.Execute<BenchmarkEntity>("Entities", q => q.WhereString("StringProperty", Condition.EQ, _data[i].StringProperty).Limit(1)));
        }
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSql_SelectSingleHash()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            result.Add(_rxClientSql.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE StringProperty = '{_data[i].StringProperty}' LIMIT 1"));
        }

        return result;
    }

    [Benchmark]
    public IList<object?> Cachalot_SelectSingleHash()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            var str = _data[i].StringProperty;
            result.Add(_caDS.FirstOrDefault(e => e.StringProperty == str));
        }
        return result;
    }

    [Benchmark]
    public IList<object?> CachalotMemory_SelectSingleHash()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            var str = _data[i].StringProperty;
            result.Add(_caDSMemory.FirstOrDefault(e => e.StringProperty == str));
        }
        return result;
    }

    [Benchmark]
    public IList<object?> CachalotCompressed_SelectSingleHash()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            var str = _data[i].StringProperty;
            result.Add(_caDSCompressed.FirstOrDefault(e => e.StringProperty == str));
        }
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDb_SelectSingleHash()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            var str = _data[i].StringProperty;
            result.Add(_liteColl.Query().Where(e => e.StringProperty == str).FirstOrDefault());
        }
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDbMemory_SelectSingleHash()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            var str = _data[i].StringProperty;
            result.Add(_liteCollMemory.Query().Where(e => e.StringProperty == str).FirstOrDefault());
        }
        return result;
    }

    [Benchmark]
    public IList<object?> Realm_SelectSingleHash()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            var str = _data[i].StringProperty;
            result.Add(_realm.All<BenchmarkRealmEntity>().Where(e => e.StringProperty == str).FirstOrDefault());
        }
        return result;
    }

    [Benchmark]
    public ConcurrentBag<object?> ReindexerNet_SelectSingleHashParallel()
    {
        var result = new ConcurrentBag<object?>();
        Parallel.For(0, N, i =>
        {
            result.Add(_rxClient.Execute<BenchmarkEntity>("Entities", q => q.WhereString("StringProperty", Condition.EQ, _data[i].StringProperty).Limit(1)));
        });
        return result;
    }

    [Benchmark]
    public ConcurrentBag<object?> ReindexerNetSpanJson_SelectSingleHashParallel()
    {
        var result = new ConcurrentBag<object?>();
        Parallel.For(0, N, i =>
        {
            result.Add(_rxClientSpanJson.Execute<BenchmarkEntity>("Entities", q => q.WhereString("StringProperty", Condition.EQ, _data[i].StringProperty).Limit(1)));
        });
        return result;
    }

    [Benchmark]
    public ConcurrentBag<object?> ReindexerNetSql_SelectSingleHashParallel()
    {
        var result = new ConcurrentBag<object?>();
        Parallel.For(0, N, i =>
        {
            result.Add(_rxClientSql.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE StringProperty = '{_data[i].StringProperty}' LIMIT 1"));
        });
        return result;
    }

    [Benchmark]
    public ConcurrentBag<object?> Cachalot_SelectSingleHashParallel()
    {
        var result = new ConcurrentBag<object?>();
        Parallel.For(0, N, i =>
        {
            var str = _data[i].StringProperty;
            result.Add(_caDS.FirstOrDefault(e => e.StringProperty == str));
        });
        return result;
    }

    [Benchmark]
    public ConcurrentBag<object?> CachalotMemory_SelectSingleHashParallel()
    {
        var result = new ConcurrentBag<object?>();
        Parallel.For(0, N, i =>
        {
            var str = _data[i].StringProperty;
            result.Add(_caDSMemory.FirstOrDefault(e => e.StringProperty == str));
        });
        return result;
    }

    [Benchmark]
    public ConcurrentBag<object?> CachalotCompressed_SelectSingleHashParallel()
    {
        var result = new ConcurrentBag<object?>();
        Parallel.For(0, N, i =>
        {
            var str = _data[i].StringProperty;
            result.Add(_caDSCompressed.FirstOrDefault(e => e.StringProperty == str));
        });
        return result;
    }

    [Benchmark]
    public ConcurrentBag<object?> LiteDb_SelectSingleHashParallel()
    {
        var result = new ConcurrentBag<object?>();
        Parallel.For(0, N, i =>
        {
            var str = _data[i].StringProperty;
            result.Add(_liteColl.Query().Where(e => e.StringProperty == str).FirstOrDefault());
        });
        return result;
    }

    [Benchmark]
    public ConcurrentBag<object?> LiteDbMemory_SelectSingleHashParallel()
    {
        var result = new ConcurrentBag<object?>();
        Parallel.For(0, N, i =>
        {
            var str = _data[i].StringProperty;
            result.Add(_liteCollMemory.Query().Where(e => e.StringProperty == str).FirstOrDefault());
        });
        return result;
    }

    [Benchmark]
    public ConcurrentBag<object?> Realm_SelectSingleHashParallel()
    {
        var result = new ConcurrentBag<object?>();
        Parallel.For(0, N, i =>
        {
            using var realm = Realm.GetInstance(new RealmConfiguration(Path.Combine(_dataPath, "Realms")));
            var str = _data[i].StringProperty;
            result.Add(realm.All<BenchmarkRealmEntity>().Where(e => e.StringProperty == str).FirstOrDefault());
        });
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNet_SelectMultipleHash()
    {
        var result = new List<object?>();
        var props = _data.Take(N).Select(i => i.StringProperty).ToArray();
        result.Add(_rxClient.Execute<BenchmarkEntity>("Entities", q => q.WhereString("StringProperty", Condition.SET, props)));
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSpanJson_SelectMultipleHash()
    {
        var result = new List<object?>();
        var props = _data.Take(N).Select(i => i.StringProperty).ToArray();
        result.Add(_rxClientSpanJson.Execute<BenchmarkEntity>("Entities", q => q.WhereString("StringProperty", Condition.SET, props)));
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSql_SelectMultipleHash()
    {
        var result = new List<object?>();
        var props = string.Join("','", _data.Take(N).Select(i => i.StringProperty));
        result.Add(_rxClientSql.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE StringProperty IN ('{props}')"));
        return result;
    }

    [Benchmark]
    public IList<object?> Cachalot_SelectMultipleHash()
    {
        var result = new List<object?>();
        var props = _data.Take(N).Select(i => i.StringProperty).ToList();
        result.Add(_caDS.Where(e => props.Contains(e.StringProperty)).ToList());
        return result;
    }

    [Benchmark]
    public IList<object?> CachalotMemory_SelectMultipleHash()
    {
        var result = new List<object?>();
        var props = _data.Take(N).Select(i => i.StringProperty).ToList();
        result.Add(_caDSMemory.Where(e => props.Contains(e.StringProperty)).ToList());
        return result;
    }

    [Benchmark]
    public IList<object?> CachalotCompressed_SelectMultipleHash()
    {
        var result = new List<object?>();
        var props = _data.Take(N).Select(i => i.StringProperty).ToList();
        result.Add(_caDSCompressed.Where(e => props.Contains(e.StringProperty)).ToList());
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDb_SelectMultipleHash()
    {
        var result = new List<object?>();
        var props = _data.Take(N).Select(i => i.StringProperty).ToList();
        result.Add(_liteColl.Query().Where(e => props.Contains(e.StringProperty)).ToList());
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDbMemory_SelectMultipleHash()
    {
        var result = new List<object?>();
        var props = _data.Take(N).Select(i => i.StringProperty).ToList();
        result.Add(_liteCollMemory.Query().Where(e => props.Contains(e.StringProperty)).ToList());
        return result;
    }

    [Benchmark]
    public IList<object?> Realm_SelectMultipleHash()
    {
        var result = new List<object?>();
        var filter = string.Join(" OR ", _data.Take(N).Select(i => $"(StringProperty == '{i.StringProperty}')"));
        result.Add(_realm.All<BenchmarkRealmEntity>().Filter(filter).ToList());
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNet_SelectRange()
    {
        var result = new List<object?>();
        var entity = _data[N / 2];
        result.Add(_rxClient.Execute<BenchmarkEntity>("Entities", q => q.WhereInt32("IntProperty", Condition.LT, entity.IntProperty ?? 0)));
        result.Add(_rxClient.Execute<BenchmarkEntity>("Entities", q => q.WhereInt32("IntProperty", Condition.GE, entity.IntProperty ?? 0)));
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSpanJson_SelectRange()
    {
        var result = new List<object?>();
        var entity = _data[N / 2];
        result.Add(_rxClientSpanJson.Execute<BenchmarkEntity>("Entities", q => q.WhereInt32("IntProperty", Condition.LT, entity.IntProperty ?? 0)));
        result.Add(_rxClientSpanJson.Execute<BenchmarkEntity>("Entities", q => q.WhereInt32("IntProperty", Condition.GE, entity.IntProperty ?? 0)));
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSql_SelectRange()
    {
        var result = new List<object?>();
        var entity = _data[N / 2];
        result.Add(_rxClientSql.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE IntProperty < {entity.IntProperty}"));
        result.Add(_rxClientSql.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE IntProperty >= {entity.IntProperty}"));
        return result;
    }

    [Benchmark]
    public IList<object?> Cachalot_SelectRange()
    {
        var result = new List<object?>();
        var entity = _data[N / 2];
        result.Add(_caDS.Where(e => e.IntProperty < entity.IntProperty).ToList());
        result.Add(_caDS.Where(e => e.IntProperty >= entity.IntProperty).ToList());
        return result;
    }

    [Benchmark]
    public IList<object?> CachalotMemory_SelectRange()
    {
        var result = new List<object?>();
        var entity = _data[N / 2];
        result.Add(_caDSMemory.Where(e => e.IntProperty < entity.IntProperty).ToList());
        result.Add(_caDSMemory.Where(e => e.IntProperty >= entity.IntProperty).ToList());
        return result;
    }

    [Benchmark]
    public IList<object?> CachalotCompressed_SelectRange()
    {
        var result = new List<object?>();
        var entity = _data[N / 2];
        result.Add(_caDSCompressed.Where(e => e.IntProperty < entity.IntProperty).ToList());
        result.Add(_caDSCompressed.Where(e => e.IntProperty >= entity.IntProperty).ToList());
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDb_SelectRange()
    {
        var result = new List<object?>();
        var entity = _data[N / 2];
        result.Add(_liteColl.Query().Where(e => e.IntProperty < entity.IntProperty).ToList());
        result.Add(_liteColl.Query().Where(e => e.IntProperty >= entity.IntProperty).ToList());
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDbMemory_SelectRange()
    {
        var result = new List<object?>();
        var entity = _data[N / 2];
        result.Add(_liteCollMemory.Query().Where(e => e.IntProperty < entity.IntProperty).ToList());
        result.Add(_liteCollMemory.Query().Where(e => e.IntProperty >= entity.IntProperty).ToList());
        return result;
    }

    [Benchmark]
    public IList<object?> Realm_SelectRange()
    {
        var result = new List<object?>();
        var entity = _data[N / 2];
        result.Add(_realm.All<BenchmarkRealmEntity>().Where(e => e.IntProperty < entity.IntProperty).ToList());
        result.Add(_realm.All<BenchmarkRealmEntity>().Where(e => e.IntProperty >= entity.IntProperty).ToList());
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNet_SelectArraySingle()
    {
        var result = new List<object?>
        {
            _rxClient.Execute<BenchmarkEntity>("Entities", q => q.WhereInt32("IntArray", Condition.SET, N)),
            _rxClient.Execute<BenchmarkEntity>("Entities", q => q.WhereString("StrArray", Condition.SET, N.ToString())),
        };
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSpanJson_SelectArraySingle()
    {
        var result = new List<object?>
        {
            _rxClientSpanJson.Execute<BenchmarkEntity>("Entities", q => q.WhereInt32("IntArray", Condition.SET, N)),
            _rxClientSpanJson.Execute<BenchmarkEntity>("Entities", q => q.WhereString("StrArray", Condition.SET, N.ToString())),
        };
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSql_SelectArraySingle()
    {
        var result = new List<object?>
        {
            _rxClientSql.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE IntArray IN ({N})"),
            _rxClientSql.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE StrArray IN ('{N}')"),
        };
        return result;
    }

    [Benchmark]
    public IList<object?> Cachalot_SelectArraySingle()
    {
        var nstr = N.ToString();
        var result = new List<object?>
        {
            _caDS.Where(e => e.IntArray.Contains(N)).ToList(),
            _caDS.Where(e => e.StrArray.Contains(nstr)).ToList(),
        };
        return result;
    }

    [Benchmark]
    public IList<object?> CachalotMemory_SelectArraySingle()
    {
        var nstr = N.ToString();
        var result = new List<object?>
        {
            _caDSMemory.Where(e => e.IntArray.Contains(N)).ToList(),
            _caDSMemory.Where(e => e.StrArray.Contains(nstr)).ToList(),
        };
        return result;
    }

    [Benchmark]
    public IList<object?> CachalotCompressed_SelectArraySingle()
    {
        var nstr = N.ToString();
        var result = new List<object?>
        {
            _caDSCompressed.Where(e => e.IntArray.Contains(N)).ToList(),
            _caDSCompressed.Where(e => e.StrArray.Contains(nstr)).ToList(),
        };
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDb_SelectArraySingle()
    {
        var nstr = N.ToString();
        var result = new List<object?>
        {
            _liteColl.Query().Where(e => e.IntArray.Contains(N)).ToList(),
            _liteColl.Query().Where(e => e.StrArray.Contains(nstr)).ToList(),
        };
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDbMemory_SelectArraySingle()
    {
        var nstr = N.ToString();
        var result = new List<object?>
        {
            _liteCollMemory.Query().Where(e => e.IntArray.Contains(N)).ToList(),
            _liteCollMemory.Query().Where(e => e.StrArray.Contains(nstr)).ToList(),
        };
        return result;
    }

    [Benchmark]
    public IList<object?> Realm_SelectArraySingle()
    {
        var result = new List<object?>
        {
            _realm.All<BenchmarkRealmEntity>().Filter("ANY IntArray.@values == $0", N).ToList(),
            _realm.All<BenchmarkRealmEntity>().Filter("ANY StrArray.@values == '$0'", N.ToString()).ToList(),
        };
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNet_SelectArrayMultiple()
    {
        var result = new List<object?>
        {
            _rxClient.Execute<BenchmarkEntity>("Entities", q => q.WhereInt32("IntArray", Condition.SET, _searchItemsInt)),
            _rxClient.Execute<BenchmarkEntity>("Entities", q => q.WhereString("StrArray", Condition.SET, _searchItemsStr)),
            _rxClient.Execute<BenchmarkEntity>("Entities", q => q.WhereInt32("IntArray", Condition.ALLSET, _searchItemsInt)),
            _rxClient.Execute<BenchmarkEntity>("Entities", q => q.WhereString("StrArray", Condition.ALLSET, _searchItemsStr))
        };
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSpanJson_SelectArrayMultiple()
    {
        var result = new List<object?>
        {
            _rxClientSpanJson.Execute<BenchmarkEntity>("Entities", q => q.WhereInt32("IntArray", Condition.SET, _searchItemsInt)),
            _rxClientSpanJson.Execute<BenchmarkEntity>("Entities", q => q.WhereString("StrArray", Condition.SET, _searchItemsStr)),
            _rxClientSpanJson.Execute<BenchmarkEntity>("Entities", q => q.WhereInt32("IntArray", Condition.ALLSET, _searchItemsInt)),
            _rxClientSpanJson.Execute<BenchmarkEntity>("Entities", q => q.WhereString("StrArray", Condition.ALLSET, _searchItemsStr))
        };
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSql_SelectArrayMultiple()
    {
        var result = new List<object?>
        {
            _rxClientSql.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE IntArray IN ({_searchItemsIntJoined})"),
            _rxClientSql.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE StrArray IN ({_searchItemsStrJoined})"),
            _rxClientSql.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE IntArray ALLSET ({_searchItemsIntJoined})"),
            _rxClientSql.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE StrArray ALLSET ({_searchItemsStrJoined})")
        };
        return result;
    }

    [Benchmark]
    public IList<object?> Cachalot_SelectArrayMultiple()
    {        
        var result = new List<object?>
        {
            _caDS.Where(_intAnyQuery).ToList(),
            _caDS.Where(_strAnyQuery).ToList(),
            _caDS.Where(_intAllQuery).ToList(),
            _caDS.Where(_strAllQuery).ToList()
        };
        return result;
    }

    [Benchmark]
    public IList<object?> CachalotMemory_SelectArrayMultiple()
    {
        var result = new List<object?>
        {
            _caDSMemory.Where(_intAnyQuery).ToList(),
            _caDSMemory.Where(_strAnyQuery).ToList(),
            _caDSMemory.Where(_intAllQuery).ToList(),
            _caDSMemory.Where(_strAllQuery).ToList()
        };
        return result;
    }

    [Benchmark]
    public IList<object?> CachalotCompressed_SelectArrayMultiple()
    {        
        var result = new List<object?>
        {
            _caDSCompressed.Where(_intAnyQuery).ToList(),
            _caDSCompressed.Where(_strAnyQuery).ToList(),
            _caDSCompressed.Where(_intAllQuery).ToList(),
            _caDSCompressed.Where(_strAllQuery).ToList()
        };
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDb_SelectArrayMultiple()
    {
        var result = new List<object?>
        {
            _liteColl.Query().Where(_intAnyQuery).ToList(),
            _liteColl.Query().Where(_strAnyQuery).ToList(),
            _liteColl.Query().Where(_intAllQuery).ToList(),
            _liteColl.Query().Where(_strAllQuery).ToList()
        };
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDbMemory_SelectArrayMultiple()
    {        
        var result = new List<object?>
        {
            _liteCollMemory.Query().Where(_intAnyQuery).ToList(),
            _liteCollMemory.Query().Where(_strAnyQuery).ToList(),
            _liteCollMemory.Query().Where(_intAllQuery).ToList(),
            _liteCollMemory.Query().Where(_strAllQuery).ToList()
        };
        return result;
    }

    [Benchmark]
    public IList<object?> Realm_SelectArrayMultiple()
    {
        var result = new List<object?>
        {
            _realm.All<BenchmarkRealmEntity>().Filter($"ANY IntArray.@values IN {{ {_searchItemsIntJoined} }}").ToList(),
            _realm.All<BenchmarkRealmEntity>().Filter($"ANY StrArray.@values IN {{ {_searchItemsStrJoined} }}").ToList(),
            _realm.All<BenchmarkRealmEntity>().Filter($"ALL IntArray.@values IN {{ {_searchItemsIntJoined} }}").ToList(),
            _realm.All<BenchmarkRealmEntity>().Filter($"ALL StrArray.@values IN {{ {_searchItemsStrJoined} }}").ToList()
        };
        return result;
    }
}