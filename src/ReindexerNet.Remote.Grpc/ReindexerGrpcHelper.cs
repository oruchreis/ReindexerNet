using Google.Protobuf;
using Grpc.Core;
using Reindexer.Grpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using QueryResultsOptions = Reindexer.Grpc.QueryResultsResponse.Types.QueryResultsOptions;

namespace ReindexerNet.Remote.Grpc;

internal static class GrpcHelper
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull           
    };

    internal static
#if !NETSTANDARD2_1_OR_GREATER && !NET5_0_OR_GREATER 
        unsafe
#endif
        T DeserializeJson<T>(this ReadOnlySpan<char> chars)
    {
#if NET6_0_OR_GREATER
        return JsonSerializer.Deserialize<T>(chars, _jsonSerializerOptions);
#else            
#if NETSTANDARD2_1 || NET5_0
        var span = new Span<byte>(new byte[chars.Length*2]);
        var byteCount = Encoding.UTF8.GetBytes(chars, span);
#else
        var bytes = new byte[chars.Length * 2];
        var span = bytes.AsSpan();
        int byteCount;
        fixed (char* charsPtr = chars)
        fixed (byte* bytesPtr = span)
        {
            byteCount = Encoding.UTF8.GetBytes(charsPtr, chars.Length, bytesPtr, bytes.Length);
        }
#endif
        return JsonSerializer.Deserialize<T>(span.Slice(0, byteCount), _jsonSerializerOptions);
#endif
    }

    internal static void HandleErrorResponse(this ErrorResponse errorResponse)
    {
        if (errorResponse.Code != ErrorResponse.Types.ErrorCode.ErrCodeOk)
            throw new ReindexerException((int)errorResponse.Code, errorResponse.What);
    }

    internal static async Task<int> HandleErrorResponseAsync(this IAsyncStreamReader<ErrorResponse> rspStream, CancellationToken cancellationToken = default)
    {
        var handledCount = 0;
#if !NETSTANDARD2_0 && !NET472
        await foreach (var item in rspStream.ReadAllAsync(cancellationToken))
        {
#else
        while (await rspStream.MoveNext(cancellationToken))
        {
            var item = rspStream.Current;
#endif                
            HandleErrorResponse(item);
            handledCount++;
        }
        return handledCount;
    }

    internal static async IAsyncEnumerable<(QueryItemsOf<TResult>, QueryResultsOptions)> HandleResponseAsync<TResult>(this AsyncServerStreamingCall<QueryResultsResponse> streamCall,
        IReindexerSerializer serializer, [EnumeratorCancellation]CancellationToken cancellationToken = default)
    {
        QueryResultsOptions resultOpt = null;
        try
        {
#if !NETSTANDARD2_0 && !NET472
            await foreach (var item in streamCall.ResponseStream.ReadAllAsync(cancellationToken))
            {
#else
            while (await streamCall.ResponseStream.MoveNext(cancellationToken))
            {
                var item = streamCall.ResponseStream.Current;
#endif
                resultOpt ??= item.Options;
                HandleErrorResponse(item.ErrorResponse);
                yield return (
                    serializer.Deserialize<QueryItemsOf<TResult>>(item.Data.Span),
                    resultOpt);
            }
        }
        finally
        {
            streamCall.Dispose();
        }
    }

    public static string ToEnumString<T>(this T type)
        where T: Enum
    {
        var enumType = typeof (T);
        var name = Enum.GetName(enumType, type);
        var enumMemberAttribute = ((EnumMemberAttribute[])enumType.GetField(name).GetCustomAttributes(typeof(EnumMemberAttribute), true)).Single();
        return enumMemberAttribute.Value;
    }

    public static ModifyMode ToModifyMode(this ItemModifyMode itemModifyMode)
    {
        return itemModifyMode switch
        {
            ItemModifyMode.Upsert => ModifyMode.Upsert,
            ItemModifyMode.Update => ModifyMode.Update,
            ItemModifyMode.Delete => ModifyMode.Delete,
            ItemModifyMode.Insert => ModifyMode.Insert,
            _ => throw new NotSupportedException()
        };
    }
}
