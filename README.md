# ReindexerNet

[![Embedded  Nuget](https://img.shields.io/nuget/v/ReindexerNet.Embedded?label=Embedded&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Embedded)
[![Remote.Grpc  Nuget](https://img.shields.io/nuget/v/ReindexerNet.Remote.Grpc?label=Remote.Grpc&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Remote.Grpc)
[![Core Nuget](https://img.shields.io/nuget/v/ReindexerNet.Core?label=Core&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Core)

[![Build, Test, Package](https://github.com/oruchreis/ReindexerNet/actions/workflows/build.yml/badge.svg)](https://github.com/oruchreis/ReindexerNet/actions/workflows/build.yml)
[![Unix Test](https://github.com/oruchreis/ReindexerNet/actions/workflows/unix-test.yml/badge.svg)](https://github.com/oruchreis/ReindexerNet/actions/workflows/unix-test.yml)
[![Windows Test](https://github.com/oruchreis/ReindexerNet/actions/workflows/windows-test.yml/badge.svg)](https://github.com/oruchreis/ReindexerNet/actions/workflows/windows-test.yml)

ReindexerNet is a .NET binding(builtin & builtinserver) and connector(Grpc, ~~OpenApi~~) for embeddable in-memory document db [Reindexer](https://github.com/Restream/reindexer).

According to our last benchmark, Reindexer is the **fastest** in every category compared to other embedded db's and it really works faster than other embedded db's. Below you can find benchmarks made with some well-known embedded dbs: ( [Performance Benchmarks](#performance) )

In addition, when used as a cache server in remote use, it works much faster than many in memory db's that have proven themselves at the point of performance such as redis or keydb. With this performance, it does not only work as key-value like other db's, but also has extra sql, dsl query support.

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
    Include="ReindexerNet.Embedded.Native.AlpineLinux-x64" Version="0.4.8.3290"
    Condition="$([MSBuild]::IsOSPlatform('Linux')) and ($(RuntimeIdentifier.StartsWith('linux-musl')) or $(RuntimeIdentifier.StartsWith('alpine')))" />
  <PackageReference
    Include="ReindexerNet.Embedded.Native.Linux-x64" Version="0.4.8.3290"
    Condition="$([MSBuild]::IsOSPlatform('Linux')) and !($(RuntimeIdentifier.StartsWith('linux-musl')) or $(RuntimeIdentifier.StartsWith('alpine')))" />
  <PackageReference 
    Include="ReindexerNet.Embedded.Native.Osx-x64" Version="0.4.8.3290"
    Condition="$([MSBuild]::IsOSPlatform('OSX'))"  />
  <PackageReference 
    Include="ReindexerNet.Embedded.Native.Win-x64" Version="0.4.8.3290"
    Condition="$([MSBuild]::IsOSPlatform('Windows'))" />
  <PackageReference 
    Include="ReindexerNet.Embedded.Native.Win-x86" Version="0.4.8.3290" 
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

### Versioning
The first three parts of the package versions refer to ReindexerNet's own version. If the version has a fourth part, this refers to the Reindexer version it supports or defines.
For example:
```
v 0.4.8. 3290
  ╚══╦══╝ ╚═╦═╝
     ╚══════╬═══ ReindexerNET version 0.4.8
            ╚═══ Reindexer version 3.29.0
```

### ReindexerNet.Embedded [![Embedded  Nuget](https://img.shields.io/nuget/v/ReindexerNet.Embedded?label=Embedded&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Embedded)
This package contains embedded Reindexer implementation(**builtin**) and embedded server implementation(**builtinserver**). You can use this for memory caching in .net without using .net heap. Also you can use server implementation to run Reindexer server in your .net application.

If you use .net heap for memory caching, you will eventually encounter long GC pauses because of enlarged .net heap and LOH. And if you can't use remote caching because of performance considerations, you have to use native memory for caching. 

There are a few native memory cache solutions, and we choose Reindexer over them because of its performance. You can check Reindexer's benchmark results in their [main page](https://github.com/Restream/reindexer). Also you can check below for comparison of .net embedded db solutions with Reindexer.

#### Native Library Dependencies
Reindexer Embedded package supports `linux-x64`, `linux-musl-x64`, `osx-x64`, `win-x64` and `win-x86` runtimes. We built Reindexer as a native library from source to use Reindexer c/c++ api via p/invoke. By doing this, we aimed at decreasing the native dependencies as much as possible and compiled dependencies such as leveldb, rocksdb, snappy into the native library as static linking. The following shows the native dependencies on some OSes, for which you should not normally need to install an extra package.:
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
## ReindexerNet.Embedded Benchmarks
```
ReindexerNet  v0.4.8 (Reindexer v3.29)
Cachalot      v2.5.13
LiteDB        v5.0.21
Realm.NET     v20.0.0

BenchmarkDotNet v0.13.11, Windows 10 (10.0.19045.3636/22H2/2022Update)
Intel Core i7-8700K CPU 3.70GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.100
  [Host]     : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
```
### Insert Benchmarks
> #### Insert (Without controlling existance)
```markdown
| Method             | N      | Mean        | Error | Gen0         | Gen1       | Gen2      | Allocated   |
|------------------- |------- |------------:|------:|-------------:|-----------:|----------:|------------:|
| ReindexerNetDense  | 10000  |    201.1 ms |    NA |    1000.0000 |  1000.0000 |         - |        9 MB |
| ReindexerNet       | 10000  |    208.3 ms |    NA |    1000.0000 |  1000.0000 |         - |        9 MB |
| CachalotOnlyMemory | 10000  |    284.4 ms |    NA |   10000.0000 |  5000.0000 | 2000.0000 |       57 MB |
| Cachalot           | 10000  |    467.5 ms |    NA |   12000.0000 |  5000.0000 | 1000.0000 |    82.63 MB |
| Realm              | 10000  |    472.8 ms |    NA |    7000.0000 |  2000.0000 | 1000.0000 |    43.27 MB |
| LiteDbMemory       | 10000  |  1,199.8 ms |    NA |  499000.0000 |  1000.0000 |         - |  3086.21 MB |
| LiteDb             | 10000  |  1,251.9 ms |    NA |  500000.0000 |  1000.0000 |         - |  3013.42 MB |
|                    |        |             |       |              |            |           |             |
| ReindexerNetDense  | 100000 |  1,396.2 ms |    NA |   16000.0000 |  2000.0000 | 1000.0000 |    94.17 MB |
| ReindexerNet       | 100000 |  1,561.6 ms |    NA |   16000.0000 |  2000.0000 | 1000.0000 |    96.72 MB |
| CachalotOnlyMemory | 100000 |  1,450.9 ms |    NA |   78000.0000 | 30000.0000 | 3000.0000 |   557.31 MB |
| Cachalot           | 100000 |  2,634.1 ms |    NA |  136000.0000 | 55000.0000 | 2000.0000 |  1154.24 MB |
| Realm              | 100000 |  2,804.3 ms |    NA |   73000.0000 | 49000.0000 | 1000.0000 |   432.64 MB |
| LiteDbMemory       | 100000 | 14,734.1 ms |    NA | 6130000.0000 | 13000.0000 | 4000.0000 | 37686.18 MB |
| LiteDb             | 100000 | 15,117.5 ms |    NA | 6124000.0000 | 12000.0000 | 3000.0000 | 36764.31 MB |
```
> #### Upsert (Update if exists, otherwise insert)
```
| Method             | N      | Mean       | Error | Gen0        | Gen1       | Gen2      | Allocated  |
|------------------- |------- |-----------:|------:|------------:|-----------:|----------:|-----------:|
| ReindexerNetDense  | 10000  |   124.4 ms |    NA |   1000.0000 |          - |         - |       9 MB |
| ReindexerNet       | 10000  |   157.2 ms |    NA |   1000.0000 |          - |         - |       9 MB |
| LiteDbMemory       | 10000  |   201.0 ms |    NA |  62000.0000 |          - |         - |  373.91 MB |
| Realm              | 10000  |   271.8 ms |    NA |   7000.0000 |  6000.0000 |         - |   43.31 MB |
| LiteDb             | 10000  |   304.7 ms |    NA |  62000.0000 |          - |         - |  374.94 MB |
| CachalotOnlyMemory | 10000  |   368.9 ms |    NA |   8000.0000 |  3000.0000 | 1000.0000 |   44.84 MB |
| Cachalot           | 10000  |   626.3 ms |    NA |  19000.0000 |  9000.0000 | 2000.0000 |  118.58 MB |
|                    |        |            |       |             |            |           |            |
| ReindexerNetDense  | 100000 | 1,020.8 ms |    NA |  15000.0000 |          - |         - |   94.52 MB |
| ReindexerNet       | 100000 | 1,120.4 ms |    NA |  15000.0000 |          - |         - |      99 MB |
| CachalotOnlyMemory | 100000 | 1,865.3 ms |    NA |  74000.0000 | 27000.0000 | 2000.0000 |   447.2 MB |
| Realm              | 100000 | 2,250.8 ms |    NA |  72000.0000 | 51000.0000 |         - |  432.61 MB |
| LiteDbMemory       | 100000 | 2,462.7 ms |    NA | 698000.0000 | 14000.0000 |         - | 4182.14 MB |
| LiteDb             | 100000 | 2,714.5 ms |    NA | 716000.0000 | 14000.0000 |         - | 4286.29 MB |
| Cachalot           | 100000 | 4,415.0 ms |    NA | 194000.0000 | 50000.0000 | 3000.0000 |  1299.3 MB |
```

### Select Benchmarks
#### Search Single Primary Key(Guid) in a loop
```
foreach (id in ids)
{
    WHERE Id = id
}

| Method               | N    | Mean       | Error      | StdDev     | Min        | Max        | Gen0       | Gen1       | Gen2      | Allocated    |
|--------------------- |----- |-----------:|-----------:|-----------:|-----------:|-----------:|-----------:|-----------:|----------:|-------------:|
| ReindexerNet         | 500  |   2.072 ms |  0.0351 ms |  0.0328 ms |   1.972 ms |   2.118 ms |    78.1250 |    11.7188 |    3.9063 |    493.42 KB |
| ReindexerNetSpanJson | 500  |   2.199 ms |  0.0394 ms |  0.0368 ms |   2.148 ms |   2.273 ms |    78.1250 |     7.8125 |    3.9063 |    491.18 KB |
| ReindexerNetSql      | 500  |  27.551 ms |  0.5348 ms |  0.8326 ms |  26.218 ms |  29.398 ms |  2156.2500 |  1062.5000 |  250.0000 |  11702.12 KB |
| Cachalot             | 500  |  30.070 ms |  0.3325 ms |  0.3110 ms |  29.551 ms |  30.569 ms |  4656.2500 |  1000.0000 |   62.5000 |   28276.5 KB |
| CachalotMemory       | 500  |  32.022 ms |  0.6398 ms |  0.7367 ms |  30.946 ms |  33.296 ms |  4687.5000 |  1000.0000 |  125.0000 |  28276.93 KB |
| LiteDb               | 500  |  54.168 ms |  1.0479 ms |  1.3253 ms |  52.253 ms |  57.222 ms |  8250.0000 |  1250.0000 |  250.0000 |  49030.27 KB |
| Realm                | 500  |  55.215 ms |  0.6272 ms |  0.5238 ms |  54.294 ms |  56.193 ms |  1000.0000 |   888.8889 |         - |   6128.16 KB |
| LiteDbMemory         | 500  |  55.976 ms |  0.7885 ms |  0.6990 ms |  55.070 ms |  57.328 ms |  8500.0000 |  1500.0000 |  400.0000 |  49778.61 KB |
|                      |      |            |            |            |            |            |            |            |           |              |
| ReindexerNet         | 2000 |   8.330 ms |  0.1625 ms |  0.2278 ms |   7.740 ms |   8.798 ms |   296.8750 |    46.8750 |         - |   1972.84 KB |
| ReindexerNetSpanJson | 2000 |   8.472 ms |  0.1660 ms |  0.1776 ms |   7.881 ms |   8.783 ms |   296.8750 |    46.8750 |         - |   1901.02 KB |
| CachalotMemory       | 2000 | 357.687 ms |  5.4327 ms |  4.5366 ms | 352.519 ms | 367.101 ms | 62000.0000 | 14000.0000 |         - | 381706.82 KB |
| ReindexerNetSql      | 2000 | 361.409 ms |  5.8897 ms |  5.5092 ms | 353.059 ms | 371.511 ms | 29000.0000 | 12000.0000 | 1000.0000 | 176449.89 KB |
| Cachalot             | 2000 | 365.361 ms |  4.3866 ms |  3.6630 ms | 360.496 ms | 373.086 ms | 62000.0000 | 14000.0000 |         - |  381707.8 KB |
| LiteDb               | 2000 | 718.270 ms | 12.2047 ms | 11.4163 ms | 700.739 ms | 737.668 ms | 95000.0000 | 18000.0000 | 1000.0000 | 577065.73 KB |
| LiteDbMemory         | 2000 | 728.215 ms | 14.1408 ms | 20.7273 ms | 701.674 ms | 788.429 ms | 94000.0000 | 18000.0000 | 1000.0000 | 575347.15 KB |
| Realm                | 2000 | 862.604 ms |  9.0422 ms |  8.4581 ms | 847.850 ms | 879.566 ms | 14000.0000 |  7000.0000 |         - |  88968.97 KB |
```

#### Search Multiple Primary Key(Guid) at once
```
WHERE Id IN (id_1,id_2,id_3,...,id_N)

| Method               | N    | Mean         | Error        | StdDev       | Min          | Max          | Gen0       | Gen1       | Gen2      | Allocated    |
|--------------------- |----- |-------------:|-------------:|-------------:|-------------:|-------------:|-----------:|-----------:|----------:|-------------:|
| ReindexerNetSpanJson | 500  |     136.1 us |      2.68 us |      2.51 us |     132.9 us |     141.7 us |     3.1738 |          - |         - |     21.46 KB |
| ReindexerNet         | 500  |     136.5 us |      2.68 us |      2.38 us |     133.4 us |     142.6 us |     3.1738 |          - |         - |     21.46 KB |
| ReindexerNetSql      | 500  |  23,098.8 us |    448.44 us |    598.66 us |  22,246.5 us |  24,132.5 us |  2250.0000 |  1281.2500 |  406.2500 |  11358.22 KB |
| Cachalot             | 500  |  27,889.7 us |    548.21 us |    631.32 us |  27,177.7 us |  29,019.8 us |  3750.0000 |  1093.7500 |   93.7500 |  26379.68 KB |
| CachalotMemory       | 500  |  31,821.9 us |    636.31 us |    595.20 us |  31,028.9 us |  33,017.6 us |  3812.5000 |  1156.2500 |  156.2500 |  26379.83 KB |
| LiteDb               | 500  |  43,851.0 us |    870.71 us |  1,220.61 us |  40,387.2 us |  46,048.8 us |  6454.5455 |  1545.4545 |  454.5455 |  37214.16 KB |
| LiteDbMemory         | 500  |  44,245.9 us |    863.23 us |  1,210.13 us |  42,381.9 us |  46,731.1 us |  6500.0000 |  1500.0000 |  416.6667 |  37329.56 KB |
| Realm                | 500  |  59,115.4 us |    823.26 us |    729.80 us |  58,027.3 us |  60,489.0 us |  1000.0000 |   888.8889 |         - |   6136.04 KB |
|                      |      |              |              |              |              |              |            |            |           |              |
| ReindexerNet         | 2000 |     525.0 us |      4.32 us |      4.04 us |     517.9 us |     530.2 us |    12.6953 |          - |         - |     79.09 KB |
| ReindexerNetSpanJson | 2000 |     530.9 us |      6.10 us |      5.70 us |     522.1 us |     539.2 us |    12.6953 |          - |         - |     79.09 KB |
| ReindexerNetSql      | 2000 | 336,158.0 us |  6,622.24 us |  8,840.50 us | 319,982.1 us | 354,733.6 us | 29000.0000 | 14000.0000 | 1000.0000 | 175072.11 KB |
| CachalotMemory       | 2000 | 360,622.8 us |  5,738.73 us |  5,368.01 us | 352,762.7 us | 370,401.7 us | 55000.0000 | 14000.0000 |         - | 406785.59 KB |
| Cachalot             | 2000 | 363,636.1 us |  6,547.07 us |  5,467.10 us | 356,196.1 us | 376,809.6 us | 55000.0000 | 14000.0000 |         - | 406786.57 KB |
| LiteDbMemory         | 2000 | 691,659.1 us | 11,076.31 us | 10,360.78 us | 674,224.9 us | 714,263.8 us | 86000.0000 | 18000.0000 | 1000.0000 | 524467.26 KB |
| LiteDb               | 2000 | 693,852.5 us | 10,159.20 us |  9,005.86 us | 672,651.5 us | 709,665.3 us | 86000.0000 | 19000.0000 | 1000.0000 | 523079.55 KB |
| Realm                | 2000 | 866,475.5 us | 15,391.65 us | 14,397.36 us | 838,643.0 us | 891,406.9 us | 14000.0000 |  7000.0000 |         - |  88988.85 KB |
```


#### Search Single Hash Index(string) in a loop
```
foreach (value in values)
{
    WHERE StringProperty = value
}

| Method               | N    | Mean        | Error     | StdDev    | Min         | Max         | Gen0       | Gen1       | Gen2      | Allocated |
|--------------------- |----- |------------:|----------:|----------:|------------:|------------:|-----------:|-----------:|----------:|----------:|
| ReindexerNetSpanJson | 500  |    16.55 ms |  0.318 ms |  0.804 ms |    14.92 ms |    18.41 ms |  1250.0000 |  1000.0000 |   62.5000 |   7.39 MB |
| ReindexerNet         | 500  |    24.40 ms |  0.483 ms |  0.847 ms |    23.09 ms |    26.08 ms |  2125.0000 |  1000.0000 |  250.0000 |  11.49 MB |
| ReindexerNetSql      | 500  |    27.45 ms |  0.534 ms |  0.713 ms |    26.16 ms |    28.78 ms |  2156.2500 |  1031.2500 |  281.2500 |  11.42 MB |
| LiteDbMemory         | 500  |    58.14 ms |  1.130 ms |  1.346 ms |    55.84 ms |    60.52 ms |  8250.0000 |  1250.0000 |  250.0000 |  48.34 MB |
| LiteDb               | 500  |    60.22 ms |  1.190 ms |  2.114 ms |    56.49 ms |    63.96 ms |  8250.0000 |  1250.0000 |  250.0000 |  48.34 MB |
| CachalotMemory       | 500  |   111.05 ms |  1.592 ms |  1.489 ms |   109.16 ms |   114.57 ms |  5666.6667 |  1000.0000 |         - |  34.99 MB |
| Cachalot             | 500  |   112.82 ms |  2.025 ms |  1.795 ms |   110.66 ms |   116.72 ms |  5666.6667 |  1000.0000 |         - |  34.99 MB |
| Realm                | 500  |   126.23 ms |  2.182 ms |  2.041 ms |   124.03 ms |   130.36 ms |  1666.6667 |  1000.0000 |         - |  10.28 MB |
|                      |      |             |           |           |             |             |            |            |           |           |
| ReindexerNetSpanJson | 2000 |   262.85 ms |  3.899 ms |  3.457 ms |   256.10 ms |   268.37 ms | 19000.0000 | 10000.0000 | 1000.0000 | 109.11 MB |
| ReindexerNet         | 2000 |   346.55 ms |  6.608 ms |  8.115 ms |   331.51 ms |   360.50 ms | 29000.0000 | 12000.0000 | 1000.0000 | 172.56 MB |
| ReindexerNetSql      | 2000 |   365.83 ms |  6.795 ms |  6.024 ms |   356.04 ms |   379.25 ms | 29000.0000 | 12000.0000 | 1000.0000 |  172.3 MB |
| Cachalot             | 2000 |   732.79 ms |  9.645 ms |  8.054 ms |   722.90 ms |   753.54 ms | 67000.0000 | 14000.0000 |         - | 402.43 MB |
| LiteDb               | 2000 |   733.13 ms | 13.393 ms | 12.528 ms |   713.02 ms |   761.23 ms | 95000.0000 | 16000.0000 | 1000.0000 | 566.56 MB |
| CachalotMemory       | 2000 |   737.49 ms | 13.158 ms | 10.987 ms |   716.99 ms |   758.09 ms | 67000.0000 | 14000.0000 |         - | 402.37 MB |
| LiteDbMemory         | 2000 |   743.29 ms | 12.117 ms | 11.334 ms |   729.52 ms |   763.15 ms | 95000.0000 | 16000.0000 | 1000.0000 | 566.56 MB |
| Realm                | 2000 | 1,132.95 ms | 15.543 ms | 13.779 ms | 1,102.39 ms | 1,150.81 ms | 17000.0000 |  8000.0000 |         - | 103.47 MB |
```

#### Search Single Hash Index(string) in a Parallel loop
```
Parallel.Foreach(values, value =>
{
    WHERE StringProperty = value
});

| Method               | N    | Mean      | Error     | StdDev    | Median    | Min       | Max         | Gen0        | Gen1       | Gen2      | Allocated |
|--------------------- |----- |----------:|----------:|----------:|----------:|----------:|------------:|------------:|-----------:|----------:|----------:|
| ReindexerNetSpanJson | 500  |  14.61 ms |  0.288 ms |  0.607 ms |  14.54 ms |  13.26 ms |    16.26 ms |   1500.0000 |   906.2500 |  250.0000 |   7.31 MB |
| ReindexerNet         | 500  |  17.25 ms |  0.361 ms |  1.064 ms |  17.06 ms |  15.62 ms |    19.95 ms |   2375.0000 |  1031.2500 |  343.7500 |  11.55 MB |
| ReindexerNetSql      | 500  |  19.12 ms |  0.379 ms |  0.590 ms |  19.10 ms |  17.82 ms |    20.15 ms |   2375.0000 |  1031.2500 |  343.7500 |  11.58 MB |
| Cachalot             | 500  |  31.15 ms |  0.583 ms |  1.217 ms |  30.77 ms |  29.58 ms |    34.38 ms |   6333.3333 |  1000.0000 |         - |  35.02 MB |
| CachalotMemory       | 500  |  34.07 ms |  0.670 ms |  0.688 ms |  33.96 ms |  32.71 ms |    35.41 ms |   6454.5455 |  1090.9091 |   90.9091 |  35.03 MB |
| Realm                | 500  |  36.46 ms |  0.707 ms |  0.968 ms |  36.61 ms |  34.59 ms |    38.45 ms |   2545.4545 |  1363.6364 |  363.6364 |  12.76 MB |
| LiteDbMemory         | 500  |  44.02 ms |  0.871 ms |  1.381 ms |  43.88 ms |  41.85 ms |    47.07 ms |   8555.5556 |  1555.5556 |  222.2222 |  48.39 MB |
| LiteDb               | 500  |  48.03 ms |  0.998 ms |  2.928 ms |  47.81 ms |  42.77 ms |    55.63 ms |   8555.5556 |  1555.5556 |  222.2222 |  48.39 MB |
|                      |      |           |           |           |           |           |             |             |            |           |           |
| ReindexerNetSpanJson | 2000 | 165.67 ms |  3.484 ms | 10.271 ms | 167.50 ms | 132.65 ms |   182.71 ms |  20000.0000 |  9666.6667 |         - | 109.31 MB |
| ReindexerNet         | 2000 | 184.04 ms |  3.625 ms |  3.027 ms | 184.44 ms | 175.80 ms |   187.58 ms |  32500.0000 | 12500.0000 |  500.0000 | 172.77 MB |
| ReindexerNetSql      | 2000 | 207.60 ms |  3.078 ms |  2.879 ms | 206.40 ms | 202.10 ms |   212.64 ms |  32500.0000 | 12000.0000 |  500.0000 | 172.31 MB |
| Cachalot             | 2000 | 295.91 ms |  5.819 ms |  5.443 ms | 293.94 ms | 290.09 ms |   312.91 ms |  74000.0000 | 21000.0000 |         - |  402.5 MB |
| CachalotMemory       | 2000 | 301.02 ms |  5.070 ms |  7.432 ms | 300.78 ms | 288.65 ms |   318.90 ms |  74000.0000 | 22000.0000 |         - | 402.39 MB |
| Realm                | 2000 | 304.32 ms |  5.887 ms |  8.443 ms | 305.08 ms | 286.63 ms |   317.37 ms |  21000.0000 | 10000.0000 | 1000.0000 | 113.94 MB |
| LiteDbMemory         | 2000 | 814.97 ms | 16.167 ms | 22.664 ms | 813.20 ms | 770.33 ms |   853.86 ms | 101000.0000 | 32000.0000 | 1000.0000 | 566.76 MB |
| LiteDb               | 2000 | 929.36 ms | 18.461 ms | 49.911 ms | 944.01 ms | 786.25 ms | 1,037.86 ms | 101000.0000 | 34000.0000 | 1000.0000 | 566.75 MB |
```

#### Search Multiple Hash at once
```
WHERE StringProperty IN ('abc', 'def', .... )

| Method               | N    | Mean      | Error     | StdDev    | Min       | Max       | Gen0       | Gen1       | Gen2      | Allocated |
|--------------------- |----- |----------:|----------:|----------:|----------:|----------:|-----------:|-----------:|----------:|----------:|
| ReindexerNetSpanJson | 500  |  12.59 ms |  0.195 ms |  0.173 ms |  12.21 ms |  12.89 ms |  1171.8750 |  1031.2500 |   46.8750 |    6.8 MB |
| ReindexerNet         | 500  |  22.16 ms |  0.288 ms |  0.256 ms |  21.72 ms |  22.56 ms |  2218.7500 |  1250.0000 |  375.0000 |  11.04 MB |
| ReindexerNetSql      | 500  |  23.00 ms |  0.444 ms |  0.562 ms |  21.98 ms |  24.18 ms |  2250.0000 |  1281.2500 |  406.2500 |  11.07 MB |
| Cachalot             | 500  |  28.31 ms |  0.448 ms |  0.419 ms |  27.63 ms |  28.88 ms |  3687.5000 |  1093.7500 |   62.5000 |  25.72 MB |
| CachalotMemory       | 500  |  28.79 ms |  0.558 ms |  0.801 ms |  27.65 ms |  30.18 ms |  3750.0000 |  1156.2500 |  125.0000 |  25.72 MB |
| LiteDb               | 500  |  45.21 ms |  0.890 ms |  1.059 ms |  42.74 ms |  46.99 ms |  6454.5455 |  1454.5455 |  454.5455 |  36.03 MB |
| LiteDbMemory         | 500  |  46.32 ms |  0.920 ms |  0.945 ms |  44.24 ms |  47.84 ms |  6416.6667 |  1500.0000 |  416.6667 |  36.03 MB |
| Realm                | 500  |  55.96 ms |  0.489 ms |  0.457 ms |  55.17 ms |  56.68 ms |   888.8889 |   777.7778 |         - |   5.96 MB |
|                      |      |           |           |           |           |           |            |            |           |           |
| ReindexerNetSpanJson | 2000 | 233.93 ms |  4.156 ms |  4.447 ms | 227.07 ms | 242.76 ms | 18000.0000 |  9000.0000 |  500.0000 | 107.29 MB |
| ReindexerNet         | 2000 | 325.90 ms |  6.165 ms |  7.099 ms | 316.94 ms | 339.31 ms | 29000.0000 | 14000.0000 | 1000.0000 | 170.74 MB |
| ReindexerNetSql      | 2000 | 336.87 ms |  5.890 ms |  5.784 ms | 327.78 ms | 349.58 ms | 29000.0000 | 14000.0000 | 1000.0000 | 170.87 MB |
| CachalotMemory       | 2000 | 364.17 ms |  6.460 ms |  8.843 ms | 355.53 ms | 386.43 ms | 55000.0000 | 14000.0000 |         - | 397.07 MB |
| Cachalot             | 2000 | 388.34 ms |  4.092 ms |  3.195 ms | 382.91 ms | 392.95 ms | 59000.0000 | 15000.0000 |         - | 418.65 MB |
| LiteDb               | 2000 | 687.87 ms | 13.097 ms | 12.863 ms | 670.79 ms | 715.91 ms | 87000.0000 | 18000.0000 | 1000.0000 | 515.18 MB |
| LiteDbMemory         | 2000 | 689.97 ms |  7.101 ms |  5.930 ms | 678.67 ms | 697.42 ms | 87000.0000 | 18000.0000 | 1000.0000 | 515.17 MB |
| Realm                | 2000 | 869.50 ms |  8.103 ms |  7.183 ms | 851.85 ms | 880.21 ms | 14000.0000 |  7000.0000 |         - |   86.8 MB |
```

#### Range Filtering
```
WHERE IntProperty < i
WHERE IntProperty >= i

| Method               | N    | Mean      | Error     | StdDev    | Min       | Max       | Gen0       | Gen1       | Gen2      | Allocated |
|--------------------- |----- |----------:|----------:|----------:|----------:|----------:|-----------:|-----------:|----------:|----------:|
| ReindexerNetSpanJson | 500  |  12.16 ms |  0.201 ms |  0.197 ms |  11.84 ms |  12.45 ms |  1187.5000 |  1046.8750 |   62.5000 |    6.8 MB |
| ReindexerNet         | 500  |  22.77 ms |  0.446 ms |  0.438 ms |  21.91 ms |  23.44 ms |  2218.7500 |  1250.0000 |  375.0000 |  11.04 MB |
| ReindexerNetSql      | 500  |  23.37 ms |  0.298 ms |  0.279 ms |  22.84 ms |  23.80 ms |  2218.7500 |  1250.0000 |  375.0000 |  11.04 MB |
| Cachalot             | 500  |  27.88 ms |  0.231 ms |  0.204 ms |  27.51 ms |  28.20 ms |  3718.7500 |   875.0000 |   62.5000 |  25.09 MB |
| CachalotMemory       | 500  |  28.27 ms |  0.553 ms |  0.811 ms |  27.25 ms |  30.03 ms |  3781.2500 |  1000.0000 |  125.0000 |  25.09 MB |
| LiteDb               | 500  |  42.70 ms |  0.817 ms |  0.973 ms |  39.68 ms |  44.02 ms |  5666.6667 |  1583.3333 |  416.6667 |  31.49 MB |
| LiteDbMemory         | 500  |  42.88 ms |  0.820 ms |  0.843 ms |  40.73 ms |  43.88 ms |  5666.6667 |  1583.3333 |  416.6667 |  31.49 MB |
| Realm                | 500  |  51.49 ms |  0.637 ms |  0.565 ms |  50.73 ms |  52.78 ms |   900.0000 |   800.0000 |         - |   5.88 MB |
|                      |      |           |           |           |           |           |            |            |           |           |
| ReindexerNetSpanJson | 2000 | 254.25 ms |  4.879 ms |  5.221 ms | 243.62 ms | 261.66 ms | 18666.6667 | 10000.0000 | 1000.0000 | 107.29 MB |
| ReindexerNet         | 2000 | 324.41 ms |  6.404 ms |  6.290 ms | 316.43 ms | 335.45 ms | 29000.0000 | 15000.0000 | 1000.0000 | 170.74 MB |
| ReindexerNetSql      | 2000 | 331.20 ms |  6.546 ms |  6.722 ms | 316.73 ms | 343.79 ms | 29000.0000 | 15000.0000 | 1000.0000 | 170.74 MB |
| CachalotMemory       | 2000 | 365.97 ms |  6.246 ms |  5.843 ms | 358.63 ms | 377.56 ms | 55000.0000 | 14000.0000 |         - | 387.73 MB |
| Cachalot             | 2000 | 397.83 ms |  4.906 ms |  4.349 ms | 388.91 ms | 403.81 ms | 59000.0000 | 15000.0000 |         - |  409.3 MB |
| LiteDbMemory         | 2000 | 679.96 ms | 13.090 ms | 12.857 ms | 660.74 ms | 700.36 ms | 82000.0000 | 19000.0000 | 1000.0000 |  488.8 MB |
| LiteDb               | 2000 | 702.81 ms | 13.586 ms | 18.137 ms | 675.78 ms | 740.48 ms | 82000.0000 | 19000.0000 | 1000.0000 |  488.8 MB |
| Realm                | 2000 | 849.74 ms | 10.906 ms |  9.107 ms | 831.99 ms | 864.90 ms | 14000.0000 |  7000.0000 |         - |  86.46 MB |
```

#### Array Column Filtering(Single Item IN)
```
WHERE N IN Integer_Array 
WHERE 'N' IN String_Array

| Method               | N    | Mean          | Error       | StdDev      | Min           | Max           | Gen0    | Gen1   | Gen2   | Allocated |
|--------------------- |----- |--------------:|------------:|------------:|--------------:|--------------:|--------:|-------:|-------:|----------:|
| ReindexerNetSpanJson | 500  |      8.055 us |   0.1605 us |   0.1849 us |      7.435 us |      8.313 us |  0.3204 | 0.0153 | 0.0153 |   2.09 KB |
| ReindexerNet         | 500  |      8.090 us |   0.1405 us |   0.2461 us |      7.514 us |      8.658 us |  0.3204 | 0.0153 | 0.0153 |   2.09 KB |
| ReindexerNetSql      | 500  |     13.776 us |   0.1091 us |   0.0967 us |     13.638 us |     13.943 us |  0.2289 | 0.0153 | 0.0153 |   1.49 KB |
| LiteDb               | 500  |     91.035 us |   1.8083 us |   1.7760 us |     89.065 us |     94.332 us | 19.7754 | 0.7324 |      - | 121.89 KB |
| LiteDbMemory         | 500  |     93.680 us |   1.8355 us |   3.3097 us |     88.354 us |    102.401 us | 19.7754 | 0.7324 |      - | 121.89 KB |
| CachalotMemory       | 500  |    351.965 us |   3.3731 us |   2.8167 us |    346.274 us |    355.280 us |  8.7891 | 3.9063 |      - |  58.44 KB |
| Cachalot             | 500  |    356.342 us |   3.9707 us |   3.7142 us |    352.090 us |    364.627 us |  8.7891 | 3.9063 |      - |  58.61 KB |
| Realm                | 500  |  5,417.244 us |  35.4272 us |  33.1386 us |  5,376.812 us |  5,478.724 us |       - |      - |      - |   4.16 KB |
|                      |      |               |             |             |               |               |         |        |        |           |
| ReindexerNet         | 2000 |      7.709 us |   0.1461 us |   0.1564 us |      7.326 us |      7.961 us |  0.3052 |      - |      - |   2.09 KB |
| ReindexerNetSpanJson | 2000 |      7.920 us |   0.1571 us |   0.2538 us |      7.382 us |      8.446 us |  0.3052 |      - |      - |   2.09 KB |
| ReindexerNetSql      | 2000 |     13.763 us |   0.2687 us |   0.3493 us |     13.158 us |     14.558 us |  0.2136 |      - |      - |    1.5 KB |
| LiteDb               | 2000 |     97.122 us |   1.6517 us |   1.6961 us |     95.046 us |    101.416 us | 21.7285 | 0.9766 |      - | 133.69 KB |
| LiteDbMemory         | 2000 |     98.404 us |   1.9609 us |   3.4854 us |     94.414 us |    107.592 us | 21.4844 | 0.9766 |      - | 133.69 KB |
| CachalotMemory       | 2000 |    353.776 us |   3.9691 us |   3.5185 us |    348.942 us |    360.016 us |  8.7891 | 3.9063 |      - |  58.46 KB |
| Cachalot             | 2000 |    356.124 us |   2.3146 us |   2.0518 us |    353.328 us |    360.685 us |  9.2773 | 4.3945 |      - |  58.73 KB |
| Realm                | 2000 | 74,558.110 us | 524.0327 us | 464.5413 us | 73,957.771 us | 75,606.629 us |       - |      - |      - |   4.31 KB |
```
#### Array Column Filtering(Contains, All)
```
WHERE Integer_Array CONTAINS (1,2,3,....,N) 
WHERE String_Array CONTAINS ('abc', 'def', .... )
WHERE Integer_Array ALL (1,2,3,....,N)
WHERE Integer_Array ALL (1,2,3,....,N)

| Method               | N    | Mean        | Error     | StdDev    | Median      | Min         | Max         | Gen0        | Gen1       | Gen2      | Allocated  |
|--------------------- |----- |------------:|----------:|----------:|------------:|------------:|------------:|------------:|-----------:|----------:|-----------:|
| ReindexerNetSpanJson | 500  |    34.96 ms |  0.687 ms |  0.985 ms |    35.16 ms |    33.21 ms |    37.32 ms |   2785.7143 |  1785.7143 |  571.4286 |   13.59 MB |
| ReindexerNet         | 500  |    50.72 ms |  1.011 ms |  1.633 ms |    51.32 ms |    47.55 ms |    53.48 ms |   4555.5556 |  2666.6667 |  888.8889 |   22.07 MB |
| ReindexerNetSql      | 500  |    52.80 ms |  1.042 ms |  1.878 ms |    52.67 ms |    49.17 ms |    57.03 ms |   4500.0000 |  2700.0000 |  900.0000 |   22.08 MB |
| CachalotMemory       | 500  |    84.46 ms |  2.338 ms |  6.895 ms |    87.81 ms |    72.95 ms |    96.45 ms |   8333.3333 |  2333.3333 |  333.3333 |   55.86 MB |
| Realm                | 500  |   139.72 ms |  2.548 ms |  2.384 ms |   139.51 ms |   137.35 ms |   145.64 ms |   2000.0000 |  1250.0000 |  250.0000 |   11.77 MB |
| LiteDb               | 500  |   152.37 ms |  1.929 ms |  1.710 ms |   151.71 ms |   150.52 ms |   154.80 ms |  21000.0000 |  2000.0000 |         - |  130.29 MB |
| LiteDbMemory         | 500  |   155.66 ms |  2.789 ms |  2.609 ms |   155.12 ms |   152.06 ms |   161.26 ms |  21000.0000 |  2000.0000 |         - |  130.32 MB |
| Cachalot *Err*       | 500  |          NA |        NA |        NA |          NA |          NA |          NA |          NA |         NA |        NA |         NA |
|                      |      |             |           |           |             |             |             |             |            |           |            |
| ReindexerNetSpanJson | 2000 |   845.99 ms | 15.925 ms | 17.039 ms |   842.23 ms |   821.65 ms |   887.90 ms |  65000.0000 | 33000.0000 | 2000.0000 |  377.29 MB |
| ReindexerNet         | 2000 | 1,124.47 ms | 18.851 ms | 19.358 ms | 1,117.41 ms | 1,095.95 ms | 1,168.15 ms | 102000.0000 | 51000.0000 | 2000.0000 |  599.54 MB |
| ReindexerNetSql      | 2000 | 1,132.17 ms | 22.287 ms | 20.848 ms | 1,126.05 ms | 1,099.18 ms | 1,171.55 ms | 102000.0000 | 50000.0000 | 2000.0000 |  599.55 MB |
| Realm                | 2000 | 2,268.42 ms |  4.777 ms |  3.730 ms | 2,267.71 ms | 2,263.39 ms | 2,276.80 ms |  29000.0000 | 15000.0000 | 1000.0000 |  172.91 MB |
| LiteDb               | 2000 | 3,651.93 ms | 30.890 ms | 27.383 ms | 3,642.70 ms | 3,613.94 ms | 3,713.06 ms | 335000.0000 | 65000.0000 | 2000.0000 | 1998.18 MB |
| LiteDbMemory         | 2000 | 3,707.25 ms | 14.479 ms | 11.304 ms | 3,708.58 ms | 3,687.41 ms | 3,725.33 ms | 335000.0000 | 64000.0000 | 2000.0000 | 1998.17 MB |
| Cachalot *Err*       | 2000 |          NA |        NA |        NA |          NA |          NA |          NA |          NA |         NA |        NA |         NA |
| CachalotMemory *Err* | 2000 |          NA |        NA |        NA |          NA |          NA |          NA |          NA |         NA |        NA |         NA |
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
