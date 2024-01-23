using System.Text.Json;

namespace JexlNet;

public static partial class ContextHelpers
{
    public static Dictionary<string, dynamic?> ConvertJsonElement(JsonElement jsonElement)
    {
        var result = new Dictionary<string, dynamic?>();

        foreach (var property in jsonElement.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                result[property.Name] = ConvertJsonElement(property.Value);
            }
            else if (property.Value.ValueKind == JsonValueKind.Array)
            {
                result[property.Name] = ConvertJsonArray(property.Value.EnumerateArray());
            }
            else
            {
                result[property.Name] = ConvertJsonValue(property.Value);
            }
        }

        return result;
    }

    public static List<dynamic?> ConvertJsonArray(JsonElement.ArrayEnumerator arrayEnumerator)
    {
        var result = new List<dynamic?>();

        while (arrayEnumerator.MoveNext())
        {
            var current = arrayEnumerator.Current;

            if (current.ValueKind == JsonValueKind.Object)
            {
                result.Add(ConvertJsonElement(current));
            }
            else if (current.ValueKind == JsonValueKind.Array)
            {
                result.Add(ConvertJsonArray(current.EnumerateArray()));
            }
            else
            {
                result.Add(ConvertJsonValue(current));
            }
        }

        return result;
    }


    public static dynamic? ConvertJsonValue(JsonElement jsonValue)
    {
        switch (jsonValue.ValueKind)
        {
            case JsonValueKind.String:
                return jsonValue.GetString();
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.Number:
                return jsonValue.GetDecimal();
            default:
                return null; // Handle other value kinds as needed
        }
    }
}