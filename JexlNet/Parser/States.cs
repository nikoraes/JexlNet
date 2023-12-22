
namespace JexlNet;

public class ParserStateTokenType(string? toState = null, Action<Parser, Node?>? handler = null)
{
    public string? ToState { get; set; } = toState;
    public Action<Parser, Node?>? Handler { get; set; } = handler;
}

public class ParserState()
{
    public Dictionary<string, ParserStateTokenType>? TokenTypes { get; set; }
    public bool? Completable { get; set; }
    public Dictionary<string, string>? EndStates { get; set; }
    public Action<Parser, Node?>? SubHandler { get; set; }

}

public class ParserStates : Dictionary<string, ParserState>
{
    public ParserStates() : base()
    {
        Add("expectOperand", new ParserState()
        {
            TokenTypes = new Dictionary<string, ParserStateTokenType>() {
                { "literal", new ParserStateTokenType("expectBinOp") },
                { "identifier", new ParserStateTokenType("identifier") },
                { "unaryOp", new ParserStateTokenType() },
                { "openParen", new ParserStateTokenType("subExpression") },
                { "openCurl", new ParserStateTokenType("expectObjKey", ParserHandlers.ObjectStart) },
                { "dot", new ParserStateTokenType("traverse") },
                { "openBracket", new ParserStateTokenType("arrayVal", ParserHandlers.ArrayStart) }
            }
        });

        Add("expectBinOp", new ParserState()
        {
            TokenTypes = new Dictionary<string, ParserStateTokenType>() {
                { "binaryOp", new ParserStateTokenType("expectOperand") },
                { "pipe", new ParserStateTokenType("expectTransform") },
                { "dot", new ParserStateTokenType("traverse") },
                { "question", new ParserStateTokenType("ternaryMid", ParserHandlers.TernaryStart) }
            },
            Completable = true
        });

        Add("expectTransform", new ParserState()
        {
            TokenTypes = new Dictionary<string, ParserStateTokenType>() {
                { "identifier", new ParserStateTokenType("postTransform", ParserHandlers.Transform) }
            }
        });

        Add("expectObjKey", new ParserState()
        {
            TokenTypes = new Dictionary<string, ParserStateTokenType>() {
                { "literal", new ParserStateTokenType("expectKeyValSep", ParserHandlers.ObjectKey) },
                { "identifier", new ParserStateTokenType("expectKeyValSep", ParserHandlers.ObjectKey) },
                { "closeCurl", new ParserStateTokenType("expectBinOp") }
            }
        });

        Add("expectKeyValSep", new ParserState()
        {
            TokenTypes = new Dictionary<string, ParserStateTokenType>() {
                { "colon", new ParserStateTokenType("objVal") }
            }
        });

        Add("postTransform", new ParserState()
        {
            TokenTypes = new Dictionary<string, ParserStateTokenType>() {
                { "openParen", new ParserStateTokenType("argVal") },
                { "binaryOp", new ParserStateTokenType("expectOperand") },
                { "dot", new ParserStateTokenType("traverse") },
                { "openBracket", new ParserStateTokenType("filter") },
                { "pipe", new ParserStateTokenType("expectTransform") },
            },
            Completable = true
        });

        Add("postArgs", new ParserState()
        {
            TokenTypes = new Dictionary<string, ParserStateTokenType>() {
                { "binaryOp", new ParserStateTokenType("expectOperand") },
                { "dot", new ParserStateTokenType("traverse") },
                { "openBracket", new ParserStateTokenType("filter") },
                { "pipe", new ParserStateTokenType("expectTransform") },
            },
            Completable = true
        });

        Add("identifier", new ParserState()
        {
            TokenTypes = new Dictionary<string, ParserStateTokenType>() {
                { "binaryOp", new ParserStateTokenType("expectOperand") },
                { "dot", new ParserStateTokenType("traverse") },
                { "openBracket", new ParserStateTokenType("filter") },
                { "openParen", new ParserStateTokenType("argVal", ParserHandlers.FunctionCall) },
                { "pipe", new ParserStateTokenType("expectTransform") },
                { "question", new ParserStateTokenType("ternaryMid", ParserHandlers.TernaryStart) }
            },
            Completable = true
        });

        Add("traverse", new ParserState()
        {
            TokenTypes = new Dictionary<string, ParserStateTokenType>() {
                { "identifier", new ParserStateTokenType("identifier") }
            }
        });

        Add("filter", new ParserState()
        {
            SubHandler = ParserHandlers.Filter,
            EndStates = new() {
                { "closeBracket", "identifier"}
            }
        });

        Add("subExpression", new ParserState()
        {
            SubHandler = ParserHandlers.SubExpression,
            EndStates = new() {
                { "closeParen", "expectBinOp"}
            }
        });

        Add("argVal", new ParserState()
        {
            SubHandler = ParserHandlers.ArgumentValue,
            EndStates = new() {
                { "comma", "argVal"},
                { "closeParen", "postArgs"}
            }
        });

        Add("objVal", new ParserState()
        {
            SubHandler = ParserHandlers.ObjectValue,
            EndStates = new() {
                { "comma", "expectObjKey"},
                { "closeCurl", "expectBinOp"}
            }
        });

        Add("arrayVal", new ParserState()
        {
            SubHandler = ParserHandlers.ArrayValue,
            EndStates = new() {
                { "comma", "arrayVal"},
                { "closeBracket", "expectBinOp"}
            }
        });

        Add("ternaryMid", new ParserState()
        {
            SubHandler = ParserHandlers.TernaryMid,
            EndStates = new() {
                { "colon", "ternaryEnd"}
            }
        });

        Add("ternaryEnd", new ParserState()
        {
            SubHandler = ParserHandlers.TernaryEnd,
            Completable = true
        });
    }
}