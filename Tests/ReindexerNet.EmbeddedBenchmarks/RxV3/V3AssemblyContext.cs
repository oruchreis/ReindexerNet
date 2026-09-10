using System.Reflection;
using System.Runtime.Loader;

namespace ReindexerNetBenchmark.EmbeddedBenchmarks;

/// <summary>
/// Isolated AssemblyLoadContext for ReindexerNet v3 (0.5.0.3310).
/// Loads v3 ReindexerNet.Embedded.dll from rxv3/ subdirectory while sharing
/// ReindexerNet.Core with the host so types remain compatible across the boundary.
/// Native library isolation: v3 reindexer_embedded_server.dll is in rxv3/runtimes/win-x64/native/
/// which the v3 ReindexerBinding static constructor discovers via Assembly.GetExecutingAssembly().Location.
/// </summary>
internal sealed class V3AssemblyContext : AssemblyLoadContext
{
    private readonly string _v3Dir;

    public V3AssemblyContext(string v3Dir) : base("RxV3", isCollectible: false)
    {
        _v3Dir = v3Dir;
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Fall back to the default (v5) context for ReindexerNet.Core and BCL assemblies.
        // This keeps ConnectionOptions, Index, IQueryBuilder, QueryItemsOf<T> etc. as the
        // same CLR types in both the v3 assembly and the benchmark host, enabling direct
        // argument passing without serialization.
        if (assemblyName.Name is "ReindexerNet.Core"
            or "System.Runtime.Loader"
            or "System.Memory"
            or "Microsoft.NETCore.Platforms"
            or "netstandard")
            return null;

        var path = Path.Combine(_v3Dir, assemblyName.Name + ".dll");
        return File.Exists(path) ? LoadFromAssemblyPath(path) : null;
    }
}
