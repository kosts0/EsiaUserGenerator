namespace EsiaUserGenerator.Utils;

public static class StringEx
{
    public static string ToPhoneValue(this long phoneValue)
    {
        var phoneValueArray = phoneValue.ToString();
        return "+" + phoneValueArray;
    }
}