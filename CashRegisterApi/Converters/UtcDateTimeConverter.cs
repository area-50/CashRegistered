using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CashRegisterApi.Converters;

public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateStr = reader.GetString();
        if (string.IsNullOrWhiteSpace(dateStr))
            return default;

        if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
        {
            return parsed;
        }

        return DateTime.SpecifyKind(DateTime.Parse(dateStr), DateTimeKind.Utc);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Garante a presença do "Z" na formatação de saída para o Frontend
        var utcValue = value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        writer.WriteStringValue(utcValue.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}
