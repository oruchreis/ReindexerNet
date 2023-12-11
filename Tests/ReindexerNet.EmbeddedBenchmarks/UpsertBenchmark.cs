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
public class UpsertBenchmark: InsertBenchmark
{    
    public override void ReindexerNetSetup()
    {
        base.ReindexerNetSetup();
        base.ReindexerNet();
    }    
    
    public override void ReindexerNetDenseSetup()
    {
        base.ReindexerNetDenseSetup();
        base.ReindexerNetDense();
    }

    public override void CachalotSetup()
    {
        base.CachalotSetup();
        base.Cachalot();
    }
    
    public override void CachalotCompressedSetup()
    {
        base.CachalotCompressedSetup();
        base.CachalotCompressed();
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
    public override void ReindexerNet()
    {
        _rxClient!.Upsert("Entities", _data);
    }

    [Benchmark]
    public override void ReindexerNetDense()
    {
        _rxClientDense!.Upsert("Entities", _data);
    }

    [Benchmark]
    public override void Cachalot()
    {
        var entities = _caConnector!.DataSource<BenchmarkEntity>("BenchmarkEntity");
        entities.PutMany(_data);
    }

    [Benchmark]
    public override void CachalotCompressed()
    {
        var entities = _caConnectorCompressed!.DataSource<BenchmarkEntity>("BenchmarkEntity");
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