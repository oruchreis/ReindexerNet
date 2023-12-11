# ReindexerNet

[![Embedded  Nuget](https://img.shields.io/nuget/v/ReindexerNet.Embedded?label=Embedded&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Embedded)
[![Remote.Grpc  Nuget](https://img.shields.io/nuget/v/ReindexerNet.Remote.Grpc?label=Remote.Grpc&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Remote.Grpc)
[![Core Nuget](https://img.shields.io/nuget/v/ReindexerNet.Core?label=Core&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Core)

[![Build, Test, Package](https://github.com/oruchreis/ReindexerNet/actions/workflows/build.yml/badge.svg)](https://github.com/oruchreis/ReindexerNet/actions/workflows/build.yml)
[![Unix Test](https://github.com/oruchreis/ReindexerNet/actions/workflows/unix-test.yml/badge.svg)](https://github.com/oruchreis/ReindexerNet/actions/workflows/unix-test.yml)
[![Windows Test](https://github.com/oruchreis/ReindexerNet/actions/workflows/windows-test.yml/badge.svg)](https://github.com/oruchreis/ReindexerNet/actions/workflows/windows-test.yml)

ReindexerNet is a .NET binding(builtin & builtinserver) and connector(Grpc, ~~OpenApi~~) for embeddable in-memory document db [Reindexer](https://github.com/Restream/reindexer). 
We are using ReindexerNET in production environments for a long time, and even if all unit tests are passed, we don't encourge you to use in a prod environment without testing. So please test in your environment before using.

If you have any questions about Reindexer, please use [main page](https://github.com/Restream/reindexer) of Reindexer. Feel free to report issues and contribute about **ReindexerNet**. You can check [change logs here](CHANGELOG.md).

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
 - Or you can add all native packages like this:

```xml
<ItemGroup>
  <PackageReference 
    Include="ReindexerNet.Embedded.Native.AlpineLinux-x64" Version="0.3.10.3200"
    Condition="$([MSBuild]::IsOSPlatform('Linux')) and ($(RuntimeIdentifier.StartsWith('linux-musl')) or $(RuntimeIdentifier.StartsWith('alpine')))" />
  <PackageReference
    Include="ReindexerNet.Embedded.Native.Linux-x64" Version="0.3.10.3200"
    Condition="$([MSBuild]::IsOSPlatform('Linux')) and !($(RuntimeIdentifier.StartsWith('linux-musl')) or $(RuntimeIdentifier.StartsWith('alpine')))" />
  <PackageReference 
    Include="ReindexerNet.Embedded.Native.Osx-x64" Version="0.3.10.3200"
    Condition="$([MSBuild]::IsOSPlatform('OSX'))"  />
  <PackageReference 
    Include="ReindexerNet.Embedded.Native.Win-x64" Version="0.3.10.3200"
    Condition="$([MSBuild]::IsOSPlatform('Windows'))" />
  <PackageReference 
    Include="ReindexerNet.Embedded.Native.Win-x86" Version="0.3.10.3200" 
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


## ReindexerNet.Embedded Benchmarks and Comparations
```
ReindexerNet  v0.4.1 (Reindexer v3.20)
Cachalot      v2.0.8
LiteDB        v5.0.7
Realm.NET     v11.6.1

BenchmarkDotNet v0.13.11, Windows 10 (10.0.19045.3636/22H2/2022Update)
Intel Core i7-8700K CPU 3.70GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.100-rc.2.23502.2
  [Host]     : .NET 7.0.13 (7.0.1323.51816), X64 RyuJIT AVX2
```
### Insert Benchmarks
> #### Insert (Without controlling existance)

> #### Upsert (Update if exists, otherwise insert)

### Select Benchmarks
> #### Single Primary Key(Guid) in a loop
> ```
> foreach (id in ids)
> {
>     WHERE Id = id
> }
> ```

| Method                                        | Categories                                    | N    | Mean             | Error          | StdDev         | Gen0        | Gen1       | Gen2      | Allocated     |
|---------------------------------------------- |---------------------------------------------- |----- |-----------------:|---------------:|---------------:|------------:|-----------:|----------:|--------------:|
| Cachalot_SelectSinglePK                       | SelectSinglePK,Cachalot                       | 1000 |   282,785.050 us |  4,029.8328 us |    623.6211 us |  30500.0000 |  4000.0000 |         - |  187511.56 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| CachalotCompressed_SelectSinglePK             | SelectSinglePK,CachalotCompressed             | 1000 |   348,094.320 us | 31,687.8269 us |  8,229.2280 us |  35000.0000 |  5000.0000 |         - |  220151.91 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| CachalotMemory_SelectSinglePK                 | SelectSinglePK,CachalotMemory                 | 1000 |   295,441.588 us | 18,397.1767 us |  2,846.9835 us |  30500.0000 |  4000.0000 |         - |  187511.17 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| LiteDb_SelectSinglePK                         | SelectSinglePK,LiteDb                         | 1000 |   257,152.880 us | 91,902.9020 us | 23,866.8918 us |  35000.0000 |  5000.0000 |         - |   216416.3 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| LiteDbMemory_SelectSinglePK                   | SelectSinglePK,LiteDbMemory                   | 1000 |   258,560.020 us | 20,879.9150 us |  5,422.4476 us |  35500.0000 |  4500.0000 |  500.0000 |  215364.41 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| Realm_SelectSinglePK                          | SelectSinglePK,Realm                          | 1000 |     2,481.698 us |    572.0812 us |    148.5677 us |     82.0313 |    78.1250 |         - |     516.25 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| ReindexerNet_SelectSinglePK                   | SelectSinglePK,ReindexerNet                   | 1000 |     3,781.301 us |    100.0469 us |     15.4824 us |    125.0000 |    31.2500 |    3.9063 |      814.7 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| ReindexerNetSpanJson_SelectSinglePK           | SelectSinglePK,ReindexerNetSpanJson           | 1000 |     3,865.852 us |     69.9667 us |     10.8274 us |    121.0938 |    27.3438 |         - |     805.36 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| ReindexerNetSql_SelectSinglePK                | SelectSinglePK,ReindexerNetSql                | 1000 |   118,826.668 us | 12,686.3854 us |  3,294.6140 us |   6800.0000 |  3600.0000 |  600.0000 |   38843.18 KB |


> #### Multiple Primary Key(Guid) at once
> ```
> WHERE Id IN (id_1,id_2,id_3,...,id_N)
> ```
> | Method                                      | Categories                                  | N    | Mean          | Allocated    | Error         | StdDev        | Median        | Gen0       | Gen1       | Gen2      |
> |-------------------------------------------- |-------------------------------------------- |----- |--------------:|-------------:|--------------:|--------------:|--------------:|-----------:|-----------:|----------:|
| Cachalot_SelectMultiplePK                     | SelectMultiplePK,Cachalot                     | 1000 |   278,948.588 us |  3,533.1024 us |    546.7515 us |  28000.0000 |  4500.0000 |         - |  187879.49 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| CachalotCompressed_SelectMultiplePK           | SelectMultiplePK,CachalotCompressed           | 1000 |   327,700.900 us |  2,259.7422 us |    586.8479 us |  34000.0000 |  4500.0000 |         - |  217708.79 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| CachalotMemory_SelectMultiplePK               | SelectMultiplePK,CachalotMemory               | 1000 |   317,860.340 us | 17,609.6170 us |  4,573.1616 us |  29000.0000 |  6000.0000 | 1000.0000 |  187881.41 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| LiteDb_SelectMultiplePK                       | SelectMultiplePK,LiteDb                       | 1000 |   218,759.327 us |  9,240.6277 us |  2,399.7617 us |  31666.6667 |  5000.0000 |  666.6667 |  190726.18 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| LiteDbMemory_SelectMultiplePK                 | SelectMultiplePK,LiteDbMemory                 | 1000 |   232,426.967 us | 40,450.5974 us | 10,504.8917 us |  32000.0000 |  5000.0000 |  666.6667 |  192243.95 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| Realm_SelectMultiplePK                        | SelectMultiplePK,Realm                        | 1000 |    10,461.847 us |    416.8394 us |     64.5064 us |     62.5000 |    46.8750 |         - |     521.59 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| ReindexerNet_SelectMultiplePK                 | SelectMultiplePK,ReindexerNet                 | 1000 |       290.542 us |      5.0227 us |      1.3044 us |      8.7891 |     0.4883 |         - |      55.64 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| ReindexerNetSpanJson_SelectMultiplePK         | SelectMultiplePK,ReindexerNetSpanJson         | 1000 |       292.723 us |      6.2390 us |      1.6203 us |      8.7891 |     0.4883 |         - |      55.64 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| ReindexerNetSql_SelectMultiplePK              | SelectMultiplePK,ReindexerNetSql              | 1000 |   103,247.260 us |  5,065.8543 us |  1,315.5863 us |   6833.3333 |  3666.6667 |  666.6667 |   38479.89 KB |


> #### Single Hash in a loop
> ```
> foreach (value in values)
> {
>     WHERE StringProperty = value
> }
> ```
> | Method                                      | Categories                                  | N    | Mean          | Allocated    | Error         | StdDev        | Median        | Gen0       | Gen1       | Gen2      |
> |-------------------------------------------- |-------------------------------------------- |----- |--------------:|-------------:|--------------:|--------------:|--------------:|-----------:|-----------:|----------:|
| Cachalot_SelectSingleHash                     | SelectSingleHash,Cachalot                     | 1000 |   495,281.120 us |  7,065.3711 us |  1,834.8544 us |  33000.0000 |  4000.0000 |         - |  203251.58 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| CachalotCompressed_SelectSingleHash           | SelectSingleHash,CachalotCompressed           | 1000 |   499,396.525 us | 11,637.6496 us |  1,800.9392 us |  33000.0000 |  4000.0000 |         - |  203259.39 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| CachalotMemory_SelectSingleHash               | SelectSingleHash,CachalotMemory               | 1000 |   494,665.950 us |  4,578.2394 us |    708.4876 us |  33000.0000 |  4000.0000 |         - |  202883.13 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| LiteDb_SelectSingleHash                       | SelectSingleHash,LiteDb                       | 1000 |   258,610.400 us |  8,568.2965 us |  2,225.1594 us |  36000.0000 |  4500.0000 |  500.0000 |  218752.75 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| LiteDbMemory_SelectSingleHash                 | SelectSingleHash,LiteDbMemory                 | 1000 |   256,300.775 us |  8,398.4181 us |  1,299.6645 us |  35500.0000 |  4500.0000 |  500.0000 |  217346.21 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| Realm_SelectSingleHash                        | SelectSingleHash,Realm                        | 1000 |   136,388.090 us |  4,077.2886 us |  1,058.8589 us |   1250.0000 |  1000.0000 |         - |    8911.91 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| ReindexerNet_SelectSingleHash                 | SelectSingleHash,ReindexerNet                 | 1000 |   112,529.608 us | 13,604.9193 us |  3,533.1543 us |   6800.0000 |  3600.0000 |  600.0000 |   38930.61 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| ReindexerNetSpanJson_SelectSingleHash         | SelectSingleHash,ReindexerNetSpanJson         | 1000 |    82,073.017 us |  7,637.4217 us |  1,983.4142 us |   4000.0000 |  2166.6667 |  500.0000 |   22327.14 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| ReindexerNetSql_SelectSingleHash              | SelectSingleHash,ReindexerNetSql              | 1000 |   119,879.556 us | 15,616.2359 us |  4,055.4869 us |   6800.0000 |  3600.0000 |  600.0000 |   38834.99 KB |


> #### Single Hash in a Parallel loop
> ```
> Parallel.Foreach(values, value =>
> {
>     WHERE StringProperty = value
> });
> ```
> | Method                                      | Categories                                  | N    | Mean          | Allocated    | Error         | StdDev        | Median        | Gen0       | Gen1       | Gen2      |
> |-------------------------------------------- |-------------------------------------------- |----- |--------------:|-------------:|--------------:|--------------:|--------------:|-----------:|-----------:|----------:|
| Cachalot_SelectSingleHashParallel             | SelectSingleHashParallel,Cachalot             | 1000 |   223,647.250 us | 11,039.6038 us |  1,708.3909 us |  35000.0000 |  7000.0000 |         - |  203285.11 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| CachalotCompressed_SelectSingleHashParallel   | SelectSingleHashParallel,CachalotCompressed   | 1000 |   220,399.560 us | 14,033.4679 us |  3,644.4471 us |  35333.3333 |  7333.3333 |         - |  203269.54 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| CachalotMemory_SelectSingleHashParallel       | SelectSingleHashParallel,CachalotMemory       | 1000 |   263,773.960 us | 27,919.2992 us |  7,250.5533 us |  35000.0000 |  8000.0000 |         - |  202945.35 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| LiteDb_SelectSingleHashParallel               | SelectSingleHashParallel,LiteDb               | 1000 |   221,579.473 us | 19,197.2353 us |  4,985.4610 us |  37000.0000 |  7666.6667 |  333.3333 |  216827.26 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| LiteDbMemory_SelectSingleHashParallel         | SelectSingleHashParallel,LiteDbMemory         | 1000 |   228,644.927 us | 29,638.3253 us |  7,696.9790 us |  37000.0000 |  7666.6667 |  333.3333 |  217929.17 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| Realm_SelectSingleHashParallel                | SelectSingleHashParallel,Realm                | 1000 |    49,549.095 us | 16,808.3345 us |  2,601.1084 us |   2300.0000 |   500.0000 |         - |   13985.24 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| ReindexerNet_SelectSingleHashParallel         | SelectSingleHashParallel,ReindexerNet         | 1000 |    55,591.460 us |  9,269.8340 us |  2,407.3465 us |   7222.2222 |  3222.2222 |  222.2222 |   39038.02 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| ReindexerNetSpanJson_SelectSingleHashParallel | SelectSingleHashParallel,ReindexerNetSpanJson | 1000 |    39,962.429 us |  5,217.6222 us |    807.4328 us |   4000.0000 |  2000.0000 |   71.4286 |   22432.14 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| ReindexerNetSql_SelectSingleHashParallel      | SelectSingleHashParallel,ReindexerNetSql      | 1000 |    59,198.408 us |  8,624.5871 us |  1,334.6644 us |   7222.2222 |  3222.2222 |  222.2222 |   38848.56 KB |


> #### Multiple Hash at once
> ```
> WHERE StringProperty IN ('abc', 'def', .... )
> ```
> | Method                                      | Categories                                  | N    | Mean          | Allocated    | Error         | StdDev        | Median        | Gen0       | Gen1       | Gen2      |
> |-------------------------------------------- |-------------------------------------------- |----- |--------------:|-------------:|--------------:|--------------:|--------------:|-----------:|-----------:|----------:|
| Cachalot_SelectMultipleHash                   | SelectMultipleHash,Cachalot                   | 1000 | 1,484,532.475 us | 37,109.4888 us |  5,742.7345 us | 152000.0000 | 22000.0000 |         - |  997263.42 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| CachalotCompressed_SelectMultipleHash         | SelectMultipleHash,CachalotCompressed         | 1000 | 1,832,603.760 us | 11,512.5847 us |  2,989.7817 us | 186000.0000 | 26000.0000 |         - | 1206270.39 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| CachalotMemory_SelectMultipleHash             | SelectMultipleHash,CachalotMemory             | 1000 |   313,764.380 us | 11,475.7239 us |  2,980.2091 us |  29000.0000 |  6000.0000 | 1000.0000 |  187772.65 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| LiteDb_SelectMultipleHash                     | SelectMultipleHash,LiteDb                     | 1000 |   226,051.750 us |  5,536.0051 us |    856.7029 us |  31666.6667 |  5000.0000 |  666.6667 |  191589.16 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| LiteDbMemory_SelectMultipleHash               | SelectMultipleHash,LiteDbMemory               | 1000 |   227,047.733 us | 11,887.6348 us |  3,087.1810 us |  31666.6667 |  5333.3333 |  666.6667 |  191670.53 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| Realm_SelectMultipleHash                      | SelectMultipleHash,Realm                      | 1000 |    11,876.545 us |    951.3057 us |    247.0511 us |     62.5000 |    46.8750 |         - |     464.74 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| ReindexerNet_SelectMultipleHash               | SelectMultipleHash,ReindexerNet               | 1000 |   102,228.168 us |  9,141.4401 us |  2,374.0030 us |   6800.0000 |  3600.0000 |  600.0000 |   38203.88 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| ReindexerNetSpanJson_SelectMultipleHash       | SelectMultipleHash,ReindexerNetSpanJson       | 1000 |    70,505.400 us |  5,345.3941 us |    827.2056 us |   4000.0000 |  2285.7143 |  571.4286 |   21600.45 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| ReindexerNetSql_SelectMultipleHash            | SelectMultipleHash,ReindexerNetSql            | 1000 |   103,781.868 us |  2,847.8748 us |    739.5840 us |   6800.0000 |  3600.0000 |  600.0000 |   38296.22 KB |


> #### Range Filtering
> ```
> WHERE IntProperty < i
> WHERE IntProperty >= i
> ```
> | Method                                      | Categories                                  | N    | Mean          | Allocated    | Error         | StdDev        | Median        | Gen0       | Gen1       | Gen2      |
> |-------------------------------------------- |-------------------------------------------- |----- |--------------:|-------------:|--------------:|--------------:|--------------:|-----------:|-----------:|----------:|
| Cachalot_SelectRange                          | SelectRange,Cachalot                          | 1000 | 2,688,260.175 us | 85,898.9574 us | 13,292.9587 us | 276000.0000 | 40000.0000 |         - | 1802101.88 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| CachalotCompressed_SelectRange                | SelectRange,CachalotCompressed                | 1000 | 3,099,263.400 us | 67,706.5567 us | 10,477.6646 us | 310000.0000 | 45000.0000 |         - | 2011108.75 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| CachalotMemory_SelectRange                    | SelectRange,CachalotMemory                    | 1000 |   313,817.340 us | 24,892.1160 us |  6,464.4035 us |  28000.0000 |  6000.0000 | 1000.0000 |  185407.89 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| LiteDb_SelectRange                            | SelectRange,LiteDb                            | 1000 |   215,661.108 us |  4,626.9026 us |    716.0183 us |  30333.3333 |  4666.6667 |  666.6667 |  182618.11 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| LiteDbMemory_SelectRange                      | SelectRange,LiteDbMemory                      | 1000 |   221,388.847 us |  6,773.8405 us |  1,759.1449 us |  30333.3333 |  4666.6667 |  666.6667 |  182624.05 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| Realm_SelectRange                             | SelectRange,Realm                             | 1000 |     1,573.357 us |    450.8583 us |    117.0865 us |     46.8750 |    44.9219 |         - |     295.97 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| ReindexerNet_SelectRange                      | SelectRange,ReindexerNet                      | 1000 |   106,644.072 us | 12,920.3608 us |  3,355.3767 us |   6800.0000 |  3600.0000 |  600.0000 |   38196.92 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| ReindexerNetSpanJson_SelectRange              | SelectRange,ReindexerNetSpanJson              | 1000 |    69,709.810 us |  7,478.6567 us |  1,942.1834 us |   4125.0000 |  2375.0000 |  625.0000 |   21593.97 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| ReindexerNetSql_SelectRange                   | SelectRange,ReindexerNetSql                   | 1000 |   102,760.600 us |  6,674.4567 us |  1,733.3352 us |   6800.0000 |  3600.0000 |  600.0000 |   38196.14 KB |

> #### Array Column Filtering(Single Item IN)
> ```
> WHERE N IN Integer_Array 
> WHERE 'N' IN String_Array
> ```
> | Method                                      | Categories                                  | N    | Mean          | Allocated    | Error         | StdDev        | Median        | Gen0       | Gen1       | Gen2      |
> |-------------------------------------------- |-------------------------------------------- |----- |--------------:|-------------:|--------------:|--------------:|--------------:|-----------:|-----------:|----------:|
| Cachalot_SelectArraySingle                    | SelectArraySingle,Cachalot                    | 1000 |       424.875 us |     32.1625 us |      8.3525 us |      9.2773 |     4.3945 |         - |      58.58 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| CachalotCompressed_SelectArraySingle          | SelectArraySingle,CachalotCompressed          | 1000 |       422.408 us |      3.3954 us |      0.8818 us |      9.2773 |     4.3945 |         - |       58.8 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| CachalotMemory_SelectArraySingle              | SelectArraySingle,CachalotMemory              | 1000 |       437.087 us |     15.0301 us |      2.3259 us |      9.2773 |     4.3945 |         - |      58.58 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| LiteDb_SelectArraySingle                      | SelectArraySingle,LiteDb                      | 1000 |       115.684 us |      2.0888 us |      0.5425 us |     17.7002 |     0.7324 |         - |      108.7 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| LiteDbMemory_SelectArraySingle                | SelectArraySingle,LiteDbMemory                | 1000 |       110.742 us |      2.0057 us |      0.5209 us |     15.9912 |     0.4883 |         - |         98 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| Realm_SelectArraySingle                       | SelectArraySingle,Realm                       | 1000 |    25,987.981 us |    450.1629 us |    116.9059 us |           - |          - |         - |       3.16 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| ReindexerNet_SelectArraySingle                | SelectArraySingle,ReindexerNet                | 1000 |         7.537 us |      0.2348 us |      0.0363 us |      0.2670 |     0.0076 |    0.0076 |       1.74 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| ReindexerNetSpanJson_SelectArraySingle        | SelectArraySingle,ReindexerNetSpanJson        | 1000 |         7.823 us |      0.7236 us |      0.1879 us |      0.2594 |          - |         - |       1.74 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| ReindexerNetSql_SelectArraySingle             | SelectArraySingle,ReindexerNetSql             | 1000 |        11.432 us |      0.3705 us |      0.0962 us |      0.1678 |          - |         - |       1.23 KB |


> #### Array Column Filtering(Contains, All)
> ```
> WHERE Integer_Array CONTAINS (1,2,3,....,N) 
> WHERE String_Array CONTAINS ('abc', 'def', .... )
> WHERE Integer_Array ALL (1,2,3,....,N)
> WHERE Integer_Array ALL (1,2,3,....,N)
> ```
> | Method                                      | Categories                                  | N    | Mean          | Allocated    | Error         | StdDev        | Median        | Gen0       | Gen1       | Gen2      |
> |-------------------------------------------- |-------------------------------------------- |----- |--------------:|-------------:|--------------:|--------------:|--------------:|-----------:|-----------:|----------:|
| Cachalot_SelectArrayMultiple                  | SelectArrayMultiple,Cachalot                  | 1000 |   652,729.720 us | 66,142.3826 us | 17,176.9667 us |  58000.0000 |  9000.0000 |         - |  392384.68 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| CachalotCompressed_SelectArrayMultiple        | SelectArrayMultiple,CachalotCompressed        | 1000 | 1,318,515.020 us | 12,199.7214 us |  3,168.2289 us | 129000.0000 | 19000.0000 |         - |  825509.77 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| CachalotMemory_SelectArrayMultiple            | SelectArrayMultiple,CachalotMemory            | 1000 |   648,278.780 us | 31,402.6038 us |  8,155.1565 us |  59000.0000 | 10000.0000 | 1000.0000 |  392384.44 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| LiteDb_SelectArrayMultiple                    | SelectArrayMultiple,LiteDb                    | 1000 | 1,221,826.140 us | 12,537.1964 us |  3,255.8701 us | 122000.0000 | 26000.0000 | 1000.0000 |  745676.34 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| LiteDbMemory_SelectArrayMultiple              | SelectArrayMultiple,LiteDbMemory              | 1000 | 1,216,860.860 us | 31,751.1468 us |  8,245.6720 us | 122000.0000 | 28000.0000 | 1000.0000 |  745677.55 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| Realm_SelectArrayMultiple                     | SelectArrayMultiple,Realm                     | 1000 |   480,289.680 us | 10,716.3749 us |  2,783.0085 us |           - |          - |         - |     589.57 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| ReindexerNet_SelectArrayMultiple              | SelectArrayMultiple,ReindexerNet              | 1000 |   204,457.533 us | 21,413.5619 us |  5,561.0340 us |  13666.6667 |  7333.3333 | 1000.0000 |   78959.62 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| ReindexerNetSpanJson_SelectArrayMultiple      | SelectArrayMultiple,ReindexerNetSpanJson      | 1000 |   135,851.400 us | 16,115.7813 us |  4,185.2172 us |   8500.0000 |  4750.0000 | 1250.0000 |   44808.57 KB |
|                                               |                                               |      |                  |                |                |             |            |           |               |
| ReindexerNetSql_SelectArrayMultiple           | SelectArrayMultiple,ReindexerNetSql           | 1000 |   206,962.613 us | 14,102.3376 us |  3,662.3323 us |  13666.6667 |  7333.3333 | 1000.0000 |    78961.6 KB |


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
