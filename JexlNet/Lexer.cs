using System.Globalization;
using System.Text.RegularExpressions;

namespace JexlNet;

public class Token
{
    public string? Raw { get; set; }
    public string? Type { get; set; }
    public dynamic? Value { get; set; }
}

/// <summary>
/// Lexer is a collection of stateless, statically-accessed functions for the
/// lexical parsing of a Jexl string.  Its responsibility is to identify the
/// "parts of speech" of a Jexl expression, and tokenize and label each, but
/// to do only the most minimal syntax checking; the only errors the Lexer
/// should be concerned with are if it's unable to identify the utility of
/// any of its tokens.Errors stemming from these tokens not being in a
/// sensible configuration should be left for the Parser to handle.
/// </summary>
/// <param name="grammar"></param>
public class Lexer(Grammar grammar)
{
    private readonly Grammar _grammar = grammar;

    public Regex numericRegex = new(@"^-?(?:(?:[0-9]*\.[0-9]+)|[0-9]+)$", RegexOptions.Compiled);
    public Regex identifierRegex = new(@"^[a-zA-Zа-яА-Я_\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u00FF$][a-zA-Zа-яА-Я0-9_\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u00FF$]*$", RegexOptions.Compiled);
    public Regex escEscRegex = new(@"\\", RegexOptions.Compiled);
    public Regex whitespaceRegex = new(@"^\s*$", RegexOptions.Compiled);
    public string[] preOpRegexElems = [
        // Strings
        @"'(?:(?:\\\\')|[^'])*'",
        @"""(?:(?:\\\\"")|[^""])*""",
        // Whitespace
        @"\\s+",
        // Booleans
        @"\\btrue\\b",
        @"\\bfalse\\b",
        ];
    public string[] postOpRegexElems = [
        // Identifiers
        @"[a-zA-Zа-яА-Я_\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u00FF\\$][a-zA-Z0-9а-яА-Я_\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u00FF\\$]*",
            // Numerics (without negative symbol)
            @"(?:(?:[0-9]*\\.[0-9]+)|[0-9]+)",
        ];
    public string[] minusNegatesAfter = [
        "binaryOp",
            "unaryOp",
            "openParen",
            "openBracket",
            "question",
            "colon",
        ];
    private Regex? _splitRegex;

    /// <summary>
    /// Splits a Jexl expression string into an array of expression elements.
    /// </summary>
    /// <param name="str">A Jexl expression string</param>
    /// <returns>An array of substrings defining the functional elements of the expression.</returns>
    public string[] GetElements(string str)
    {
        var regex = GetSplitRegex();
        return regex.Split(str).ToList().Where(elem => !string.IsNullOrEmpty(elem)).ToArray();
    }

    /// <summary>
    /// Converts an array of expression elements into an array of tokens.  Note that
    /// the resulting array may not equal the element array in length, as any
    /// elements that consist only of whitespace get appended to the previous
    /// token's "raw" property.  For the structure of a token object, please see Lexer.Tokenize.
    /// </summary>
    /// <param name="elements">An array of expression elements</param>
    /// <returns>An array of token objects</returns>
    public List<Token> GetTokens(IEnumerable<string> elements)
    {
        List<Token> tokens = [];
        bool negate = false;
        foreach (var element in elements)
        {
            if (IsWhiteSpace(element))
            {
                if (tokens.Count != 0)
                {
                    tokens[^1].Raw += element;
                }
            }
            else if (element == "-" && IsNegative(tokens))
            {
                negate = true;
            }
            else
            {
                if (negate)
                {
                    tokens.Add(CreateToken("-" + element));
                    negate = false;
                }
                else
                {
                    tokens.Add(CreateToken(element));
                }
            }
        }
        if (negate)
        {
            tokens.Add(CreateToken("-"));
        }
        return tokens;
    }

    /// <summary>
    /// Converts a Jexl expression string into an array of tokens. 
    /// </summary>
    public List<Token> Tokenize(string str)
    {
        var elements = GetElements(str);
        return GetTokens(elements);
    }

