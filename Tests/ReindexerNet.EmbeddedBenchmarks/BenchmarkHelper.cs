using ReindexerNet;
using ReindexerNetBenchmark.EmbeddedBenchmarks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNetBenchmark;

internal static class BenchmarkHelper
{
    public static object CaptureResult(this BenchmarkEntity entity)
        => entity.PreventLazy();

    public static object CaptureResult(this BenchmarkRealmEntity entity)
        => entity.PreventLazy();

    public static object CaptureResult(this IEnumerable<BenchmarkEntity> entites)
        => entites.Select(e => e.PreventLazy()).ToList();

    public static object CaptureResult(this IEnumerable<BenchmarkRealmEntity> entites)
        => entites.Select(e => e.PreventLazy()).ToList();

    public static object CaptureResult(this QueryItemsOf<BenchmarkEntity> result)
        => result.Items.Select(e => e.PreventLazy()).ToList();
}
