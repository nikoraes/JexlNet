namespace JexlNet;
public static class GrammarType
{
    public const string
        Dot = "dot",
        OpenBracket = "openBracket",
        CloseBracket = "closeBracket",
        Pipe = "pipe",
        OpenCurl = "openCurl",
        CloseCurl = "closeCurl",
        Colon = "colon",
        Comma = "comma",
        OpenParen = "openParen",
        CloseParen = "closeParen",
        Question = "question",
        BinaryOperator = "binaryOp",
        UnaryOperator = "unaryOp";
}

public class ElementGrammar
{
    public ElementGrammar(string type)
    {
        Type = type;
    }
    public ElementGrammar(string type, int precedence, Func<dynamic[], dynamic> evaluate, bool evalOnDemand = false)
    {
        Type = type;
        Precedence = precedence;
        Evaluate = evaluate;
        EvalOnDemand = evalOnDemand;
    }
    public string Type { get; set; }
    public int Precedence { get; set; }
    public Func<dynamic[], dynamic>? Evaluate { get; set; }
    public bool EvalOnDemand { get; set; }
}

public class BinaryOperatorGrammar(
    int precedence,
    Func<dynamic[], dynamic> evaluate,
    bool evalOnDemand = false
    ) : ElementGrammar(
        GrammarType.BinaryOperator,
        precedence,
        evaluate,
        evalOnDemand)
{
}

public class UnaryOperatorGrammar(
    int precedence,
    Func<dynamic[], dynamic> evaluate,
    bool evalOnDemand = false
    ) : ElementGrammar(
        GrammarType.UnaryOperator,
        precedence,
        evaluate,
        evalOnDemand)
{
}

public class Grammar
{
    /**
    * A map of all expression elements to their properties. Note that changes
    * here may require changes in the Lexer or Parser.
    */
    public readonly Dictionary<string, ElementGrammar> Elements = new()
        {
            { ".", new ElementGrammar(GrammarType.Dot)},
            { "[", new ElementGrammar(GrammarType.OpenBracket) },
            { "]", new ElementGrammar(GrammarType.CloseBracket) },
            { "|", new ElementGrammar(GrammarType.Pipe) },
            { "{", new ElementGrammar(GrammarType.OpenCurl) },
            { "}", new ElementGrammar(GrammarType.CloseCurl) },
            { ":", new ElementGrammar(GrammarType.Colon) },
            { ",", new ElementGrammar(GrammarType.Comma) },
            { "(", new ElementGrammar(GrammarType.OpenParen) },
            { ")", new ElementGrammar(GrammarType.CloseParen) },
            { "?", new ElementGrammar(GrammarType.Question) },
            {
                "+", new BinaryOperatorGrammar(30, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for + operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a + b;
                })
            },
            {
                "-", new BinaryOperatorGrammar(30, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for - operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a - b;
                })
            },
            {
                "*", new BinaryOperatorGrammar(40, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for * operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a * b;
                })
            },
            {
                "/", new BinaryOperatorGrammar(40, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for / operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a / b;
                })
            },
            {
                "//", new BinaryOperatorGrammar(40, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for // operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return Math.Floor(a / b);
                })
            },
            {
                "%", new BinaryOperatorGrammar(40, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for % operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a % b;
                })
            },
            {
                "^", new BinaryOperatorGrammar(50, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for ^ operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return Math.Pow(a, b);
                })
            },
            {
                "==", new BinaryOperatorGrammar(20, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for == operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a == b;
                })
            },
            {
                "!=", new BinaryOperatorGrammar(20, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for != operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a != b;
                })
            },
            {
                ">", new BinaryOperatorGrammar(20, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for > operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a > b;
                })
            },
            {
                ">=", new BinaryOperatorGrammar(20, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for >= operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a >= b;
                })
            },
            {
                "<", new BinaryOperatorGrammar(20, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for < operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a < b;
                })
            },
            {
                "<=", new BinaryOperatorGrammar(20, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for <= operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a <= b;
                })
            },
            {
                "&&", new BinaryOperatorGrammar(10, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for && operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    if (a is bool || b is bool)
                    {
                        return (bool)a && (bool)b;
                    }
                    else
                    {
                        throw new Exception("Unsupported type for && operator");
                    }
                }, true)
            },
            {
                "||", new BinaryOperatorGrammar(10, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for || operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    if (a is bool || b is bool)
                    {
                        return (bool)a || (bool)b;
                    }
                    else
                    {
                        throw new Exception("Unsupported type for || operator");
                    }
                }, true)
            },
            {
                "in", new BinaryOperatorGrammar(20, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for in operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    if (b is IEnumerable<dynamic> enumerable)
                    {
                        return enumerable.Any(elem => elem == a);
                    }
                    else
                    {
                        throw new Exception("Unsupported type for in operator");
                    }
                })
            },
            {
                "!", new UnaryOperatorGrammar(100, (args) =>
                {
                    if (args.Length != 1)
                    {
                        throw new Exception("Unsupported number of arguments for ! operator");
                    }
                    var a = args[0];
                    if (a is bool v)
                    {
                        return !v;
                    }
                    else
                    {
                        throw new Exception("Unsupported type for ! operator");
                    }
                })
            }
        };

    /**
    * A map of function names to javascript functions. A Jexl function
    * takes zero ore more arguemnts:
    *
    *     - {*} ...args: A variable number of arguments passed to this function.
    *       All of these are pre-evaluated to their actual values before calling
    *       the function.
    *
    * The Jexl function should return either the transformed value, or
    * a Promises/A+ Promise object that resolves with the value and rejects
    * or throws only when an unrecoverable error occurs. Functions should
    * generally return undefined when they don't make sense to be used on the
    * given value type, rather than throw/reject. An error is only
    * appropriate when the function would normally return a value, but
    * cannot due to some other failure.
    */
    public Dictionary<string, Func<object[], object>> Functions = new();

    /**
    * A map of transform names to transform functions. A transform function
    * takes one ore more arguemnts:
    *
    *     - {*} val: A value to be transformed
    *     - {*} ...args: A variable number of arguments passed to this transform.
    *       All of these are pre-evaluated to their actual values before calling
    *       the function.
    *
    * The transform function should return either the transformed value, or
    * a Promises/A+ Promise object that resolves with the value and rejects
    * or throws only when an unrecoverable error occurs. Transforms should
    * generally return undefined when they don't make sense to be used on the
    * given value type, rather than throw/reject. An error is only
    * appropriate when the transform would normally return a value, but
    * cannot due to some other failure.
    */
    public Dictionary<string, Func<object, object[], object>> Transforms = new();
}
