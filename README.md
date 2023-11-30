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
ReindexerNet  v0.3.10 (Reindexer v3.20)
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
| Method                    | N      | Mean        |  Allocated   |Error | Gen0         | Gen1        | Gen2      |
|-------------------------- |------- |------------:|-------------:|-----:|-------------:|------------:|----------:|
| ReindexerNet_Insert       | 10000  |    226.0 ms |      9.23 MB |   NA |    1000.0000 |   1000.0000 |         - |
| ReindexerNetDense_Insert  | 10000  |    225.6 ms |      9.23 MB |   NA |    1000.0000 |   1000.0000 |         - |
| Cachalot_Insert           | 10000  |    429.6 ms |    175.88 MB |   NA |   26000.0000 |   8000.0000 |         - |
| CachalotCompressed_Insert | 10000  |  1,136.9 ms |    3309.6 MB |   NA |  547000.0000 |  32000.0000 |         - |
| CachalotOnlyMemory_Insert | 10000  |    395.1 ms |    139.33 MB |   NA |   23000.0000 |   8000.0000 | 2000.0000 |
| LiteDb_Insert             | 10000  |  1,306.7 ms |   1801.26 MB |   NA |  297000.0000 |   1000.0000 |         - |
| LiteDbMemory_Insert       | 10000  |  1,177.6 ms |   1878.36 MB |   NA |  297000.0000 |   1000.0000 |         - |
| Realm_Insert              | 10000  |    344.6 ms |     43.28 MB |   NA |    7000.0000 |   2000.0000 | 1000.0000 |

| ReindexerNet_Insert       | 100000 |  2,156.9 ms |     101.3 MB |   NA |   16000.0000 |   2000.0000 | 1000.0000 |
| ReindexerNetDense_Insert  | 100000 |  1,919.7 ms |     96.81 MB |   NA |   16000.0000 |   2000.0000 | 1000.0000 |
| Cachalot_Insert           | 100000 |  3,962.2 ms |   2160.02 MB |   NA |  290000.0000 |  88000.0000 |         - |
| CachalotCompressed_Insert | 100000 | 11,544.9 ms |  33216.79 MB |   NA | 5494000.0000 | 311000.0000 |         - |
| CachalotOnlyMemory_Insert | 100000 |  3,357.4 ms |   1380.09 MB |   NA |  213000.0000 |  57000.0000 | 1000.0000 |
| LiteDb_Insert             | 100000 | 15,021.3 ms |   22970.9 MB |   NA | 3819000.0000 |  14000.0000 | 3000.0000 |
| LiteDbMemory_Insert       | 100000 | 15,389.7 ms |  23202.01 MB |   NA | 3709000.0000 |  15000.0000 | 4000.0000 |
| Realm_Insert              | 100000 |  3,750.4 ms |    432.73 MB |   NA |   73000.0000 |  49000.0000 | 1000.0000 |

| ReindexerNet_Upsert       | 10000  |    189.4 ms |      9.23 MB |   NA |    1000.0000 |           - |         - |
| ReindexerNetDense_Upsert  | 10000  |    175.2 ms |      9.23 MB |   NA |    1000.0000 |           - |         - |
| Cachalot_Upsert           | 10000  |  1,677.6 ms |    634.55 MB |   NA |  102000.0000 |   9000.0000 |         - |
| CachalotCompressed_Upsert | 10000  |  2,430.4 ms |   3779.14 MB |   NA |  625000.0000 |  32000.0000 |         - |
| CachalotOnlyMemory_Upsert | 10000  |    319.6 ms |    126.34 MB |   NA |   20000.0000 |   5000.0000 |         - |
| LiteDb_Upsert             | 10000  |    274.9 ms |    333.25 MB |   NA |   55000.0000 |           - |         - |
| LiteDbMemory_Upsert       | 10000  |    264.3 ms |    327.55 MB |   NA |   54000.0000 |           - |         - |
| Realm_Upsert              | 10000  |    219.4 ms |     43.32 MB |   NA |    7000.0000 |   6000.0000 |         - |

