using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNet
{
    public interface IReindexerSerializer
    {
        public SerializerType Type { get; }

        public ReadOnlySpan<byte> Serialize<T>(T item);
        public T Deserialize<T>(ReadOnlySpan<byte> bytes);
    }

    public enum SerializerType
    {
        Json = 0,
        Cjson = 1,
        Msgpack = 2,
        Protobuf = 3,
    }
}
