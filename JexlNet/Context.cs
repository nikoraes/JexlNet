namespace JexlNet;

public class Context : Dictionary<string, dynamic>
{
    public Context() : base() { }
    public Context(IDictionary<string, dynamic> dictionary) : base(dictionary) { }
}