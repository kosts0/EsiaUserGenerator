using System.Text.Json;

namespace EsiaUserGenerator.Utils.JsonConverter;
using System.Text.Json.Serialization;
using System.Globalization;

   public class CustomDateTimeConverter : JsonConverter<DateTimeOffset?>
{
    private const string _format = "dd.MM.yyyy";

    public CustomDateTimeConverter()
    {
    }

    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Read the string value from the JSON
        string dateString = reader.GetString();
        // Parse the string into a DateTime using the specified format
        return DateTime.ParseExact(dateString, _format, CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
    {
        // Format the DateTime value into a string using the specified format
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString(_format, CultureInfo.InvariantCulture));
        }
        else
        {
            writer.WriteNullValue();
        }
        
    }
}