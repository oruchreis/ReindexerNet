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

// [Config] and job are inherited from InsertBenchmark (ConfigAttribute has Inherited = true).
// Do NOT redeclare [SimpleJob] here — a second job declaration would cause BenchmarkDotNet
// to run each benchmark twice (once per job).
[MemoryDiagnoser()]
[CustomCategoryDiscoverer]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams, BenchmarkLogicalGroupRule.ByCategory)]
[PlainExporter]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
public class UpsertBenchmark : InsertBenchmark
{
    public override void ReindexerNetV5Setup()
    {
        base.ReindexerNetV5Setup();
        base.ReindexerNetV5();
    }

    public override void ReindexerNetDenseV5Setup()
    {
        base.ReindexerNetDenseV5Setup();
        base.ReindexerNetDenseV5();
    }

    public override void ReindexerNetV3Setup()
    {
        base.ReindexerNetV3Setup();
        base.ReindexerNetV3();
    }

    public override void ReindexerNetDenseV3Setup()
    {
        base.ReindexerNetDenseV3Setup();
        base.ReindexerNetDenseV3();
    }

    public override void CachalotSetup()
    {
        base.CachalotSetup();
        base.Cachalot();
    }

    public override void CachalotOnlyMemorySetup()
    {
        base.CachalotOnlyMemorySetup();
        base.CachalotOnlyMemory();
    }

    public override void LiteDbSetup()
    {
        base.LiteDbSetup();
        base.LiteDb();
    }

    public override void LiteDbMemorySetup()
    {
        base.LiteDbMemorySetup();
        base.LiteDbMemory();
    }

    public override void RealmSetup()
    {
        base.RealmSetup();
        base.Realm();
    }

    [Benchmark]
    public override void ReindexerNetV5()
    {
        _rxClient!.Upsert("Entities", _data);
    }

    [Benchmark]
    public override void ReindexerNetDenseV5()
    {
        _rxClientDense!.Upsert("Entities", _data);
    }

    [Benchmark]
    public override void ReindexerNetV3()
    {
        _rxClientV3!.Upsert("Entities", _data);
    }

    [Benchmark]
    public override void ReindexerNetDenseV3()
    {
        _rxClientDenseV3!.Upsert("Entities", _data);
    }

    [Benchmark]
    public override void Cachalot()
    {
        var entities = _caConnector!.DataSource<BenchmarkEntity>("BenchmarkEntity");
        entities.PutMany(_data);
    }

    [Benchmark]
    public override void CachalotOnlyMemory()
    {
        var entities = _caMemoryConnector!.DataSource<BenchmarkEntity>("BenchmarkEntity");
        entities.PutMany(_data);
    }

    [Benchmark]
    public override void LiteDb()
    {
        _liteColl.Upsert(_data);
    }

    [Benchmark]
    public override void LiteDbMemory()
    {
        _liteCollMemory.Upsert(_data);
    }

    [Benchmark]
    public override void Realm()
    {
        _realm.Write(() =>
        {
            _realm.Add(_data.Select(e => (BenchmarkRealmEntity)e), update: true);
        });
    }
}
