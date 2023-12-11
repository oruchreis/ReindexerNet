using BenchmarkDotNet.Attributes;
using Cachalot.Linq;
using Client.Interface;
using LiteDB;
using Realms;
using ReindexerNet.Embedded;
using Server;
using ReindexerNet;
using Index = ReindexerNet.Index;
using IndexType = ReindexerNet.IndexType;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using System.Reflection;

namespace ReindexerNetBenchmark.EmbeddedBenchmarks;

//[Config(typeof(AntiVirusFriendlyConfig))]
[SimpleJob(launchCount: 0, warmupCount: 0, iterationCount: 1)]
[MemoryDiagnoser()]
[CategoriesColumn]
[CustomCategoryDiscoverer]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory, BenchmarkLogicalGroupRule.ByParams)]
[PlainExporter]
public class InsertBenchmark
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

    [Params(10_000, 100_000)]
    public int N;

    private ReindexerEmbedded? _rxClient;
    private ReindexerEmbedded? _rxClientDense;
    private LiteDatabase _liteDb;
    private LiteDatabase _liteDbMemory;
    private ILiteCollection<BenchmarkEntity> _liteColl;
    private ILiteCollection<BenchmarkEntity> _liteCollMemory;
    private Server.Server _caServer;
    private Connector? _caConnector;
    private Connector? _caConnectorCompressed;
    private Connector _caMemoryConnector;
    private Realm _realm;

    public void Setup()
    {
        _dataPath = Directory.CreateTempSubdirectory().FullName;
        _data = new BenchmarkEntity[N];
        for (int i = 0; i < N; i++)
        {
            _data[i] = new BenchmarkEntity()
            {
                Id = Guid.NewGuid(),
                IntProperty = i,
                StringProperty = "ÇŞĞÜÖİöçşğüı",
                CreateDate = DateTime.UtcNow,
                IntArray = new[] { 123, 124, 456, 456, 6777, 3123, 123123, 333 },
                StrArray = new[] { "", "Abc", "Def", "FFF", "GGG", "HHH", "HGFDF", "asd" }
            };
        }
        Console.WriteLine(_dataPath);
    }

    public void Cleanup()
    {
        Directory.Delete(_dataPath, true);
    }

    [IterationSetup(Targets = new[] { nameof(ReindexerNet_Insert) })]
    public void ReindexerNetInsertSetup()
    {
        ReindexerNetSetup();
    }
    [IterationSetup(Targets = new[] { nameof(ReindexerNet_Upsert) })]
    public void ReindexerNetUpsertSetup()
    {
        ReindexerNetSetup();
        ReindexerNet_Insert();
    }

    public void ReindexerNetSetup()
    {
        Setup();
        var rxDbPath = Path.Combine(_dataPath, "ReindexerEmbedded");
        if (Directory.Exists(rxDbPath))
            Directory.Delete(rxDbPath, true);
        _rxClient = new ReindexerEmbedded(rxDbPath);
        _rxClient.Connect(new ConnectionOptions { Engine = StorageEngine.LevelDb });
        _rxClient.OpenNamespace("Entities");
        _rxClient.TruncateNamespace("Entities");
        _rxClient.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.Id), IsPk = true, IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = false });
        _rxClient.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.IntProperty), IndexType = IndexType.Tree, FieldType = FieldType.Int, IsDense = false });
        _rxClient.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.StringProperty), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = false });
        _rxClient.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.CreateDate), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = false });
        _rxClient.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.IntArray), IndexType = IndexType.Hash, FieldType = FieldType.Int, IsDense = false, IsArray = true });
        _rxClient.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.StrArray), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = false, IsArray = true });

    }

    [IterationCleanup(Targets = new[] { nameof(ReindexerNet_Insert), nameof(ReindexerNet_Upsert) })]
    public void ReindexerNetClean()
    {
        _rxClient!.Dispose();
        Cleanup();
    }

    [IterationSetup(Targets = new[] { nameof(ReindexerNetDense_Insert) })]
    public void ReindexerNetDenseInsertSetup()
    {
        ReindexerNetDenseSetup();
    }
    [IterationSetup(Targets = new[] { nameof(ReindexerNetDense_Upsert) })]
    public void ReindexerNetDenseUpsertSetup()
    {
        ReindexerNetDenseSetup();
        ReindexerNetDense_Insert();
    }

    public void ReindexerNetDenseSetup()
    {
        Setup();
        var rxDensedbPath = Path.Combine(_dataPath, "ReindexerEmbeddedDense");
        if (Directory.Exists(rxDensedbPath))
            Directory.Delete(rxDensedbPath, true);
        _rxClientDense = new ReindexerEmbedded(rxDensedbPath);
        _rxClientDense.Connect(new ConnectionOptions { Engine = StorageEngine.LevelDb });
        _rxClientDense.OpenNamespace("Entities");
        _rxClientDense.TruncateNamespace("Entities");
        _rxClientDense.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.Id), IsPk = true, IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = true });
        _rxClientDense.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.IntProperty), IndexType = IndexType.Tree, FieldType = FieldType.Int, IsDense = true });
        _rxClientDense.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.StringProperty), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = true });
        _rxClientDense.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.CreateDate), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = true });
        _rxClientDense.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.IntArray), IndexType = IndexType.Hash, FieldType = FieldType.Int, IsDense = true, IsArray = true });
        _rxClientDense.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.StrArray), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = true, IsArray = true });
    }

    [IterationCleanup(Targets = new[] { nameof(ReindexerNetDense_Insert), nameof(ReindexerNetDense_Upsert) })]
    public void ReindexerNetDenseClean()
    {
        _rxClientDense!.Dispose();
        Cleanup();
    }

    [IterationSetup(Targets = new[] { nameof(Cachalot_Insert) })]
    public void CachalotInsertSetup()
    {
        CachalotSetup();
    }
    [IterationSetup(Targets = new[] { nameof(Cachalot_Upsert) })]
    public void CachalotUpsertSetup()
    {
        CachalotSetup();
        Cachalot_Insert();
    }

    public void CachalotSetup()
    {
        Setup();
        Directory.CreateDirectory(Path.Combine(_dataPath, "Cachalot"));
        _caServer?.Stop();
        _caServer = new Server.Server(new NodeConfig { DataPath = Path.Combine(_dataPath, "Cachalot"), IsPersistent = true, ClusterName = "Cachalot" });
        _caConnector = new Connector(new ClientConfig { IsPersistent = true });
        _caConnector.DeclareCollection<BenchmarkEntity>("BenchmarkEntity");
        _caConnector.GetCollectionSchema("BenchmarkEntity").UseCompression = false;
    }

    [IterationCleanup(Targets = new[] { nameof(Cachalot_Insert), nameof(Cachalot_Upsert) })]
    public void CachalotClean()
    {
        _caConnector!.Dispose();
        _caServer.Stop();
        Cleanup();
    }

    [IterationSetup(Targets = new[] { nameof(CachalotCompressed_Insert) })]
    public void CachalotCompressedInsertSetup()
    {
        CachalotCompressedSetup();
    }
    [IterationSetup(Targets = new[] { nameof(CachalotCompressed_Upsert) })]
    public void CachalotCompressedUpsertSetup()
    {
        CachalotCompressedSetup();
        CachalotCompressed_Insert();
    }

    public void CachalotCompressedSetup()
    {
        Setup();
        Directory.CreateDirectory(Path.Combine(_dataPath, "CachalotCompressed"));
        _caServer?.Stop();
        _caServer = new Server.Server(new NodeConfig { DataPath = Path.Combine(_dataPath, "CachalotCompressed"), IsPersistent = true, ClusterName = "CachalotCompressed" });
        _caConnectorCompressed = new Connector(new ClientConfig { IsPersistent = true });
        _caConnectorCompressed.DeclareCollection<BenchmarkEntity>("BenchmarkEntity");
        _caConnectorCompressed.GetCollectionSchema("BenchmarkEntity").UseCompression = true;
    }

    [IterationCleanup(Targets = new[] { nameof(CachalotCompressed_Insert), nameof(CachalotCompressed_Upsert) })]
    public void CachalotCompressedClean()
    {
        _caConnectorCompressed!.Dispose();
        _caServer.Stop();
        Cleanup();
    }

    [IterationSetup(Targets = new[] { nameof(CachalotOnlyMemory_Insert) })]
    public void CachalotOnlyMemoryInsertSetup()
    {
        CachalotOnlyMemorySetup();
    }
    [IterationSetup(Targets = new[] { nameof(CachalotOnlyMemory_Upsert) })]
    public void CachalotOnlyMemoryUpsertSetup()
    {
        CachalotOnlyMemorySetup();
        CachalotOnlyMemory_Insert();
    }

    public void CachalotOnlyMemorySetup()
    {
        Setup();
        _caServer = new Server.Server(new NodeConfig { DataPath = Path.Combine(_dataPath, "CachalotOnlyMemory"), IsPersistent = false, ClusterName = "CachalotOnlyMemory" });
        _caMemoryConnector = new Connector(new ClientConfig { IsPersistent = false });
        _caMemoryConnector.DeclareCollection<BenchmarkEntity>("BenchmarkEntity");
    }

    [IterationCleanup(Targets = new[] { nameof(CachalotOnlyMemory_Insert), nameof(CachalotOnlyMemory_Upsert) })]
    public void CachalotOnlyMemoryClean()
    {
        _caMemoryConnector!.Dispose();
        _caServer.Stop();
        Cleanup();
    }

    [IterationSetup(Targets = new[] { nameof(LiteDb_Insert) })]
    public void LiteDbInsertSetup()
    {
        LiteDbSetup();
    }
    [IterationSetup(Targets = new[] { nameof(LiteDb_Upsert) })]
    public void LiteDbUpsertSetup()
    {
        LiteDbSetup();
        LiteDb_Insert();
    }
    public void LiteDbSetup()
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
    }

    [IterationCleanup(Targets = new[] { nameof(LiteDb_Insert), nameof(LiteDb_Upsert) })]
    public void LiteDbClean()
    {
        _liteDb!.Dispose();
        Cleanup();
    }

    [IterationSetup(Targets = new[] { nameof(LiteDbMemory_Insert) })]
    public void LiteDbMemoryInsertSetup()
    {
        LiteDbMemorySetup();
    }
    [IterationSetup(Targets = new[] { nameof(LiteDbMemory_Upsert) })]
    public void LiteDbMemoryUpsertSetup()
    {
        LiteDbMemorySetup();
        LiteDbMemory_Insert();
    }
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
    }

    [IterationCleanup(Targets = new[] { nameof(LiteDbMemory_Insert), nameof(LiteDbMemory_Upsert) })]
    public void LiteDbMemoryClean()
    {
        _liteDbMemory!.Dispose();
        Cleanup();
    }

    [IterationSetup(Targets = new[] { nameof(Realm_Insert) })]
    public void RealmInsertSetup()
    {
        RealmSetup();
    }
    [IterationSetup(Targets = new[] { nameof(Realm_Upsert) })]
    public void RealmUpsertSetup()
    {
        RealmSetup();
        Realm_Insert();
    }
    public void RealmSetup()
    {
        Setup();
        _realm = Realm.GetInstance(new RealmConfiguration(Path.Combine(_dataPath, "Realm")));
    }

    [IterationCleanup(Targets = new[] { nameof(Realm_Insert), nameof(Realm_Upsert) })]
    public void RealmClean()
    {
        _realm.Dispose();
        Realm.DeleteRealm(new RealmConfiguration(Path.Combine(_dataPath, "Realm")));
        Cleanup();
    }

    [BenchmarkCategory("Insert")]
    [Benchmark]
    public void ReindexerNet_Insert()
    {
        _rxClient!.Insert("Entities", _data);
    }

    [BenchmarkCategory("Insert")]
    [Benchmark]
    public void ReindexerNetDense_Insert()
    {
        _rxClientDense!.Insert("Entities", _data);
    }

    [BenchmarkCategory("Insert")]
    [Benchmark]
    public void Cachalot_Insert()
    {
        var entities = _caConnector!.DataSource<BenchmarkEntity>("BenchmarkEntity");
        entities.PutMany(_data);
    }

    [BenchmarkCategory("Insert")]
    [Benchmark]
    public void CachalotCompressed_Insert()
    {
        var entities = _caConnectorCompressed!.DataSource<BenchmarkEntity>("BenchmarkEntity");
        entities.PutMany(_data);
    }

    [BenchmarkCategory("Insert")]
    [Benchmark]
    public void CachalotOnlyMemory_Insert()
    {
        var entities = _caMemoryConnector!.DataSource<BenchmarkEntity>("BenchmarkEntity");
        entities.PutMany(_data);
    }

    [BenchmarkCategory("Insert")]
    [Benchmark]
    public void LiteDb_Insert()
    {
        _liteColl.InsertBulk(_data);
    }

    [BenchmarkCategory("Insert")]
    [Benchmark]
    public void LiteDbMemory_Insert()
    {
        _liteCollMemory.InsertBulk(_data);
    }

    [BenchmarkCategory("Insert")]
    [Benchmark]
    public void Realm_Insert()
    {
        _realm.Write(() =>
        {
            _realm.Add(_data.Select(e => (BenchmarkRealmEntity)e), update: false);
        });
    }

    [BenchmarkCategory("Upsert")]
    [Benchmark]
    public void ReindexerNet_Upsert()
    {
        _rxClient!.Upsert("Entities", _data);
    }

    [BenchmarkCategory("Upsert")]
    [Benchmark]
    public void ReindexerNetDense_Upsert()
    {
        _rxClientDense!.Upsert("Entities", _data);
    }

    [BenchmarkCategory("Upsert")]
    [Benchmark]
    public void Cachalot_Upsert()
    {
        var entities = _caConnector!.DataSource<BenchmarkEntity>("BenchmarkEntity");
        entities.PutMany(_data);
    }

    [BenchmarkCategory("Upsert")]
    [Benchmark]
    public void CachalotCompressed_Upsert()
    {
        var entities = _caConnectorCompressed!.DataSource<BenchmarkEntity>("BenchmarkEntity");
        entities.PutMany(_data);
    }

    [BenchmarkCategory("Upsert")]
    [Benchmark]
    public void CachalotOnlyMemory_Upsert()
    {
        var entities = _caMemoryConnector!.DataSource<BenchmarkEntity>("BenchmarkEntity");
        entities.PutMany(_data);
    }

    [BenchmarkCategory("Upsert")]
    [Benchmark]
    public void LiteDb_Upsert()
    {
        _liteColl.Upsert(_data);
    }

    [BenchmarkCategory("Upsert")]
    [Benchmark]
    public void LiteDbMemory_Upsert()
    {
        _liteCollMemory.Upsert(_data);
    }

    [BenchmarkCategory("Upsert")]
    [Benchmark]
    public void Realm_Upsert()
    {
        _realm.Write(() =>
        {
            _realm.Add(_data.Select(e => (BenchmarkRealmEntity)e), update: true);
        });
    }
}