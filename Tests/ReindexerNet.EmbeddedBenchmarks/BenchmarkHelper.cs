using LiteDB;
using ReindexerNet;
using ReindexerNetBenchmark.EmbeddedBenchmarks;

namespace ReindexerNetBenchmark;

internal static class BenchmarkHelper
{
    public static object CaptureResult(this BenchmarkEntity entity)
        => entity.PreventLazy();

    public static object CaptureResult(this BenchmarkRealmEntity entity)
        => entity.PreventLazy();

    public static object CaptureResult(this IEnumerable<BenchmarkEntity> entites)
        => entites.Select(e => e.PreventLazy()).ToList();

    public static object CaptureResult(this ILiteQueryable<BenchmarkEntity> entites)
        => entites.ToEnumerable().Select(e => e.PreventLazy()).ToList();

    public static object CaptureResult(this IEnumerable<BenchmarkRealmEntity> entites)
        => entites.Select(e => e.PreventLazy()).ToList();

    public static object CaptureResult(this QueryItemsOf<BenchmarkEntity> result)
        => result.Items.Select(e => e.PreventLazy()).ToList();
}
