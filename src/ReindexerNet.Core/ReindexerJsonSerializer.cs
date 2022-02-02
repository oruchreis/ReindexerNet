using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReindexerNet
{
    public class ReindexerJsonSerializer : IReindexerSerializer
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
#if NET5_0_OR_GREATER
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
#else
            IgnoreNullValues = true,
#endif
            
        };

        public SerializerType Type => SerializerType.Json;
        public T Deserialize<T>(ReadOnlySpan<byte> bytes)
        {
            return JsonSerializer.Deserialize<T>(bytes, _jsonSerializerOptions);
        }

        public ReadOnlySpan<byte> Serialize<T>(T item)
        {
            return JsonSerializer.SerializeToUtf8Bytes(item, _jsonSerializerOptions);
        }
    }
}
