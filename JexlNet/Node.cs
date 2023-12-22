namespace JexlNet;

public class Node : Token, IEquatable<Node>
{
    public Node(string type) : base(type) { }
    public Node(string type, dynamic value) : base(type)
    {
        Value = value;
    }
    public Node(Token token) : base(token.Type)
    {
        Value = token.Value;
        Raw = token.Raw;
    }
    public Node? Left { get; set; }
    public Node? Right { get; set; }
    public Node? Parent { get; set; }
    public bool? Writable { get; set; }
    public List<Node>? Args { get; set; }
    public string? Operator { get; set; }
    public Node? Expr { get; set; }
    public bool? Relative { get; set; }
    public Node? Subject { get; set; }
    public string? Name { get; set; }
    public string? Pool { get; set; }
    public Node? From { get; set; } // Used for chained identifiers
    public Node? Alternate { get; set; }
    public Node? Consequent { get; set; }
    public Node? Test { get; set; }

    public bool Equals(Node? other)
    {
        if (other == null) return false;
        bool equalValue;
        if (Value is Dictionary<string, Node> dictionary && other.Value is Dictionary<string, Node> dictionary1)
        {
            if (dictionary.Count != dictionary1.Count)
            {
                return false;
            }
            equalValue = true;
            foreach (var key in dictionary.Keys)
            {
                if (!dictionary1.ContainsKey(key))
                {
                    equalValue = false;
                    break;
                }
                if (!dictionary[key].Equals(dictionary1[key]))
                {
                    equalValue = false;
                    break;
                }
            }
        }
        else if (Value is List<Node> list && other.Value is List<Node> list1)
        {
            equalValue = list.SequenceEqual(list1);
        }
        else
        {
            equalValue = Value == other.Value;
        }
        return
            Type == other.Type &&
            equalValue &&
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

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj.GetType() != typeof(Node)) return false;
        return Equals(obj as Node);
    }

    public override int GetHashCode()
    {
        throw new ApplicationException("This class does not support GetHashCode and should not be used as a key for a dictionary");
    }
}
