using System;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReindexerNet
{
    [DataContract]
    public enum FieldType
    {
        [EnumMember(Value = "int")]
        Int, 
        [EnumMember(Value = "int64")]
        Int64, 
        [EnumMember(Value = "double")]
        Double, 
        [EnumMember(Value = "string")]
        String, 
        [EnumMember(Value = "bool")]
        Bool, 
        [EnumMember(Value = "composite")]
        Composite
    }

    public sealed class FieldTypeConverter : JsonConverter<FieldType>
    {
        internal const string IntValueStr = "int";
        internal const string Int64ValueStr = "int64";
        internal const string DoubleValueStr = "double";
        internal const string StringValueStr = "string";
        internal const string BoolValueStr = "bool";
        internal const string ComposieValueStr = "composite";

        public override FieldType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch ( reader.GetString())
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
