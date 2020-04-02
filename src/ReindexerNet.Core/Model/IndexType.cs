using System;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReindexerNet
{
    [JsonConverter(typeof(IndexTypeConverter))]
    [DataContract]
    public enum IndexType
    {
        [EnumMember(Value = IndexTypeConverter.HashValueStr)]
        Hash,
        [EnumMember(Value = IndexTypeConverter.TreeValueStr)]
        Tree,
        [EnumMember(Value = IndexTypeConverter.TextValueStr)]
        Text,
        [EnumMember(Value = IndexTypeConverter.ColumnIndexValueStr)]
        ColumnIndex
    }

    public sealed class IndexTypeConverter : JsonConverter<IndexType>
    {
        internal const string HashValueStr = "hash";
        internal const string TreeValueStr = "tree";
        internal const string TextValueStr = "text";
        internal const string ColumnIndexValueStr = "-";

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
