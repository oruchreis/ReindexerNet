using BenchmarkDotNet.Attributes;
using ReindexerNetBenchmark.EmbeddedBenchmarks;
using ReindexerNet;

namespace ReindexerNetBenchmark;

public class SelectRange : SelectBenchmarkBase
{
    [Benchmark]
    public IList<object?> ReindexerNetV5()
    {
        var result = new List<object?>();
        var entity = Data[N / 2];
        result.Add(RxClient!.Execute<BenchmarkEntity>("Entities", q => q.WhereInt32("IntProperty", Condition.LT, entity.IntProperty ?? 0)).CaptureResult());
        result.Add(RxClient!.Execute<BenchmarkEntity>("Entities", q => q.WhereInt32("IntProperty", Condition.GE, entity.IntProperty ?? 0)).CaptureResult());
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSpanJsonV5()
    {
        var result = new List<object?>();
        var entity = Data[N / 2];
        result.Add(RxClientSpanJson!.Execute<BenchmarkEntity>("Entities", q => q.WhereInt32("IntProperty", Condition.LT, entity.IntProperty ?? 0)).CaptureResult());
        result.Add(RxClientSpanJson!.Execute<BenchmarkEntity>("Entities", q => q.WhereInt32("IntProperty", Condition.GE, entity.IntProperty ?? 0)).CaptureResult());
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSqlV5()
    {
        var result = new List<object?>();
        var entity = Data[N / 2];
        result.Add(RxClientSql!.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE IntProperty < {entity.IntProperty}").CaptureResult());
        result.Add(RxClientSql!.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE IntProperty >= {entity.IntProperty}").CaptureResult());
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetV3()
    {
        var result = new List<object?>();
        var entity = Data[N / 2];
        result.Add(RxClientV3!.Execute("Entities", q => q.WhereInt32("IntProperty", Condition.LT, entity.IntProperty ?? 0)).CaptureResult());
        result.Add(RxClientV3!.Execute("Entities", q => q.WhereInt32("IntProperty", Condition.GE, entity.IntProperty ?? 0)).CaptureResult());
        return result;
    }

    [Benchmark]
    public IList<object?> Cachalot()
    {
        var result = new List<object?>();
        var entity = Data[N / 2];
        result.Add(CaDS.Where(e => e.IntProperty < entity.IntProperty).AsEnumerable().CaptureResult());
        result.Add(CaDS.Where(e => e.IntProperty >= entity.IntProperty).AsEnumerable().CaptureResult());
        return result;
    }

    [Benchmark]
    public IList<object?> CachalotMemory()
    {
        var result = new List<object?>();
        var entity = Data[N / 2];
        result.Add(CaDSMemory.Where(e => e.IntProperty < entity.IntProperty).AsEnumerable().CaptureResult());
        result.Add(CaDSMemory.Where(e => e.IntProperty >= entity.IntProperty).AsEnumerable().CaptureResult());
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDb()
    {
        var result = new List<object?>();
        var entity = Data[N / 2];
        result.Add(LiteColl.Query().Where(e => e.IntProperty < entity.IntProperty).ToEnumerable().CaptureResult());
        result.Add(LiteColl.Query().Where(e => e.IntProperty >= entity.IntProperty).ToEnumerable().CaptureResult());
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDbMemory()
    {
        var result = new List<object?>();
        var entity = Data[N / 2];
        result.Add(LiteCollMemory.Query().Where(e => e.IntProperty < entity.IntProperty).ToEnumerable().CaptureResult());
        result.Add(LiteCollMemory.Query().Where(e => e.IntProperty >= entity.IntProperty).ToEnumerable().CaptureResult());
        return result;
    }

    [Benchmark]
    public IList<object?> Realm()
    {
        var result = new List<object?>();
        var entity = Data[N / 2];
        result.Add(RealmCli.All<BenchmarkRealmEntity>().Where(e => e.IntProperty < entity.IntProperty).CaptureResult());
        result.Add(RealmCli.All<BenchmarkRealmEntity>().Where(e => e.IntProperty >= entity.IntProperty).CaptureResult());

        return result;
    }
}
