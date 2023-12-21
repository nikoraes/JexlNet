
namespace JexlNet;

public class ParserStateTokenType(string toState, Func<Token>? handler = null)
{
    public string ToState { get; set; } = toState;
    public Func<Token>? Handler { get; set; } = handler;
}

public class ParserState
{
    public Dictionary<string, ParserStateTokenType> TokenTypes
}

public class ParserStates
{

}