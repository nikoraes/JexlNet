using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JexlNet
{
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
                return decimal.Parse(valString, System.Globalization.CultureInfo.InvariantCulture);
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