| ReindexerNet_Upsert       | 100000 |  1,693.0 ms |      96.8 MB |   NA |   15000.0000 |           - |         - |
| ReindexerNetDense_Upsert  | 100000 |  1,634.4 ms |    101.29 MB |   NA |   15000.0000 |           - |         - |
| Cachalot_Upsert           | 100000 |  5,983.6 ms |    2998.9 MB |   NA |  469000.0000 |  88000.0000 |         - |
| CachalotCompressed_Upsert | 100000 | 14,182.0 ms |  34496.12 MB |   NA | 5707000.0000 | 300000.0000 |         - |
| CachalotOnlyMemory_Upsert | 100000 |  3,645.0 ms |   1262.06 MB |   NA |  208000.0000 |  55000.0000 |         - |
| LiteDb_Upsert             | 100000 |  3,409.8 ms |   3641.76 MB |   NA |  608000.0000 |  14000.0000 |         - |
| LiteDbMemory_Upsert       | 100000 |  2,951.2 ms |   4032.64 MB |   NA |  673000.0000 |  14000.0000 |         - |
| Realm_Upsert              | 100000 |  2,271.6 ms |    432.61 MB |   NA |   72000.0000 |  51000.0000 |         - |
```
### Select
```
| Method                                      | N    | Mean          | Allocated    | Error        | StdDev        | Median        | Gen0       | Gen1       | Gen2      |
|-------------------------------------------- |----- |--------------:|-------------:|-------------:|--------------:|--------------:|-----------:|-----------:|----------:|
| ReindexerNet_SelectArray                    | 1000 |      20.68 us |      2.64 KB |     0.413 us |      1.158 us |      19.90 us |     0.3662 |          - |         - |
| ReindexerNetDense_SelectArray               | 1000 |      20.70 us |      2.36 KB |     0.411 us |      1.146 us |      20.31 us |     0.3662 |          - |         - |
| Cachalot_SelectArray                        | 1000 |     479.50 us |     79.77 KB |     4.007 us |      3.748 us |     481.32 us |    12.6953 |     3.9063 |         - |
| CachalotMemory_SelectArray                  | 1000 |     475.81 us |     79.92 KB |     3.601 us |      3.007 us |     475.04 us |    12.6953 |     3.9063 |         - |
| CachalotCompressed_SelectArray              | 1000 |     552.73 us |    182.23 KB |     4.218 us |      3.739 us |     551.01 us |    29.2969 |     6.8359 |         - |
| LiteDb_SelectArray                          | 1000 |     120.22 us |     85.02 KB |     1.471 us |      1.376 us |     120.81 us |    13.6719 |     0.4883 |         - |
| LiteDbMemory_SelectArray                    | 1000 |     125.85 us |     89.73 KB |     1.861 us |      1.740 us |     125.33 us |    14.6484 |     0.4883 |         - |
| Realm_SelectArray                           | 1000 |   2,382.06 us |      3.92 KB |    14.874 us |     13.185 us |   2,380.92 us |          - |          - |         - |

| ReindexerNet_SelectMultipleHash             | 1000 |     497.05 us |     85.66 KB |     2.150 us |      2.012 us |     497.04 us |    13.6719 |     0.4883 |         - |
| ReindexerNetDense_SelectMultipleHash        | 1000 |     498.64 us |     86.15 KB |     2.285 us |      2.026 us |     498.30 us |    13.6719 |     0.4883 |         - |
| Cachalot_SelectMultipleHash                 | 1000 |  63,708.75 us |  82125.33 KB | 1,133.240 us |  1,588.644 us |  63,751.00 us | 12875.0000 |  1250.0000 |  250.0000 |
| CachalotMemory_SelectMultipleHash           | 1000 |  15,273.87 us |  13686.04 KB |   258.597 us |    229.240 us |  15,196.53 us |  2250.0000 |  1156.2500 |  187.5000 |
| CachalotCompressed_SelectMultipleHash       | 1000 |  95,574.86 us | 135045.06 KB | 1,372.783 us |  1,284.102 us |  95,628.97 us | 21500.0000 |  1666.6667 |  166.6667 |
| LiteDb_SelectMultipleHash                   | 1000 |   6,087.17 us |   5348.04 KB |   121.119 us |    161.690 us |   6,024.89 us |   867.1875 |   484.3750 |         - |
| LiteDbMemory_SelectMultipleHash             | 1000 |   6,161.51 us |   5346.95 KB |    55.217 us |     48.949 us |   6,158.41 us |   867.1875 |   484.3750 |         - |
| Realm_SelectMultipleHash                    | 1000 | 102,545.73 us |    459.31 KB | 1,588.907 us |  1,486.264 us | 102,687.98 us |          - |          - |         - |

