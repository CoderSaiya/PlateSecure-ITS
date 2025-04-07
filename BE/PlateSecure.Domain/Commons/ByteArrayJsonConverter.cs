using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlateSecure.Domain.Commons;

public class ByteArrayJsonConverter : JsonConverter<byte[]>
{
    public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var base64 = reader.GetString();
        return string.IsNullOrEmpty(base64)
            ? Array.Empty<byte>()
            : Convert.FromBase64String(base64);
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Convert.ToBase64String(value));
    }
}