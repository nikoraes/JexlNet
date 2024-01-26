namespace JexlNet;

internal class ParserStateTokenType
{
    public ParserStateTokenType(GrammarType? toState = null, Action<Parser, Node?>? handler = null)
    {
        ToState = toState;
        Handler = handler;
    }
    internal GrammarType? ToState { get; set; }
    internal Action<Parser, Node?>? Handler { get; set; }
}

internal class ParserState
{
    internal Dictionary<GrammarType, ParserStateTokenType>? TokenTypes { get; set; }
    internal bool? Completable { get; set; }
    internal Dictionary<GrammarType, GrammarType>? EndStates { get; set; }
    internal Action<Parser, Node?>? SubHandler { get; set; }

}

internal static class ParserStates
{
    internal static Dictionary<GrammarType, ParserState> States = new()
    {
        { GrammarType.ExpectOperand, new ParserState()
            {
                TokenTypes = new() {
                        { GrammarType.Literal, new ParserStateTokenType(GrammarType.ExpectBinaryOperator) },
                        { GrammarType.Identifier, new ParserStateTokenType(GrammarType.Identifier) },
                        { GrammarType.UnaryOperator, new ParserStateTokenType() },
                        { GrammarType.OpenParen, new ParserStateTokenType(GrammarType.SubExpression) },
                        { GrammarType.OpenCurl, new ParserStateTokenType(GrammarType.ExpectObjectKey, ParserHandlers.ObjectStart) },
                        { GrammarType.Dot, new ParserStateTokenType(GrammarType.Traverse) },
                        { GrammarType.OpenBracket, new ParserStateTokenType(GrammarType.ArrayValue, ParserHandlers.ArrayStart) }
                    }
                }
        },

        { GrammarType.ExpectBinaryOperator, new ParserState()
            {
                TokenTypes = new() {
                    { GrammarType.BinaryOperator, new ParserStateTokenType(GrammarType.ExpectOperand) },
                    { GrammarType.Pipe, new ParserStateTokenType(GrammarType.ExpectTransform) },
                    { GrammarType.Dot, new ParserStateTokenType(GrammarType.Traverse) },
                    { GrammarType.Question, new ParserStateTokenType(GrammarType.TernaryMid, ParserHandlers.TernaryStart) }
                },
                Completable = true
            }
        },

        { GrammarType.ExpectTransform, new ParserState()
            {
                TokenTypes = new() {
                    { GrammarType.Identifier, new ParserStateTokenType(GrammarType.PostTransform, ParserHandlers.Transform) }
                }
            }
        },

        { GrammarType.ExpectObjectKey, new ParserState()
            {
                TokenTypes = new() {
                    { GrammarType.Literal, new ParserStateTokenType(GrammarType.ExpectKeyValueSeperator, ParserHandlers.ObjectKey) },
                    { GrammarType.Identifier, new ParserStateTokenType(GrammarType.ExpectKeyValueSeperator, ParserHandlers.ObjectKey) },
                    { GrammarType.CloseCurl, new ParserStateTokenType(GrammarType.ExpectBinaryOperator) }
                }
            }
        },

        { GrammarType.ExpectKeyValueSeperator, new ParserState()
            {
                TokenTypes = new() {
                    { GrammarType.Colon, new ParserStateTokenType(GrammarType.ObjectValue) }
                }
            }
        },

        { GrammarType.PostTransform, new ParserState()
            {
                TokenTypes = new() {
                    { GrammarType.OpenParen, new ParserStateTokenType(GrammarType.ArgumentValue) },
                    { GrammarType.BinaryOperator, new ParserStateTokenType(GrammarType.ExpectOperand) },
                    { GrammarType.Dot, new ParserStateTokenType(GrammarType.Traverse) },
                    { GrammarType.OpenBracket, new ParserStateTokenType(GrammarType.Filter) },
                    { GrammarType.Pipe, new ParserStateTokenType(GrammarType.ExpectTransform) },
                },
                Completable = true
            }
        },

        { GrammarType.PostArgs, new ParserState()
            {
                TokenTypes = new() {
                    { GrammarType.BinaryOperator, new ParserStateTokenType(GrammarType.ExpectOperand) },
                    { GrammarType.Dot, new ParserStateTokenType(GrammarType.Traverse) },
                    { GrammarType.OpenBracket, new ParserStateTokenType(GrammarType.Filter) },
                    { GrammarType.Pipe, new ParserStateTokenType(GrammarType.ExpectTransform) },
                },
                Completable = true
            }
        },

        { GrammarType.Identifier, new ParserState()
            {
                TokenTypes = new() {
                    { GrammarType.BinaryOperator, new ParserStateTokenType(GrammarType.ExpectOperand) },
                    { GrammarType.Dot, new ParserStateTokenType(GrammarType.Traverse) },
                    { GrammarType.OpenBracket, new ParserStateTokenType(GrammarType.Filter) },
                    { GrammarType.OpenParen, new ParserStateTokenType(GrammarType.ArgumentValue, ParserHandlers.FunctionCall) },
                    { GrammarType.Pipe, new ParserStateTokenType(GrammarType.ExpectTransform) },
                    { GrammarType.Question, new ParserStateTokenType(GrammarType.TernaryMid, ParserHandlers.TernaryStart) }
                },
                Completable = true
            }
        },

        { GrammarType.Traverse, new ParserState()
            {
                TokenTypes = new() {
                    { GrammarType.Identifier, new ParserStateTokenType(GrammarType.Identifier) }
                }
            }
        },

        { GrammarType.Filter, new ParserState()
            {
                SubHandler = ParserHandlers.Filter,
                EndStates = new() {
                    { GrammarType.CloseBracket, GrammarType.Identifier}
                }
            }
        },

        { GrammarType.SubExpression, new ParserState()
            {
                SubHandler = ParserHandlers.SubExpression,
                EndStates = new() {
                    { GrammarType.CloseParen, GrammarType.ExpectBinaryOperator}
                }
            }
        },

        { GrammarType.ArgumentValue, new ParserState()
            {
                SubHandler = ParserHandlers.ArgumentValue,
                EndStates = new() {
                    { GrammarType.Comma, GrammarType.ArgumentValue},
                    { GrammarType.CloseParen, GrammarType.PostArgs}
                }
            }
        },

        { GrammarType.ObjectValue, new ParserState()
            {
                SubHandler = ParserHandlers.ObjectValue,
                EndStates = new() {
                    { GrammarType.Comma, GrammarType.ExpectObjectKey},
                    { GrammarType.CloseCurl, GrammarType.ExpectBinaryOperator}
                }
            }
        },

        { GrammarType.ArrayValue, new ParserState()
            {
                SubHandler = ParserHandlers.ArrayValue,
                EndStates = new() {
                    { GrammarType.Comma, GrammarType.ArrayValue},
                    { GrammarType.CloseBracket, GrammarType.ExpectBinaryOperator}
                }
            }
        },

        { GrammarType.TernaryMid, new ParserState()
            {
                SubHandler = ParserHandlers.TernaryMid,
                EndStates = new() {
                    { GrammarType.Colon, GrammarType.TernaryEnd}
                }
            }
        },

        { GrammarType.TernaryEnd, new ParserState()
            {
                SubHandler = ParserHandlers.TernaryEnd,
                Completable = true
            }
        },

    };
}