using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JexlNet
{
    public class Node : Token, IEquatable<Node>
    {
        [JsonConstructorAttribute]
        public Node() { }
        public Node(GrammarType type, JsonNode value = null) : base(type, value) { }
        public Node(GrammarType type, bool value) : base(type, JsonValue.Create(value)) { }
        public Node(GrammarType type, decimal value) : base(type, JsonValue.Create(value)) { }
        public Node(GrammarType type, string value) : base(type, JsonValue.Create(value)) { }
        public Node(Token token) : base(token.Type, token.Raw, token.Value) { }
        public Node Left { get; set; }
        public Node Right { get; set; }
        public Node Parent { get; set; }
        public List<Node> Args { get; set; }
        public List<Node> Array { get; set; }
        public Dictionary<string, Node> Object { get; set; }
        public string Operator { get; set; }
        public Node Expr { get; set; }
        public bool? Relative { get; set; }
        public Node Subject { get; set; }
        public string Name { get; set; }
        public Grammar.PoolType? Pool { get; set; }
        public Node From { get; set; } // Used for chained identifiers
        public Node Alternate { get; set; }
        public Node Consequent { get; set; }
        public Node Test { get; set; }

        public bool Equals(Node other)
        {
            if (other == null) return false;
            return
                Type == other.Type &&
                JsonNode.DeepEquals(Value, other.Value) &&
                Operator == other.Operator &&
                Relative == other.Relative &&
                ((Left == null && other.Left == null) || (Left != null && Left.Equals(other.Left))) &&
                ((Right == null && other.Right == null) || (Right != null && Right.Equals(other.Right))) &&
                ((From == null && other.From == null) || (From != null && From.Equals(other.From))) &&
                Name == other.Name &&
                Pool == other.Pool &&
                ((Args == null && other.Args == null) || (Args != null && other.Args != null && Args.SequenceEqual(other.Args))) &&
                ((Test == null && other.Test == null) || (Test != null && Test.Equals(other.Test))) &&
                ((Consequent == null && other.Consequent == null) || (Consequent != null && Consequent.Equals(other.Consequent))) &&
                ((Alternate == null && other.Alternate == null) || (Alternate != null && Alternate.Equals(other.Alternate)));
        }

        public override int GetHashCode()
        {
            throw new ApplicationException("This class does not support GetHashCode and should not be used as a key for a dictionary");
        }
    }
}
