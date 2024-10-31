using BenchmarkDotNet.Attributes;
using Realms;
using ReindexerNetBenchmark.EmbeddedBenchmarks;
using ReindexerNet;

namespace ReindexerNetBenchmark;

public class SelectMultipleHash : SelectBenchmarkBase
{
    [Benchmark]
    public IList<object?> ReindexerNet()
    {
        var result = new List<object?>
        {
            RxClient.Execute<BenchmarkEntity>("Entities", q => q.WhereString("StringProperty", Condition.SET, SearchStringProperties)).CaptureResult()
        };
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSpanJson()
    {
        var result = new List<object?>
        {
            RxClientSpanJson.Execute<BenchmarkEntity>("Entities", q => q.WhereString("StringProperty", Condition.SET, SearchStringProperties)).CaptureResult()
        };
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSql()
    {
        var result = new List<object?>
        {
            RxClientSql.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE StringProperty IN ({SearchStringPropertiesJoined})").CaptureResult()
        };
        return result;
    }

    [Benchmark]
    public IList<object?> Cachalot()
    {
        var result = new List<object?>();
        result.Add(CaDS.Where(e => SearchStringProperties.Contains(e.StringProperty)).AsEnumerable().CaptureResult());
        return result;
    }

    [Benchmark]
    public IList<object?> CachalotMemory()
    {
        var result = new List<object?>();
        result.Add(CaDSMemory.Where(e => SearchStringProperties.Contains(e.StringProperty)).AsEnumerable().CaptureResult());
        return result;
    }

    //[Benchmark]
    //public IList<object?> CachalotCompressed()
    //{
    //    var result = new List<object?>();
    //    result.Add(CaDSCompressed.Where(e => SearchStringProperties.Contains(e.StringProperty)).AsEnumerable().CaptureResult());
    //    return result;
    //}

    [Benchmark]
    public IList<object?> LiteDb()
    {
        var result = new List<object?>();
        result.Add(LiteColl.Query().Where(e => SearchStringProperties.Contains(e.StringProperty)).ToEnumerable().CaptureResult());
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDbMemory()
    {
        var result = new List<object?>();
        result.Add(LiteCollMemory.Query().Where(e => SearchStringProperties.Contains(e.StringProperty)).ToEnumerable().CaptureResult());
        return result;
    }

    [Benchmark]
    public IList<object?> Realm()
    {
        var result = new List<object?>();
        var filter = string.Join(" OR ", SearchStringProperties.Select(i => $"(StringProperty == '{i}')"));
        result.Add(RealmCli.All<BenchmarkRealmEntity>().Filter(filter).CaptureResult());
        return result;
    }
}