| ReindexerNet_SelectMultiplePK               | 1000 |   3,875.06 us |   1089.68 KB |    47.259 us |     44.206 us |   3,864.38 us |   171.8750 |    62.5000 |         - |
| ReindexerNetDense_SelectMultiplePK          | 1000 |   3,886.71 us |   1089.68 KB |    19.024 us |     14.853 us |   3,886.37 us |   171.8750 |    62.5000 |         - |
| Cachalot_SelectMultiplePK                   | 1000 |  15,007.71 us |  13882.34 KB |   146.743 us |    130.084 us |  14,950.01 us |  2156.2500 |   593.7500 |   46.8750 |
| CachalotMemory_SelectMultiplePK             | 1000 |  15,879.25 us |  13882.73 KB |   307.890 us |    378.117 us |  15,843.41 us |  2281.2500 |  1093.7500 |  187.5000 |
| CachalotCompressed_SelectMultiplePK         | 1000 |  33,889.38 us |  54444.86 KB |   208.973 us |    195.473 us |  33,927.05 us |  8666.6667 |   533.3333 |         - |
| LiteDb_SelectMultiplePK                     | 1000 |  11,985.18 us |  14308.54 KB |   195.025 us |    172.885 us |  11,930.28 us |  2453.1250 |   750.0000 |  125.0000 |
| LiteDbMemory_SelectMultiplePK               | 1000 |  12,314.88 us |  15005.37 KB |   232.383 us |    217.372 us |  12,356.35 us |  2531.2500 |   437.5000 |   93.7500 |
| Realm_SelectMultiplePK                      | 1000 |  10,803.55 us |    521.63 KB |   197.409 us |    256.688 us |  10,734.61 us |    78.1250 |    62.5000 |   31.2500 |

| ReindexerNet_SelectRange                    | 1000 |   2,904.81 us |    806.04 KB |    18.369 us |     15.339 us |   2,899.83 us |   128.9063 |    74.2188 |         - |
| ReindexerNetDense_SelectRange               | 1000 |   2,885.90 us |    806.11 KB |    13.943 us |     10.886 us |   2,887.42 us |   128.9063 |    74.2188 |         - |
| Cachalot_SelectRange                        | 1000 | 157,648.68 us | 216866.98 KB | 2,001.131 us |  1,965.378 us | 157,020.75 us | 34000.0000 |  2000.0000 |         - |
| CachalotMemory_SelectRange                  | 1000 |  14,858.12 us |  13518.65 KB |    37.579 us |     31.380 us |  14,864.40 us |  2265.6250 |  1031.2500 |  187.5000 |
| CachalotCompressed_SelectRange              | 1000 | 186,140.78 us | 269784.83 KB | 1,060.359 us |    939.980 us | 185,768.93 us | 42333.3333 |  1666.6667 |         - |
| LiteDb_SelectRange                          | 1000 |   6,025.36 us |   5300.22 KB |   100.969 us |     94.447 us |   5,988.67 us |   859.3750 |   445.3125 |         - |
| LiteDbMemory_SelectRange                    | 1000 |   6,002.40 us |    5291.6 KB |    25.633 us |     23.977 us |   6,004.77 us |   859.3750 |   437.5000 |         - |
| Realm_SelectRange                           | 1000 |   1,428.46 us |    295.98 KB |    28.450 us |     58.116 us |   1,433.40 us |    46.8750 |    44.9219 |   13.6719 |

| ReindexerNet_SelectSingleHash               | 1000 |  11,967.97 us |   1445.93 KB |   238.821 us |    594.747 us |  11,636.06 us |   218.7500 |   125.0000 |         - |
| ReindexerNetDense_SelectSingleHash          | 1000 |  12,080.70 us |   1589.54 KB |   241.247 us |    648.094 us |  12,174.07 us |   218.7500 |   125.0000 |         - |
| Cachalot_SelectSingleHash                   | 1000 | 566,395.05 us | 448780.73 KB | 5,077.294 us |  4,749.304 us | 565,979.10 us | 37000.0000 | 10000.0000 | 9000.0000 |
| CachalotMemory_SelectSingleHash             | 1000 | 246,873.49 us |  81734.38 KB | 3,507.760 us |  3,281.161 us | 245,253.00 us | 13000.0000 |          - |         - |
| CachalotCompressed_SelectSingleHash         | 1000 | 570,570.63 us | 456589.13 KB | 3,041.912 us |  2,845.406 us | 570,178.10 us | 37000.0000 | 10000.0000 | 8000.0000 |
| LiteDb_SelectSingleHash                     | 1000 |  46,364.72 us |  30784.98 KB |   684.551 us |    640.329 us |  46,732.16 us |  5000.0000 |   454.5455 |         - |
| LiteDbMemory_SelectSingleHash               | 1000 |  46,483.96 us |  30769.38 KB |   217.355 us |    192.679 us |  46,552.43 us |  5000.0000 |   454.5455 |         - |
| Realm_SelectSingleHash                      | 1000 | 162,260.66 us |   8913.38 KB | 1,274.319 us |  1,191.998 us | 162,132.40 us |  1333.3333 |  1000.0000 |         - |

