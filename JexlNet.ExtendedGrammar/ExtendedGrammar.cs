using System.Text.Json;
using System.Text.Json.Nodes;

namespace JexlNet;

public class ExtendedGrammar : Grammar
{
    public ExtendedGrammar()
    {
        // String
        AddFunction("string", String);
        AddFunction("$string", String);
        AddTransform("string", String);
        // Length
        AddFunction("length", Length);
        AddFunction("$length", Length);
        AddTransform("length", Length);
        // Substring
        AddFunction("substring", Substring);
        AddFunction("$substring", Substring);
        AddTransform("substring", Substring);
    }

    /// <summary>
    /// Casts the arg parameter to a string.
    /// </summary>
    /// <example><code>string(arg)</code><code>$string(arg)</code><code>arg|string</code></example>
    /// <returns>The string JsonValue that represents the input</returns>
    public static JsonNode? String(JsonNode? input)
    {
        return JsonValue.Create(input?.ToString());
    }

    /// <summary>
    /// Returns the number of characters in a string, or the length of an array.
    /// </summary>
    /// <example><code>length(arg)</code><code>$length(arg)</code><code>arg|length</code></example>
    /// <returns>The length of the input</returns>
    public static JsonNode? Length(JsonNode? input)
    {
        if (input is JsonArray array)
        {
            return JsonValue.Create(array.Count);
        }
        else if (input is JsonValue value)
        {
            return JsonValue.Create(value.ToString().Length);
        }
        return null;
    }

    /// <summary>
    /// Gets a substring of a string.
    /// </summary>
    /// <example><code>substring(arg, start, length)</code><code>$substring(arg, start, length)</code><code>arg|substring(start, length)</code></example>
    /// <returns>The substring of the input</returns>
    public static JsonNode? Substring(JsonNode? input, JsonNode? start, JsonNode? length)
    {
        if (input is JsonValue value && start is JsonValue startValue && length is JsonValue lengthValue)
        {
            string str = value.ToString();
            int startNum = startValue.ToInt32();
            int len = lengthValue?.ToInt32() ?? int.MaxValue;

            if (startNum < 0)
            {
                startNum = str.Length + startNum;
                if (startNum < 0)
                {
                    startNum = 0;
                }
            };
            if (startNum + len > str.Length)
            {
                len = str.Length - startNum;
            };
            if (len < 0)
            {
                len = 0;
            };
            return str.Substring(startNum, len);
        }
        return null;
    }
}
