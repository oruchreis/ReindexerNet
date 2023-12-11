using BenchmarkDotNet.Attributes;
using Realms;
using ReindexerNetBenchmark.EmbeddedBenchmarks;
using ReindexerNet;

namespace ReindexerNetBenchmark;

public class SelectMultiplePK : SelectBenchmarkBase
{
    [Benchmark]
    public IList<object?> ReindexerNet()
    {
        var result = new List<object?>();
        result.Add(RxClient.Execute<BenchmarkEntity>("Entities", q => q.WhereGuid("Id", Condition.SET, SearchIds)).CaptureResult());
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSpanJson()
    {
        var result = new List<object?>();
        result.Add(RxClientSpanJson.Execute<BenchmarkEntity>("Entities", q => q.WhereGuid("Id", Condition.SET, SearchIds)).CaptureResult());
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSql()
    {
        var result = new List<object?>();
        result.Add(RxClientSql.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE Id IN ({SearchIdsJoined})").CaptureResult());
        return result;
    }

    [Benchmark]
    public IList<object?> Cachalot()
    {
        var result = new List<object?>();
        result.Add(CaDS.Where(e => SearchIds.Contains(e.Id)).AsEnumerable().CaptureResult());
        return result;
    }

    [Benchmark]
    public IList<object?> CachalotMemory()
    {
        var result = new List<object?>();
        result.Add(CaDSMemory.Where(e => SearchIds.Contains(e.Id)).AsEnumerable().CaptureResult());
        return result;
    }

    [Benchmark]
    public IList<object?> CachalotCompressed()
    {
        var result = new List<object?>();
        result.Add(CaDSCompressed.Where(e => SearchIds.Contains(e.Id)).AsEnumerable().CaptureResult());
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDb()
    {
        var result = new List<object?>();
        result.Add(LiteColl.Query().Where(e => SearchIds.Contains(e.Id)).ToEnumerable().CaptureResult());
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDbMemory()
    {
        var result = new List<object?>();
        result.Add(LiteCollMemory.Query().Where(e => SearchIds.Contains(e.Id)).ToEnumerable().CaptureResult());
        return result;
    }

    [Benchmark]
    public IList<object?> Realm()
    {
        var result = new List<object?>();
        var filter = string.Join(" OR ", Data.Take(N).Select(i => $"(Id == uuid({i.Id}))"));
        result.Add(RealmCli.All<BenchmarkRealmEntity>().Filter(filter).CaptureResult());
        return result;
    }
}
