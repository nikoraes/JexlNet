using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JexlNet
{
    public partial class ExtendedGrammar : Grammar
    {
        public ExtendedGrammar()
        {
            // String
            AddFunction("string", ToString);
            AddFunction("$string", ToString);
            AddTransform("string", ToString);
            AddTransform("toString", ToString);
            // JSON
            AddFunction("json", ToJson);
            AddFunction("$json", ToJson);
            AddTransform("toJson", ToJson);
            AddFunction("parseJson", ToJson);
            AddFunction("$parseJson", ToJson);
            AddTransform("parseJson", ToJson);
            // Length
            AddFunction("length", Length);
            AddFunction("$length", Length);
            AddTransform("length", Length);
            AddFunction("count", Length);
            AddFunction("$count", Length);
            AddTransform("count", Length);
            AddFunction("size", Length);
            AddFunction("$size", Length);
            AddTransform("size", Length);
            // Substring
            AddFunction("substring", Substring);
            AddFunction("$substring", Substring);
            AddTransform("substring", Substring);
            // SubstringBefore
            AddFunction("substringBefore", SubstringBefore);
            AddFunction("$substringBefore", SubstringBefore);
            AddTransform("substringBefore", SubstringBefore);
            // SubstringAfter
            AddFunction("substringAfter", SubstringAfter);
            AddFunction("$substringAfter", SubstringAfter);
            AddTransform("substringAfter", SubstringAfter);
            // Uppercase
            AddFunction("uppercase", Uppercase);
            AddFunction("$uppercase", Uppercase);
            AddTransform("uppercase", Uppercase);
            AddFunction("upper", Uppercase);
            AddFunction("$upper", Uppercase);
            AddTransform("upper", Uppercase);
            // Lowercase
            AddFunction("lowercase", Lowercase);
            AddFunction("$lowercase", Lowercase);
            AddTransform("lowercase", Lowercase);
            AddFunction("lower", Lowercase);
            AddFunction("$lower", Lowercase);
            AddTransform("lower", Lowercase);
            // Camelcase
            AddFunction("camelCase", CamelCase);
            AddFunction("$camelCase", CamelCase);
            AddTransform("camelcase", CamelCase);
            AddTransform("camelCase", CamelCase);
            AddTransform("toCamelCase", CamelCase);
            // PascalCase
            AddFunction("pascalCase", PascalCase);
            AddFunction("$pascalCase", PascalCase);
            AddTransform("pascalcase", PascalCase);
            AddTransform("pascalCase", PascalCase);
            AddTransform("toPascalCase", PascalCase);
            // Trim
            AddFunction("trim", Trim);
            AddFunction("$trim", Trim);
            AddTransform("trim", Trim);
            // Pad
            AddFunction("pad", Pad);
            AddFunction("$pad", Pad);
            AddTransform("pad", Pad);
            // Contains
            AddFunction("contains", Contains);
            AddFunction("$contains", Contains);
            AddTransform("contains", Contains);
            AddFunction("includes", Contains);
            AddFunction("$includes", Contains);
            AddTransform("includes", Contains);
            // StartsWith
            AddFunction("startsWith", StartsWith);
            AddFunction("$startsWith", StartsWith);
            AddTransform("startsWith", StartsWith);
            // EndsWith
            AddFunction("endsWith", EndsWith);
            AddFunction("$endsWith", EndsWith);
            AddTransform("endsWith", EndsWith);
            // Split
            AddFunction("split", Split);
            AddFunction("$split", Split);
            AddTransform("split", Split);
            // Join
            AddFunction("join", Join);
            AddFunction("$join", Join);
            AddTransform("join", Join);
            // Replace
            AddFunction("replace", Replace);
            AddFunction("$replace", Replace);
            AddTransform("replace", Replace);
            // Base64Encode
            AddFunction("base64Encode", Base64Encode);
            AddFunction("$base64Encode", Base64Encode);
            AddTransform("base64Encode", Base64Encode);
            // Base64Decode
            AddFunction("base64Decode", Base64Decode);
            AddFunction("$base64Decode", Base64Decode);
            AddTransform("base64Decode", Base64Decode);
            // URLFormEncoded
            AddFunction("formUrlEncoded", FormUrlEncoded);
            AddFunction("$formUrlEncoded", FormUrlEncoded);
            AddTransform("formUrlEncoded", FormUrlEncoded);
            // Regex
            AddFunction("regexMatch", RegexMatch);
            AddFunction("$regexMatch", RegexMatch);
            AddTransform("regexMatch", RegexMatch);
            AddFunction("regexMatches", RegexMatches);
            AddFunction("$regexMatches", RegexMatches);
            AddTransform("regexMatches", RegexMatches);
            AddFunction("regexReplace", RegexReplace);
            AddFunction("$regexReplace", RegexReplace);
            AddTransform("regexReplace", RegexReplace);
            // Number
            AddFunction("number", ToNumber);
            AddFunction("$number", ToNumber);
            AddTransform("number", ToNumber);
            AddTransform("toNumber", ToNumber);
            AddFunction("parseFloat", ToNumber);
            AddFunction("$parseFloat", ToNumber);
            AddTransform("float", ToNumber);
            AddTransform("toFloat", ToNumber);
            // AbsoluteValue
            AddFunction("abs", AbsoluteValue);
            AddFunction("$abs", AbsoluteValue);
            AddTransform("abs", AbsoluteValue);
            // Floor
            AddFunction("floor", Floor);
            AddFunction("$floor", Floor);
            AddTransform("floor", Floor);
            // Ceil
            AddFunction("ceil", Ceil);
            AddFunction("$ceil", Ceil);
            AddTransform("ceil", Ceil);
            // Round
            AddFunction("round", Round);
            AddFunction("$round", Round);
            AddTransform("round", Round);
            // Power
            AddFunction("power", Power);
            AddFunction("$power", Power);
            AddTransform("power", Power);
            // Sqrt
            AddFunction("sqrt", Sqrt);
            AddFunction("$sqrt", Sqrt);
            AddTransform("sqrt", Sqrt);
            // Random
            AddFunction("random", RandomNumber);
            AddFunction("$random", RandomNumber);
            // FormatNumber
            AddFunction("formatNumber", FormatNumber);
            AddFunction("$formatNumber", FormatNumber);
            AddTransform("formatNumber", FormatNumber);
            // FormatBase
            AddFunction("formatBase", FormatBase);
            AddFunction("$formatBase", FormatBase);
            AddTransform("formatBase", FormatBase);
            // FormatInteger
            AddFunction("formatInteger", FormatInteger);
            AddFunction("$formatInteger", FormatInteger);
            AddTransform("formatInteger", FormatInteger);
            // ParseInteger
            AddFunction("parseInteger", ParseInteger);
            AddFunction("parseInt", ParseInteger);
            AddFunction("$parseInteger", ParseInteger);
            AddTransform("parseInteger", ParseInteger);
            AddTransform("parseInt", ParseInteger);
            AddTransform("toInt", ParseInteger);
            AddTransform("integer", ParseInteger);
            // Sum
            AddFunction("sum", Sum);
            AddFunction("$sum", Sum);
            AddTransform("sum", Sum);
            // Max
            AddFunction("max", Max);
            AddFunction("$max", Max);
            AddTransform("max", Max);
            // Min
            AddFunction("min", Min);
            AddFunction("$min", Min);
            AddTransform("min", Min);
            // Average
            AddFunction("average", Average);
            AddFunction("avg", Average);
            AddFunction("$average", Average);
            AddTransform("average", Average);
            AddTransform("avg", Average);
            // Boolean
            AddFunction("boolean", ToBoolean);
            AddFunction("$boolean", ToBoolean);
            AddTransform("boolean", ToBoolean);
            AddFunction("bool", ToBoolean);
            AddFunction("$bool", ToBoolean);
            AddTransform("bool", ToBoolean);
            AddTransform("toBoolean", ToBoolean);
            AddTransform("toBool", ToBoolean);
            // Not
            AddFunction("not", Not);
            AddFunction("$not", Not);
            AddTransform("not", Not);
            // Case / Switch
            AddFunction("case", Case);
            AddFunction("$case", Case);
            AddTransform("case", Case);
            AddFunction("switch", Case);
            AddFunction("$switch", Case);
            AddTransform("switch", Case);
            // ArrayAppend
            AddFunction("append", ArrayAppend);
            AddFunction("$append", ArrayAppend);
            AddTransform("append", ArrayAppend);
            AddFunction("concat", ArrayAppend);
            AddFunction("$concat", ArrayAppend);
            AddTransform("concat", ArrayAppend);
            // ArrayReverse
            AddFunction("reverse", ArrayReverse);
            AddFunction("$reverse", ArrayReverse);
            AddTransform("reverse", ArrayReverse);
            // ArrayShuffle
            AddFunction("shuffle", ArrayShuffle);
            AddFunction("$shuffle", ArrayShuffle);
            AddTransform("shuffle", ArrayShuffle);
            // ArraySort
            AddFunction("sort", ArraySort);
            AddFunction("$sort", ArraySort);
            AddTransform("sort", ArraySort);
            AddFunction("order", ArraySort);
            AddFunction("$order", ArraySort);
            AddTransform("order", ArraySort);
            // ArrayDistinct
            AddFunction("distinct", ArrayDistinct);
            AddFunction("$distinct", ArrayDistinct);
            AddTransform("distinct", ArrayDistinct);
            // ArrayToObject
            AddFunction("toObject", ArrayToObject);
            AddFunction("$toObject", ArrayToObject);
            AddTransform("toObject", ArrayToObject);
            AddFunction("fromEntries", ArrayToObject);
            AddFunction("$fromEntries", ArrayToObject);
            AddTransform("fromEntries", ArrayToObject);
            // MapField
            AddFunction("mapField", MapField);
            AddFunction("$mapField", MapField);
            AddTransform("mapField", MapField);
            // Map
            AddFunction("map", Map);
            AddFunction("$map", Map);
            AddTransform("map", Map);
            // Any
            AddFunction("any", Any);
            AddFunction("$any", Any);
            AddTransform("any", Any);
            AddFunction("some", Any);
            AddFunction("$some", Any);
            AddTransform("some", Any);
            // All
            AddFunction("all", All);
            AddFunction("$all", All);
            AddTransform("all", All);
            AddFunction("every", All);
            AddFunction("$every", All);
            AddTransform("every", All);
            // Filter
            AddFunction("filter", Filter);
            AddFunction("$filter", Filter);
            AddTransform("filter", Filter);
            // Find
            AddFunction("find", ArrayFind);
            AddFunction("$find", ArrayFind);
            AddTransform("find", ArrayFind);
            // Reduce
            AddFunction("reduce", Reduce);
            AddFunction("$reduce", Reduce);
            AddTransform("reduce", Reduce);
            // ObjectKeys
            AddFunction("keys", ObjectKeys);
            AddFunction("$keys", ObjectKeys);
            AddTransform("keys", ObjectKeys);
            // ObjectValues
            AddFunction("values", ObjectValues);
            AddFunction("$values", ObjectValues);
            AddTransform("values", ObjectValues);
            // ObjectEntries
            AddFunction("entries", ObjectEntries);
            AddFunction("$entries", ObjectEntries);
            AddTransform("entries", ObjectEntries);
            // ObjectMerge
            AddFunction("merge", ObjectMerge);
            AddFunction("$merge", ObjectMerge);
            AddTransform("merge", ObjectMerge);
            // Now
            AddFunction("now", Now);
            AddFunction("$now", Now);
            // Millis
            AddFunction("millis", Millis);
            AddFunction("$millis", Millis);
            // MillisToDateTime
            AddFunction("millisToDateTime", ToDateTime);
            AddFunction("$millisToDateTime", ToDateTime);
            AddTransform("millisToDateTime", ToDateTime);
            AddFunction("fromMillis", ToDateTime);
            AddFunction("$fromMillis", ToDateTime);
            AddTransform("fromMillis", ToDateTime);
            AddFunction("toDateTime", ToDateTime);
            AddFunction("$toDateTime", ToDateTime);
            AddTransform("toDateTime", ToDateTime);
            AddFunction("parseDateTime", ToDateTime);
            AddFunction("$parseDateTime", ToDateTime);
            AddFunction("dateTimeString", ToDateTime);
            // DateTimeFormat
            AddFunction("dateTimeFormat", DateTimeFormat);
            AddFunction("$dateTimeFormat", DateTimeFormat);
            AddTransform("dateTimeFormat", DateTimeFormat);
            // DateTimeToMillis
            AddFunction("dateTimeToMillis", DateTimeToMillis);
            AddFunction("$dateTimeToMillis", DateTimeToMillis);
            AddTransform("dateTimeToMillis", DateTimeToMillis);
            AddFunction("toMillis", DateTimeToMillis);
            AddFunction("$toMillis", DateTimeToMillis);
            AddTransform("toMillis", DateTimeToMillis);
            // DateTimeAdd
            AddFunction("dateTimeAdd", DateTimeAdd);
            AddFunction("$dateTimeAdd", DateTimeAdd);
            AddTransform("dateTimeAdd", DateTimeAdd);
            // Eval
            AddFunction("eval", Eval);
            AddFunction("$eval", Eval);
            AddTransform("eval", Eval);
            // Uuid
            AddFunction("uuid", Uuid);
            AddFunction("$uuid", Uuid);
            AddFunction("uid", Uuid);
            AddFunction("$uid", Uuid);
            // Type checks
            AddFunction("isArray", IsArray);
            AddFunction("$isArray", IsArray);
            AddTransform("isArray", IsArray);
            AddFunction("isObject", IsObject);
            AddFunction("$isObject", IsObject);
            AddTransform("isObject", IsObject);
        }

        private static readonly JsonSerializerOptions _prettyPrintOptions =
            new JsonSerializerOptions() { WriteIndented = true, };
        private static readonly JsonSerializerOptions _defaultOptions = new JsonSerializerOptions();

        /// <summary>
        /// Casts the arg parameter to a string.
        /// </summary>
        /// <example><code>string(arg)</code><code>$string(arg)</code><code>arg|string</code></example>
        /// <returns>The string JsonValue that represents the input</returns>
        public static JsonNode ToString(JsonNode input, JsonNode prettify = null)
        {
            if (input is JsonValue inputValue && inputValue.GetValueKind() == JsonValueKind.String)
            {
                return input;
            }
            JsonSerializerOptions options =
                prettify is JsonValue prettifyValue
                && prettifyValue.GetValueKind() == JsonValueKind.True
                    ? _prettyPrintOptions
                    : _defaultOptions;
            string jsonString = JsonSerializer.Serialize(input, options);
            return JsonValue.Create(jsonString);
        }

        /**
         * Parses the string and returns a JSON object.
         *
         * @example
         * ```jexl
         * parseJson('{"key": "value"}') // { key: "value" }
         * '{"key": "value"}'|toJson // { key: "value" }
         */

        /// <summary>
        /// Parses the string and returns a JSON object.
        /// </summary>
        /// <example><code>json(arg)</code><code>$json(arg)</code><code>arg|json</code></example>
        /// <returns>The JSON object that represents the input</returns>
        public static JsonNode ToJson(JsonNode input)
        {
            if (input is JsonValue value && value.GetValueKind() == JsonValueKind.String)
            {
                string str = value.ToString();
                try
                {
                    return JsonNode.Parse(str);
                }
                catch (JsonException)
                {
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the number of characters in a string, or the length of an array.
        /// </summary>
        /// <example><code>length(arg)</code><code>$length(arg)</code><code>arg|length</code></example>
        /// <returns>The length of the input</returns>
        public static JsonNode Length(JsonNode input)
        {
            if (input is JsonArray array)
            {
                return JsonValue.Create(array.Count);
            }
            else if (input is JsonObject obj)
            {
                return JsonValue.Create(obj.Count);
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
        public static JsonNode Substring(JsonNode input, JsonNode start, JsonNode length)
        {
            if (input is JsonValue value && start is JsonValue startValue)
            {
                string str = value.ToString();
                int startNum = startValue.ToInt32();
                int len = length?.AsValue().ToInt32() ?? str.Length;

                if (startNum < 0)
                {
                    startNum = str.Length + startNum;
                    if (startNum < 0)
                    {
                        startNum = 0;
                    }
                }
                ;
                if (startNum + len > str.Length)
                {
                    len = str.Length - startNum;
                }
                ;
                if (len < 0)
                {
                    len = 0;
                }
                ;
                return str.Substring(startNum, len);
            }
            return null;
        }

        /// <summary>
        /// Returns the substring before the first occurrence of the character sequence chars in str.
        /// </summary>
        /// <example><code>substringBefore(str, chars)</code><code>$substringBefore(str, chars)</code><code>str|substringBefore(chars)</code></example>
        /// <returns>The substring before the first occurrence of the character sequence chars in str</returns>

        public static JsonNode SubstringBefore(JsonNode input, JsonNode chars)
        {
            if (input is JsonValue value && chars is JsonValue charsValue)
            {
                string str = value.ToString();
                string charsStr = charsValue.ToString();
                int index = str.IndexOf(charsStr);
                if (index == -1)
                {
                    return str;
                }
                return str.Substring(0, index);
            }
            return null;
        }

        /// <summary>
        /// Returns the substring after the first occurrence of the character sequence chars in str.
        /// </summary>
        /// <example><code>substringAfter(str, chars)</code><code>$substringAfter(str, chars)</code><code>str|substringAfter(chars)</code></example>
        /// <returns>The substring before the first occurrence of the character sequence chars in str</returns>

        public static JsonNode SubstringAfter(JsonNode input, JsonNode chars)
        {
            if (input is JsonValue value && chars is JsonValue charsValue)
            {
                string str = value.ToString();
                string charsStr = charsValue.ToString();
                int index = str.IndexOf(charsStr);
                if (index == -1)
                {
                    return str;
                }
                return str.Substring(index + charsStr.Length).ToString();
            }
            return null;
        }

        /// <summary>
        /// Returns a string with all the characters of str converted to uppercase.
        /// </summary>
        /// <example><code>uppercase(str)</code><code>$uppercase(str)</code><code>str|uppercase</code></example>
        /// <returns>A string with all the characters of str converted to uppercase</returns>
        public static JsonNode Uppercase(JsonNode input)
        {
            if (input is JsonValue value)
            {
                return value.ToString().ToUpper();
            }
            return null;
        }

        /// <summary>
        /// Returns a string with all the characters of str converted to lowercase.
        /// </summary>
        /// <example><code>lowercase(str)</code><code>$lowercase(str)</code><code>str|lowercase</code></example>
        /// <returns>A string with all the characters of str converted to lowercase</returns>
        public static JsonNode Lowercase(JsonNode input)
        {
            if (input is JsonValue value)
            {
                return value.ToString().ToLower();
            }
            return null;
        }

        public static Regex SplitRegex = new Regex(
            @"(?<!^)(?=[A-Z])|[`~!@#%^&*()|+\\\-=?;:'.,\s_']+",
            RegexOptions.Compiled
        );

        /// <summary>
        /// Returns a string in camelcase
        /// </summary>
        /// <example><code>camelCase(str)</code><code>$camelCase(str)</code><code>str|toCamelCase</code></example>
        /// <returns>A string in camelcase</returns>
        public static JsonNode CamelCase(JsonNode input)
        {
            if (input is JsonValue value)
            {
                string str = value.ToString();
                string[] words = SplitRegex.Split(str);
                string camelCase = words[0].ToLower();
                for (int i = 1; i < words.Length; i++)
                {
                    camelCase +=
                        words[i].Substring(0, 1).ToUpper() + words[i].Substring(1).ToLower();
                }
                return camelCase;
            }
            return null;
        }

        /// <summary>
        /// Returns a string in PascalCase
        /// </summary>
        /// <example><code>pascalCase(str)</code><code>$pascalCase(str)</code><code>str|toPascalCase</code></example>
        /// <returns>A string in PascalCase</returns>
        public static JsonNode PascalCase(JsonNode input)
        {
            if (input is JsonValue value)
            {
                string str = value.ToString();
                string[] words = SplitRegex.Split(str);
                string pascalCase = "";
                for (int i = 0; i < words.Length; i++)
                {
                    pascalCase +=
                        words[i].Substring(0, 1).ToUpper() + words[i].Substring(1).ToLower();
                }
                return pascalCase;
            }
            return null;
        }

        /// <summary>
        /// Normalizes and trims all whitespace characters in str by applying the following steps:
        /// All tabs, carriage returns, and line feeds are replaced with spaces.
        /// Contiguous sequences of spaces are reduced to a single space.
        /// Trailing and leading spaces are removed.
        /// </summary>
        /// <example><code>trim(str)</code><code>$trim(str)</code><code>str|trim</code></example>
        /// <returns>A string with all the characters of str converted to lowercase</returns>
        public static JsonNode Trim(JsonNode input, JsonNode trimChar = null)
        {
            if (input is JsonValue value)
            {
                if (trimChar is JsonValue trimCharValue)
                {
                    return value.ToString().Trim(trimCharValue.ToString().ToCharArray());
                }
                return value.ToString().Trim();
            }
            return null;
        }

        /// <summary>
        /// Returns a copy of the string str with extra padding, if necessary, so that its total number of characters is at least the absolute value of the width parameter.
        /// If width is a positive number, then the string is padded to the right;
        /// if negative, it is padded to the left.
        /// The optional char argument specifies the padding character(s) to use.
        /// If not specified, it defaults to the space character
        /// </summary>
        /// <example><code>pad(str, width, char)</code><code>$pad(str, width, char)</code><code>str|pad(width, char)</code></example>
        /// <returns>A padded string</returns>
        public static JsonNode Pad(JsonNode input, JsonNode width, JsonNode paddingChar = null)
        {
            if (input is JsonValue value && width is JsonValue widthValue)
            {
                string str = value.ToString();
                int widthNum = widthValue.ToInt32();
                string padChar = paddingChar?.ToString() ?? " ";
                if (widthNum > 0)
                {
                    return str.PadRight(widthNum, padChar[0]);
                }
                else
                {
                    return str.PadLeft(-widthNum, padChar[0]);
                }
            }
            return null;
        }

        /// <summary>
        /// Returns true if the characters within pattern are contained contiguously within str.
        /// If the input is an array, the function will return true if any of the elements are an exact match to the string or regex.
        /// </summary>
        /// <example><code>contains(str, pattern)</code><code>$contains(str, pattern)</code><code>str|contains(pattern)</code></example>
        /// <returns>true if input is matched by pattern, otherwise it returns false</returns>
        public static JsonNode Contains(JsonNode input, JsonNode pattern)
        {
            if (input is JsonValue value && pattern is JsonValue patternValue)
            {
                string str = value.ToString();
                string patternStr = patternValue.ToString();
                return str.Contains(patternStr);
            }
            else if (input is JsonArray array)
            {
                return array.Any(
                    elem =>
                        elem is JsonValue elementValue
                        && pattern is JsonValue
                        && elementValue.ToString().Equals(pattern.ToString())
                );
            }
            return false;
        }

        /// <summary>
        /// Returns true if the string str starts with the character sequence chars.
        /// </summary>
        /// <example><code>startsWith(str, chars)</code><code>$startsWith(str, chars)</code><code>str|startsWith(chars)</code></example>
        /// <returns>true if the string str starts with the character sequence chars</returns>
        public static JsonNode StartsWith(JsonNode input, JsonNode chars)
        {
            if (input is JsonValue value && chars is JsonValue charsValue)
            {
                string str = value.ToString();
                string charsStr = charsValue.ToString();
                return str.StartsWith(charsStr);
            }
            return false;
        }

        /// <summary>
        /// Returns true if the string str ends with the character sequence chars.
        /// </summary>
        /// <example><code>endsWith(str, chars)</code><code>$endsWith(str, chars)</code><code>str|endsWith(chars)</code></example>
        /// <returns>true if the string str ends with the character sequence chars</returns>
        public static JsonNode EndsWith(JsonNode input, JsonNode chars)
        {
            if (input is JsonValue value && chars is JsonValue charsValue)
            {
                string str = value.ToString();
                string charsStr = charsValue.ToString();
                return str.EndsWith(charsStr);
            }
            return false;
        }

        /// <summary>
        /// Splits a string into an array of substrings based on a specified separator.
        /// </summary>
        /// <example><code>split(str, separator)</code><code>$split(str, separator)</code><code>str|split(separator)</code></example>
        /// <returns>An array of substrings</returns>
        public static JsonNode Split(JsonNode input, JsonNode separator)
        {
            if (input is JsonValue value && separator is JsonValue separatorValue)
            {
                string str = value.ToString();
                string separatorStr = separatorValue.ToString();
                string[] splitted =
                    str?.Split(new string[] { separatorStr }, StringSplitOptions.None)
                    ?? new string[0];
                return new JsonArray(splitted.Select(x => (JsonNode)x).ToArray());
            }
            return null;
        }

        /// <summary>
        /// Joins the elements of an array into a string, with a specified separator.
        /// </summary>
        /// <example><code>join(array, separator)</code><code>$join(array, separator)</code><code>array|join(separator)</code></example>
        /// <returns>A string that consists of the elements of the array separated by the separator</returns>
        public static JsonNode Join(JsonNode input, JsonNode separator)
        {
            if (input is JsonArray array)
            {
                string separatorStr = separator is JsonValue separatorValue
                    ? separatorValue.ToString()
                    : ",";
                return string.Join(separatorStr, array.Select(x => x?.ToString()));
            }
            return null;
        }

        /// <summary>
        /// Replaces all occurrences of a substring within a string, with a new substring.
        /// </summary>
        /// <example><code>replace(str, old, new)</code><code>$replace(str, old, new)</code><code>str|replace(old, new)</code></example>
        /// <returns>A string with all occurrences of old replaced by new</returns>
        public static JsonNode Replace(JsonNode input, JsonNode old, JsonNode newStr)
        {
            if (input is JsonValue value && old is JsonValue oldValue)
            {
                string str = value.ToString();
                string oldStr = oldValue.ToString();
                string newStrStr = newStr is JsonValue newValue ? newValue.ToString() : "";
                return str.Replace(oldStr, newStrStr);
            }
            return null;
        }

        /// <summary>
        /// Encodes a string to base64.
        /// </summary>
        /// <example><code>base64Encode(str)</code><code>$base64Encode(str)</code><code>str|base64Encode</code></example>
        /// <returns>A base64 encoded string</returns>
        public static JsonNode Base64Encode(JsonNode input)
        {
            if (input is JsonValue value)
            {
                string str = value.ToString();
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
                return Convert.ToBase64String(bytes);
            }
            return null;
        }

        /// <summary>
        /// Decodes a base64 encoded string.
        /// </summary>
        /// <example><code>base64Decode(str)</code><code>$base64Decode(str)</code><code>str|base64Decode</code></example>
        /// <returns>A decoded string</returns>
        public static JsonNode Base64Decode(JsonNode input)
        {
            if (input is JsonValue value)
            {
                string str = value.ToString();
                byte[] bytes = Convert.FromBase64String(str);
                return System.Text.Encoding.UTF8.GetString(bytes);
            }
            return null;
        }

        /// <summary>
        /// Converts an object to a form URL encoded string.
        /// </summary>
        /// <param name="input"></param>
        /// <example><code>formUrlEncoded(obj)</code><code>$formUrlEncoded(obj)</code><code>obj|formUrlEncoded</code></example>
        /// <returns>A form url encoded string</returns>
        public static JsonNode FormUrlEncoded(JsonNode input)
        {
            if (input is JsonValue value)
            {
                string str = value.ToString();
                return Uri.EscapeDataString(str);
            }
            else if (input is JsonObject obj)
            {
                return string.Join(
                    "&",
                    obj.Select(
                        x =>
                            $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value.ToString())}"
                    )
                );
            }
            return null;
        }

        /// <summary>
        /// Check whether a string matches a regex pattern
        /// </summary>
        /// <example><code>regexMatch(str, pattern)</code><code>$regexMatch(str, pattern)</code><code>str|regexMatch(pattern)</code></example>
        /// <returns>true if the string str matches the pattern</returns>
        public static JsonNode RegexMatch(JsonNode input, JsonNode pattern)
        {
            if (input is JsonValue value && pattern is JsonValue patternValue)
            {
                string str = value.ToString();
                string patternStr = patternValue.ToString();
                return Regex.IsMatch(str, patternStr);
            }
            return false;
        }

        /// <summary>
        /// Gets all regex matches in a string
        /// </summary>
        /// <example><code>regexMatches(str, pattern)</code><code>$regexMatches(str, pattern)</code><code>str|regexMatches(pattern)</code></example>
        /// <returns>An array of all regex matches</returns>
        public static JsonNode RegexMatches(JsonNode input, JsonNode pattern)
        {
            if (input is JsonValue value && pattern is JsonValue patternValue)
            {
                string str = value.ToString();
                string patternStr = patternValue.ToString();
                MatchCollection matches = Regex.Matches(str, patternStr);
                List<Match> matchList = matches.Cast<Match>().ToList(); // Convert to list
                return new JsonArray(
                    matchList
                        .Select(x =>
                        {
                            JsonObject groups = new JsonObject();
                            foreach (Group group in x.Groups.Cast<Group>())
                            {
#if NET8_0
                                groups[group.Name] = group.Value;
#else
                                // lower versions don't support group names
                                groups[$"{group.Index}"] = group.Value;
#endif
                            }
                            return groups;
                        })
                        .ToArray()
                );
            }
            return null;
        }

        /// <summary>
        /// Replace by Regex
        /// </summary>
        /// <example><code>regexReplace(str, pattern, replacement)</code><code>$regexReplace(str, pattern, replacement)</code><code>str|regexReplace(pattern, replacement)</code></example>
        /// <returns>A string with all occurrences of the pattern replaced by the replacement</returns>
        public static JsonNode RegexReplace(JsonNode input, JsonNode pattern, JsonNode replacement)
        {
            if (input is JsonValue value && pattern is JsonValue patternValue)
            {
                string str = value.ToString();
                string patternStr = patternValue.ToString();
                string replacementStr = replacement is JsonValue replacementValue
                    ? replacementValue.ToString()
                    : "";
                return Regex.Replace(str, patternStr, replacementStr);
            }
            return null;
        }

        /// <summary>
        /// Casts the arg parameter to a number.
        /// </summary>
        /// <example><code>number(arg)</code><code>$number(arg)</code><code>arg|number</code></example>
        /// <returns>The number JsonValue that represents the input</returns>
        public static JsonNode ToNumber(JsonNode input)
        {
            if (input is JsonValue value)
            {
                if (value.GetValueKind() == JsonValueKind.String)
                {
                    var decVal = value.ToDecimal();
                    return JsonValue.Create(decVal);
                }
                else if (value.GetValueKind() == JsonValueKind.Number)
                {
                    return value;
                }
                else if (value.GetValueKind() == JsonValueKind.True)
                {
                    return JsonValue.Create(1);
                }
                else if (value.GetValueKind() == JsonValueKind.False)
                {
                    return JsonValue.Create(0);
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the absolute value of a number.
        /// </summary>
        /// <example><code>abs(arg)</code><code>$abs(arg)</code><code>arg|abs</code></example>
        /// <returns>The absolute value of the input</returns>
        public static JsonNode AbsoluteValue(JsonNode input)
        {
            if (input is JsonValue value)
            {
                return Math.Abs(value.ToDecimal());
            }
            return null;
        }

        /// <summary>
        /// Returns the largest integer less than or equal to a number.
        /// </summary>
        /// <example><code>floor(arg)</code><code>$floor(arg)</code><code>arg|floor</code></example>
        /// <returns>The largest integer less than or equal to the input</returns>
        public static JsonNode Floor(JsonNode input)
        {
            if (input is JsonValue value)
            {
                return Math.Floor(value.ToDecimal());
            }
            return null;
        }

        /// <summary>
        /// Returns the smallest integer greater than or equal to a number.
        /// </summary>
        /// <example><code>ceil(arg)</code><code>$ceil(arg)</code><code>arg|ceil</code></example>
        /// <returns>The smallest integer greater than or equal to the input</returns>
        public static JsonNode Ceil(JsonNode input)
        {
            if (input is JsonValue value)
            {
                return Math.Ceiling(value.ToDecimal());
            }
            return null;
        }

        /// <summary>
        /// Returns the value of the number parameter rounded to the number of decimal places specified by the optional precision parameter.
        /// </summary>
        /// <example><code>round(arg, precision)</code><code>$round(arg, precision)</code><code>arg|round(precision)</code></example>
        /// <returns>The value of the input rounded to the number of decimal places specified by the optional precision parameter</returns>
        public static JsonNode Round(JsonNode input, JsonNode precision = null)
        {
            if (input is JsonValue value)
            {
                decimal decVal = value.ToDecimal();
                if (precision is JsonValue precisionValue)
                {
                    int prec = precisionValue.ToInt32();
                    return Math.Round(decVal, prec);
                }
                return Math.Round(decVal);
            }
            return null;
        }

        /// <summary>
        /// Returns the value of the first parameter raised to the power of the second parameter.
        /// </summary>
        /// <example><code>power(arg, exponent)</code><code>$power(arg, exponent)</code><code>arg|power(exponent)</code></example>
        /// <returns>The value of the first parameter raised to the power of the second parameter</returns>
        public static JsonNode Power(JsonNode input, JsonNode exponent)
        {
            if (input is JsonValue value && exponent is JsonValue exponentValue)
            {
                return Math.Pow(value.ToDouble(), exponentValue.ToDouble());
            }
            if (input is JsonValue inputValue && exponent == null)
            {
                return Math.Pow(inputValue.ToDouble(), 2);
            }
            return null;
        }

        /// <summary>
        /// Returns the square root of a number.
        /// </summary>
        /// <example><code>sqrt(arg)</code><code>$sqrt(arg)</code><code>arg|sqrt</code></example>
        /// <returns>The square root of the input</returns>
        public static JsonNode Sqrt(JsonNode input)
        {
            if (input is JsonValue value)
            {
                return Math.Sqrt(value.ToDouble());
            }
            return null;
        }

        /// <summary>
        /// Returns a random number between 0 and 1.
        /// </summary>
        /// <example><code>random()</code><code>$random()</code><code>random</code></example>
        /// <returns>A random number between 0 and 1</returns>
        public static JsonNode RandomNumber()
        {
            return new Random().NextDouble();
        }

        /// <summary>
        /// Casts the number to a string and formats it to a decimal representation as specified by the picture string.
        /// The behaviour of this function is consistent with the XPath/XQuery function fn:format-number as defined in the XPath F&O 3.1 specification.
        /// The picture string parameter defines how the number is formatted and has the same syntax as fn:format-number.
        /// </summary>
        /// <example><code>formatNumber(number, picture)</code><code>$formatNumber(number, picture)</code><code>number|formatNumber(picture)</code></example>
        /// <returns>A string representation of the number formatted according to the picture string</returns>
        public static JsonNode FormatNumber(JsonNode input, JsonNode picture)
        {
            if (input is JsonValue value && picture is JsonValue pictureValue)
            {
                decimal number = value.ToDecimal();
                return number.ToString(
                    pictureValue.ToString(),
                    System.Globalization.CultureInfo.InvariantCulture
                );
            }
            return null;
        }

        /// <summary>
        /// Casts the number to a string and formats it to an integer represented in the number base specified by the radix argument.
        /// If radix is not specified, then it defaults to base 10. radix can be between 2 and 36, otherwise an error is thrown.
        /// </summary>
        /// <example><code>formatBase(number, radix)</code><code>$formatBase(number, radix)</code><code>number|formatBase(radix)</code></example>
        /// <returns>A string representation of the number formatted according to the radix</returns>
        public static JsonNode FormatBase(JsonNode input, JsonNode radix)
        {
            if (input is JsonValue value && radix is JsonValue radixValue)
            {
                int number = value.ToInt32();
                int rad = radixValue.ToInt32();
                return Convert.ToString(number, rad);
            }
            return null;
        }

        /// <summary>
        /// Casts the number to a string and formats it to an integer representation.
        /// </summary>
        /// <example><code>formatInteger(number)</code><code>$formatInteger(number)</code><code>number|formatInteger</code></example>
        /// <returns>A string representation of the number formatted as an integer</returns>
        public static JsonNode FormatInteger(JsonNode input, JsonNode picture)
        {
            if (input is JsonValue value && picture is JsonValue pictureValue)
            {
                int number = value.ToInt32();
                return number.ToString(
                    pictureValue.ToString(),
                    System.Globalization.CultureInfo.InvariantCulture
                );
            }
            return null;
        }

        private static readonly Regex FindIntegerRegex = new Regex(@"^\d+", RegexOptions.Compiled);
        private static readonly Regex FindHexRegex = new Regex(
            @"^[0-9a-fA-F]+",
            RegexOptions.Compiled
        );
        private static readonly Regex FindOctalRegex = new Regex(@"^[0-7]+", RegexOptions.Compiled);
        private static readonly Regex FindBinaryRegex = new Regex(
            @"^[0-1]+",
            RegexOptions.Compiled
        );
        private static readonly Regex FindAlphaNumericRegex = new Regex(
            @"^[0-9a-zA-Z]+",
            RegexOptions.Compiled
        );

        /// <summary>
        /// Parses the string argument as a signed integer in the specified base (default is base 10).
        /// </summary>
        /// <example><code>parseInteger(string)</code><code>parseInt(string)</code><code>parseInteger(string, base)</code><code>parseInt(string, base)</code><code>$parseInteger(string)</code><code>$parseInt(string)</code><code>string|parseInteger</code><code>string|parseInt</code><code>string|parseInteger(base)</code><code>string|parseInt(base)</code><code>string|toInt(base)</code></example>
        /// <returns>A number representation of the input</returns>
        public static JsonNode ParseInteger(JsonNode input, JsonNode baseNode = null)
        {
            if (input is JsonValue value)
            {
                int parseBase = 10; // Default base is 10

                if (baseNode is JsonValue baseValue)
                {
                    parseBase = baseValue.ToInt32();
                    if (parseBase < 2 || parseBase > 36)
                    {
                        throw new ArgumentException("Base must be between 2 and 36");
                    }
                }

                string inputStr = value.ToString();
                Regex regexToUse;

                // Choose appropriate regex based on base
                switch (parseBase)
                {
                    case 2:
                        regexToUse = FindBinaryRegex;
                        break;
                    case 8:
                        regexToUse = FindOctalRegex;
                        break;
                    case 16:
                        regexToUse = FindHexRegex;
                        break;
                    case 10:
                        regexToUse = FindIntegerRegex;
                        break;
                    default:
                        regexToUse = FindAlphaNumericRegex;
                        break;
                }

                foreach (Match match in regexToUse.Matches(inputStr))
                {
                    try
                    {
                        return Convert.ToInt32(match.Value, parseBase);
                    }
                    catch (FormatException)
                    {
                        // If parsing fails, try with the next match or return null
                        continue;
                    }
                    catch (OverflowException)
                    {
                        // If the number is too large for int, try parsing as long
                        try
                        {
                            return Convert.ToInt64(match.Value, parseBase);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the sum of all the numbers in an array.
        /// </summary>
        /// <example><code>sum(array)</code><code>$sum(array)</code><code>array|sum</code></example>
        /// <returns>The sum of all the numbers in the array</returns>
        public static JsonNode Sum(JsonNode[] input)
        {
            decimal total = 0m;
            foreach (JsonNode item in input)
            {
                if (item is JsonValue value)
                {
                    total += value.ToDecimal();
                }
                else if (item is JsonArray array)
                {
                    total += array.Sum(x => x?.AsValue().ToDecimal() ?? 0);
                }
            }
            return total;
        }

        /// <summary>
        /// Returns the largest number in an array.
        /// </summary>
        /// <example><code>max(array)</code><code>$max(array)</code><code>array|max</code></example>
        /// <returns>The largest number in the array</returns>
        public static JsonNode Max(JsonNode[] input)
        {
            decimal max = decimal.MinValue;
            foreach (JsonNode item in input)
            {
                if (item is JsonValue value)
                {
                    max = Math.Max(max, value.ToDecimal());
                }
                else if (item is JsonArray array)
                {
                    max = Math.Max(
                        max,
                        array.Max(x => x?.AsValue().ToDecimal() ?? decimal.MinValue)
                    );
                }
            }
            return max;
        }

        /// <summary>
        /// Returns the smallest number in an array.
        /// </summary>
        /// <example><code>min(array)</code><code>$min(array)</code><code>array|min</code></example>
        /// <returns>The smallest number in the array</returns>
        public static JsonNode Min(JsonNode[] input)
        {
            decimal min = decimal.MaxValue;
            foreach (JsonNode item in input)
            {
                if (item is JsonValue value)
                {
                    min = Math.Min(min, value.ToDecimal());
                }
                else if (item is JsonArray array)
                {
                    min = Math.Min(
                        min,
                        array.Min(x => x?.AsValue().ToDecimal() ?? decimal.MaxValue)
                    );
                }
            }
            return min;
        }

        /// <summary>
        /// Returns the average of all the numbers in an array.
        /// </summary>
        /// <example><code>average(array)</code><code>$average(array)</code><code>array|average</code></example>
        /// <returns>The average of all the numbers in the array</returns>
        public static JsonNode Average(JsonNode[] input)
        {
            decimal total = 0m;
            int count = 0;
            foreach (JsonNode item in input)
            {
                if (item is JsonValue value)
                {
                    total += value.ToDecimal();
                    count++;
                }
                else if (item is JsonArray array)
                {
                    total += array.Sum(x => x?.AsValue().ToDecimal() ?? 0);
                    count += array.Count;
                }
            }
            return total / count;
        }

        /// <summary>
        /// Casts the arg parameter to a boolean.
        /// In addition to the standard JSON boolean values, the following string values are also recognized: "true", "false", "1", and "0".
        /// </summary>
        /// <example><code>boolean(arg)</code><code>$boolean(arg)</code><code>arg|boolean</code></example>
        /// <returns>The boolean JsonValue that represents the input</returns>
        public static JsonNode ToBoolean(JsonNode input)
        {
            if (input is JsonValue value)
            {
                if (
                    value.GetValueKind() == JsonValueKind.String
                    && value.TryGetValue(out string sValue)
                )
                {
                    if (
                        sValue.Equals("true", StringComparison.CurrentCultureIgnoreCase)
                        || sValue.Equals("1", StringComparison.CurrentCultureIgnoreCase)
                    )
                    {
                        return true;
                    }
                    else if (
                        sValue.Equals("false", StringComparison.CurrentCultureIgnoreCase)
                        || sValue.Equals("0", StringComparison.CurrentCultureIgnoreCase)
                    )
                    {
                        return false;
                    }
                }
                try
                {
                    return value.ToBoolean();
                }
                catch (FormatException)
                {
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the logical negation of the input (first casts the input to a boolean if it is not already a boolean value)
        /// </summary>
        /// <example><code>not(arg)</code><code>$not(arg)</code><code>arg|not</code></example>
        /// <returns>The logical negation of the input</returns>
        public static JsonNode Not(JsonNode input)
        {
            JsonNode booleanValue = ToBoolean(input);
            if (booleanValue?.GetValueKind() == JsonValueKind.True)
            {
                return false;
            }
            else if (booleanValue?.GetValueKind() == JsonValueKind.False)
            {
                return true;
            }
            return null;
        }

        /// <summary>
        /// Evaluates a list of predicates and returns the first result expression whose predicate is satisfied.
        /// </summary>
        /// <example><code>switch(expression, case1, result1, case2, result2, ..., default)</code><code>$switch(expression, case1, result1, case2, result2, ..., default)</code><code>expression|switch(case1, result1, case2, result2, ..., default)</code></example>
        /// <returns>The result of the first case whose predicate is satisfied, or the default value if no case is satisfied</returns>
        public static JsonNode Case(JsonNode[] args)
        {
            if (args.Length < 3)
                return null;

            JsonNode expressionResult = args[0];

            for (int i = 1; i < args.Length - 1; i += 2)
            {
                JsonNode caseResult = args[i];
                if (JsonNode.DeepEquals(expressionResult, caseResult))
                {
                    return args[i + 1];
                }
            }
            // Return default
            if (args.Length % 2 == 0)
            {
                return args[args.Length - 1];
            }
            // Return null if no default specified
            return null;
        }

        /// <summary>
        /// Returns a new array with the elements of the input array and the elements of the other array.
        /// </summary>
        /// <example><code>append(array, other)</code><code>$append(array, other)</code><code>array|append(other)</code></example>
        /// <returns>A new array with the elements of the input array and the elements of the other array</returns>
        public static JsonNode ArrayAppend(JsonNode[] args)
        {
            JsonArray result = new JsonArray();
            foreach (JsonNode arg in args)
            {
                if (arg is JsonArray array)
                {
                    foreach (JsonNode item in array)
                    {
                        result.Add(item?.DeepClone());
                    }
                }
                else
                {
                    result.Add(arg?.DeepClone());
                }
            }
            return result;
        }

        /// <summary>
        /// Returns a new array with the elements of the input array in reverse order.
        /// </summary>
        /// <example><code>reverse(array)</code><code>$reverse(array)</code><code>array|reverse</code></example>
        /// <returns>A new array with the elements of the input array in reverse order</returns>
        public static JsonNode ArrayReverse(JsonNode input)
        {
            if (input is JsonArray array)
            {
                return new JsonArray(array.Select(x => x?.DeepClone()).Reverse().ToArray());
            }
            return null;
        }

        /// <summary>
        /// Returns a new array with the elements of the input array in random order.
        /// </summary>
        /// <example><code>shuffle(array)</code><code>$shuffle(array)</code><code>array|shuffle</code></example>
        /// <returns>A new array with the elements of the input array in random order</returns>
        public static JsonNode ArrayShuffle(JsonNode input)
        {
            if (input is JsonArray array)
            {
                return new JsonArray(
                    array.Select(x => x?.DeepClone()).OrderBy(x => Guid.NewGuid()).ToArray()
                );
            }
            return null;
        }

        private class JsonValueComparer : IComparer<JsonValue>
        {
            public int Compare(JsonValue x, JsonValue y)
            {
                if (x == null && y == null)
                {
                    return 0;
                }
                if (x == null)
                {
                    return -1;
                }
                if (y == null)
                {
                    return 1;
                }
                if (
                    x.GetValueKind() == JsonValueKind.Number
                    && y.GetValueKind() == JsonValueKind.Number
                )
                {
                    return x.ToDecimal().CompareTo(y.ToDecimal());
                }
                return string.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// Returns a new array with the elements of the input array sorted in ascending order.
        /// </summary>
        /// <example><code><code>array|sort</code></example>
        public static JsonNode ArraySort(
            JsonNode input,
            JsonNode expression = null,
            JsonNode descending = null
        )
        {
            if (input is JsonObject inputObject)
            {
                input = new JsonArray() { inputObject.DeepClone() };
            }

            if (input is JsonArray array)
            {
                bool isDescending =
                    descending is JsonValue descVal && descVal.GetValueKind() == JsonValueKind.True;
                Expression jExpression = null;
                if (expression != null)
                {
                    Jexl jexl = new Jexl(new ExtendedGrammar());
                    jExpression = jexl.CreateExpression(expression.ToString());
                }
                JsonValue getValue(JsonNode x)
                {
                    if (expression == null)
                    {
                        return x?.AsValue();
                    }
                    else if (x is JsonObject obj)
                    {
                        return jExpression.Eval(obj)?.AsValue();
                    }
                    else
                    {
                        var context = new JsonObject() { ["value"] = x?.DeepClone() };
                        return jExpression.Eval(context)?.AsValue();
                    }
                }

                var sortedArray = isDescending
                    ? array
                        .Select(x => x.DeepClone())
                        .OrderByDescending(getValue, new JsonValueComparer())
                        .ToArray()
                    : array
                        .Select(x => x.DeepClone())
                        .OrderBy(getValue, new JsonValueComparer())
                        .ToArray();

                return new JsonArray(sortedArray);
            }
            return null;
        }

        /// <summary>
        /// Returns a new array with the elements of the input array with duplicates removed.
        /// </summary>
        /// <example><code>distinct(array)</code><code>$distinct(array)</code><code>array|distinct</code></example>
        /// <returns>A new array with the elements of the input array with duplicates removed</returns>
        public static JsonNode ArrayDistinct(JsonNode input)
        {
            if (input is JsonArray array)
            {
                // Check for deep equality using JsonNode.DeepEquals
                JsonArray result = new JsonArray();
                foreach (JsonNode item in array)
                {
                    if (result.Any(x => JsonNode.DeepEquals(x, item)))
                    {
                        continue;
                    }
                    result.Add(item?.DeepClone());
                }
                return result;
            }
            return null;
        }

        /// <summary>
        /// Create a new object based on an array of key-value pairs.
        /// <example><code>toObject(array)</code><code>$fromEntries(array)</code><code>array|fromEntries</code><code>array|toObject</code></example>
        /// </summary>
        /// <returns>A new object based on an array of key-value pairs</returns>
        public static JsonNode ArrayToObject(JsonNode input, JsonNode val)
        {
            if (input is JsonObject inputObject)
            {
                return inputObject.DeepClone();
            }

            if (input is JsonArray array)
            {
                JsonObject result = new JsonObject();
                foreach (JsonNode item in array)
                {
                    if (item is JsonArray pair && pair.Count == 2)
                    {
                        if (pair[0] is JsonValue key && pair[1] is JsonNode value)
                        {
                            result[key.ToString()] = value.DeepClone();
                        }
                    }
                    else if (
                        item is JsonValue itemValue
                        && itemValue.GetValueKind() == JsonValueKind.String
                    )
                    {
                        result[itemValue.ToString()] = val?.DeepClone();
                    }
                }
                return result;
            }
            if (input is JsonValue inputValue && inputValue.GetValueKind() == JsonValueKind.String)
            {
                return new JsonObject() { [inputValue.ToString()] = val?.DeepClone() };
            }
            return null;
        }

        /// <summary>
        /// Returns a new array with the elements of the input array transformed by the specified map function.
        /// <example><code>mapField(array, field)</code><code>$mapField(array, field)</code><code>array|mapField(field)</code></example>
        /// </summary>
        /// <returns>A new array with the elements of the input array transformed by the specified map function</returns>
        public static JsonNode MapField(JsonNode input, JsonNode field)
        {
            if (input is JsonObject inputObject)
            {
                input = new JsonArray() { inputObject.DeepClone() };
            }

            if (input is JsonArray array && field is JsonValue fieldVal)
            {
                return new JsonArray(
                    array.Select(x => x?[fieldVal.ToString()]?.DeepClone()).ToArray()
                );
            }
            return null;
        }

        /// <summary>
        /// Returns an array containing the results of applying the expression parameter to each value in the array parameter.
        /// The expression must be a valid JEXL expression string, which is applied to each element of the array.
        /// The relative context provided to the expression is an object with the properties value, index and array (the original array).
        /// <example>
        /// For example (with context <c>{assoc: [{age: 20}, {age: 30}, {age: 40}]}</c>)
        /// <c>assoc|map('value.age')</c> returns an array containing the results of applying the expression 'value.age'
        /// to each value in the assoc array: <c>[20, 30, 40]</c>
        /// </example>
        /// </summary>
        /// <returns>A new array with the elements of the input array transformed by the specified map function</returns>
        public static JsonNode Map(JsonNode input, JsonNode expression)
        {
            if (input is JsonObject inputObject)
            {
                input = new JsonArray() { inputObject.DeepClone() };
            }

            if (input is JsonArray array)
            {
                Jexl jexl = new Jexl(new ExtendedGrammar());
                if (expression is JsonValue exprVal)
                {
                    Expression jExpression = jexl.CreateExpression(exprVal.ToString());
                    return new JsonArray(
                        array
                            .Select(
                                (x, i) =>
                                {
                                    var context = new JsonObject()
                                    {
                                        ["value"] = x?.DeepClone(),
                                        ["index"] = i,
                                        ["array"] = array.DeepClone(),
                                    };
                                    return jExpression.Eval(context)?.DeepClone();
                                }
                            )
                            .ToArray()
                    );
                }
                else if (
                    expression is JsonObject exprObject
                    && exprObject.TryGetPropertyValue("variables", out JsonNode variables)
                    && variables is JsonArray variablesArray
                    && exprObject.TryGetPropertyValue("expression", out JsonNode exprNode)
                    && exprNode is JsonValue exprValue
                )
                {
                    Node ast = JsonSerializer.Deserialize<Node>(
                        exprValue.ToString(),
                        SerializerOptions
                    );
                    Expression jExpression = jexl.CreateExpression(ast);
                    return new JsonArray(
                        array
                            .Select(
                                (x, i) =>
                                {
                                    var context = new JsonObject();
                                    if (variablesArray.ElementAtOrDefault(0) is JsonValue var1)
                                    {
                                        context[var1.ToString()] = x?.DeepClone();
                                    }
                                    if (variablesArray.ElementAtOrDefault(1) is JsonValue var2)
                                    {
                                        context[var2.ToString()] = i;
                                    }
                                    if (variablesArray.ElementAtOrDefault(2) is JsonValue var3)
                                    {
                                        context[var3.ToString()] = array.DeepClone();
                                    }
                                    return jExpression.Eval(context)?.DeepClone();
                                }
                            )
                            .ToArray()
                    );
                }
            }
            return null;
        }

        /// <summary>
        /// Checks whether the provided array has any elements that match the specified expression.
        /// The expression must be a valid JEXL expression string, and is applied to each element of the array.
        /// The relative context provided to the expression is an object with the properties value and array (the original array).
        /// <example>
        /// For example (with context <c>{assoc: [{age: 20}, {age: 30}, {age: 40}]}</c>)
        /// <c>assoc|some('value.age > 30')</c> returns true because at least one element
        /// in the assoc array has an age greater than 30.
        /// </example>
        /// </summary>
        /// <returns>true if the array has any elements that match the specified expression, otherwise it returns false</returns>
        public static JsonNode Any(JsonNode input, JsonNode expression)
        {
            if (input is JsonObject inputObject)
            {
                input = new JsonArray() { inputObject.DeepClone() };
            }

            if (input is JsonArray array)
            {
                Jexl jexl = new Jexl(new ExtendedGrammar());
                if (expression is JsonValue exprVal)
                {
                    Expression jExpression = jexl.CreateExpression(exprVal.ToString());
                    return array.Any(
                        (x) =>
                        {
                            var context = new JsonObject()
                            {
                                ["value"] = x?.DeepClone(),
                                ["array"] = array.DeepClone(),
                            };
                            return jExpression.Eval(context)?.DeepClone()?.AsValue().ToBoolean()
                                ?? false;
                        }
                    );
                }
                else if (
                    expression is JsonObject exprObject
                    && exprObject.TryGetPropertyValue("variables", out JsonNode variables)
                    && variables is JsonArray variablesArray
                    && exprObject.TryGetPropertyValue("expression", out JsonNode exprNode)
                    && exprNode is JsonValue exprValue
                )
                {
                    Node ast = JsonSerializer.Deserialize<Node>(
                        exprValue.ToString(),
                        SerializerOptions
                    );
                    Expression jExpression = jexl.CreateExpression(ast);
                    return array.Any(
                        (x) =>
                        {
                            var context = new JsonObject();
                            if (variablesArray.ElementAtOrDefault(0) is JsonValue var1)
                            {
                                context[var1.ToString()] = x?.DeepClone();
                            }
                            if (variablesArray.ElementAtOrDefault(1) is JsonValue var2)
                            {
                                context[var2.ToString()] = array.DeepClone();
                            }
                            return jExpression.Eval(context)?.DeepClone()?.AsValue().ToBoolean()
                                ?? false;
                        }
                    );
                }
            }
            return false;
        }

        /// <summary>
        /// Checks whether the provided array has all elements that match the specified expression.
        /// The expression must be a valid JEXL expression string, and is applied to each element of the array.
        /// The relative context provided to the expression is an object with the properties value and array (the original array).
        /// <example>
        /// For example (with context <c>{assoc: [{age: 20}, {age: 30}, {age: 40}]}</c>)
        /// <c>assoc|every('value.age > 30')</c> returns false because not all elements
        /// in the assoc array have an age greater than 30.
        /// </example>
        /// </summary>
        /// <returns>true if the array has all elements that match the specified expression, otherwise it returns false</returns>
        public static JsonNode All(JsonNode input, JsonNode expression)
        {
            if (input is JsonObject inputObject)
            {
                input = new JsonArray() { inputObject.DeepClone() };
            }

            if (input is JsonArray array)
            {
                Jexl jexl = new Jexl(new ExtendedGrammar());
                if (expression is JsonValue exprVal)
                {
                    Expression jExpression = jexl.CreateExpression(exprVal.ToString());
                    return array.All(
                        (x) =>
                        {
                            var context = new JsonObject()
                            {
                                ["value"] = x?.DeepClone(),
                                ["array"] = array.DeepClone(),
                            };
                            return jExpression.Eval(context)?.DeepClone()?.AsValue().ToBoolean()
                                ?? false;
                        }
                    );
                }
                else if (
                    expression is JsonObject exprObject
                    && exprObject.TryGetPropertyValue("variables", out JsonNode variables)
                    && variables is JsonArray variablesArray
                    && exprObject.TryGetPropertyValue("expression", out JsonNode exprNode)
                    && exprNode is JsonValue exprValue
                )
                {
                    Node ast = JsonSerializer.Deserialize<Node>(
                        exprValue.ToString(),
                        SerializerOptions
                    );
                    Expression jExpression = jexl.CreateExpression(ast);
                    return array.All(
                        (x) =>
                        {
                            var context = new JsonObject();
                            if (variablesArray.ElementAtOrDefault(0) is JsonValue var1)
                            {
                                context[var1.ToString()] = x?.DeepClone();
                            }
                            if (variablesArray.ElementAtOrDefault(1) is JsonValue var2)
                            {
                                context[var2.ToString()] = array.DeepClone();
                            }
                            return jExpression.Eval(context)?.DeepClone()?.AsValue().ToBoolean()
                                ?? false;
                        }
                    );
                }
            }
            return false;
        }

        /// <summary>
        /// Returns a new array with the elements of the input array that match the specified expression.
        /// The expression must be a valid JEXL expression string, and is applied to each element of the array.
        /// The relative context provided to the expression is an object with the properties value and array (the original array).
        /// <example>
        /// For example (with context <c>{assoc: [{age: 20}, {age: 30}, {age: 40}]}</c>)
        /// <c>assoc|filter('value.age > 30')</c> returns a new array containing only the element
        /// in the assoc array with an age greater than 30: <c>[{age: 40}]</c>
        /// </example>
        /// </summary>
        /// <returns>A new array with the elements of the input array that match the specified expression</returns>
        public static JsonNode Filter(JsonNode input, JsonNode expression)
        {
            if (input is JsonObject inputObject)
            {
                input = new JsonArray() { inputObject.DeepClone() };
            }

            if (input is JsonArray array)
            {
                Jexl jexl = new Jexl(new ExtendedGrammar());
                if (expression is JsonValue exprVal)
                {
                    Expression jExpression = jexl.CreateExpression(exprVal.ToString());
                    JsonArray filteredArray = new JsonArray();
                    for (int i = 0; i < array.Count; i++)
                    {
                        var context = new JsonObject()
                        {
                            ["value"] = array[i]?.DeepClone(),
                            ["index"] = i,
                            ["array"] = array.DeepClone(),
                        };
                        if (jExpression.Eval(context)?.AsValue().ToBoolean() ?? false)
                        {
                            filteredArray.Add(array[i]?.DeepClone());
                        }
                    }
                    return filteredArray;
                }
                else if (
                    expression is JsonObject exprObject
                    && exprObject.TryGetPropertyValue("variables", out JsonNode variables)
                    && variables is JsonArray variablesArray
                    && exprObject.TryGetPropertyValue("expression", out JsonNode exprNode)
                    && exprNode is JsonValue exprValue
                )
                {
                    Node ast = JsonSerializer.Deserialize<Node>(
                        exprValue.ToString(),
                        SerializerOptions
                    );
                    Expression jExpression = jexl.CreateExpression(ast);
                    JsonArray filteredArray = new JsonArray();
                    for (int i = 0; i < array.Count; i++)
                    {
                        var context = new JsonObject();
                        if (variablesArray.ElementAtOrDefault(0) is JsonValue var1)
                        {
                            context[var1.ToString()] = array[i]?.DeepClone();
                        }
                        if (variablesArray.ElementAtOrDefault(1) is JsonValue var2)
                        {
                            context[var2.ToString()] = i;
                        }
                        if (variablesArray.ElementAtOrDefault(2) is JsonValue var3)
                        {
                            context[var3.ToString()] = array.DeepClone();
                        }
                        if (jExpression.Eval(context)?.AsValue().ToBoolean() ?? false)
                        {
                            filteredArray.Add(array[i]?.DeepClone());
                        }
                    }
                    return filteredArray;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds the first element in an array that matches the specified expression.
        /// The expression must be a valid JEXL expression string, and is applied to each element of the array.
        /// The relative context provided to the expression is an object with the properties value and array (the original array).
        /// <example>
        /// For example (with context <c>{assoc: [{age: 20}, {age: 30}, {age: 40}]}</c>)
        /// <c>assoc|find('value.age > 29')</c> returns the first element
        /// in the assoc array with an age greater than 29: <c>{age: 30}</c>
        /// This example is equivalent to <c>assoc[.age > 29][0]</c>.
        /// </example>
        /// </summary>
        /// <returns>The first matching element in the array</returns>
        public static JsonNode ArrayFind(JsonNode input, JsonNode expression)
        {
            if (input is JsonObject inputObject)
            {
                input = new JsonArray() { inputObject.DeepClone() };
            }

            if (input is JsonArray array)
            {
                Jexl jexl = new Jexl(new ExtendedGrammar());
                if (expression is JsonValue exprVal)
                {
                    Expression jExpression = jexl.CreateExpression(exprVal.ToString());
                    for (int i = 0; i < array.Count; i++)
                    {
                        var context = new JsonObject()
                        {
                            ["value"] = array[i]?.DeepClone(),
                            ["index"] = i,
                            ["array"] = array.DeepClone(),
                        };
                        if (jExpression.Eval(context)?.AsValue().ToBoolean() ?? false)
                        {
                            return array[i]?.DeepClone();
                        }
                    }
                    return null;
                }
                else if (
                    expression is JsonObject exprObject
                    && exprObject.TryGetPropertyValue("variables", out JsonNode variables)
                    && variables is JsonArray variablesArray
                    && exprObject.TryGetPropertyValue("expression", out JsonNode exprNode)
                    && exprNode is JsonValue exprValue
                )
                {
                    Node ast = JsonSerializer.Deserialize<Node>(
                        exprValue.ToString(),
                        SerializerOptions
                    );
                    Expression jExpression = jexl.CreateExpression(ast);
                    for (int i = 0; i < array.Count; i++)
                    {
                        var context = new JsonObject();
                        if (variablesArray.ElementAtOrDefault(0) is JsonValue var1)
                        {
                            context[var1.ToString()] = array[i]?.DeepClone();
                        }
                        if (variablesArray.ElementAtOrDefault(1) is JsonValue var2)
                        {
                            context[var2.ToString()] = i;
                        }
                        if (variablesArray.ElementAtOrDefault(2) is JsonValue var3)
                        {
                            context[var3.ToString()] = array.DeepClone();
                        }
                        if (jExpression.Eval(context)?.AsValue().ToBoolean() ?? false)
                        {
                            return array[i]?.DeepClone();
                        }
                    }
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns an aggregated value derived from applying the function parameter successively to each value
        /// in array in combination with the result of the previous application of the function.
        /// The expression must be a valid JEXL expression string, and behaves like an infix operator between each value within the array.
        /// The relative context provided to the expression is an object with the properties accumulator, value, index and array (the original array).
        /// <example>
        /// For example (with context <c>{assoc: [{age: 20}, {age: 30}, {age: 40}]}</c>)
        /// <c>assoc|reduce('accumulator + value.age',0)</c> returns the sum of all age values: <c>90</c>
        /// </example>
        /// </summary>
        /// <returns>A new array with the elements of the input array transformed by the specified map function</returns>
        public static JsonNode Reduce(
            JsonNode input,
            JsonNode expression,
            JsonNode initialValue = null
        )
        {
            if (input is JsonObject inputObject)
            {
                input = new JsonArray() { inputObject.DeepClone() };
            }

            if (input is JsonArray array)
            {
                Jexl jexl = new Jexl(new ExtendedGrammar());
                if (expression is JsonValue exprVal)
                {
                    Expression jExpression = jexl.CreateExpression(exprVal.ToString());
                    JsonNode accumulator = initialValue?.DeepClone();
                    for (int i = 0; i < array.Count; i++)
                    {
                        var context = new JsonObject()
                        {
                            ["accumulator"] = accumulator?.DeepClone(),
                            ["value"] = array[i]?.DeepClone(),
                            ["index"] = i,
                            ["array"] = array.DeepClone(),
                        };
                        accumulator = jExpression.Eval(context)?.DeepClone();
                    }
                    return accumulator;
                }
                else if (
                    expression is JsonObject exprObject
                    && exprObject.TryGetPropertyValue("variables", out JsonNode variables)
                    && variables is JsonArray variablesArray
                    && exprObject.TryGetPropertyValue("expression", out JsonNode exprNode)
                    && exprNode is JsonValue exprValue
                )
                {
                    Node ast = JsonSerializer.Deserialize<Node>(
                        exprValue.ToString(),
                        SerializerOptions
                    );
                    Expression jExpression = jexl.CreateExpression(ast);
                    JsonNode accumulator = initialValue?.DeepClone();
                    for (int i = 0; i < array.Count; i++)
                    {
                        var context = new JsonObject();
                        if (variablesArray.ElementAtOrDefault(0) is JsonValue var1)
                        {
                            context[var1.ToString()] = accumulator?.DeepClone();
                        }
                        if (variablesArray.ElementAtOrDefault(1) is JsonValue var2)
                        {
                            context[var2.ToString()] = array[i]?.DeepClone();
                        }
                        if (variablesArray.ElementAtOrDefault(2) is JsonValue var3)
                        {
                            context[var3.ToString()] = i;
                        }
                        if (variablesArray.ElementAtOrDefault(3) is JsonValue var4)
                        {
                            context[var4.ToString()] = array.DeepClone();
                        }
                        accumulator = jExpression.Eval(context)?.DeepClone();
                    }
                    return accumulator;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the keys of an object.
        /// </summary>
        /// <example><code>keys(object)</code><code>$keys(object)</code><code>object|keys</code></example>
        /// <returns>The keys of the input object</returns>
        public static JsonNode ObjectKeys(JsonNode input)
        {
            if (input is JsonObject obj)
            {
                return new JsonArray(obj.Select(x => JsonValue.Create(x.Key)).ToArray());
            }
            return null;
        }

        /// <summary>
        /// Returns the values of an object.
        /// </summary>
        /// <example><code>values(object)</code><code>$values(object)</code><code>object|values</code></example>
        /// <returns>The values of the input object</returns>
        public static JsonNode ObjectValues(JsonNode input)
        {
            if (input is JsonObject obj)
            {
                return new JsonArray(obj.Select(x => x.Value?.DeepClone()).ToArray());
            }
            return null;
        }

        /// <summary>
        /// Returns an array of key-value pairs from the input object.
        /// <example><code>entries(object)</code><code>$entries(object)</code><code>object|entries</code></example>
        /// </summary>
        /// <returns>The values of the input object</returns>
        public static JsonNode ObjectEntries(JsonNode input)
        {
            if (input is JsonObject obj)
            {
                return new JsonArray(
                    obj.Select(
                            x =>
                                new JsonArray(
                                    new[] { JsonValue.Create(x.Key), x.Value?.DeepClone() }
                                )
                        )
                        .ToArray()
                );
            }
            return null;
        }

        /// <summary>
        /// Returns a new object with the properties of the input objects merged together.
        /// </summary>
        /// <example><code>merge(object, other)</code><code>$merge(object, other)</code><code>object|merge(other)</code></example>
        /// <returns>A new object with the properties of the input objects merged together</returns>
        public static JsonNode ObjectMerge(JsonNode[] args)
        {
            JsonObject result = new JsonObject();
            foreach (JsonNode arg in args)
            {
                if (arg is JsonObject obj)
                {
                    foreach (var item in obj)
                    {
                        result[item.Key] = item.Value?.DeepClone();
                    }
                }
                else if (arg is JsonArray array)
                {
                    foreach (JsonNode item in array)
                    {
                        if (item is JsonObject objItem)
                        {
                            foreach (var subItem in objItem)
                            {
                                result[subItem.Key] = subItem.Value?.DeepClone();
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the current date and time in the ISO 8601 format.
        /// </summary>
        /// <example><code>now()</code><code>$now()</code><code>now</code></example>
        /// <returns>The current date and time in the ISO 8601 format</returns>
        public static JsonNode Now()
        {
            return DateTimeOffset.UtcNow.ToString("o");
        }

        /// <summary>
        /// Returns the current date and time in milliseconds since the Unix epoch.
        /// </summary>
        /// <example><code>millis()</code><code>$millis()</code><code>millis</code></example>
        /// <returns>The current date and time in milliseconds since the Unix epoch</returns>
        public static JsonNode Millis()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Parses the number of milliseconds since the Unix epoch or parses a string (with or without specified format) and returns the date and time in the ISO 8601 format
        /// </summary>
        /// <example><code>millisToDateTime(millis)</code><code>$millisToDateTime(millis)</code><code>millis|millisToDateTime</code></example>
        /// <returns>The date and time in the ISO 8601 format</returns>
        public static JsonNode ToDateTime(JsonNode input, JsonNode format = null)
        {
            if (input is JsonValue value)
            {
                if (value.GetValueKind() == JsonValueKind.Number)
                {
                    long millis = value.ToInt64();
                    return DateTimeOffset.FromUnixTimeMilliseconds(millis).ToString("o");
                }
                else if (
                    value.GetValueKind() == JsonValueKind.String
                    && (format == null || format.GetValueKind() != JsonValueKind.String)
                )
                {
                    return DateTimeOffset
                        .Parse(value.ToString(), null, DateTimeStyles.AssumeUniversal)
                        .ToString("o");
                }
                else if (
                    value.GetValueKind() == JsonValueKind.String
                    && format.GetValueKind() == JsonValueKind.String
                )
                {
                    return DateTimeOffset
                        .ParseExact(
                            value.ToString(),
                            format.ToString(),
                            null,
                            DateTimeStyles.AssumeUniversal
                        )
                        .ToString("o");
                }
            }
            else if (input is null && format is JsonValue formatValue)
            {
                return DateTimeOffset.UtcNow.ToString(formatValue.ToString());
            }
            else if (input is null && format is null)
            {
                return DateTimeOffset.UtcNow.ToString("o");
            }
            return null;
        }

        /// <summary>
        /// Converts a date and time to a provided format
        /// </summary>
        /// <example><code>dateTimeFormat(datetime, format)</code><code>$dateTimeFormat(datetime, format)</code><code>datetime|dateTimeFormat(format)</code></example>
        /// <returns>The date and time in the specified format</returns>
        public static JsonNode DateTimeFormat(JsonNode input, JsonNode format)
        {
            if (input is JsonValue value && format is JsonValue formatValue)
            {
                DateTimeOffset dateTime;
                if (value.GetValueKind() == JsonValueKind.String)
                {
                    dateTime = DateTimeOffset.Parse(
                        value.ToString(),
                        null,
                        DateTimeStyles.AssumeUniversal
                    );
                }
                else if (value.GetValueKind() == JsonValueKind.Number)
                {
                    long millis = value.ToInt64();
                    dateTime = DateTimeOffset.FromUnixTimeMilliseconds(millis);
                }
                else
                {
                    return null;
                }
                return dateTime.ToString(formatValue.ToString());
            }
            return null;
        }

        /// <summary>
        /// Parses the date and time in the ISO 8601 format and returns the number of milliseconds since the Unix epoch.
        /// </summary>
        /// <example><code>dateTimeToMillis(datetime)</code><code>$dateTimeToMillis(datetime)</code><code>datetime|dateTimeToMillis</code></example>
        /// <returns>The number of milliseconds since the Unix epoch</returns>
        public static JsonNode DateTimeToMillis(JsonNode input)
        {
            if (input is JsonValue value)
            {
                string datetime = value.ToString();
                return DateTimeOffset
                    .Parse(datetime, null, DateTimeStyles.AssumeUniversal)
                    .ToUnixTimeMilliseconds();
            }
            return null;
        }

        /// <summary>
        /// Adds a time range to a date and time in the ISO 8601 format.
        /// </summary>
        /// <example><code>datetime|dateAdd('week',1)</code></example>
        /// <returns>The number of milliseconds since the Unix epoch</returns>
        public static JsonNode DateTimeAdd(JsonNode input, JsonNode unit, JsonNode value)
        {
            if (
                input is JsonValue inputVal
                && unit is JsonValue unitVal
                && value is JsonValue valueVal
            )
            {
                string datetime = inputVal.ToString();
                DateTimeOffset dt = DateTimeOffset.Parse(
                    datetime,
                    null,
                    DateTimeStyles.AssumeUniversal
                );
                switch (unitVal.ToString())
                {
                    case "years":
                    case "year":
                        dt = dt.AddYears(valueVal.ToInt32());
                        break;
                    case "months":
                    case "month":
                        dt = dt.AddMonths(valueVal.ToInt32());
                        break;
                    case "weeks":
                    case "week":
                        dt = dt.AddDays(valueVal.ToInt32() * 7);
                        break;
                    case "days":
                    case "day":
                        dt = dt.AddDays(valueVal.ToInt32());
                        break;
                    case "hours":
                    case "hour":
                        dt = dt.AddHours(valueVal.ToInt32());
                        break;
                    case "minutes":
                    case "minute":
                        dt = dt.AddMinutes(valueVal.ToInt32());
                        break;
                    case "seconds":
                    case "second":
                        dt = dt.AddSeconds(valueVal.ToInt32());
                        break;
                    case "milliseconds":
                    case "millisecond":
                        dt = dt.AddMilliseconds(valueVal.ToInt32());
                        break;
                }
                return dt.ToString("o");
            }
            return null;
        }

        /// <summary>
        /// Evaluate provided and return the result.
        /// If only one argument is provided, it is expected that the first argument is a JEXL expression.
        /// If two arguments are provided, the first argument is the context (must be an object) and the second argument is the JEXL expression.
        /// The expression uses the default JEXL extended grammar and can't use any custom defined functions or transforms.
        /// <example><code>eval(expression)</code><code>object|eval(expression)</code></example>
        /// </summary>
        /// <param name="input">The input object (context) or JEXL expression (if second argument isn't defined)</param>
        /// <param name="expression">The JEXL expression</param>
        /// <returns>The result of the evaluation</returns>
        public static JsonNode Eval(JsonNode input, JsonNode expression)
        {
            // If no second argument is provided, it is expected that the first argument is a JEXL expression
            if (input is JsonValue inputValue && expression == null)
            {
                Jexl jexl = new Jexl(new ExtendedGrammar());
                return jexl.Eval(inputValue.ToString());
            }
            // If two arguments are provided, the first argument is the context (must be an object) and the second argument is the JEXL expression
            else if (input is JsonObject inputObject && expression is JsonValue exprVal)
            {
                Jexl jexl = new Jexl(new ExtendedGrammar());
                return jexl.Eval(exprVal.ToString(), inputObject.DeepClone().AsObject());
            }
            return null;
        }

        /// <summary>
        /// Generate a new UUID (Universally Unique Identifier).
        /// <example><code>uid()</code><code>$uuid()</code></example>
        /// </summary>
        public static JsonNode Uuid()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Checks whether the provided value is an array
        /// </summary>
        /// <example><code>isArray(value)</code><code>$isArray(value)</code></example>
        public static JsonNode IsArray(JsonNode input)
        {
            return input is JsonArray;
        }

        /// <summary>
        /// Checks whether the provided value is an object
        /// </summary>
        /// <example><code>isObject(value)</code><code>$isObject(value)</code></example>
        public static JsonNode IsObject(JsonNode input)
        {
            return input is JsonObject;
        }
    }
}
