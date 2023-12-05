using BenchmarkDotNet.Attributes;
using Cachalot.Linq;
using Client.Interface;
using LiteDB;
using Newtonsoft.Json.Linq;
using Realms;
using ReindexerNet;
using ReindexerNet.Embedded;
using Server;
using System.Collections;
using System.Collections.Concurrent;
using Index = ReindexerNet.Index;
using IndexType = ReindexerNet.IndexType;

namespace ReindexerNetBenchmark.EmbeddedBenchmarks;

[SimpleJob()]
[MemoryDiagnoser()]
public class SelectBenchmark
{
    private string _dataPath;
    private BenchmarkEntity[] _data;

    [Params(1_000)]
    public int N;

    #region Setups

    private ReindexerEmbedded? _rxClient;
    private ReindexerEmbedded? _rxClientDense;
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
                StringProperty = "ÇŞĞÜÖİöçşğüı",
                CreateDate = DateTime.UtcNow,
                IntArray = i == N - 1 ? new[] { N } : new[] { 123, 124, 456, 456, 6777, 3123, 123123, 333 },
                StrArray = i == N - 1 ? new[] { N.ToString() } : new[] { "", "Abc", "Def", "FFF", "GGG", "HHH", "HGFDF", "asd" }
            };
        }
    }

    public void Cleanup()
    {
        Directory.Delete(_dataPath, true);
    }

    [GlobalSetup(Targets = new[] {
        nameof(ReindexerNet_SelectArray),
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
        nameof(ReindexerNet_SelectArray),
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

    [GlobalSetup(Targets = new[] {
        nameof(ReindexerNetDense_SelectArray),
        nameof(ReindexerNetDense_SelectMultipleHash),
        nameof(ReindexerNetDense_SelectMultiplePK),
        nameof(ReindexerNetDense_SelectRange),
        nameof(ReindexerNetDense_SelectSingleHash),
        nameof(ReindexerNetDense_SelectSingleHashParallel),
        nameof(ReindexerNetDense_SelectSinglePK)
        })]
    public void ReindexerNetDenseSetup()
    {
        Setup();
        var dbPathDense = Path.Combine(_dataPath, "ReindexerEmbeddedDense");
        if (Directory.Exists(dbPathDense))
            Directory.Delete(dbPathDense, true);
        _rxClientDense = new ReindexerEmbedded(dbPathDense);
        _rxClientDense.Connect(new ConnectionOptions { Engine = StorageEngine.LevelDb });
        _rxClientDense.OpenNamespace("Entities");
        _rxClientDense.TruncateNamespace("Entities");
        _rxClientDense.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.Id), IsPk = true, IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = true });
        _rxClientDense.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.IntProperty), IndexType = IndexType.Tree, FieldType = FieldType.Int, IsDense = true });
        _rxClientDense.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.StringProperty), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = true });
        _rxClientDense.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.CreateDate), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = true });
        _rxClientDense.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.IntArray), IndexType = IndexType.Hash, FieldType = FieldType.Int, IsDense = true, IsArray = true });
        _rxClientDense.AddIndex("Entities", new Index { Name = nameof(BenchmarkEntity.StrArray), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = true, IsArray = true });
        _rxClientDense!.Insert("Entities", _data);

    }

    [GlobalCleanup(Targets = new[] {
        nameof(ReindexerNetDense_SelectArray),
        nameof(ReindexerNetDense_SelectMultipleHash),
        nameof(ReindexerNetDense_SelectMultiplePK),
        nameof(ReindexerNetDense_SelectRange),
        nameof(ReindexerNetDense_SelectSingleHash),
        nameof(ReindexerNetDense_SelectSingleHashParallel),
        nameof(ReindexerNetDense_SelectSinglePK)
        })]
    public void ReindexerNetDenseClean()
    {
        _rxClientDense!.Dispose();
        Cleanup();
    }

    [GlobalSetup(Targets = new[] {
        nameof(Cachalot_SelectArray),
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
    }

    [GlobalCleanup(Targets = new[] {
        nameof(Cachalot_SelectArray),
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
        nameof(CachalotCompressed_SelectArray),
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
    }

    [GlobalCleanup(Targets = new[] {
        nameof(CachalotCompressed_SelectArray),
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
        nameof(CachalotMemory_SelectArray),
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
    }

    [GlobalCleanup(Targets = new[] {
        nameof(CachalotMemory_SelectArray),
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
        nameof(LiteDb_SelectArray),
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
        _liteDb = new LiteDatabase( Path.Combine(_dataPath, "LiteDB"));
        _liteColl = _liteDb.GetCollection<BenchmarkEntity>("Entities");
        _liteColl.EnsureIndex(e => e.Id, true);
        _liteColl.EnsureIndex(e => e.IntProperty);
        _liteColl.EnsureIndex(e => e.StringProperty);
        _liteColl.EnsureIndex(e => e.CreateDate);
        _liteColl.EnsureIndex(e => e.IntArray);
        _liteColl.EnsureIndex(e => e.StrArray);
        _liteColl.Upsert(_data);
    }

    [GlobalCleanup(Targets = new[] {
        nameof(LiteDb_SelectArray),
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
        nameof(LiteDbMemory_SelectArray),
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
    }

    [GlobalCleanup(Targets = new[] {
        nameof(LiteDbMemory_SelectArray),
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
        nameof(Realm_SelectArray),
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
        nameof(Realm_SelectArray),
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

    [BenchmarkCategory("SelectSinglePK")]
    [Benchmark]
    public IList<object?> ReindexerNet_SelectSinglePK()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            result.Add(_rxClient.ExecuteSql($"Select * FROM Entities WHERE Id = '{_data[i].Id}' LIMIT 1"));
        }

        return result;
    }

    [BenchmarkCategory("SelectSinglePK")]
    [Benchmark]
    public IList<object?> ReindexerNetDense_SelectSinglePK()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            result.Add(_rxClientDense.ExecuteSql($"Select * FROM Entities WHERE Id = '{_data[i].Id}' LIMIT 1"));
        }
        return result;
    }

    [BenchmarkCategory("SelectSinglePK")]
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

    [BenchmarkCategory("SelectSinglePK")]
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

    [BenchmarkCategory("SelectSinglePK")]
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

    [BenchmarkCategory("SelectSinglePK")]
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

    [BenchmarkCategory("SelectSinglePK")]
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

    [BenchmarkCategory("SelectSinglePK")]
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

    [BenchmarkCategory("SelectMultiplePK")]
    [Benchmark]
    public IList<object?> ReindexerNet_SelectMultiplePK()
    {
        var result = new List<object?>();
        var ids = string.Join("','", _data.Take(N).Select(i => i.Id.ToString()));
        result.Add(_rxClient.ExecuteSql($"Select * FROM Entities WHERE Id IN ('{ids}')"));
        return result;
    }

    [BenchmarkCategory("SelectMultiplePK")]
    [Benchmark]
    public IList<object?> ReindexerNetDense_SelectMultiplePK()
    {
        var result = new List<object?>();
        var ids = string.Join("','", _data.Take(N).Select(i => i.Id.ToString()));
        result.Add(_rxClientDense.ExecuteSql($"Select * FROM Entities WHERE Id IN ('{ids}')"));
        return result;
    }

    [BenchmarkCategory("SelectMultiplePK")]
    [Benchmark]
    public IList<object?> Cachalot_SelectMultiplePK()
    {
        var result = new List<object?>();
        var ids = _data.Take(N).Select(i => i.Id).ToList();
        result.Add(_caDS.Where(e => ids.Contains(e.Id)).ToList());
        return result;
    }

    [BenchmarkCategory("SelectMultiplePK")]
    [Benchmark]
    public IList<object?> CachalotMemory_SelectMultiplePK()
    {
        var result = new List<object?>();
        var ids = _data.Take(N).Select(i => i.Id).ToList();
        result.Add(_caDSMemory.Where(e => ids.Contains(e.Id)).ToList());
        return result;
    }

    [BenchmarkCategory("SelectMultiplePK")]
    [Benchmark]
    public IList<object?> CachalotCompressed_SelectMultiplePK()
    {
        var result = new List<object?>();
        var ids = _data.Take(N).Select(i => i.Id).ToList();
        result.Add(_caDSCompressed.Where(e => ids.Contains(e.Id)).ToList());
        return result;
    }

    [BenchmarkCategory("SelectMultiplePK")]
    [Benchmark]
    public IList<object?> LiteDb_SelectMultiplePK()
    {
        var result = new List<object?>();
        var ids = _data.Take(N).Select(i => i.Id).ToList();
        result.Add(_liteColl.Query().Where(e => ids.Contains(e.Id)).ToList());
        return result;
    }

    [BenchmarkCategory("SelectMultiplePK")]
    [Benchmark]
    public IList<object?> LiteDbMemory_SelectMultiplePK()
    {
        var result = new List<object?>();
        var ids = _data.Take(N).Select(i => i.Id).ToList();
        result.Add(_liteCollMemory.Query().Where(e => ids.Contains(e.Id)).ToList());
        return result;
    }

    [BenchmarkCategory("SelectMultiplePK")]
    [Benchmark]
    public IList<object?> Realm_SelectMultiplePK()
    {
        var result = new List<object?>();
        var filter = string.Join(" OR ", _data.Take(N).Select(i => $"(Id == uuid({i.Id}))"));
        result.Add(_realm.All<BenchmarkRealmEntity>().Filter(filter).ToList());
        return result;
    }

    [BenchmarkCategory("SelectSingleHash")]
    [Benchmark]
    public IList<object?> ReindexerNet_SelectSingleHash()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            result.Add(_rxClient.ExecuteSql($"Select * FROM Entities WHERE StringProperty = '{_data[i].StringProperty}' LIMIT 1"));
        }

        return result;
    }

    [BenchmarkCategory("SelectSingleHash")]
    [Benchmark]
    public IList<object?> ReindexerNetDense_SelectSingleHash()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            result.Add(_rxClientDense.ExecuteSql($"Select * FROM Entities WHERE StringProperty = '{_data[i].StringProperty}' LIMIT 1"));
        }
        return result;
    }

    [BenchmarkCategory("SelectSingleHash")]
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

    [BenchmarkCategory("SelectSingleHash")]
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

    [BenchmarkCategory("SelectSingleHash")]
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

    [BenchmarkCategory("SelectSingleHash")]
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

    [BenchmarkCategory("SelectSingleHash")]
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

    [BenchmarkCategory("SelectSingleHash")]
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

    [BenchmarkCategory("SelectSingleHashParallel")]
    [Benchmark]
    public ConcurrentBag<object?> ReindexerNet_SelectSingleHashParallel()
    {
        var result = new ConcurrentBag<object?>();
        Parallel.For(0, N, i =>
        {
            result.Add(_rxClient.ExecuteSql($"Select * FROM Entities WHERE StringProperty = '{_data[i].StringProperty}' LIMIT 1"));
        });
        return result;
    }

    [BenchmarkCategory("SelectSingleHashParallel")]
    [Benchmark]
    public ConcurrentBag<object?> ReindexerNetDense_SelectSingleHashParallel()
    {
        var result = new ConcurrentBag<object?>();
        Parallel.For(0, N, i =>
        {
            result.Add(_rxClientDense.ExecuteSql($"Select * FROM Entities WHERE StringProperty = '{_data[i].StringProperty}' LIMIT 1"));
        });
        return result;
    }

    [BenchmarkCategory("SelectSingleHashParallel")]
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

    [BenchmarkCategory("SelectSingleHashParallel")]
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

    [BenchmarkCategory("SelectSingleHashParallel")]
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

    [BenchmarkCategory("SelectSingleHashParallel")]
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

    [BenchmarkCategory("SelectSingleHashParallel")]
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

    [BenchmarkCategory("SelectSingleHashParallel")]
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

    [BenchmarkCategory("SelectMultipleHash")]
    [Benchmark]
    public IList<object?> ReindexerNet_SelectMultipleHash()
    {
        var result = new List<object?>();
        var props = string.Join("','", _data.Take(N).Select(i => i.StringProperty));
        result.Add(_rxClient.ExecuteSql($"Select * FROM Entities WHERE Id IN ('{props}')"));
        return result;
    }

    [BenchmarkCategory("SelectMultipleHash")]
    [Benchmark]
    public IList<object?> ReindexerNetDense_SelectMultipleHash()
    {
        var result = new List<object?>();
        var props = string.Join("','", _data.Take(N).Select(i => i.StringProperty));
        result.Add(_rxClientDense.ExecuteSql($"Select * FROM Entities WHERE Id IN ('{props}')"));
        return result;
    }

    [BenchmarkCategory("SelectMultipleHash")]
    [Benchmark]
    public IList<object?> Cachalot_SelectMultipleHash()
    {
        var result = new List<object?>();
        var props = _data.Take(N).Select(i => i.StringProperty).ToList();
        result.Add(_caDS.Where(e => props.Contains(e.StringProperty)).ToList());
        return result;
    }

    [BenchmarkCategory("SelectMultipleHash")]
    [Benchmark]
    public IList<object?> CachalotMemory_SelectMultipleHash()
    {
        var result = new List<object?>();
        var props = _data.Take(N).Select(i => i.StringProperty).ToList();
        result.Add(_caDSMemory.Where(e => props.Contains(e.StringProperty)).ToList());
        return result;
    }

    [BenchmarkCategory("SelectMultipleHash")]
    [Benchmark]
    public IList<object?> CachalotCompressed_SelectMultipleHash()
    {
        var result = new List<object?>();
        var props = _data.Take(N).Select(i => i.StringProperty).ToList();
        result.Add(_caDSCompressed.Where(e => props.Contains(e.StringProperty)).ToList());
        return result;
    }

    [BenchmarkCategory("SelectMultipleHash")]
    [Benchmark]
    public IList<object?> LiteDb_SelectMultipleHash()
    {
        var result = new List<object?>();
        var props = _data.Take(N).Select(i => i.StringProperty).ToList();
        result.Add(_liteColl.Query().Where(e => props.Contains(e.StringProperty)).ToList());
        return result;
    }

    [BenchmarkCategory("SelectMultipleHash")]
    [Benchmark]
    public IList<object?> LiteDbMemory_SelectMultipleHash()
    {
        var result = new List<object?>();
        var props = _data.Take(N).Select(i => i.StringProperty).ToList();
        result.Add(_liteCollMemory.Query().Where(e => props.Contains(e.StringProperty)).ToList());
        return result;
    }

    [BenchmarkCategory("SelectMultipleHash")]
    [Benchmark]
    public IList<object?> Realm_SelectMultipleHash()
    {
        var result = new List<object?>();
        var filter = string.Join(" OR ", _data.Take(N).Select(i => $"(StringProperty == '{i.StringProperty}')"));
        result.Add(_realm.All<BenchmarkRealmEntity>().Filter(filter).ToList());
        return result;
    }

    [BenchmarkCategory("SelectRange")]
    [Benchmark]
    public IList<object?> ReindexerNet_SelectRange()
    {
        var result = new List<object?>();
        var entity = _data[N / 2];
        result.Add(_rxClient.ExecuteSql($"Select * FROM Entities WHERE IntProperty < {entity.IntProperty}"));
        result.Add(_rxClient.ExecuteSql($"Select * FROM Entities WHERE IntProperty >= {entity.IntProperty}"));
        return result;
    }

    [BenchmarkCategory("SelectRange")]
    [Benchmark]
    public IList<object?> ReindexerNetDense_SelectRange()
    {
        var result = new List<object?>();
        var entity = _data[N / 2];
        result.Add(_rxClientDense.ExecuteSql($"Select * FROM Entities WHERE IntProperty < {entity.IntProperty}"));
        result.Add(_rxClientDense.ExecuteSql($"Select * FROM Entities WHERE IntProperty >= {entity.IntProperty}"));
        return result;
    }

    [BenchmarkCategory("SelectRange")]
    [Benchmark]
    public IList<object?> Cachalot_SelectRange()
    {
        var result = new List<object?>();
        var entity = _data[N / 2];
        result.Add(_caDS.Where(e => e.IntProperty < entity.IntProperty).ToList());
        result.Add(_caDS.Where(e => e.IntProperty >= entity.IntProperty).ToList());
        return result;
    }

    [BenchmarkCategory("SelectRange")]
    [Benchmark]
    public IList<object?> CachalotMemory_SelectRange()
    {
        var result = new List<object?>();
        var entity = _data[N / 2];
        result.Add(_caDSMemory.Where(e => e.IntProperty < entity.IntProperty).ToList());
        result.Add(_caDSMemory.Where(e => e.IntProperty >= entity.IntProperty).ToList());
        return result;
    }

    [BenchmarkCategory("SelectRange")]
    [Benchmark]
    public IList<object?> CachalotCompressed_SelectRange()
    {
        var result = new List<object?>();
        var entity = _data[N / 2];
        result.Add(_caDSCompressed.Where(e => e.IntProperty < entity.IntProperty).ToList());
        result.Add(_caDSCompressed.Where(e => e.IntProperty >= entity.IntProperty).ToList());
        return result;
    }

    [BenchmarkCategory("SelectRange")]
    [Benchmark]
    public IList<object?> LiteDb_SelectRange()
    {
        var result = new List<object?>();
        var entity = _data[N / 2];
        result.Add(_liteColl.Query().Where(e => e.IntProperty < entity.IntProperty).ToList());
        result.Add(_liteColl.Query().Where(e => e.IntProperty >= entity.IntProperty).ToList());
        return result;
    }

    [BenchmarkCategory("SelectRange")]
    [Benchmark]
    public IList<object?> LiteDbMemory_SelectRange()
    {
        var result = new List<object?>();
        var entity = _data[N / 2];
        result.Add(_liteCollMemory.Query().Where(e => e.IntProperty < entity.IntProperty).ToList());
        result.Add(_liteCollMemory.Query().Where(e => e.IntProperty >= entity.IntProperty).ToList());
        return result;
    }

    [BenchmarkCategory("SelectRange")]
    [Benchmark]
    public IList<object?> Realm_SelectRange()
    {
        var result = new List<object?>();
        var entity = _data[N / 2];
        result.Add(_realm.All<BenchmarkRealmEntity>().Where(e => e.IntProperty < entity.IntProperty).ToList());
        result.Add(_realm.All<BenchmarkRealmEntity>().Where(e => e.IntProperty >= entity.IntProperty).ToList());
        return result;
    }

    [BenchmarkCategory("SelectArray")]
    [Benchmark]
    public IList<object?> ReindexerNet_SelectArray()
    {
        var result = new List<object?>
        {
            _rxClient.ExecuteSql($"Select * FROM Entities WHERE IntArray IN ({N})"),
            _rxClient.ExecuteSql($"Select * FROM Entities WHERE StrArray IN ('{N}')")
        };
        return result;
    }

    [BenchmarkCategory("SelectArray")]
    [Benchmark]
    public IList<object?> ReindexerNetDense_SelectArray()
    {
        var result = new List<object?>
        {
            _rxClientDense.ExecuteSql($"Select * FROM Entities WHERE IntArray IN ({N})"),
            _rxClientDense.ExecuteSql($"Select * FROM Entities WHERE StrArray IN ('{N}')")
        };
        return result;
    }

    [BenchmarkCategory("SelectArray")]
    [Benchmark]
    public IList<object?> Cachalot_SelectArray()
    {
        var result = new List<object?>();
        var nstr = N.ToString();
        result.Add(_caDS.Where(e => e.IntArray.Contains(N)).ToList());
        result.Add(_caDS.Where(e => e.StrArray.Contains(nstr)).ToList());
        return result;
    }

    [BenchmarkCategory("SelectArray")]
    [Benchmark]
    public IList<object?> CachalotMemory_SelectArray()
    {
        var result = new List<object?>();
        var nstr = N.ToString();
        result.Add(_caDSMemory.Where(e => e.IntArray.Contains(N)).ToList());
        result.Add(_caDSMemory.Where(e => e.StrArray.Contains(nstr)).ToList());
        return result;
    }

    [BenchmarkCategory("SelectArray")]
    [Benchmark]
    public IList<object?> CachalotCompressed_SelectArray()
    {
        var result = new List<object?>();
        var nstr = N.ToString();
        result.Add(_caDSCompressed.Where(e => e.IntArray.Contains(N)).ToList());
        result.Add(_caDSCompressed.Where(e => e.StrArray.Contains(nstr)).ToList());
        return result;
    }

    [BenchmarkCategory("SelectArray")]
    [Benchmark]
    public IList<object?> LiteDb_SelectArray()
    {
        var result = new List<object?>();
        var nstr = N.ToString();
        result.Add(_liteColl.Query().Where(e => e.IntArray.Contains(N)).ToList());
        result.Add(_liteColl.Query().Where(e => e.StrArray.Contains(nstr)).ToList());
        return result;
    }

    [BenchmarkCategory("SelectArray")]
    [Benchmark]
    public IList<object?> LiteDbMemory_SelectArray()
    {
        var result = new List<object?>();
        var nstr = N.ToString();
        result.Add(_liteCollMemory.Query().Where(e => e.IntArray.Contains(N)).ToList());
        result.Add(_liteCollMemory.Query().Where(e => e.StrArray.Contains(nstr)).ToList());
        return result;
    }

    [BenchmarkCategory("SelectArray")]
    [Benchmark]
    public IList<object?> Realm_SelectArray()
    {
        var result = new List<object?>
        {
            _realm.All<BenchmarkRealmEntity>().Filter("ANY IntArray.@values == $0", N).ToList(),
            _realm.All<BenchmarkRealmEntity>().Filter("ANY StrArray.@values == '$0'", N.ToString()).ToList()
        };
        return result;
    }
}