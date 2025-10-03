using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JexlNet
{
    public static class JsonObjectExtensions
    {
        public static void RemoveDuplicateKeys(this JsonObject obj)
        {
            if (obj == null)
                return;
            // Collect all key-value pairs in order
            var keyValuePairs = new List<KeyValuePair<string, JsonNode>>();
            foreach (var property in obj)
            {
                keyValuePairs.Add(new KeyValuePair<string, JsonNode>(property.Key, property.Value));
            }
            // Clear the object
            obj.Clear();
            // Add only the last occurrence of each key
            var seen = new HashSet<string>();
            for (int i = keyValuePairs.Count - 1; i >= 0; i--)
            {
                var kvp = keyValuePairs[i];
                if (!seen.Contains(kvp.Key))
                {
                    obj.Add(kvp.Key, kvp.Value);
                    seen.Add(kvp.Key);
                }
            }
            // Restore order (with last occurrence kept)
            var reordered = new List<KeyValuePair<string, JsonNode>>();
            var keysList = new List<string>();
            foreach (var property in obj)
            {
                keysList.Add(property.Key);
            }
            foreach (var key in keysList)
            {
                reordered.Add(new KeyValuePair<string, JsonNode>(key, obj[key]));
            }
            obj.Clear();
            foreach (var kvp in reordered)
            {
                obj.Add(kvp.Key, kvp.Value);
            }
            // Recursively process nested JsonObjects
            keysList.Clear();
            foreach (var property in obj)
            {
                keysList.Add(property.Key);
            }
            foreach (var key in keysList)
            {
                var value = obj[key];
                var nestedObj = value as JsonObject;
                if (nestedObj != null)
                {
                    nestedObj.RemoveDuplicateKeys();
                }
            }
        }
    }

    public static class JsonValueExtensions
    {
        /// <summary>
        /// Converts a JsonValue to a decimal.
        /// </summary>
        /// <param name="value"></param>
        public static decimal ToDecimal(this JsonValue value)
        {
            if (value.TryGetValue(out decimal valDecimal))
            {
                return valDecimal;
            }
            else if (value.TryGetValue(out int valInt))
            {
                return Convert.ToDecimal(valInt);
            }
            else if (value.TryGetValue(out double valDouble))
            {
                return Convert.ToDecimal(valDouble);
            }
            else if (value.TryGetValue(out long valLong))
            {
                return Convert.ToDecimal(valLong);
            }
            else if (value.TryGetValue(out string valString))
            {
                return decimal.Parse(valString, NumberStyles.Any, CultureInfo.InvariantCulture);
            }
            else if (value.TryGetValue(out object valObj))
            {
                return Convert.ToDecimal(valObj);
            }
            else
            {
                throw new Exception("Unsupported type");
            }
        }

        /// <summary>
        /// Converts a JsonValue to a double.
        /// </summary>
        /// <param name="value"></param>
        public static double ToDouble(this JsonValue value)
        {
            if (value.TryGetValue(out double valDouble))
            {
                return valDouble;
            }
            else if (value.TryGetValue(out int valInt))
            {
                return Convert.ToDouble(valInt);
            }
            else if (value.TryGetValue(out decimal valDecimal))
            {
                return Convert.ToDouble(valDecimal);
            }
            else if (value.TryGetValue(out long valLong))
            {
                return Convert.ToDouble(valLong);
            }
            else if (value.TryGetValue(out string valString))
            {
                return double.Parse(valString, System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (value.TryGetValue(out object valObj))
            {
                return Convert.ToDouble(valObj);
            }
            else
            {
                throw new Exception("Unsupported type");
            }
        }

        /// <summary>
        /// Converts a JsonValue to a int32.
        /// </summary>
        /// <param name="value"></param>
        public static int ToInt32(this JsonValue value)
        {
            if (value.TryGetValue(out int valInt))
            {
                return valInt;
            }
            else if (value.TryGetValue(out double valDouble))
            {
                return Convert.ToInt32(valDouble);
            }
            else if (value.TryGetValue(out decimal valDecimal))
            {
                return decimal.ToInt32(valDecimal);
            }
            else if (value.TryGetValue(out long valLong))
            {
                return Convert.ToInt32(valLong);
            }
            else if (value.TryGetValue(out string valString))
            {
                return int.Parse(valString, System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (value.TryGetValue(out object valObj))
            {
                return Convert.ToInt32(valObj);
            }
            else
            {
                throw new Exception("Unsupported type");
            }
        }

        /// <summary>
        /// Converts a JsonValue to a int64.
        /// </summary>
        /// <param name="value"></param>
        public static long ToInt64(this JsonValue value)
        {
            if (value.TryGetValue(out long valLong))
            {
                return valLong;
            }
            else if (value.TryGetValue(out int valInt))
            {
                return Convert.ToInt64(valInt);
            }
            else if (value.TryGetValue(out double valDouble))
            {
                return Convert.ToInt64(valDouble);
            }
            else if (value.TryGetValue(out decimal valDecimal))
            {
                return decimal.ToInt64(valDecimal);
            }
            else if (value.TryGetValue(out string valString))
            {
                return long.Parse(valString, System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (value.TryGetValue(out object valObj))
            {
                return Convert.ToInt64(valObj);
            }
            else
            {
                throw new Exception("Unsupported type");
            }
        }

        /// <summary>
        /// Converts a JsonValue to a boolean.
        /// </summary>
        /// <param name="value"></param>
        public static bool ToBoolean(this JsonValue value)
        {
            if (value.TryGetValue(out bool valBool))
            {
                return valBool;
            }
            else if (value.GetValueKind() == JsonValueKind.Number)
            {
                return value.ToDecimal() != 0;
            }
            else if (value.TryGetValue(out object valObj))
            {
                return Convert.ToBoolean(valObj);
            }
            else
            {
                throw new Exception("Unsupported type");
            }
        }
    }
}
