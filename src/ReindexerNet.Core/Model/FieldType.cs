using System;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReindexerNet
{
    /// <summary>
    /// Reindexer Field types
    /// </summary>
    [DataContract]
    [JsonConverter(typeof(FieldTypeConverter))]
    public enum FieldType
    {
        /// <summary>
        /// 32 bit Integer
        /// </summary>
        [EnumMember(Value = FieldTypeConverter.IntValueStr)]
        Int,
        /// <summary>
        /// 64 bit Integer
        /// </summary>
        [EnumMember(Value = FieldTypeConverter.Int64ValueStr)]
        Int64,
        /// <summary>
        /// Double
        /// </summary>
        [EnumMember(Value = FieldTypeConverter.DoubleValueStr)]
        Double,
        /// <summary>
        /// String
        /// </summary>
        [EnumMember(Value = FieldTypeConverter.StringValueStr)]
        String,
        /// <summary>
        /// Boolean
        /// </summary>
        [EnumMember(Value = FieldTypeConverter.BoolValueStr)]
        Bool,
        /// <summary>
        /// Composite object
        /// </summary>
        [EnumMember(Value = FieldTypeConverter.ComposieValueStr)]
        Composite
    }

    /// <summary>
    /// FieldType Json Converter
    /// </summary>
    public sealed class FieldTypeConverter : JsonConverter<FieldType>
    {
        internal const string IntValueStr = "int";
        internal const string Int64ValueStr = "int64";
        internal const string DoubleValueStr = "double";
        internal const string StringValueStr = "string";
        internal const string BoolValueStr = "bool";
        internal const string ComposieValueStr = "composite";

        /// <inheritdoc/>
        public override FieldType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.GetString())
            {
                case IntValueStr:
                    return FieldType.Int;
                case Int64ValueStr:
                    return FieldType.Int64;
                case DoubleValueStr:
                    return FieldType.Double;
                case StringValueStr:
                    return FieldType.String;
                case BoolValueStr:
                    return FieldType.Bool;
                case ComposieValueStr:
                    return FieldType.Composite;
                default:
                    throw new JsonException("Unknown FieldType value");
            }
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, FieldType value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case FieldType.Int:
                    writer.WriteStringValue(IntValueStr);
                    break;
                case FieldType.Int64:
                    writer.WriteStringValue(Int64ValueStr);
                    break;
                case FieldType.Double:
                    writer.WriteStringValue(DoubleValueStr);
                    break;
                case FieldType.String:
                    writer.WriteStringValue(StringValueStr);
                    break;
                case FieldType.Bool:
                    writer.WriteStringValue(BoolValueStr);
                    break;
                case FieldType.Composite:
                    writer.WriteStringValue(ComposieValueStr);
                    break;
                default:
                    writer.WriteNullValue();
                    break;
            }
        }
    }
}
