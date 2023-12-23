using Newtonsoft.Json.Linq;

namespace JexlNet.JsonNet;

public static partial class ContextHelpers
{
    public static Dictionary<string, dynamic?>? ConvertJObject(JObject? jObject)
    {
        var result = new Dictionary<string, dynamic?>();
        if (jObject is null)
        {
            return result;
        }

        foreach (var property in jObject.Properties())
        {
            if (property.Value.Type == JTokenType.Object)
            {
                result[property.Name] = ConvertJObject(property.Value as JObject);
            }
            else if (property.Value.Type == JTokenType.Array)
            {
                result[property.Name] = ConvertJArray(property.Value as JArray);
            }
            else
            {
                result[property.Name] = ConvertJValue(property.Value as JValue);
            }
        }

        return result;
    }

    public static List<dynamic?> ConvertJArray(JArray? jArray)
    {
        var result = new List<dynamic?>();
        if (jArray is null)
        {
            return result;
        }

        foreach (var item in jArray)
        {
            if (item.Type == JTokenType.Object)
            {
                result.Add(ConvertJObject(item as JObject));
            }
            else if (item.Type == JTokenType.Array)
            {
                result.Add(ConvertJArray(item as JArray));
            }
            else
            {
                result.Add(ConvertJValue(item as JValue));
            }
        }

        return result;
    }

    public static dynamic? ConvertJValue(JValue? jValue)
    {
        switch (jValue?.Type)
        {
            case JTokenType.String:
                return jValue.Value<string>();
            case JTokenType.Boolean:
                return jValue.Value<bool>();
            case JTokenType.Float:
            case JTokenType.Integer:
                return jValue.Value<decimal>();
            default:
                return null; // Handle other value types as needed
        }
    }
}
