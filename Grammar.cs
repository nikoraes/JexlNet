namespace JexlNet
{
    enum GrammarType
    {
        Dot,
        OpenBracket,
        CloseBracket,
        Pipe,
        OpenCurl,
        CLoseCurl,
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
        public ElementGrammar(string type)
        {
            Type = type;
        }
        public ElementGrammar(string type, int precedence, Func<object, object, object> evaluate)
        {
            Type = type;
            Precedence = precedence;
            Evaluate = evaluate;
        }
        public string Type { get; set; }
        public int Precedence { get; set; }
        public Func<object, object, object>? Evaluate { get; set; }
    }

    public class Grammar
    {
        readonly Dictionary<string, ElementGrammar> Elements = new()
        {
            { ".", new ElementGrammar("Dot")},
            { "[", new ElementGrammar("OpenBracket") },
            { "]", new ElementGrammar("CloseBracket") },
            { "|", new ElementGrammar("Pipe") },
            { "{", new ElementGrammar("OpenCurl") },
            { "}", new ElementGrammar("CloseCurl") },
            { ":", new ElementGrammar("Colon") },
            { ",", new ElementGrammar("Comma") },
            { "(", new ElementGrammar("OpenParen") },
            { ")", new ElementGrammar("CloseParen") },
            { "?", new ElementGrammar("Question") },
            {
                "+", new ElementGrammar("BinaryOperator", 30, (a, b) =>
                {
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
                "-", new ElementGrammar("BinaryOperator", 30, (a, b) =>
                {
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
                "*", new ElementGrammar("BinaryOperator", 40, (a, b) =>
                {
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
                "/", new ElementGrammar("BinaryOperator", 40, (a, b) =>
                {
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
            }
        };
    }
}