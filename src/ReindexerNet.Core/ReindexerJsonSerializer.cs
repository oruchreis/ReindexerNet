using System;
using System.Text.Json;

namespace ReindexerNet;

/// <summary>
/// Default serializer for reindexer operations. This serializer uses <see cref="JsonSerializer"/> in background.
/// </summary>
public class ReindexerJsonSerializer : IReindexerSerializer
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <inheritdoc />
    public SerializerType Type => SerializerType.Json;

    /// <inheritdoc />
    public T Deserialize<T>(ReadOnlySpan<byte> bytes)
    {
        return JsonSerializer.Deserialize<T>(bytes, _jsonSerializerOptions);
    }

    /// <inheritdoc />
    public ReadOnlySpan<byte> Serialize<T>(T item)
    {
        return JsonSerializer.SerializeToUtf8Bytes(item, _jsonSerializerOptions);
    }
}
