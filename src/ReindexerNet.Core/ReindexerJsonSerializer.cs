using System;
using System.Buffers.Text;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReindexerNet;

/// <summary>
/// Default serializer for reindexer operations. This serializer uses <see cref="JsonSerializer"/> in background.
/// </summary>
public class ReindexerJsonSerializer : IReindexerSerializer
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        Converters = { new DoubleConverter() }
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

class DoubleConverter : JsonConverter<double>
{
    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetDouble();
#if NET6_0_OR_GREATER
    static readonly StandardFormat f = StandardFormat.Parse("F");
    static readonly StandardFormat g = StandardFormat.Parse("G");

    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        Span<byte> buffer = stackalloc byte[30];
        var format = value % 1 == 0 ? f : g;
        Utf8Formatter.TryFormat(value, buffer, out var written, format);        
        writer.WriteRawValue(buffer[..written]);
    }
#else
    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        //https://stackoverflow.com/questions/66309150/how-do-i-write-a-float-like-82-0-with-the-0-intact-using-utf8jsonwriter
        // 2^49 is the largest power of 2 with fewer than 15 decimal digits.  
        // From experimentation casting to decimal does not lose precision for these values.
        const double MaxPreciselyRepresentedIntValue = (1L << 49);

        bool written = false;
        // For performance check to see that the incoming double is an integer
        if ((value % 1) == 0)
        {
            if (value < MaxPreciselyRepresentedIntValue && value > -MaxPreciselyRepresentedIntValue)
            {
                writer.WriteNumberValue(0.0m + (decimal)value);
                written = true;
            }
            else
            {
                // Directly casting these larger values from double to decimal seems to result in precision loss, as noted in  https://stackoverflow.com/q/7453900/3744182
                // And also: https://learn.microsoft.com/en-us/dotnet/api/system.convert.todecimal?redirectedfrom=MSDN&view=net-5.0#System_Convert_ToDecimal_System_Double_
                // > The Decimal value returned by Convert.ToDecimal(Double) contains a maximum of 15 significant digits.
                // So if we want the full G17 precision we have to format and parse ourselves.
                //
                // Utf8Formatter and Utf8Parser should give the best performance for this, but, according to MSFT, 
                // on frameworks earlier than .NET Core 3.0 Utf8Formatter does not produce roundtrippable strings.  For details see
                // https://github.com/dotnet/runtime/blob/eb03e0f7bc396736c7ac59cf8f135d7c632860dd/src/libraries/System.Text.Json/src/System/Text/Json/Writer/Utf8JsonWriter.WriteValues.Double.cs#L103
                // You may want format to string and parse in earlier frameworks -- or just use JsonDocument on these earlier versions.
                Span<byte> utf8bytes = stackalloc byte[32];
                if (Utf8Formatter.TryFormat(value, utf8bytes.Slice(0, utf8bytes.Length - 2), out var bytesWritten)
                    && IsInteger(utf8bytes, bytesWritten))
                {
                    utf8bytes[bytesWritten++] = (byte)'.';
                    utf8bytes[bytesWritten++] = (byte)'0';
                    if (Utf8Parser.TryParse(utf8bytes.Slice(0, bytesWritten), out decimal d, out var _))
                    {
                        writer.WriteNumberValue(d);
                        written = true;
                    }
                }
            }
        }
        if (!written)
        {
            #if NETSTANDARD2_0 || NET472
            static unsafe bool IsFinite(double d)
            {
                long bits = BitConverter.DoubleToInt64Bits(d);
                return (bits & 0x7FFFFFFFFFFFFFFF) < 0x7FF0000000000000;
            }
            var isFinite = IsFinite(value);
            #else
            var isFinite = double.IsFinite(value);
            #endif
            if (isFinite)
                writer.WriteNumberValue(value);
            else
                // Utf8JsonWriter does not take into account JsonSerializerOptions.NumberHandling so we have to make a recursive call to serialize
                JsonSerializer.Serialize(writer, value, new JsonSerializerOptions { NumberHandling = options.NumberHandling });
        }

        static bool IsInteger(Span<byte> utf8bytes, int bytesWritten)
        {
            if (bytesWritten <= 0)
                return false;
            var start = utf8bytes[0] == '-' ? 1 : 0;
            for (var i = start; i < bytesWritten; i++)
                if (!(utf8bytes[i] >= '0' && utf8bytes[i] <= '9'))
                    return false;
            return start < bytesWritten;
        }
    }
#endif    
}

