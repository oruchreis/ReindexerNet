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
[CustomCategoryDiscoverer]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams, BenchmarkLogicalGroupRule.ByCategory)]
[PlainExporter]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
public class InsertBenchmark
{
    protected class CustomCategoryDiscoverer : DefaultCategoryDiscoverer
    {
        public override string[] GetCategories(MethodInfo method) =>
            method.Name.Split('_').Reverse().ToArray();
    }

    [AttributeUsage(AttributeTargets.Class)]
    protected class CustomCategoryDiscovererAttribute : Attribute, IConfigSource
    {
        public CustomCategoryDiscovererAttribute()
        {
            Config = ManualConfig.CreateEmpty()
                .WithCategoryDiscoverer(new CustomCategoryDiscoverer());
        }

        public IConfig Config { get; }
    }

    protected string _dataPath;
    protected BenchmarkEntity[] _data;

    [Params(10_000, 100_000)]
    public int N;

    protected ReindexerEmbedded? _rxClient;
    protected ReindexerEmbedded? _rxClientDense;
    protected LiteDatabase _liteDb;
    protected LiteDatabase _liteDbMemory;
    protected ILiteCollection<BenchmarkEntity> _liteColl;
    protected ILiteCollection<BenchmarkEntity> _liteCollMemory;
    protected Server.Server _caServer;
    protected Connector? _caConnector;
    //protected Connector? _caConnectorCompressed;
    protected Connector _caMemoryConnector;
    protected Realm _realm;

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
                IntArray = [ 123, 124, 456, 456, 6777, 3123, 123123, 333 ],
                StrArray = [ "", "Abc", "Def", "FFF", "GGG", "HHH", "HGFDF", "asd" ]
            };
        }
        Console.WriteLine(_dataPath);
    }

    public void Cleanup()
    {
        Directory.Delete(_dataPath, true);
    }

    [IterationSetup(Targets = new[] { nameof(ReindexerNet) })]
    public virtual void ReindexerNetSetup()
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

    [IterationCleanup(Targets = new[] { nameof(ReindexerNet) })]
    public void ReindexerNetClean()
    {
        _rxClient!.Dispose();
        Cleanup();
    }

    [IterationSetup(Targets = new[] { nameof(ReindexerNetDense) })]    
    public virtual void ReindexerNetDenseSetup()
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

    [IterationCleanup(Targets = new[] { nameof(ReindexerNetDense) })]
    public void ReindexerNetDenseClean()
    {
        _rxClientDense!.Dispose();
        Cleanup();
    }

    [IterationSetup(Targets = new[] { nameof(Cachalot) })]    
    public virtual void CachalotSetup()
    {
        Setup();
        Directory.CreateDirectory(Path.Combine(_dataPath, "Cachalot"));
        _caServer?.Stop();
        _caServer = new Server.Server(new NodeConfig { DataPath = Path.Combine(_dataPath, "Cachalot"), IsPersistent = true, ClusterName = "Cachalot" });
        _caConnector = new Connector(new ClientConfig { IsPersistent = true });
        _caConnector.DeclareCollection<BenchmarkEntity>("BenchmarkEntity");
        //_caConnector.GetCollectionSchema("BenchmarkEntity").UseCompression = false;
    }

    [IterationCleanup(Targets = new[] { nameof(Cachalot) })]
    public void CachalotClean()
    {
        _caConnector!.Dispose();
        _caServer.Stop();
        Cleanup();
    }

    //[IterationSetup(Targets = new[] { nameof(CachalotCompressed) })]    
    //public virtual void CachalotCompressedSetup()
    //{
    //    Setup();
    //    Directory.CreateDirectory(Path.Combine(_dataPath, "CachalotCompressed"));
    //    _caServer?.Stop();
    //    _caServer = new Server.Server(new NodeConfig { DataPath = Path.Combine(_dataPath, "CachalotCompressed"), IsPersistent = true, ClusterName = "CachalotCompressed" });
    //    _caConnectorCompressed = new Connector(new ClientConfig { IsPersistent = true });
    //    _caConnectorCompressed.DeclareCollection<BenchmarkEntity>("BenchmarkEntity");
    //    _caConnectorCompressed.GetCollectionSchema("BenchmarkEntity").UseCompression = true;
    //}

    //[IterationCleanup(Targets = new[] { nameof(CachalotCompressed) })]
    //public void CachalotCompressedClean()
    //{
    //    _caConnectorCompressed!.Dispose();
    //    _caServer.Stop();
    //    Cleanup();
    //}

    [IterationSetup(Targets = new[] { nameof(CachalotOnlyMemory) })]    
    public virtual void CachalotOnlyMemorySetup()
    {
        Setup();
        _caServer = new Server.Server(new NodeConfig { DataPath = Path.Combine(_dataPath, "CachalotOnlyMemory"), IsPersistent = false, ClusterName = "CachalotOnlyMemory" });
        _caMemoryConnector = new Connector(new ClientConfig { IsPersistent = false });
        _caMemoryConnector.DeclareCollection<BenchmarkEntity>("BenchmarkEntity");
    }

    [IterationCleanup(Targets = new[] { nameof(CachalotOnlyMemory) })]
    public void CachalotOnlyMemoryClean()
    {
        _caMemoryConnector!.Dispose();
        _caServer.Stop();
        Cleanup();
    }

    [IterationSetup(Targets = new[] { nameof(LiteDb) })]    
    public virtual void LiteDbSetup()
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

    [IterationCleanup(Targets = new[] { nameof(LiteDb) })]
    public void LiteDbClean()
    {
        _liteDb!.Dispose();
        Cleanup();
    }

    [IterationSetup(Targets = new[] { nameof(LiteDbMemory) })]    
    public virtual void LiteDbMemorySetup()
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

    [IterationCleanup(Targets = new[] { nameof(LiteDbMemory) })]
    public void LiteDbMemoryClean()
    {
        _liteDbMemory!.Dispose();
        Cleanup();
    }

    [IterationSetup(Targets = new[] { nameof(Realm) })]    
    public virtual void RealmSetup()
    {
        Setup();
        _realm = Realms.Realm.GetInstance(new RealmConfiguration(Path.Combine(_dataPath, "Realm")));
    }

    [IterationCleanup(Targets = new[] { nameof(Realm) })]
    public void RealmClean()
    {
        _realm.Dispose();
        Realms.Realm.DeleteRealm(new RealmConfiguration(Path.Combine(_dataPath, "Realm")));
        Cleanup();
    }

    [Benchmark]
    public virtual void ReindexerNet()
    {
        _rxClient!.Insert("Entities", _data);
    }

    [Benchmark]
    public virtual void ReindexerNetDense()
    {
        _rxClientDense!.Insert("Entities", _data);
    }

    [Benchmark]
    public virtual void Cachalot()
    {
        var entities = _caConnector!.DataSource<BenchmarkEntity>("BenchmarkEntity");
        entities.PutMany(_data);
    }

    //[Benchmark]
    //public virtual void CachalotCompressed()
    //{
    //    var entities = _caConnectorCompressed!.DataSource<BenchmarkEntity>("BenchmarkEntity");
    //    entities.PutMany(_data);
    //}

    [Benchmark]
    public virtual void CachalotOnlyMemory()
    {
        var entities = _caMemoryConnector!.DataSource<BenchmarkEntity>("BenchmarkEntity");
        entities.PutMany(_data);
    }

    [Benchmark]
    public virtual void LiteDb()
    {
        _liteColl.InsertBulk(_data);
    }

    [Benchmark]
    public virtual void LiteDbMemory()
    {
        _liteCollMemory.InsertBulk(_data);
    }

    [Benchmark]
    public virtual void Realm()
    {
        _realm.Write(() =>
        {
            _realm.Add(_data.Select(e => (BenchmarkRealmEntity)e), update: false);
        });
    }
}