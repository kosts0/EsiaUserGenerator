namespace EsiaUserGenerator.Utils.JsonConverter;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;

public class CustomDateTimeConverter : DateTimeConverterBase
{
    private readonly string _format;

    public CustomDateTimeConverter(string format)
    {
        _format = format;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        DateTime dateTime = (DateTime)value;
        writer.WriteValue(dateTime.ToString(_format, CultureInfo.InvariantCulture));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        string dateString = reader.Value.ToString();
        return DateTime.ParseExact(dateString, _format, CultureInfo.InvariantCulture);
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(DateTime) || objectType == typeof(DateTime?);
    }
}