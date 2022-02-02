# ReindexerNet

[![Embedded  Nuget](https://img.shields.io/nuget/v/ReindexerNet.Embedded?label=Embedded&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Embedded)
[![Remote.Grpc  Nuget](https://img.shields.io/nuget/v/ReindexerNet.Remote.Grpc?label=Remote.Grpc&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Remote.Grpc)
[![Core Nuget](https://img.shields.io/nuget/v/ReindexerNet.Core?label=Core&color=1182c2&style=flat-square&logo=nuget)](https://www.nuget.org/packages/ReindexerNet.Core)
![Build, Test](https://github.com/oruchreis/ReindexerNet/workflows/Build,%20Test,%20Package/badge.svg)

![win-x64, win-x86 Test](https://github.com/oruchreis/ReindexerNet/workflows/win-x64,%20win-x86%20Test/badge.svg)
![linux-x64 Test](https://github.com/oruchreis/ReindexerNet/workflows/linux-x64%20Test/badge.svg)
![osx-x64 Test](https://github.com/oruchreis/ReindexerNet/workflows/osx-x64%20Test/badge.svg)

ReindexerNet is a .NET binding(builtin & builtinserver) and connector(Grpc, ~~OpenApi~~) for embeddable in-memory document db [Reindexer](https://github.com/Restream/reindexer). 
It is still in alpha state and there are a lot of works to do and .net apis are changing constantly. Even though we are using in production for a long time, and even if all unit tests are passed, we don't encourge you to use in a prod environment. So please test in your environment before using.

If you have any questions about Reindexer, please use [main page](https://github.com/Restream/reindexer) of Reindexer. Feel free to report issues and contribute about **ReindexerNet**. You can check [change logs here](CHANGELOG.md).

## ReindexerNet.Embedded
This package contains embedded Reindexer implementation(**builtin**) and embedded server implementation(**builtinserver**). You can use this for memory caching in .net without using .net heap. 
If you use .net heap for memory caching, you will eventually encounter long GC pauses because of enlarged .net heap and LOH. And if you can't use remote caching because of performance considerations, you have to use native memory for caching. 

There are a few native memory cache solutions, and we choose Reindexer over them because of its performance. You can check Reindexer's benchmark results in their [main page](https://github.com/Restream/reindexer). Also you can use server implementation to run Reindexer server in your .net application.

### Native Library Dependencies
This package supports `linux-x64`, `osx-x64`, `win-x64` and `win-x86` runtimes. We built Reindexer as a native library from source to use Reindexer c/c++ api via p/invoke. By doing this, we aimed at decreasing the native dependencies as much as possible, and compiled dependencies into the native library as static linking. These are minimum native dependencies for the libraries:
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


## ReindexerNet.Remote.Grpc
This package contains Grpc client to use Reindexer server over grpc protocol. It uses new [grpc for dotnet](https://github.com/grpc/grpc-dotnet) library by Microsoft for .Net Core 3.1, .Net 5.0 and up. And it uses legacy [grpc-core](https://github.com/grpc/grpc/tree/master/src/csharp) library for .Net Framework and .Net Standard 2.0 because of http/2 support.

## ReindexerNet.Core
This package contains base types and common models for Reindexer and .net packages. You can use the models in this package as OpenApi/Rest models. Every model in this package has `DataContract` and `JsonPropertyName` attributes to support valid json serialization for Reindexer rest api.


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