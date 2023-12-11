using BenchmarkDotNet.Attributes;
using ReindexerNetBenchmark.EmbeddedBenchmarks;
using ReindexerNet;

namespace ReindexerNetBenchmark;

public class SelectSingleHash : SelectBenchmarkBase
{
    [Benchmark]
    public IList<object?> ReindexerNet()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            result.Add(RxClient.Execute<BenchmarkEntity>("Entities", q => q.WhereString("StringProperty", Condition.EQ, Data[i].StringProperty).Limit(1)).CaptureResult());
        }

        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSpanJson()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            result.Add(RxClientSpanJson.Execute<BenchmarkEntity>("Entities", q => q.WhereString("StringProperty", Condition.EQ, Data[i].StringProperty).Limit(1)).CaptureResult());
        }
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSql()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            result.Add(RxClientSql.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE StringProperty = '{Data[i].StringProperty}' LIMIT 1").CaptureResult());
        }

        return result;
    }

    [Benchmark]
    public IList<object?> Cachalot()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            var str = Data[i].StringProperty;
            result.Add(CaDS.FirstOrDefault(e => e.StringProperty == str).CaptureResult());
        }
        return result;
    }

    [Benchmark]
    public IList<object?> CachalotMemory()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            var str = Data[i].StringProperty;
            result.Add(CaDSMemory.FirstOrDefault(e => e.StringProperty == str).CaptureResult());
        }
        return result;
    }

    [Benchmark]
    public IList<object?> CachalotCompressed()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            var str = Data[i].StringProperty;
            result.Add(CaDSCompressed.FirstOrDefault(e => e.StringProperty == str).CaptureResult());
        }
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDb()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            var str = Data[i].StringProperty;
            result.Add(LiteColl.Query().Where(e => e.StringProperty == str).FirstOrDefault().CaptureResult());
        }
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDbMemory()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            var str = Data[i].StringProperty;
            result.Add(LiteCollMemory.Query().Where(e => e.StringProperty == str).FirstOrDefault().CaptureResult());
        }
        return result;
    }

    [Benchmark]
    public IList<object?> Realm()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            var str = Data[i].StringProperty;
            result.Add(RealmCli.All<BenchmarkRealmEntity>().Where(e => e.StringProperty == str).FirstOrDefault().CaptureResult());
        }
        return result;
    }
}
