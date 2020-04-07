using System;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReindexerNet
{
    /// <summary>
    /// Index Type
    /// </summary>
    [JsonConverter(typeof(IndexTypeConverter))]
    [DataContract]
    public enum IndexType
    {
        /// <summary>
        /// Hash Index for hash table lookup
        /// </summary>
        [EnumMember(Value = IndexTypeConverter.HashValueStr)]
        Hash,
        /// <summary>
        /// Tree Index for search trees.
        /// </summary>
        [EnumMember(Value = IndexTypeConverter.TreeValueStr)]
        Tree,
        /// <summary>
        /// Fulltext Index
        /// </summary>
        [EnumMember(Value = IndexTypeConverter.TextValueStr)]
        Text,
        /// <summary>
        /// Column index
        /// </summary>
        [EnumMember(Value = IndexTypeConverter.ColumnIndexValueStr)]
        ColumnIndex
    }

    /// <summary>
    /// Index type json converter.
    /// </summary>
    public sealed class IndexTypeConverter : JsonConverter<IndexType>
    {
        internal const string HashValueStr = "hash";
        internal const string TreeValueStr = "tree";
        internal const string TextValueStr = "text";
        internal const string ColumnIndexValueStr = "-";

        /// <inheritdoc/>
        public override IndexType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.GetString())
            {
                case HashValueStr:
                    return IndexType.Hash;
                case TreeValueStr:
                    return IndexType.Tree;
                case TextValueStr:
                    return IndexType.Text;
                case ColumnIndexValueStr:
                    return IndexType.ColumnIndex;
                default:
                    throw new JsonException("Unknown IndexType value");
            }
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, IndexType value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case IndexType.Hash:
                    writer.WriteStringValue(HashValueStr);
                    break;
                case IndexType.Tree:
                    writer.WriteStringValue(TreeValueStr);
                    break;
                case IndexType.Text:
                    writer.WriteStringValue(TextValueStr);
                    break;
                case IndexType.ColumnIndex:
                    writer.WriteStringValue(ColumnIndexValueStr);
                    break;
                default:
                    writer.WriteNullValue();
                    break;
            }
        }
    }
}
