using BenchmarkDotNet.Attributes;
using ReindexerNetBenchmark.EmbeddedBenchmarks;
using ReindexerNet;

namespace ReindexerNetBenchmark;

public class SelectSinglePK: SelectBenchmarkBase
{
    [Benchmark]
    public IList<object?> ReindexerNet()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            result.Add(RxClient.Execute<BenchmarkEntity>("Entities", q => q.Limit(1).WhereGuid("Id", Condition.EQ, Data[i].Id)).CaptureResult());
        }

        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSpanJson()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            result.Add(RxClientSpanJson.Execute<BenchmarkEntity>("Entities", q => q.Limit(1).WhereGuid("Id", Condition.EQ, Data[i].Id)).CaptureResult());
        }
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSql()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            result.Add(RxClientSql.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE Id = '{Data[i].Id}' LIMIT 1").CaptureResult());
        }

        return result;
    }

    [Benchmark]
    public IList<object?> Cachalot()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            var id = Data[i].Id;
            result.Add(CaDS[id].CaptureResult());
        }

        return result;
    }

    [Benchmark]
    public IList<object?> CachalotMemory()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            var id = Data[i].Id;
            result.Add(CaDSMemory[id].CaptureResult());
        }

        return result;
    }

    //[Benchmark]
    //public IList<object?> CachalotCompressed()
    //{
    //    var result = new List<object?>();
    //    for (int i = 0; i < N; i++)
    //    {
    //        var id = Data[i].Id;
    //        result.Add(CaDSCompressed[id].CaptureResult());
    //    }
    //    return result;
    //}

    [Benchmark]
    public IList<object?> LiteDb()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            var id = Data[i].Id;
            result.Add(LiteColl.FindById(id).CaptureResult());
        }
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDbMemory()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            var id = Data[i].Id;
            result.Add(LiteCollMemory.FindById(id).CaptureResult());
        }
        return result;
    }

    [Benchmark]
    public IList<object?> Realm()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {            
            result.Add(RealmCli.Find<BenchmarkRealmEntity>(Data[i].Id)!.CaptureResult());
        }
        return result;
    }
}
