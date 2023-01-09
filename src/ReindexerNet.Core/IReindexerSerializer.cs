using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace ReindexerNet
{
    /// <summary>
    /// You can change default serialization behavior of ReindexerNet by implementing this interface and register it to the client. Default serializer (<see cref="ReindexerJsonSerializer"/>) uses System.Text.Json in background.
    /// </summary>
    public interface IReindexerSerializer
    {
        /// <summary>
        /// Serialized data type that supported by Reindexer.
        /// </summary>
        public SerializerType Type { get; }

        /// <summary>
        /// Serialize the <paramref name="item"/> as byte array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public ReadOnlySpan<byte> Serialize<T>(T item);

        /// <summary>
        /// Deserialize byte array <paramref name="bytes"/> to <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public T Deserialize<T>(ReadOnlySpan<byte> bytes);
    }

    /// <summary>
    /// Serialized data type that supported by Reindexer.
    /// </summary>
    public enum SerializerType
    {
        /// <summary>
        /// Json
        /// </summary>
        Json = 0,

        /// <summary>
        /// CJson
        /// </summary>
        Cjson = 1,

        /// <summary>
        /// Msgpack
        /// </summary>
        Msgpack = 2,

        /// <summary>
        /// Protobuf
        /// </summary>
        Protobuf = 3,
    }
}
