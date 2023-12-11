# ReindexerNet

[![Embedded  Nuget](https://img.shields.io/nuget/v/ReindexerNet.Embedded?label=Embedded&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Embedded)
[![Remote.Grpc  Nuget](https://img.shields.io/nuget/v/ReindexerNet.Remote.Grpc?label=Remote.Grpc&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Remote.Grpc)
[![Core Nuget](https://img.shields.io/nuget/v/ReindexerNet.Core?label=Core&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Core)

[![Build, Test, Package](https://github.com/oruchreis/ReindexerNet/actions/workflows/build.yml/badge.svg)](https://github.com/oruchreis/ReindexerNet/actions/workflows/build.yml)
[![Unix Test](https://github.com/oruchreis/ReindexerNet/actions/workflows/unix-test.yml/badge.svg)](https://github.com/oruchreis/ReindexerNet/actions/workflows/unix-test.yml)
[![Windows Test](https://github.com/oruchreis/ReindexerNet/actions/workflows/windows-test.yml/badge.svg)](https://github.com/oruchreis/ReindexerNet/actions/workflows/windows-test.yml)

ReindexerNet is a .NET binding(builtin & builtinserver) and connector(Grpc, ~~OpenApi~~) for embeddable in-memory document db [Reindexer](https://github.com/Restream/reindexer). 
We are using ReindexerNET in production environments for a long time, and even if all unit tests are passed, we don't encourge you to use in a prod environment without testing. So please test in your environment before using.

If you have any questions about Reindexer, please use [main page](https://github.com/Restream/reindexer) of Reindexer. Feel free to report issues and contribute about **ReindexerNet**. You can check the [change logs here](CHANGELOG.md).

## Contents
- [Sample Usage](#sample-usage)
- [Packages](#packages)
- [Performance](#performance)
- [Roadmap](#to-do-list)

## Sample Usage:
- Add one of these client packages according to your use case
  - For builtin/builtinserver mode which is embedded client: [![Embedded  Nuget](https://img.shields.io/nuget/v/ReindexerNet.Embedded?label=Embedded&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Embedded)
  - For grpc usage to remote reindexer server: [![Remote.Grpc  Nuget](https://img.shields.io/nuget/v/ReindexerNet.Remote.Grpc?label=Remote.Grpc&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Remote.Grpc)
  - For rest api usage to remote reindexer server:  [![Core Nuget](https://img.shields.io/nuget/v/ReindexerNet.Core?label=Core&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Core)
- If you plan to use ReindexerNet.Embedded client, you must add at least one of these native packages
  - [![Windows-x64 Nuget](https://img.shields.io/nuget/v/ReindexerNet.Embedded.Native.Win-x64?label=Embedded.Native.Win-x64&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Embedded.Native.Win-x64)
  - [![Windows-x86 Nuget](https://img.shields.io/nuget/v/ReindexerNet.Embedded.Native.Win-x86?label=Embedded.Native.Win-x86&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Embedded.Native.Win-x86)
  - [![Osx Nuget](https://img.shields.io/nuget/v/ReindexerNet.Embedded.Native.Osx-x64?label=Embedded.Native.Osx&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Embedded.Native.Osx-x64)
  - [![Linux Nuget](https://img.shields.io/nuget/v/ReindexerNet.Embedded.Native.Linux-x64?label=Embedded.Native.Linux&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Embedded.Native.Linux-x64)
  - [![AlpineLinux Nuget](https://img.shields.io/nuget/v/ReindexerNet.Embedded.Native.AlpineLinux-x64?label=Embedded.Native.AlpineLinux&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Embedded.Native.AlpineLinux-x64)
 - Or you can use conditon to add native packages that are only needed according to the operating system:

```xml
<ItemGroup>
  <PackageReference 
    Include="ReindexerNet.Embedded.Native.AlpineLinux-x64" Version="0.4.1.3200"
    Condition="$([MSBuild]::IsOSPlatform('Linux')) and ($(RuntimeIdentifier.StartsWith('linux-musl')) or $(RuntimeIdentifier.StartsWith('alpine')))" />
  <PackageReference
    Include="ReindexerNet.Embedded.Native.Linux-x64" Version="0.4.1.3200"
    Condition="$([MSBuild]::IsOSPlatform('Linux')) and !($(RuntimeIdentifier.StartsWith('linux-musl')) or $(RuntimeIdentifier.StartsWith('alpine')))" />
  <PackageReference 
    Include="ReindexerNet.Embedded.Native.Osx-x64" Version="0.4.1.3200"
    Condition="$([MSBuild]::IsOSPlatform('OSX'))"  />
  <PackageReference 
    Include="ReindexerNet.Embedded.Native.Win-x64" Version="0.4.1.3200"
    Condition="$([MSBuild]::IsOSPlatform('Windows'))" />
  <PackageReference 
    Include="ReindexerNet.Embedded.Native.Win-x86" Version="0.4.1.3200" 
    Condition="$([MSBuild]::IsOSPlatform('Windows'))" />
</ItemGroup>
```

- You should start to create a client object, connect to a remote server or builtin/builtinserver like in this example:
```csharp
private static readonly IReindexerClient _rxClient;

public async Task InitClientAsync()
{
    DbPath = Path.Combine(Path.GetTempPath(), "ReindexerDB");
    _rxClient = new ReindexerEmbedded(DbPath);
    await _rxClient.ConnectAsync();	
}
```
Normally ReindexerEmbedded client is IDisposable, and you should dispose it if you want to use it for a short period of time. But usually cache operations are used for a long time in the lifetime of the application. So we defined client object here as a static object. If you want to close connection at shutdown of the application, you should call Dispose method on the shutdown event of the application. This frees the file handles kept on the disk.

- Then you need a class for schema and you should create cache table and add proper indicies for searching:
```csharp
internal class CacheEntity
{
    public Guid Id { get; set; }
    public int? IntProperty { get; set; }
    public string? StringProperty { get; set; }
    public DateTimeOffset? CreateDate { get; set; }
    public int[] IntArray { get; set; }
    public string[] StrArray { get; set; }
    public string Payload { get; set; }
}

public async Task CreateCacheTable()
{
    await _rxClient.OpenNamespaceAsync("CacheTable");
    await _rxClient.AddIndexAsync("CacheTable", new Index { Name = nameof(CacheEntity.Id), IsPk = true, IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = true });
    await _rxClient.AddIndexAsync("CacheTable", new Index { Name = nameof(CacheEntity.IntProperty), IndexType = IndexType.Tree, FieldType = FieldType.Int, IsDense = false });
    await _rxClient.AddIndexAsync("CacheTable", new Index { Name = nameof(CacheEntity.StringProperty), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = true });
    await _rxClient.AddIndexAsync("CacheTable", new Index { Name = nameof(CacheEntity.CreateDate), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = true });
    await _rxClient.AddIndexAsync("CacheTable", new Index { Name = nameof(CacheEntity.IntArray), IndexType = IndexType.Hash, FieldType = FieldType.Int, IsDense = true, IsArray = true });
    await _rxClient.AddIndexAsync("CacheTable", new Index { Name = nameof(CacheEntity.StrArray), IndexType = IndexType.Hash, FieldType = FieldType.String, IsDense = true, IsArray = true });

    //Please refer to Reindexer's documentation for more index specifications
}
```
- Insert and Query operations:
```csharp
public async Task InsertAsync()
{
    var data = new CacheEntity[10];
    for (int i = 0; i < N; i++)
    {
        _data[i] = new CacheEntity()
        {
            Id = Guid.NewGuid(),
            IntProperty = i,
            StringProperty = "ÇŞĞÜÖİöçşğüı",
            CreateDate = DateTime.UtcNow,
            IntArray = new[] { 123, 124, 456, 456, 6777, 3123, 123123, 333 },
            StrArray = new[] { "", "Abc", "Def", "FFF", "GGG", "HHH", "HGFDF", "asd" },
            Paylod = "Not Indexed DATA here.."
        };
    }
    await _rxClient.InsertAsync("CacheTable", data); //for performance reasons don't call client in a loop, instead send multiple items at once in a single client call.
    //or
    await _rxClient.UpsertAsync("CacheTable", data); //for update or insert
}

public async Task QueryAsync()
{
    var result1 = await _rxClient.ExecuteSqlASync("Select * FROM CacheTable WHERE StringProperty IN ('abc', 'def')");
    var result2 = await _rxClient.ExecuteSqlASync("Select * FROM CacheTable WHERE IntProperty > 1000");
    var result3 = await _rxClient.ExecuteSqlASync("Select * FROM CacheTable WHERE IntArray IN (100, 500, 20)");
    //Please refer to Reindexer's documentation for more query samples.
}
```
You can find Reindexer Documentation at [their github page](https://github.com/Restream/reindexer). You can also check unit tests and benchmark project for usage samples in this repository.

## Packages

The first three parts of the package versions refer to ReindexerNet's own version. If the version has a fourth part, this refers to the Reindexer version it supports or defines.
For example:
```
v 0.3.10. 3200
  ╚══╦══╝ ╚═╦═╝
     ╚══════╬═══ ReindexerNET version 0.3.10
            ╚═══ Reindexer version 3.20.0
```

### ReindexerNet.Embedded [![Embedded  Nuget](https://img.shields.io/nuget/v/ReindexerNet.Embedded?label=Embedded&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Embedded)
This package contains embedded Reindexer implementation(**builtin**) and embedded server implementation(**builtinserver**). You can use this for memory caching in .net without using .net heap. Also you can use server implementation to run Reindexer server in your .net application.

If you use .net heap for memory caching, you will eventually encounter long GC pauses because of enlarged .net heap and LOH. And if you can't use remote caching because of performance considerations, you have to use native memory for caching. 

There are a few native memory cache solutions, and we choose Reindexer over them because of its performance. You can check Reindexer's benchmark results in their [main page](https://github.com/Restream/reindexer). Also you can check below for comparison of .net embedded db solutions with Reindexer.

#### Native Library Dependencies
This package supports `linux-x64`, `linux-musl-x64`, `osx-x64`, `win-x64` and `win-x86` runtimes. We built Reindexer as a native library from source to use Reindexer c/c++ api via p/invoke. By doing this, we aimed at decreasing the native dependencies as much as possible and compiled dependencies such as leveldb, rocksdb, snappy into the native library as static linking. These are minimum native dependencies for the libraries:
##### linux-x64 (`libreindexer_embedded_server.so`)
> Tested on Ubuntu 18.04, 20.04
> ```
> GLIBC_2.2.5(libdl, libpthread, librt, libm, libc)
> GCC_3.3.1(libgcc)
> GLIBCXX_3.4(libstdc++)
> ```

##### osx-x64 (`libreindexer_embedded_server.dylib`)
> Tested on MacOs 10.15
> ```
> libc++
> libresolv
> ```

##### win-x64/win-x86 (`reindexer_embedded_server.dll`)
> Tested on Windows 10, Server 2022
> ```
> No c/c++ library or other dependencies, it has been statically linked.
> ```


### ReindexerNet.Remote.Grpc [![Remote.Grpc  Nuget](https://img.shields.io/nuget/v/ReindexerNet.Remote.Grpc?label=Remote.Grpc&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Remote.Grpc)
This package contains Grpc client to use Reindexer server over grpc protocol. It uses new [grpc for dotnet](https://github.com/grpc/grpc-dotnet) library by Microsoft for .Net Core 3.1, .Net 5.0 and up. And it uses legacy [grpc-core](https://github.com/grpc/grpc/tree/master/src/csharp) library for .Net Framework and .Net Standard 2.0 because of http/2 support.

### ReindexerNet.Core [![Core Nuget](https://img.shields.io/nuget/v/ReindexerNet.Core?label=Core&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Core)
This package contains base types and common models for Reindexer and .net packages. You can use the models in this package as OpenApi/Rest models. Every model in this package has `DataContract` and `JsonPropertyName` attributes to support valid json serialization for Reindexer rest api.

## Performance
## ReindexerNet.Embedded Benchmarks and Comparisons
```
ReindexerNet  v0.4.1 (Reindexer v3.20)
Cachalot      v2.0.8
LiteDB        v5.0.7
Realm.NET     v11.6.1

BenchmarkDotNet v0.13.11, Windows 10 (10.0.19045.3636/22H2/2022Update)
Intel Core i7-8700K CPU 3.70GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.100
  [Host]     : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
```
### Insert Benchmarks
> #### Insert (Without controlling existance)
```markdown
| Method             | N      | Mean        | Error | Gen0         | Gen1        | Gen2      | Allocated   |
|------------------- |------- |------------:|------:|-------------:|------------:|----------:|------------:|
| Cachalot           | 10000  |    503.0 ms |    NA |   30000.0000 |   9000.0000 |         - |   211.52 MB |
| CachalotCompressed | 10000  |  1,361.1 ms |    NA |  563000.0000 |  35000.0000 |         - |   3408.5 MB |
| CachalotOnlyMemory | 10000  |    520.3 ms |    NA |   23000.0000 |   8000.0000 | 2000.0000 |   138.14 MB |
| LiteDb             | 10000  |  1,018.3 ms |    NA |  302000.0000 |   1000.0000 |         - |  1829.46 MB |
| LiteDbMemory       | 10000  |    978.7 ms |    NA |  304000.0000 |   1000.0000 |         - |  1919.56 MB |
| Realm              | 10000  |    500.5 ms |    NA |    7000.0000 |   2000.0000 | 1000.0000 |    43.29 MB |
| ReindexerNet       | 10000  |    215.8 ms |    NA |    1000.0000 |   1000.0000 |         - |        9 MB |
| ReindexerNetDense  | 10000  |    215.1 ms |    NA |    1000.0000 |   1000.0000 |         - |        9 MB |
| Cachalot           | 100000 |  3,574.3 ms |    NA |  293000.0000 |  88000.0000 |         - |  2157.84 MB |
| CachalotCompressed | 100000 | 11,675.4 ms |    NA | 5506000.0000 | 315000.0000 |         - | 33376.07 MB |
| CachalotOnlyMemory | 100000 |  2,587.6 ms |    NA |  211000.0000 |  56000.0000 | 1000.0000 |  1373.43 MB |
| LiteDb             | 100000 | 13,296.9 ms |    NA | 3836000.0000 |  14000.0000 | 3000.0000 | 23072.51 MB |
| LiteDbMemory       | 100000 | 12,576.0 ms |    NA | 3769000.0000 |  16000.0000 | 4000.0000 | 23563.28 MB |
| Realm              | 100000 |  3,121.4 ms |    NA |   73000.0000 |  49000.0000 | 1000.0000 |   432.62 MB |
| ReindexerNet       | 100000 |  1,970.3 ms |    NA |   16000.0000 |   2000.0000 | 1000.0000 |    94.52 MB |
| ReindexerNetDense  | 100000 |  1,824.3 ms |    NA |   16000.0000 |   2000.0000 | 1000.0000 |    94.52 MB |
```
> #### Upsert (Update if exists, otherwise insert)
```
| Method             | N      | Mean        | Error | Gen0         | Gen1        | Allocated   |
|------------------- |------- |------------:|------:|-------------:|------------:|------------:|
| Cachalot           | 10000  |  1,707.0 ms |    NA |  106000.0000 |  10000.0000 |   658.26 MB |
| CachalotCompressed | 10000  |  2,853.8 ms |    NA |  671000.0000 |  32000.0000 |  4059.77 MB |
| CachalotOnlyMemory | 10000  |    315.8 ms |    NA |   20000.0000 |   5000.0000 |   125.98 MB |
| LiteDb             | 10000  |    306.9 ms |    NA |   54000.0000 |           - |   327.39 MB |
| LiteDbMemory       | 10000  |    213.3 ms |    NA |   53000.0000 |           - |   320.88 MB |
| Realm              | 10000  |    218.5 ms |    NA |    7000.0000 |   6000.0000 |    43.31 MB |
| ReindexerNet       | 10000  |    182.2 ms |    NA |    1000.0000 |           - |        9 MB |
| ReindexerNetDense  | 10000  |    183.0 ms |    NA |    1000.0000 |           - |        9 MB |
| Cachalot           | 100000 |  5,693.4 ms |    NA |  465000.0000 |  90000.0000 |     2979 MB |
| CachalotCompressed | 100000 | 14,753.4 ms |    NA | 5712000.0000 | 303000.0000 | 34521.09 MB |
| CachalotOnlyMemory | 100000 |  3,495.1 ms |    NA |  207000.0000 |  51000.0000 |  1258.28 MB |
| LiteDb             | 100000 |  2,760.3 ms |    NA |  701000.0000 |  14000.0000 |   4196.2 MB |
| LiteDbMemory       | 100000 |  2,582.1 ms |    NA |  633000.0000 |  14000.0000 |  3793.94 MB |
| Realm              | 100000 |  2,061.4 ms |    NA |   72000.0000 |  51000.0000 |   432.61 MB |
| ReindexerNet       | 100000 |  1,531.4 ms |    NA |   15000.0000 |           - |       99 MB |
| ReindexerNetDense  | 100000 |  1,497.3 ms |    NA |   15000.0000 |           - |    96.62 MB |
```

### Select Benchmarks
#### Single Primary Key(Guid) in a loop
```
foreach (id in ids)
{
    WHERE Id = id
}

| Method               | N    | Mean       | Error     | StdDev    | Min        | Max        | Gen0       | Gen1      | Gen2      | Allocated    |
|--------------------- |----- |-----------:|----------:|----------:|-----------:|-----------:|-----------:|----------:|----------:|-------------:|
| ReindexerNet         | 1000 |   3.772 ms | 0.0608 ms | 0.0569 ms |   3.675 ms |   3.909 ms |   140.6250 |   19.5313 |    3.9063 |    916.25 KB |
| ReindexerNetSpanJson | 1000 |   4.087 ms | 0.0577 ms | 0.0540 ms |   3.963 ms |   4.184 ms |   132.8125 |   15.6250 |         - |    880.35 KB |
| ReindexerNetSql      | 1000 | 106.503 ms | 2.1241 ms | 4.9651 ms |  97.281 ms | 117.538 ms |  7333.3333 | 4000.0000 | 1000.0000 |  39063.15 KB |
| CachalotMemory       | 1000 | 206.542 ms | 2.2395 ms | 2.0949 ms | 201.875 ms | 209.943 ms | 30000.0000 | 5000.0000 |         - | 187603.98 KB |
| Cachalot             | 1000 | 208.367 ms | 1.5068 ms | 1.2582 ms | 206.252 ms | 210.109 ms | 30333.3333 | 4000.0000 |         - | 187603.27 KB |
| LiteDbMemory         | 1000 | 210.757 ms | 3.0700 ms | 2.8717 ms | 206.822 ms | 216.022 ms | 36000.0000 | 6000.0000 | 1000.0000 |  215746.8 KB |
| LiteDb               | 1000 | 212.069 ms | 3.6957 ms | 6.5691 ms | 197.385 ms | 225.860 ms | 36000.0000 | 6000.0000 | 1000.0000 | 215787.06 KB |
| Realm                | 1000 | 233.696 ms | 3.1694 ms | 2.9647 ms | 228.573 ms | 239.720 ms |  4333.3333 | 2666.6667 |  666.6667 |  23001.83 KB |
| CachalotCompressed   | 1000 | 251.389 ms | 1.9690 ms | 1.7455 ms | 248.861 ms | 254.736 ms | 35500.0000 | 4500.0000 |         - | 220243.17 KB |
```

#### Multiple Primary Key(Guid) at once
```
WHERE Id IN (id_1,id_2,id_3,...,id_N)

| Method               | N    | Mean         | Error       | StdDev      | Median       | Min          | Max          | Gen0       | Gen1      | Gen2      | Allocated    |
|--------------------- |----- |-------------:|------------:|------------:|-------------:|-------------:|-------------:|-----------:|----------:|----------:|-------------:|
| ReindexerNet         | 1000 |     280.5 us |     1.20 us |     1.06 us |     280.6 us |     278.9 us |     282.8 us |     6.3477 |         - |         - |     40.91 KB |
| ReindexerNetSpanJson | 1000 |     285.1 us |     5.63 us |     8.07 us |     280.7 us |     278.0 us |     302.9 us |     6.3477 |         - |         - |     39.96 KB |
| ReindexerNetSql      | 1000 | 106,486.3 us | 2,129.47 us | 3,438.69 us | 105,796.1 us |  98,176.9 us | 113,709.0 us |  8400.0000 | 4800.0000 | 1200.0000 |  44307.83 KB |
| LiteDb               | 1000 | 185,631.6 us | 2,317.46 us | 2,167.75 us | 186,081.7 us | 181,218.5 us | 188,559.2 us | 32500.0000 | 5000.0000 |  500.0000 | 197863.84 KB |
| LiteDbMemory         | 1000 | 201,107.3 us | 3,993.21 us | 9,870.23 us | 196,599.5 us | 187,358.6 us | 226,621.6 us | 33000.0000 | 6000.0000 | 1000.0000 | 197536.98 KB |
| CachalotMemory       | 1000 | 208,060.8 us | 1,865.30 us | 1,653.54 us | 207,961.0 us | 205,400.9 us | 211,116.9 us | 28666.6667 | 4333.3333 |         - | 193851.34 KB |
| Cachalot             | 1000 | 227,040.3 us | 3,343.96 us | 3,127.94 us | 226,465.9 us | 223,353.5 us | 235,004.1 us | 28000.0000 | 5000.0000 |         - | 193853.33 KB |
| Realm                | 1000 | 243,557.0 us | 1,778.28 us | 1,576.39 us | 243,424.7 us | 241,051.8 us | 246,540.9 us |  4333.3333 | 2333.3333 |  666.6667 |  23013.14 KB |
| CachalotCompressed   | 1000 | 251,255.9 us | 1,220.44 us |   952.84 us | 251,492.5 us | 249,395.1 us | 252,855.4 us | 35000.0000 | 5000.0000 |         - | 223683.38 KB |
```


#### Single Hash in a loop
```
foreach (value in values)
{
    WHERE StringProperty = value
}

| Method               | N    | Mean      | Error    | StdDev   | Min       | Max       | Gen0       | Gen1      | Gen2      | Allocated |
|--------------------- |----- |----------:|---------:|---------:|----------:|----------:|-----------:|----------:|----------:|----------:|
| ReindexerNetSpanJson | 1000 |  76.73 ms | 1.531 ms | 2.195 ms |  72.30 ms |  80.41 ms |  4285.7143 | 2428.5714 |  714.2857 |  22.02 MB |
| ReindexerNet         | 1000 |  99.67 ms | 1.593 ms | 2.181 ms |  96.82 ms | 104.71 ms |  7333.3333 | 4000.0000 | 1000.0000 |  38.23 MB |
| ReindexerNetSql      | 1000 | 109.15 ms | 2.178 ms | 4.499 ms |  99.27 ms | 118.38 ms |  7333.3333 | 4000.0000 | 1000.0000 |  38.14 MB |
| LiteDb               | 1000 | 219.26 ms | 4.327 ms | 6.476 ms | 211.27 ms | 230.07 ms | 36000.0000 | 6000.0000 | 1000.0000 | 211.92 MB |
| LiteDbMemory         | 1000 | 225.70 ms | 4.345 ms | 6.764 ms | 212.11 ms | 236.48 ms | 36000.0000 | 6000.0000 | 1000.0000 | 213.21 MB |
| Realm                | 1000 | 371.69 ms | 3.742 ms | 3.500 ms | 366.67 ms | 378.11 ms |  5000.0000 | 2000.0000 |         - |  30.75 MB |
| CachalotMemory       | 1000 | 395.27 ms | 3.091 ms | 2.891 ms | 391.74 ms | 402.18 ms | 33000.0000 | 5000.0000 |         - | 197.73 MB |
| Cachalot             | 1000 | 396.81 ms | 3.622 ms | 3.211 ms | 392.42 ms | 401.60 ms | 33000.0000 | 4000.0000 |         - | 198.07 MB |
| CachalotCompressed   | 1000 | 400.09 ms | 5.197 ms | 4.607 ms | 394.77 ms | 410.26 ms | 33000.0000 | 4000.0000 |         - | 198.08 MB |
```

#### Single Hash in a Parallel loop
```
Parallel.Foreach(values, value =>
{
    WHERE StringProperty = value
});

| Method               | N    | Mean      | Error    | StdDev    | Min       | Max       | Gen0       | Gen1      | Gen2     | Allocated |
|--------------------- |----- |----------:|---------:|----------:|----------:|----------:|-----------:|----------:|---------:|----------:|
| ReindexerNetSpanJson | 1000 |  40.75 ms | 0.808 ms |  1.415 ms |  38.07 ms |  43.88 ms |  4076.9231 | 2076.9231 | 153.8462 |  22.12 MB |
| ReindexerNet         | 1000 |  54.56 ms | 1.067 ms |  1.979 ms |  49.93 ms |  58.52 ms |  7300.0000 | 3200.0000 | 300.0000 |  38.34 MB |
| ReindexerNetSql      | 1000 |  58.70 ms | 1.157 ms |  1.421 ms |  54.43 ms |  60.77 ms |  7300.0000 | 3200.0000 | 300.0000 |  38.15 MB |
| Realm                | 1000 |  98.26 ms | 1.563 ms |  1.462 ms |  94.69 ms | 100.04 ms |  6600.0000 | 3400.0000 | 400.0000 |  36.01 MB |
| CachalotMemory       | 1000 | 200.90 ms | 3.963 ms |  9.342 ms | 183.63 ms | 228.97 ms | 35000.0000 | 7000.0000 |        - | 197.77 MB |
| CachalotCompressed   | 1000 | 209.76 ms | 4.179 ms | 10.328 ms | 189.66 ms | 228.95 ms | 35000.0000 | 7500.0000 |        - | 198.15 MB |
| Cachalot             | 1000 | 214.24 ms | 4.240 ms |  9.570 ms | 192.58 ms | 230.81 ms | 35000.0000 | 8000.0000 |        - | 198.14 MB |
| LiteDbMemory         | 1000 | 225.41 ms | 4.471 ms | 10.363 ms | 206.92 ms | 247.77 ms | 37000.0000 | 8500.0000 | 500.0000 | 211.58 MB |
| LiteDb               | 1000 | 242.61 ms | 5.148 ms | 15.178 ms | 206.78 ms | 265.36 ms | 37000.0000 | 8333.3333 | 333.3333 | 210.86 MB |
```

#### Multiple Hash at once
```
WHERE StringProperty IN ('abc', 'def', .... )

| Method               | N    | Mean      | Error    | StdDev   | Min       | Max       | Gen0       | Gen1       | Gen2      | Allocated |
|--------------------- |----- |----------:|---------:|---------:|----------:|----------:|-----------:|-----------:|----------:|----------:|
| ReindexerNetSpanJson | 1000 |  76.59 ms | 1.484 ms | 1.388 ms |  75.02 ms |  80.02 ms |  5285.7143 |  3000.0000 |  857.1429 |  26.94 MB |
| ReindexerNetSql      | 1000 | 108.02 ms | 2.122 ms | 3.486 ms | 101.71 ms | 114.66 ms |  8400.0000 |  4800.0000 | 1200.0000 |  43.22 MB |
| ReindexerNet         | 1000 | 110.42 ms | 2.192 ms | 3.722 ms | 104.36 ms | 116.44 ms |  8400.0000 |  4800.0000 | 1200.0000 |  43.16 MB |
| LiteDbMemory         | 1000 | 193.75 ms | 2.587 ms | 2.420 ms | 191.26 ms | 199.34 ms | 33000.0000 |  6000.0000 | 1000.0000 | 193.29 MB |
| LiteDb               | 1000 | 197.36 ms | 2.779 ms | 3.985 ms | 192.03 ms | 208.02 ms | 33000.0000 |  6000.0000 | 1000.0000 | 193.13 MB |
| CachalotMemory       | 1000 | 204.27 ms | 2.363 ms | 2.210 ms | 200.86 ms | 208.97 ms | 28000.0000 |  5000.0000 |         - | 189.21 MB |
| Cachalot             | 1000 | 212.28 ms | 3.811 ms | 5.217 ms | 205.63 ms | 226.48 ms | 28000.0000 |  5000.0000 |         - | 189.21 MB |
| Realm                | 1000 | 243.08 ms | 3.056 ms | 2.859 ms | 238.90 ms | 248.40 ms |  4333.3333 |  2333.3333 |  666.6667 |  22.42 MB |
| CachalotCompressed   | 1000 | 454.42 ms | 3.513 ms | 2.743 ms | 450.31 ms | 458.75 ms | 64000.0000 | 10000.0000 |         - | 399.26 MB |
```

#### Range Filtering
```
WHERE IntProperty < i
WHERE IntProperty >= i

| Method               | N    | Mean        | Error     | StdDev    | Min         | Max         | Gen0        | Gen1       | Gen2      | Allocated |
|--------------------- |----- |------------:|----------:|----------:|------------:|------------:|------------:|-----------:|----------:|----------:|
| ReindexerNetSpanJson | 1000 |    68.98 ms |  1.234 ms |  1.884 ms |    65.60 ms |    72.79 ms |   4250.0000 |  2500.0000 |  750.0000 |  21.18 MB |
| ReindexerNet         | 1000 |    91.01 ms |  1.781 ms |  2.316 ms |    86.20 ms |    94.46 ms |   7166.6667 |  4000.0000 | 1000.0000 |  37.39 MB |
| ReindexerNetSql      | 1000 |    94.10 ms |  0.945 ms |  0.838 ms |    93.12 ms |    96.12 ms |   7166.6667 |  4000.0000 | 1000.0000 |  37.39 MB |
| LiteDbMemory         | 1000 |   166.43 ms |  3.151 ms |  4.207 ms |   162.31 ms |   177.94 ms |  29000.0000 |  4000.0000 |         - | 178.44 MB |
| LiteDb               | 1000 |   183.24 ms |  3.307 ms |  3.094 ms |   178.14 ms |   186.81 ms |  30500.0000 |  5000.0000 | 1000.0000 | 178.44 MB |
| CachalotMemory       | 1000 |   206.58 ms |  1.655 ms |  1.548 ms |   203.80 ms |   208.95 ms |  28000.0000 |  5000.0000 |         - | 181.15 MB |
| Realm                | 1000 |   228.51 ms |  4.063 ms |  3.800 ms |   223.23 ms |   233.08 ms |   4000.0000 |  2000.0000 |  500.0000 |  22.25 MB |
| Cachalot             | 1000 | 2,125.57 ms | 41.912 ms | 86.555 ms | 1,989.19 ms | 2,358.20 ms | 276000.0000 | 40000.0000 |         - | 1742.7 MB |
| CachalotCompressed   | 1000 | 2,267.47 ms | 35.480 ms | 59.279 ms | 2,215.05 ms | 2,421.11 ms | 310000.0000 | 45000.0000 |         - | 1946.9 MB |
```

#### Array Column Filtering(Single Item IN)
```
WHERE N IN Integer_Array 
WHERE 'N' IN String_Array

| Method               | N    | Mean          | Error       | StdDev      | Min           | Max           | Gen0    | Gen1   | Gen2   | Allocated |
|--------------------- |----- |--------------:|------------:|------------:|--------------:|--------------:|--------:|-------:|-------:|----------:|
| ReindexerNet         | 1000 |      7.277 us |   0.0620 us |   0.0580 us |      7.131 us |      7.365 us |  0.2975 | 0.0076 | 0.0076 |   1.95 KB |
| ReindexerNetSpanJson | 1000 |      7.330 us |   0.0774 us |   0.0724 us |      7.110 us |      7.406 us |  0.2975 | 0.0076 | 0.0076 |   1.95 KB |
| ReindexerNetSql      | 1000 |     11.273 us |   0.0702 us |   0.0657 us |     11.148 us |     11.382 us |  0.1984 |      - |      - |    1.4 KB |
| LiteDb               | 1000 |     89.227 us |   1.5614 us |   1.4605 us |     86.572 us |     91.953 us | 17.0898 | 0.4883 |      - | 105.03 KB |
| LiteDbMemory         | 1000 |     89.300 us |   1.2470 us |   1.1054 us |     88.122 us |     91.294 us | 17.5781 | 0.4883 |      - | 109.07 KB |
| Cachalot             | 1000 |    383.703 us |   5.4934 us |   5.1385 us |    375.625 us |    392.781 us |  9.2773 | 4.3945 |      - |  57.52 KB |
| CachalotMemory       | 1000 |    384.297 us |   7.5295 us |  10.7985 us |    367.856 us |    403.976 us |  8.7891 | 3.9063 |      - |  57.35 KB |
| CachalotCompressed   | 1000 |    391.048 us |   7.6554 us |   8.1912 us |    377.069 us |    404.002 us |  9.2773 | 4.3945 |      - |   57.4 KB |
| Realm                | 1000 | 25,942.719 us | 516.0005 us | 614.2618 us | 25,371.772 us | 27,380.528 us |       - |      - |      - |   4.23 KB |
```
#### Array Column Filtering(Contains, All)
```
WHERE Integer_Array CONTAINS (1,2,3,....,N) 
WHERE String_Array CONTAINS ('abc', 'def', .... )
WHERE Integer_Array ALL (1,2,3,....,N)
WHERE Integer_Array ALL (1,2,3,....,N)

| Method               | N    | Mean       | Error    | StdDev   | Median   | Min      | Max        | Gen0        | Gen1       | Gen2      | Allocated |
|--------------------- |----- |-----------:|---------:|---------:|---------:|---------:|-----------:|------------:|-----------:|----------:|----------:|
| ReindexerNetSpanJson | 1000 |   134.1 ms |  2.59 ms |  2.66 ms | 133.6 ms | 129.9 ms |   139.3 ms |   8250.0000 |  4500.0000 | 1000.0000 |  43.95 MB |
| ReindexerNet         | 1000 |   178.5 ms |  3.56 ms |  6.68 ms | 181.8 ms | 166.6 ms |   187.0 ms |  13000.0000 |  7000.0000 | 1000.0000 |   77.3 MB |
| ReindexerNetSql      | 1000 |   186.7 ms |  2.89 ms |  2.71 ms | 186.9 ms | 182.4 ms |   190.5 ms |  13500.0000 |  7000.0000 | 1000.0000 |   77.3 MB |
| Cachalot             | 1000 |   464.7 ms |  9.26 ms |  9.10 ms | 462.2 ms | 451.8 ms |   489.3 ms |  58000.0000 |  9000.0000 |         - |  383.3 MB |
| CachalotMemory       | 1000 |   475.2 ms |  5.93 ms |  4.63 ms | 475.5 ms | 468.9 ms |   481.5 ms |  58000.0000 |  9000.0000 |         - |  383.3 MB |
| LiteDbMemory         | 1000 |   832.3 ms |  8.96 ms |  7.48 ms | 834.3 ms | 816.2 ms |   841.8 ms | 122000.0000 | 20000.0000 | 1000.0000 | 728.39 MB |
| LiteDb               | 1000 |   868.9 ms |  7.69 ms |  6.82 ms | 870.3 ms | 855.7 ms |   878.2 ms | 123000.0000 | 29000.0000 | 2000.0000 | 728.39 MB |
| Realm                | 1000 |   945.7 ms |  9.82 ms |  9.19 ms | 946.8 ms | 923.2 ms |   958.6 ms |   7000.0000 |  3000.0000 |         - |   44.5 MB |
| CachalotCompressed   | 1000 | 1,000.4 ms | 11.46 ms | 10.16 ms | 998.4 ms | 982.3 ms | 1,021.5 ms | 129000.0000 | 19000.0000 |         - | 806.47 MB |
```


## To Do List
 - [x] RestApi models [![Core  Nuget](https://img.shields.io/nuget/v/ReindexerNet.Core?label=Core&color=1182c2&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Core)
 - [x] Embedded mode binding (Builtin) [![Embedded  Nuget](https://img.shields.io/nuget/v/ReindexerNet.Embedded?label=Embedded&color=1182c2&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Embedded)
 - [x] Embedded Server mode binding (Builtin-server) [![Embedded  Nuget](https://img.shields.io/nuget/v/ReindexerNet.Embedded?label=Embedded&color=1182c2&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Embedded)
 - [x] Embedded bindings for win-x64, linux-x64, linux-musl-x64, osx-x64 and win-x86
 - [x] GRPC connector for remote servers (Standalone server/Grpc) [![Remote.Grpc  Nuget](https://img.shields.io/nuget/v/ReindexerNet.Remote.Grpc?label=Remote.Grpc&color=1182c2&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Remote.Grpc)
 - [x] Query Interface
 - [x] CJson Serializer for Query
 - [ ] CJson Serializer for items(encoder/decoder)
 - [x] Dsl Query Builder
 - [ ] OpenApi/Json connector for remote servers
 - [ ] CProto connector for remote servers
 - [ ] Documentation 