| ReindexerNet_SelectSingleHashParallel       | 1000 |   3,678.09 us |   1528.92 KB |    71.551 us |    117.560 us |   3,666.83 us |   257.8125 |   250.0000 |  140.6250 |
| ReindexerNetDense_SelectSingleHashParallel  | 1000 |   3,642.94 us |   1529.33 KB |    72.784 us |     89.385 us |   3,650.05 us |   257.8125 |   246.0938 |  140.6250 |
| Cachalot_SelectSingleHashParallel           | 1000 | 227,967.30 us | 465054.76 KB | 6,279.472 us | 18,515.164 us | 227,367.63 us | 22333.3333 |  9000.0000 | 4333.3333 |
| CachalotMemory_SelectSingleHashParallel     | 1000 |  50,631.14 us |  81795.08 KB | 1,011.906 us |  1,718.293 us |  50,913.46 us | 14333.3333 |   444.4444 |  111.1111 |
| CachalotCompressed_SelectSingleHashParallel | 1000 | 204,663.47 us | 472611.69 KB | 4,834.912 us | 14,255.847 us | 205,433.22 us | 21000.0000 |  5666.6667 | 3000.0000 |
| LiteDb_SelectSingleHashParallel             | 1000 |  15,927.30 us |  31090.84 KB |   317.587 us |    297.071 us |  15,927.33 us |  5171.8750 |   234.3750 |   62.5000 |
| LiteDbMemory_SelectSingleHashParallel       | 1000 |  15,791.68 us |  30796.77 KB |   254.174 us |    237.754 us |  15,773.44 us |  5125.0000 |   218.7500 |   62.5000 |
| Realm_SelectSingleHashParallel              | 1000 |  50,975.27 us |  14010.35 KB | 1,015.807 us |  2,120.367 us |  50,673.50 us |  2454.5455 |   636.3636 |   90.9091 |

| ReindexerNet_SelectSinglePK                 | 1000 |  11,959.25 us |   1620.32 KB |   239.046 us |    608.448 us |  11,569.75 us |   218.7500 |   156.2500 |         - |
| ReindexerNetDense_SelectSinglePK            | 1000 |  12,112.73 us |   1620.37 KB |   242.242 us |    638.163 us |  11,900.51 us |   218.7500 |   125.0000 |         - |
| Cachalot_SelectSinglePK                     | 1000 |  29,179.90 us |   20951.7 KB |   216.095 us |    191.562 us |  29,119.11 us |  3406.2500 |   343.7500 |         - |
| CachalotMemory_SelectSinglePK               | 1000 |  28,379.18 us |  20951.67 KB |   215.965 us |    191.447 us |  28,360.19 us |  3406.2500 |   343.7500 |         - |
| CachalotCompressed_SelectSinglePK           | 1000 |  50,546.28 us |  61438.63 KB |   592.111 us |    553.861 us |  50,537.03 us | 10000.0000 |   300.0000 |         - |
| LiteDb_SelectSinglePK                       | 1000 |  41,583.49 us |  38202.01 KB |   339.008 us |    317.108 us |  41,673.85 us |  6166.6667 |   416.6667 |         - |
| LiteDbMemory_SelectSinglePK                 | 1000 |  43,173.16 us |  40096.84 KB |   792.935 us |    973.795 us |  43,125.02 us |  6583.3333 |   583.3333 |   83.3333 |
| Realm_SelectSinglePK                        | 1000 |   2,239.86 us |    516.26 KB |    44.106 us |     99.555 us |   2,251.89 us |    82.0313 |    78.1250 |   19.5313 |
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
