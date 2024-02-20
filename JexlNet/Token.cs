using System;
using System.Text.Json.Nodes;

namespace JexlNet
{
    public class Token : IEquatable<Token>
    {
        public Token(GrammarType type, JsonNode value = null)
        {
            Type = type;
            Value = value;
        }
        public Token(GrammarType type, string raw, JsonNode value = null)
        {
            Type = type;
            Value = value;
            Raw = raw;
        }

        public GrammarType Type { get; set; }
        public JsonNode Value { get; set; }
        public string Raw { get; set; }

        public bool Equals(Token other)
        {
            if (other == null)
            {
                return false;
            }
            return
                Raw == other.Raw &&
                Type == other.Type &&
                JsonNode.DeepEquals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            throw new ApplicationException("This class does not support GetHashCode and should not be used as a key for a dictionary");
        }
    }
}