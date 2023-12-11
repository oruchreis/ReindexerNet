using BenchmarkDotNet.Attributes;
using Realms;
using ReindexerNetBenchmark.EmbeddedBenchmarks;
using System.Collections.Concurrent;
using ReindexerNet;

namespace ReindexerNetBenchmark;

public class SelectSingleHashParallel : SelectBenchmarkBase
{


    [Benchmark]
    public ConcurrentBag<object?> ReindexerNet()
    {
        var result = new ConcurrentBag<object?>();
        Parallel.For(0, N, i =>
        {
            result.Add(RxClient.Execute<BenchmarkEntity>("Entities", q => q.WhereString("StringProperty", Condition.EQ, Data[i].StringProperty).Limit(1)).CaptureResult());
        });
        return result;
    }

    [Benchmark]
    public ConcurrentBag<object?> ReindexerNetSpanJson()
    {
        var result = new ConcurrentBag<object?>();
        Parallel.For(0, N, i =>
        {
            result.Add(RxClientSpanJson.Execute<BenchmarkEntity>("Entities", q => q.WhereString("StringProperty", Condition.EQ, Data[i].StringProperty).Limit(1)).CaptureResult());
        });
        return result;
    }

    [Benchmark]
    public ConcurrentBag<object?> ReindexerNetSql()
    {
        var result = new ConcurrentBag<object?>();
        Parallel.For(0, N, i =>
        {
            result.Add(RxClientSql.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE StringProperty = '{Data[i].StringProperty}' LIMIT 1").CaptureResult());
        });
        return result;
    }

    [Benchmark]
    public ConcurrentBag<object?> Cachalot()
    {
        var result = new ConcurrentBag<object?>();
        Parallel.For(0, N, i =>
        {
            var str = Data[i].StringProperty;
            result.Add(CaDS.FirstOrDefault(e => e.StringProperty == str).CaptureResult());
        });
        return result;
    }

    [Benchmark]
    public ConcurrentBag<object?> CachalotMemory()
    {
        var result = new ConcurrentBag<object?>();
        Parallel.For(0, N, i =>
        {
            var str = Data[i].StringProperty;
            result.Add(CaDSMemory.FirstOrDefault(e => e.StringProperty == str).CaptureResult());
        });
        return result;
    }

    [Benchmark]
    public ConcurrentBag<object?> CachalotCompressed()
    {
        var result = new ConcurrentBag<object?>();
        Parallel.For(0, N, i =>
        {
            var str = Data[i].StringProperty;
            result.Add(CaDSCompressed.FirstOrDefault(e => e.StringProperty == str).CaptureResult());
        });
        return result;
    }

    [Benchmark]
    public ConcurrentBag<object?> LiteDb()
    {
        var result = new ConcurrentBag<object?>();
        Parallel.For(0, N, i =>
        {
            var str = Data[i].StringProperty;
            result.Add(LiteColl.Query().Where(e => e.StringProperty == str).FirstOrDefault().CaptureResult());
        });
        return result;
    }

    [Benchmark]
    public ConcurrentBag<object?> LiteDbMemory()
    {
        var result = new ConcurrentBag<object?>();
        Parallel.For(0, N, i =>
        {
            var str = Data[i].StringProperty;
            result.Add(LiteCollMemory.Query().Where(e => e.StringProperty == str).FirstOrDefault().CaptureResult());
        });
        return result;
    }

    [Benchmark]
    public ConcurrentBag<object?> Realm()
    {
        var result = new ConcurrentBag<object?>();
        Parallel.For(0, N, i =>
        {
            using var realm = Realms.Realm.GetInstance(new RealmConfiguration(Path.Combine(DataPath, "Realms")));
            var str = Data[i].StringProperty;
            result.Add(realm.All<BenchmarkRealmEntity>().Where(e => e.StringProperty == str).FirstOrDefault().CaptureResult());
        });
        return result;
    }
}
