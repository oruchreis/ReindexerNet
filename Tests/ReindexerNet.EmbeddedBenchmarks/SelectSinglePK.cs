using BenchmarkDotNet.Attributes;
using ReindexerNetBenchmark.EmbeddedBenchmarks;
using ReindexerNet;

namespace ReindexerNetBenchmark;

public class SelectSinglePK : SelectBenchmarkBase
{
    [Benchmark]
    public IList<object?> ReindexerNetV5()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            result.Add(RxClient!.Execute<BenchmarkEntity>("Entities", q => q.Limit(1).WhereGuid("Id", Condition.EQ, Data[i].Id)).CaptureResult());
        }

        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSpanJsonV5()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            result.Add(RxClientSpanJson!.Execute<BenchmarkEntity>("Entities", q => q.Limit(1).WhereGuid("Id", Condition.EQ, Data[i].Id)).CaptureResult());
        }
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSqlV5()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            result.Add(RxClientSql!.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE Id = '{Data[i].Id}' LIMIT 1").CaptureResult());
        }

        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetV3()
    {
        var result = new List<object?>();
        for (int i = 0; i < N; i++)
        {
            result.Add(RxClientV3!.Execute("Entities", q => q.Limit(1).WhereGuid("Id", Condition.EQ, Data[i].Id)).CaptureResult());
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
