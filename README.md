# ReindexerNet

[![Embedded  Nuget](https://img.shields.io/nuget/v/ReindexerNet.Embedded?label=Embedded&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Embedded)
[![Remote.Grpc  Nuget](https://img.shields.io/nuget/v/ReindexerNet.Remote.Grpc?label=Remote.Grpc&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Remote.Grpc)
[![Core Nuget](https://img.shields.io/nuget/v/ReindexerNet.Core?label=Core&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Core)

[![Build, Test, Package](https://github.com/oruchreis/ReindexerNet/actions/workflows/build.yml/badge.svg)](https://github.com/oruchreis/ReindexerNet/actions/workflows/build.yml)
[![Unix Test](https://github.com/oruchreis/ReindexerNet/actions/workflows/unix-test.yml/badge.svg)](https://github.com/oruchreis/ReindexerNet/actions/workflows/unix-test.yml)
[![Windows Test](https://github.com/oruchreis/ReindexerNet/actions/workflows/windows-test.yml/badge.svg)](https://github.com/oruchreis/ReindexerNet/actions/workflows/windows-test.yml)

ReindexerNet is a .NET binding(builtin & builtinserver) and connector(Grpc, ~~OpenApi~~) for embeddable in-memory document db [Reindexer](https://github.com/Restream/reindexer). 
We are using ReindexerNET in production environments for a long time, and even if all unit tests are passed, we don't encourge you to use in a prod environment. So please test in your environment before using.

If you have any questions about Reindexer, please use [main page](https://github.com/Restream/reindexer) of Reindexer. Feel free to report issues and contribute about **ReindexerNet**. You can check [change logs here](CHANGELOG.md).

## Sample Usage:
```csharp
private static readonly IReindexerClient _rxClient;

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

public async Task InitClientAsync()
{
    DbPath = Path.Combine(Path.GetTempPath(), "ReindexerDB");
    _rxClient = new ReindexerEmbedded(DbPath);
    await _rxClient.ConnectAsync();	
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
ReindexerNet  v0.3.8 (Reindexer v3.12)
Cachalot      v2.0.8
LiteDB        v5.0.7
Realm.NET     v11.6.0

BenchmarkDotNet v0.13.10, Windows 10 (10.0.19045.3636/22H2/2022Update)
Intel Core i7-8700K CPU 3.70GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.100-rc.2.23502.2
  [Host]     : .NET 7.0.13 (7.0.1323.51816), X64 RyuJIT AVX2
```
### Insert
```
| Method                    | N      | Mean        | Allocated   | Error | Gen0         | Gen1        | Gen2      |
|-------------------------- |------- |------------:|------------:|------:|-------------:|------------:|----------:|
| ReindexerNet_Insert       | 10000  |    243.6 ms |     9.23 MB |    NA |    1000.0000 |   1000.0000 |         - |
| ReindexerNetDense_Insert  | 10000  |    236.9 ms |     9.23 MB |    NA |    1000.0000 |   1000.0000 |         - |
| Cachalot_Insert           | 10000  |    412.3 ms |   176.34 MB |    NA |   26000.0000 |   8000.0000 |         - |
| CachalotCompressed_Insert | 10000  |  1,128.2 ms |  3309.96 MB |    NA |  547000.0000 |  32000.0000 |         - |
| CachalotOnlyMemory_Insert | 10000  |    340.0 ms |   139.34 MB |    NA |   23000.0000 |   8000.0000 | 2000.0000 |
| LiteDb_Insert             | 10000  |  1,241.1 ms |  1828.16 MB |    NA |  302000.0000 |   1000.0000 |         - |
| LiteDbMemory_Insert       | 10000  |  1,208.0 ms |  1896.66 MB |    NA |  300000.0000 |   1000.0000 |         - |
| Realm_Insert              | 10000  |    368.8 ms |    43.39 MB |    NA |    7000.0000 |   2000.0000 | 1000.0000 |

| ReindexerNet_Insert       | 100000 |  2,088.5 ms |    101.3 MB |    NA |   16000.0000 |   2000.0000 | 1000.0000 |
| ReindexerNetDense_Insert  | 100000 |  1,871.8 ms |    96.81 MB |    NA |   16000.0000 |   2000.0000 | 1000.0000 |
| Cachalot_Insert           | 100000 |  4,342.8 ms |  2157.43 MB |    NA |  290000.0000 |  91000.0000 |         - |
| CachalotCompressed_Insert | 100000 | 11,597.4 ms | 33467.36 MB |    NA | 5494000.0000 | 309000.0000 |         - |
| CachalotOnlyMemory_Insert | 100000 |  3,390.1 ms |  1379.79 MB |    NA |  213000.0000 |  57000.0000 | 1000.0000 |
| LiteDb_Insert             | 100000 | 15,070.6 ms | 23185.48 MB |    NA | 3854000.0000 |  14000.0000 | 3000.0000 |
| LiteDbMemory_Insert       | 100000 | 14,407.5 ms | 23804.96 MB |    NA | 3809000.0000 |  15000.0000 | 4000.0000 |
| Realm_Insert              | 100000 |  3,393.9 ms |   432.73 MB |    NA |   73000.0000 |  49000.0000 | 1000.0000 |

| ReindexerNet_Upsert       | 10000  |    173.7 ms |     9.23 MB |    NA |    1000.0000 |           - |         - |
| ReindexerNetDense_Upsert  | 10000  |    180.8 ms |     9.23 MB |    NA |    1000.0000 |           - |         - |
| Cachalot_Upsert           | 10000  |  1,617.7 ms |   630.15 MB |    NA |  101000.0000 |   9000.0000 |         - |
| CachalotCompressed_Upsert | 10000  |  2,259.9 ms |  3778.51 MB |    NA |  625000.0000 |  32000.0000 |         - |
| CachalotOnlyMemory_Upsert | 10000  |    276.0 ms |   126.34 MB |    NA |   20000.0000 |   5000.0000 |         - |
| LiteDb_Upsert             | 10000  |    296.7 ms |    330.3 MB |    NA |   55000.0000 |           - |         - |
| LiteDbMemory_Upsert       | 10000  |    261.2 ms |    325.1 MB |    NA |   54000.0000 |           - |         - |
| Realm_Upsert              | 10000  |    223.8 ms |    43.38 MB |    NA |    7000.0000 |   5000.0000 |         - |

| ReindexerNet_Upsert       | 100000 |  1,544.8 ms |   101.29 MB |    NA |   15000.0000 |           - |         - |
| ReindexerNetDense_Upsert  | 100000 |  1,560.1 ms |   101.29 MB |    NA |   15000.0000 |           - |         - |
| Cachalot_Upsert           | 100000 |  5,659.6 ms |  2998.96 MB |    NA |  469000.0000 |  89000.0000 |         - |
| CachalotCompressed_Upsert | 100000 | 14,087.9 ms | 34498.03 MB |    NA | 5707000.0000 | 300000.0000 |         - |
| CachalotOnlyMemory_Upsert | 100000 |  3,071.0 ms |  1262.06 MB |    NA |  208000.0000 |  55000.0000 |         - |
| LiteDb_Upsert             | 100000 |  3,210.3 ms |  3727.06 MB |    NA |  622000.0000 |  14000.0000 |         - |
| LiteDbMemory_Upsert       | 100000 |  2,951.9 ms |  3900.06 MB |    NA |  651000.0000 |  14000.0000 |         - |
| Realm_Upsert              | 100000 |  2,251.7 ms |   432.61 MB |    NA |   72000.0000 |  51000.0000 |         - |
```
### Select
```
| Method                                      | N    | Mean          | Allocated    | Error        | StdDev        | Median        | Gen0       | Gen1       | Gen2      |
|-------------------------------------------- |----- |--------------:|-------------:|-------------:|--------------:|--------------:|-----------:|-----------:|----------:|
| ReindexerNet_SelectArray                    | 1000 |      21.36 us |      2.36 KB |     0.436 us |      1.278 us |      21.48 us |     0.3662 |          - |         - |
| ReindexerNetDense_SelectArray               | 1000 |      20.52 us |      2.64 KB |     0.410 us |      1.128 us |      19.84 us |     0.3662 |          - |         - |
| Cachalot_SelectArray                        | 1000 |     476.35 us |     79.77 KB |     7.290 us |      6.087 us |     474.31 us |    12.6953 |     3.9063 |         - |
| CachalotMemory_SelectArray                  | 1000 |     484.11 us |     80.36 KB |     3.754 us |      3.512 us |     485.03 us |    12.6953 |     3.9063 |         - |
| CachalotCompressed_SelectArray              | 1000 |     554.73 us |    182.24 KB |     4.627 us |      4.328 us |     555.04 us |    29.2969 |     6.8359 |         - |
| LiteDb_SelectArray                          | 1000 |     126.44 us |     94.43 KB |     1.039 us |      0.972 us |     126.58 us |    15.3809 |     0.4883 |         - |
| LiteDbMemory_SelectArray                    | 1000 |     127.33 us |     94.91 KB |     1.105 us |      1.033 us |     127.35 us |    15.3809 |     0.4883 |         - |
| Realm_SelectArray                           | 1000 |   2,353.79 us |      3.92 KB |     8.071 us |      7.550 us |   2,352.11 us |          - |          - |         - |

| ReindexerNet_SelectMultipleHash             | 1000 |     493.92 us |     86.15 KB |     1.318 us |      1.169 us |     494.00 us |    13.6719 |     0.4883 |         - |
| ReindexerNetDense_SelectMultipleHash        | 1000 |     493.40 us |     85.66 KB |     1.351 us |      1.128 us |     493.69 us |    13.6719 |     0.4883 |         - |
| Cachalot_SelectMultipleHash                 | 1000 |  63,317.76 us |  82125.33 KB | 1,165.042 us |    972.863 us |  63,521.21 us | 12875.0000 |  1250.0000 |  250.0000 |
| CachalotMemory_SelectMultipleHash           | 1000 |  15,789.65 us |  13686.23 KB |   124.884 us |    104.283 us |  15,801.51 us |  2250.0000 |  1156.2500 |  187.5000 |
| CachalotCompressed_SelectMultipleHash       | 1000 |  95,465.03 us | 135045.06 KB | 1,636.390 us |  1,530.680 us |  94,857.00 us | 21500.0000 |  1666.6667 |  166.6667 |
| LiteDb_SelectMultipleHash                   | 1000 |   6,170.50 us |   5346.89 KB |   122.958 us |    146.373 us |   6,108.70 us |   867.1875 |   484.3750 |         - |
| LiteDbMemory_SelectMultipleHash             | 1000 |   6,138.40 us |   5347.01 KB |    25.067 us |     22.221 us |   6,129.20 us |   867.1875 |   484.3750 |         - |
| Realm_SelectMultipleHash                    | 1000 | 102,890.16 us |    459.31 KB | 2,009.205 us |  2,063.306 us | 102,343.24 us |          - |          - |         - |

| ReindexerNet_SelectMultiplePK               | 1000 |   3,882.27 us |   1089.72 KB |    30.675 us |     25.615 us |   3,882.45 us |   171.8750 |    62.5000 |         - |
| ReindexerNetDense_SelectMultiplePK          | 1000 |   3,938.59 us |   1089.71 KB |    76.446 us |     67.767 us |   3,909.33 us |   171.8750 |    62.5000 |         - |
| Cachalot_SelectMultiplePK                   | 1000 |  15,202.62 us |  13882.33 KB |   217.455 us |    203.408 us |  15,203.71 us |  2156.2500 |   843.7500 |   46.8750 |
| CachalotMemory_SelectMultiplePK             | 1000 |  16,073.41 us |  13883.09 KB |   314.232 us |    419.491 us |  15,861.93 us |  2281.2500 |  1093.7500 |  187.5000 |
| CachalotCompressed_SelectMultiplePK         | 1000 |  33,612.55 us |  54444.87 KB |   330.608 us |    309.251 us |  33,661.75 us |  8666.6667 |   533.3333 |         - |
| LiteDb_SelectMultiplePK                     | 1000 |  12,170.54 us |  14671.22 KB |   214.303 us |    220.074 us |  12,156.95 us |  2500.0000 |   562.5000 |  109.3750 |
| LiteDbMemory_SelectMultiplePK               | 1000 |  11,783.52 us |  14042.17 KB |   233.842 us |    229.664 us |  11,776.13 us |  2390.6250 |   406.2500 |  109.3750 |
| Realm_SelectMultiplePK                      | 1000 |  10,348.39 us |    521.63 KB |   155.305 us |    145.273 us |  10,312.47 us |    78.1250 |    62.5000 |   31.2500 |

| ReindexerNet_SelectRange                    | 1000 |   2,913.91 us |    806.08 KB |    16.506 us |     15.440 us |   2,915.91 us |   128.9063 |    74.2188 |         - |
| ReindexerNetDense_SelectRange               | 1000 |   2,934.49 us |     806.1 KB |    27.858 us |     26.059 us |   2,929.04 us |   128.9063 |    74.2188 |         - |
| Cachalot_SelectRange                        | 1000 | 159,140.19 us | 216865.28 KB | 1,876.735 us |  1,567.158 us | 159,708.62 us | 34250.0000 |  1500.0000 |  250.0000 |
| CachalotMemory_SelectRange                  | 1000 |  15,463.43 us |  13518.58 KB |   258.035 us |    241.366 us |  15,472.01 us |  2265.6250 |  1093.7500 |  187.5000 |
| CachalotCompressed_SelectRange              | 1000 | 185,259.12 us | 269784.83 KB | 1,430.498 us |  1,338.089 us | 185,616.33 us | 42333.3333 |  1666.6667 |         - |
| LiteDb_SelectRange                          | 1000 |   5,731.51 us |   5289.38 KB |    79.386 us |    100.398 us |   5,696.71 us |   859.3750 |   445.3125 |         - |
| LiteDbMemory_SelectRange                    | 1000 |   5,861.13 us |   5295.59 KB |    38.904 us |     36.391 us |   5,859.37 us |   859.3750 |   453.1250 |         - |
| Realm_SelectRange                           | 1000 |   1,520.93 us |    295.98 KB |    30.146 us |     58.081 us |   1,511.48 us |    48.8281 |    46.8750 |   11.7188 |

| ReindexerNet_SelectSingleHash               | 1000 |  12,284.97 us |   1589.54 KB |   244.064 us |    540.829 us |  11,969.56 us |   218.7500 |   125.0000 |         - |
| ReindexerNetDense_SelectSingleHash          | 1000 |  12,333.23 us |   1445.93 KB |   235.116 us |    606.909 us |  12,459.30 us |   218.7500 |   125.0000 |         - |
| Cachalot_SelectSingleHash                   | 1000 | 572,763.56 us |  448778.3 KB | 4,550.538 us |  4,033.934 us | 573,883.35 us | 37000.0000 | 10000.0000 | 9000.0000 |
| CachalotMemory_SelectSingleHash             | 1000 | 245,945.03 us |  81734.38 KB | 2,615.589 us |  2,446.623 us | 246,316.10 us | 13000.0000 |          - |         - |
| CachalotCompressed_SelectSingleHash         | 1000 | 571,683.23 us | 456591.53 KB | 2,835.159 us |  2,652.009 us | 571,604.40 us | 37000.0000 |  9000.0000 | 8000.0000 |
| LiteDb_SelectSingleHash                     | 1000 |  46,782.00 us |  30878.76 KB |   447.413 us |    418.511 us |  46,788.32 us |  5000.0000 |   454.5455 |         - |
| LiteDbMemory_SelectSingleHash               | 1000 |  46,509.21 us |  30800.63 KB |   407.438 us |    381.118 us |  46,425.81 us |  5000.0000 |   454.5455 |         - |
| Realm_SelectSingleHash                      | 1000 | 164,277.72 us |   8913.38 KB | 1,240.257 us |  1,160.137 us | 164,195.87 us |  1333.3333 |  1000.0000 |         - |

| ReindexerNet_SelectSingleHashParallel       | 1000 |   3,768.44 us |   1509.89 KB |    75.287 us |    165.256 us |   3,743.78 us |   250.0000 |   242.1875 |  148.4375 |
| ReindexerNetDense_SelectSingleHashParallel  | 1000 |   3,707.60 us |   1529.42 KB |    72.397 us |    126.798 us |   3,712.63 us |   257.8125 |   242.1875 |  125.0000 |
| Cachalot_SelectSingleHashParallel           | 1000 | 225,878.42 us | 464924.95 KB | 7,894.844 us | 23,278.127 us | 224,913.40 us | 21333.3333 |  6666.6667 | 3333.3333 |
| CachalotMemory_SelectSingleHashParallel     | 1000 |  53,300.67 us |   81838.9 KB | 1,063.448 us |  2,123.820 us |  53,274.35 us | 14200.0000 |   700.0000 |         - |
| CachalotCompressed_SelectSingleHashParallel | 1000 | 217,690.28 us | 472814.15 KB | 6,726.759 us | 19,728.419 us | 213,321.60 us | 20333.3333 |  6333.3333 | 2333.3333 |
| LiteDb_SelectSingleHashParallel             | 1000 |  15,928.59 us |   30545.7 KB |   314.975 us |    337.020 us |  15,859.54 us |  5062.5000 |   187.5000 |   46.8750 |
| LiteDbMemory_SelectSingleHashParallel       | 1000 |  15,836.94 us |  30780.82 KB |   307.393 us |    399.697 us |  15,847.45 us |  5125.0000 |   218.7500 |   62.5000 |
| Realm_SelectSingleHashParallel              | 1000 |  54,464.45 us |  14017.54 KB | 1,187.194 us |  3,500.469 us |  53,847.35 us |  2400.0000 |   600.0000 |  100.0000 |

| ReindexerNet_SelectSinglePK                 | 1000 |  12,674.95 us |   1620.36 KB |   252.947 us |    652.936 us |  12,917.24 us |   218.7500 |   156.2500 |         - |
| ReindexerNetDense_SelectSinglePK            | 1000 |  12,661.83 us |   1476.67 KB |   251.582 us |    557.488 us |  12,420.92 us |   218.7500 |   156.2500 |         - |
| Cachalot_SelectSinglePK                     | 1000 |  29,198.15 us |   20951.7 KB |   266.865 us |    236.569 us |  29,127.77 us |  3406.2500 |   343.7500 |         - |
| CachalotMemory_SelectSinglePK               | 1000 |  28,729.80 us |  20951.67 KB |   278.447 us |    246.836 us |  28,674.28 us |  3406.2500 |   343.7500 |         - |
| CachalotCompressed_SelectSinglePK           | 1000 |  50,075.32 us |  61438.34 KB |   985.687 us |  1,619.512 us |  49,313.44 us | 10000.0000 |   300.0000 |         - |
| LiteDb_SelectSinglePK                       | 1000 |  42,946.50 us |  38741.01 KB |   425.930 us |    398.416 us |  42,852.97 us |  6250.0000 |   416.6667 |         - |
| LiteDbMemory_SelectSinglePK                 | 1000 |  41,516.13 us |  38077.19 KB |   397.980 us |    372.271 us |  41,358.75 us |  6166.6667 |   416.6667 |         - |
| Realm_SelectSinglePK                        | 1000 |   2,369.90 us |    516.26 KB |    47.229 us |    125.244 us |   2,370.56 us |    82.0313 |    78.1250 |   23.4375 |
```


## To Do List
 - [x] RestApi models [![Core  Nuget](https://img.shields.io/nuget/v/ReindexerNet.Core?label=Core&color=1182c2&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Core)
 - [x] Embedded mode binding (Builtin) [![Embedded  Nuget](https://img.shields.io/nuget/v/ReindexerNet.Embedded?label=Embedded&color=1182c2&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Embedded)
 - [x] Embedded Server mode binding (Builtin-server) [![Embedded  Nuget](https://img.shields.io/nuget/v/ReindexerNet.Embedded?label=Embedded&color=1182c2&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Embedded)
 - [x] Embedded bindings for win-x64, linux-x64, osx-x64 and win-x86
 - [x] GRPC connector for remote servers (Standalone server/Grpc) [![Remote.Grpc  Nuget](https://img.shields.io/nuget/v/ReindexerNet.Remote.Grpc?label=Remote.Grpc&color=1182c2&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Remote.Grpc)
 - [ ] Query Interface
 - [ ] CJson Serializer
 - [ ] Dsl Query Builder
 - [ ] OpenApi/Json connector for remote servers
 - [ ] CProto connector for remote servers
 - [ ] Documentation 
