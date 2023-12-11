using BenchmarkDotNet.Attributes;
using ReindexerNetBenchmark.EmbeddedBenchmarks;
using ReindexerNet;
using Realms;

namespace ReindexerNetBenchmark;

public class SelectArrayMultiple : SelectBenchmarkBase
{
    [Benchmark]
    public IList<object?> ReindexerNet()
    {
        var result = new List<object?>
        {
            RxClient.Execute<BenchmarkEntity>("Entities", q => q.WhereInt32("IntArray", Condition.SET, SearchItemsInt)).CaptureResult(),
            RxClient.Execute<BenchmarkEntity>("Entities", q => q.WhereString("StrArray", Condition.SET, SearchItemsStr)).CaptureResult(),
            RxClient.Execute<BenchmarkEntity>("Entities", q => q.WhereInt32("IntArray", Condition.ALLSET, SearchItemsInt)).CaptureResult(),
            RxClient.Execute<BenchmarkEntity>("Entities", q => q.WhereString("StrArray", Condition.ALLSET, SearchItemsStr)).CaptureResult()
        };
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSpanJson()
    {
        var result = new List<object?>
        {
            RxClientSpanJson.Execute<BenchmarkEntity>("Entities", q => q.WhereInt32("IntArray", Condition.SET, SearchItemsInt)).CaptureResult(),
            RxClientSpanJson.Execute<BenchmarkEntity>("Entities", q => q.WhereString("StrArray", Condition.SET, SearchItemsStr)).CaptureResult(),
            RxClientSpanJson.Execute<BenchmarkEntity>("Entities", q => q.WhereInt32("IntArray", Condition.ALLSET, SearchItemsInt)).CaptureResult(),
            RxClientSpanJson.Execute<BenchmarkEntity>("Entities", q => q.WhereString("StrArray", Condition.ALLSET, SearchItemsStr)).CaptureResult()
        };
        return result;
    }

    [Benchmark]
    public IList<object?> ReindexerNetSql()
    {
        var result = new List<object?>
        {
            RxClientSql.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE IntArray IN ({SearchItemsIntJoined})").CaptureResult(),
            RxClientSql.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE StrArray IN ({SearchItemsStrJoined})").CaptureResult(),
            RxClientSql.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE IntArray ALLSET ({SearchItemsIntJoined})").CaptureResult(),
            RxClientSql.ExecuteSql<BenchmarkEntity>($"SELECT * FROM Entities WHERE StrArray ALLSET ({SearchItemsStrJoined})").CaptureResult()
        };
        return result;
    }

    [Benchmark]
    public IList<object?> Cachalot()
    {
        var result = new List<object?>
        {
            CaDS.Where(IntAnyQuery).AsEnumerable().CaptureResult(),
            CaDS.Where(StrAnyQuery).AsEnumerable().CaptureResult(),
            CaDS.Where(IntAllQuery).AsEnumerable().CaptureResult(),
            CaDS.Where(StrAllQuery).AsEnumerable().CaptureResult()
        };
        return result;
    }

    [Benchmark]
    public IList<object?> CachalotMemory()
    {
        var result = new List<object?>
        {
            CaDSMemory.Where(IntAnyQuery).AsEnumerable().CaptureResult(),
            CaDSMemory.Where(StrAnyQuery).AsEnumerable().CaptureResult(),
            CaDSMemory.Where(IntAllQuery).AsEnumerable().CaptureResult(),
            CaDSMemory.Where(StrAllQuery).AsEnumerable().CaptureResult()
        };
        return result;
    }

    [Benchmark]
    public IList<object?> CachalotCompressed()
    {
        var result = new List<object?>
        {
            CaDSCompressed.Where(IntAnyQuery).AsEnumerable().CaptureResult(),
            CaDSCompressed.Where(StrAnyQuery).AsEnumerable().CaptureResult(),
            CaDSCompressed.Where(IntAllQuery).AsEnumerable().CaptureResult(),
            CaDSCompressed.Where(StrAllQuery).AsEnumerable().CaptureResult()
        };
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDb()
    {
        var result = new List<object?>
        {
            LiteColl.Query().Where(IntAnyQuery).ToEnumerable().CaptureResult(),
            LiteColl.Query().Where(StrAnyQuery).ToEnumerable().CaptureResult(),
            LiteColl.Query().Where(IntAllQuery).ToEnumerable().CaptureResult(),
            LiteColl.Query().Where(StrAllQuery).ToEnumerable().CaptureResult()
        };
        return result;
    }

    [Benchmark]
    public IList<object?> LiteDbMemory()
    {
        var result = new List<object?>
        {
            LiteCollMemory.Query().Where(IntAnyQuery).ToEnumerable().CaptureResult(),
            LiteCollMemory.Query().Where(StrAnyQuery).ToEnumerable().CaptureResult(),
            LiteCollMemory.Query().Where(IntAllQuery).ToEnumerable().CaptureResult(),
            LiteCollMemory.Query().Where(StrAllQuery).ToEnumerable().CaptureResult()
        };
        return result;
    }

    [Benchmark]
    public IList<object?> Realm()
    {
        
     

        var result = new List<object?>
        {
            RealmCli.All<BenchmarkRealmEntity>().Filter($"ANY IntArray.@values IN {{ {SearchItemsIntJoined} }}").CaptureResult(),
            RealmCli.All<BenchmarkRealmEntity>().Filter($"ANY StrArray.@values IN {{ {SearchItemsStrJoined} }}").CaptureResult(),
            RealmCli.All<BenchmarkRealmEntity>().Filter($"ALL IntArray.@values IN {{ {SearchItemsIntJoined} }}").CaptureResult(),
            RealmCli.All<BenchmarkRealmEntity>().Filter($"ALL StrArray.@values IN {{ {SearchItemsStrJoined} }}").CaptureResult()
        };
       
        return result;
    }
}
