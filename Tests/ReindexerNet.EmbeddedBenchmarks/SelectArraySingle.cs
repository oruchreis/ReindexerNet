using BenchmarkDotNet.Attributes;
using ReindexerNetBenchmark.EmbeddedBenchmarks;
using ReindexerNet;
using Realms;

namespace ReindexerNetBenchmark;

public class SelectArraySingle: SelectBenchmarkBase
{
    [Benchmark]
    public IList<object?> ReindexerNet()
    {
        var result = new List<object?>
        {
            RxClient.Execute<BenchmarkEntity>("Entities", q => q.WhereInt32("IntArray", Condition.SET, N)).CaptureResult(),
            RxClient.Execute<BenchmarkEntity>("Entities", q => q.WhereString("StrArray", Condition.SET, N.ToString())).CaptureResult(),
        };
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSpanJson()
    {
        var result = new List<object?>
        {
            RxClientSpanJson.Execute<BenchmarkEntity>("Entities", q => q.WhereInt32("IntArray", Condition.SET, N)).CaptureResult(),
            RxClientSpanJson.Execute<BenchmarkEntity>("Entities", q => q.WhereString("StrArray", Condition.SET, N.ToString())).CaptureResult(),
        };
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSql()
    {
        var result = new List<object?>
        {
            RxClientSql.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE IntArray IN ({N})").CaptureResult(),
            RxClientSql.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE StrArray IN ('{N}')").CaptureResult(),
        };
        return result;
    }

    [Benchmark]
    public IList<object?> Cachalot()
    {
        var nstr = N.ToString();
        var result = new List<object?>
        {
            CaDS.Where(e => e.IntArray.Contains(N)).AsEnumerable().CaptureResult(),
            CaDS.Where(e => e.StrArray.Contains(nstr)).AsEnumerable().CaptureResult(),
        };
        return result;
    }

    [Benchmark]
    public IList<object?> CachalotMemory()
    {
        var nstr = N.ToString();
        var result = new List<object?>
        {
            CaDSMemory.Where(e => e.IntArray.Contains(N)).AsEnumerable().CaptureResult(),
            CaDSMemory.Where(e => e.StrArray.Contains(nstr)).AsEnumerable().CaptureResult(),
        };
        return result;
    }

    //[Benchmark]
    //public IList<object?> CachalotCompressed()
    //{
    //    var nstr = N.ToString();
    //    var result = new List<object?>
    //    {
    //        CaDSCompressed.Where(e => e.IntArray.Contains(N)).AsEnumerable().CaptureResult(),
    //        CaDSCompressed.Where(e => e.StrArray.Contains(nstr)).AsEnumerable().CaptureResult(),
    //    };
    //    return result;
    //}

    [Benchmark]
    public IList<object?> LiteDb()
    {
        var nstr = N.ToString();
        var result = new List<object?>
        {
            LiteColl.Query().Where(e => e.IntArray.Contains(N)).CaptureResult(),
            LiteColl.Query().Where(e => e.StrArray.Contains(nstr)).CaptureResult(),
        };
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDbMemory()
    {
        var nstr = N.ToString();
        var result = new List<object?>
        {
            LiteCollMemory.Query().Where(e => e.IntArray.Contains(N)).CaptureResult(),
            LiteCollMemory.Query().Where(e => e.StrArray.Contains(nstr)).CaptureResult(),
        };
        return result;
    }

    [Benchmark]
    public IList<object?> Realm()
    {
        var nstr = N.ToString();
        var result = new List<object?>
        {
            RealmCli.All<BenchmarkRealmEntity>().Filter("ANY IntArray == $0", N).CaptureResult(),
            RealmCli.All<BenchmarkRealmEntity>().Filter("ANY StrArray == $0", N.ToString()).CaptureResult()
        };
       
        return result;
    }
}
