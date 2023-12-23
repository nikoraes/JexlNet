
namespace JexlNet;

internal class ParserStateTokenType
{
    public ParserStateTokenType(string? toState = null, Action<Parser, Node?>? handler = null)
    {
        ToState = toState;
        Handler = handler;
    }
    internal string? ToState { get; set; }
    internal Action<Parser, Node?>? Handler { get; set; }
}

internal class ParserState
{
    internal Dictionary<string, ParserStateTokenType>? TokenTypes { get; set; }
    internal bool? Completable { get; set; }
    internal Dictionary<string, string>? EndStates { get; set; }
    internal Action<Parser, Node?>? SubHandler { get; set; }

}

internal static class ParserStates
{
    internal static Dictionary<string, ParserState> States = new()
    {
        { "expectOperand", new ParserState()
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
                }
        },

        { "expectBinOp", new ParserState()
            {
                TokenTypes = new Dictionary<string, ParserStateTokenType>() {
                    { "binaryOp", new ParserStateTokenType("expectOperand") },
                    { "pipe", new ParserStateTokenType("expectTransform") },
                    { "dot", new ParserStateTokenType("traverse") },
                    { "question", new ParserStateTokenType("ternaryMid", ParserHandlers.TernaryStart) }
                },
                Completable = true
            }
        },

        { "expectTransform", new ParserState()
            {
                TokenTypes = new Dictionary<string, ParserStateTokenType>() {
                    { "identifier", new ParserStateTokenType("postTransform", ParserHandlers.Transform) }
                }
            }
        },

        { "expectObjKey", new ParserState()
            {
                TokenTypes = new Dictionary<string, ParserStateTokenType>() {
                    { "literal", new ParserStateTokenType("expectKeyValSep", ParserHandlers.ObjectKey) },
                    { "identifier", new ParserStateTokenType("expectKeyValSep", ParserHandlers.ObjectKey) },
                    { "closeCurl", new ParserStateTokenType("expectBinOp") }
                }
            }
        },

        { "expectKeyValSep", new ParserState()
            {
                TokenTypes = new Dictionary<string, ParserStateTokenType>() {
                    { "colon", new ParserStateTokenType("objVal") }
                }
            }
        },

        { "postTransform", new ParserState()
            {
                TokenTypes = new Dictionary<string, ParserStateTokenType>() {
                    { "openParen", new ParserStateTokenType("argVal") },
                    { "binaryOp", new ParserStateTokenType("expectOperand") },
                    { "dot", new ParserStateTokenType("traverse") },
                    { "openBracket", new ParserStateTokenType("filter") },
                    { "pipe", new ParserStateTokenType("expectTransform") },
                },
                Completable = true
            }
        },

        { "postArgs", new ParserState()
            {
                TokenTypes = new Dictionary<string, ParserStateTokenType>() {
                    { "binaryOp", new ParserStateTokenType("expectOperand") },
                    { "dot", new ParserStateTokenType("traverse") },
                    { "openBracket", new ParserStateTokenType("filter") },
                    { "pipe", new ParserStateTokenType("expectTransform") },
                },
                Completable = true
            }
        },

        { "identifier", new ParserState()
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
            }
        },

        { "traverse", new ParserState()
            {
                TokenTypes = new Dictionary<string, ParserStateTokenType>() {
                    { "identifier", new ParserStateTokenType("identifier") }
                }
            }
        },

        { "filter", new ParserState()
            {
                SubHandler = ParserHandlers.Filter,
                EndStates = new() {
                    { "closeBracket", "identifier"}
                }
            }
        },

        { "subExpression", new ParserState()
            {
                SubHandler = ParserHandlers.SubExpression,
                EndStates = new() {
                    { "closeParen", "expectBinOp"}
                }
            }
        },

        { "argVal", new ParserState()
            {
                SubHandler = ParserHandlers.ArgumentValue,
                EndStates = new() {
                    { "comma", "argVal"},
                    { "closeParen", "postArgs"}
                }
            }
        },

        { "objVal", new ParserState()
            {
                SubHandler = ParserHandlers.ObjectValue,
                EndStates = new() {
                    { "comma", "expectObjKey"},
                    { "closeCurl", "expectBinOp"}
                }
            }
        },

        { "arrayVal", new ParserState()
            {
                SubHandler = ParserHandlers.ArrayValue,
                EndStates = new() {
                    { "comma", "arrayVal"},
                    { "closeBracket", "expectBinOp"}
                }
            }
        },

        { "ternaryMid", new ParserState()
            {
                SubHandler = ParserHandlers.TernaryMid,
                EndStates = new() {
                    { "colon", "ternaryEnd"}
                }
            }
        },

        { "ternaryEnd", new ParserState()
            {
                SubHandler = ParserHandlers.TernaryEnd,
                Completable = true
            }
        },
    };
}