    /// <summary>
    /// Creates a new token object from an element of a Jexl string.
    /// </summary>
    /// <param name="element">A string representing an element of a Jexl string</param>
    /// <returns>A token object</returns>
    private Token CreateToken(string element)
    {
        var token = new Token
        {
            Raw = element,
            Type = "literal",
            Value = element
        };
        if (element[0] == '"' || element[0] == '\'')
        {
            token.Value = Unquote(element);
        }
        else if (numericRegex.IsMatch(element))
        {
            token.Value = float.Parse(element, CultureInfo.InvariantCulture);
        }
        else if (element == "true")
        {
            token.Value = true;
        }
        else if (element == "false")
        {
            token.Value = false;
        }
        else if (_grammar.Elements.TryGetValue(element, out ElementGrammar? value))
        {
            token.Type = value.Type;
        }
        else if (identifierRegex.IsMatch(element))
        {
            token.Type = "identifier";
        }
        return token;
    }

    /// <summary>
    /// Escapes a string so that it can be treated as a string literal within a
    /// regular expression.
    /// </summary>
    /// <param name="str">A string to be escaped</param>
    /// <returns>A string with all RegExp special characters escaped</returns>
    private string EscapeRegex(string str)
    {
        string escapedString = Regex.Escape(str);
        if (identifierRegex.IsMatch(str))
        {
            escapedString = "\\b" + escapedString + "\\b";
        }
        return escapedString;
    }

    /// <summary>
    /// Gets a RegEx object appropriate for splitting a Jexl string into its core
    /// elements.
    /// </summary>
    /// <returns>An element-splitting RegExp object</returns>
    private Regex GetSplitRegex()
    {
        if (_splitRegex == null)
        {
            // Sort by most characters to least, then regex escape each
            var elemArray = _grammar.Elements.Keys
                .OrderByDescending((a) => a.Length)
                .Select(EscapeRegex);
            _splitRegex = new Regex(
                $"({string.Join('|', preOpRegexElems)}|{string.Join('|', elemArray)}|{string.Join('|', postOpRegexElems)})"
            );
        }
        return _splitRegex;
    }

    /// <summary>
    /// Determines whether the addition of a '-' token should be interpreted as a
    /// negative symbol for an upcoming number, given an array of tokens already
    /// processed.
    /// </summary>
    /// <param name="tokens">An array of tokens already processed</param>
    /// <returns>true if the '-' should be interpreted as a negative symbol; false otherwise.</returns>
    private bool IsNegative(IEnumerable<Token> tokens)
    {
        if (!tokens.Any())
        {
            return false;
        }
        return minusNegatesAfter.Any((type) => tokens.Last().Type == type);
    }

    /// <summary>
    /// A utility function to determine if a string consists of only space 
    /// characters.
    /// </summary>
    /// <param name="str">A string to be tested</param>
    /// <returns>true if the string is empty or consists of only spaces; false otherwise.</returns>
    private bool IsWhiteSpace(string str)
    {
        return whitespaceRegex.IsMatch(str);
    }

    /// <summary>
    /// Removes the beginning and trailing quotes from a string, unescapes any
    /// escaped quotes on its interior, and unescapes any escaped escape
    /// characters. Note that this function is not defensive; it assumes that the
    /// provided string is not empty, and that its first and last characters are
    /// actually quotes.
    /// </summary>
    /// <param name="str">A string whose first and last characters are quotes</param>
    /// <returns>a string with the surrounding quotes stripped and escapes properly processed.</returns>
    private string Unquote(string str)
    {
        char quote = str[0];
        Regex escapeQuoteRegex = new(@$"\\{quote}");
        string unquotedString = str[1..^1];
        // Replace escaped quotes with unescaped quotes
        escapeQuoteRegex.Replace(unquotedString, quote.ToString());
        // Replace escaped escape characters with unescaped escape characters
        escEscRegex.Replace(unquotedString, @"\");
        return unquotedString;
    }
}
