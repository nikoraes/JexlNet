namespace JexlNet
{
    public enum GrammarType
    {
        Dot,
        OpenBracket,
        CloseBracket,
        Pipe,
        OpenCurl,
        CloseCurl,
        Colon,
        Comma,
        OpenParen,
        CloseParen,
        Question,
        BinaryOperator,
        UnaryOperator
    }

    public class ElementGrammar
    {
        public ElementGrammar(GrammarType type)
        {
            Type = type;
        }
        public ElementGrammar(GrammarType type, int precedence, Func<object[], object> evaluate)
        {
            Type = type;
            Precedence = precedence;
        }
        public GrammarType Type { get; set; }
        public int Precedence { get; set; }
        public Func<object[], object>? Evaluate { get; set; }
    }

    public class BinaryOperatorGrammar(int precedence, Func<object[], object> evaluate) : ElementGrammar(GrammarType.BinaryOperator, precedence, evaluate)
    {
    }

    public class UnaryOperatorGrammar(int precedence, Func<object[], object> evaluate) : ElementGrammar(GrammarType.UnaryOperator, precedence, evaluate)
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
                    if (a is string || b is string)
                    {
                        return a.ToString() + b.ToString();
                    }
                    else if (a is int || b is int)
                    {
                        return (int)a + (int)b;
                    }
                    else if (a is double || b is double)
                    {
                        return (double)a + (double)b;
                    }
                    else
                    {
                        throw new Exception("Unsupported type for + operator");
                    }
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
                    if (a is int || b is int)
                    {
                        return (int)a - (int)b;
                    }
                    else if (a is double || b is double)
                    {
                        return (double)a - (double)b;
                    }
                    else
                    {
                        throw new Exception("Unsupported type for - operator");
                    }
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
                    if (a is int || b is int)
                    {
                        return (int)a * (int)b;
                    }
                    else if (a is double || b is double)
                    {
                        return (double)a * (double)b;
                    }
                    else
                    {
                        throw new Exception("Unsupported type for * operator");
                    }
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
                    if (a is int || b is int)
                    {
                        return (int)a / (int)b;
                    }
                    else if (a is double || b is double)
                    {
                        return (double)a / (double)b;
                    }
                    else
                    {
                        throw new Exception("Unsupported type for / operator");
                    }
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
                    if (a is int || b is int)
                    {
                        return Math.Floor((decimal)((int)a / (int)b));
                    }
                    else if (a is double || b is double)
                    {
                        return Math.Floor((double)a / (double)b);
                    }
                    else
                    {
                        throw new Exception("Unsupported type for // operator");
                    }
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
                    if (a is int || b is int)
                    {
                        return (int)a % (int)b;
                    }
                    else if (a is double || b is double)
                    {
                        return (double)a % (double)b;
                    }
                    else
                    {
                        throw new Exception("Unsupported type for % operator");
                    }
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
                    if (a is int || b is int)
                    {
                        return (int)Math.Pow((int)a, (int)b);
                    }
                    else if (a is double || b is double)
                    {
                        return Math.Pow((double)a, (double)b);
                    }
                    else
                    {
                        throw new Exception("Unsupported type for ^ operator");
                    }
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
                    if (a is string || b is string)
                    {
                        return a.ToString() == b.ToString();
                    }
                    else if (a is int || b is int)
                    {
                        return (int)a == (int)b;
                    }
                    else if (a is double || b is double)
                    {
                        return (double)a == (double)b;
                    }
                    else if (a is bool || b is bool)
                    {
                        return (bool)a == (bool)b;
                    }
                    else
                    {
                        throw new Exception("Unsupported type for == operator");
                    }
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
                    if (a is string || b is string)
                    {
                        return a.ToString() != b.ToString();
                    }
                    else if (a is int || b is int)
                    {
                        return (int)a != (int)b;
                    }
                    else if (a is double || b is double)
                    {
                        return (double)a != (double)b;
                    }
                    else if (a is bool || b is bool)
                    {
                        return (bool)a != (bool)b;
                    }
                    else
                    {
                        throw new Exception("Unsupported type for != operator");
                    }
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
                    if (a is int || b is int)
                    {
                        return (int)a > (int)b;
                    }
                    else if (a is double || b is double)
                    {
                        return (double)a > (double)b;
                    }
                    else
                    {
                        throw new Exception("Unsupported type for > operator");
                    }
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
                    if (a is int || b is int)
                    {
                        return (int)a >= (int)b;
                    }
                    else if (a is double || b is double)
                    {
                        return (double)a >= (double)b;
                    }
                    else
                    {
                        throw new Exception("Unsupported type for >= operator");
                    }
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
                    if (a is int || b is int)
                    {
                        return (int)a < (int)b;
                    }
                    else if (a is double || b is double)
                    {
                        return (double)a < (double)b;
                    }
                    else
                    {
                        throw new Exception("Unsupported type for < operator");
                    }
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
                    if (a is int || b is int)
                    {
                        return (int)a <= (int)b;
                    }
                    else if (a is double || b is double)
                    {
                        return (double)a <= (double)b;
                    }
                    else
                    {
                        throw new Exception("Unsupported type for <= operator");
                    }
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
                })
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
                })
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
                    if (b is IEnumerable<object> enumerable)
                    {
                        return enumerable.Contains(a);
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
}