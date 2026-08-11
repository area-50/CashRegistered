using System.Text.Json;
using System.Text.Json.Serialization;

namespace CashRegisterApi.Converters;

public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Garante que qualquer data recebida no JSON vire UTC absoluto
        return DateTime.Parse(reader.GetString()!).ToUniversalTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Garante a presença do "Z" na formatação de saída para o Front end
        writer.WriteStringValue(value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}
