using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace JexlNet
{
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
    public class Lexer
    {
        public Lexer(Grammar grammar)
        {
            Grammar = grammar;
        }
        private readonly Grammar Grammar;
        public Regex numericRegex = new Regex(@"^-?(?:(?:[0-9]*\.[0-9]+)|[0-9]+)$", RegexOptions.Compiled);
        public Regex identifierRegex = new Regex(@"^[a-zA-Zа-яА-Я_\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u00FF$][a-zA-Zа-яА-Я0-9_\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u00FF$]*$", RegexOptions.Compiled);
        public Regex escEscRegex = new Regex(@"\\", RegexOptions.Compiled);
        public Regex whitespaceRegex = new Regex(@"^\s*$", RegexOptions.Compiled);
        public string[] preOpRegexElems = new string[]
        {
            // Strings
            @"'(?:(?:\\')|[^'])*'",
            @"""(?:(?:\\"")|[^""])*""",
            // Whitespace
            @"\s+",
            // Booleans
            @"\btrue\b",
            @"\bfalse\b",
        };
        public string[] postOpRegexElems = new string[]
        {
            // Identifiers
            @"[a-zA-Zа-яА-Я_\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u00FF\$][a-zA-Z0-9а-яА-Я_\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u00FF\$]*",
            // Numerics (without negative symbol)
            @"(?:(?:[0-9]*\.[0-9]+)|[0-9]+)",
        };
        public GrammarType[] minusNegatesAfter = new GrammarType[]
        {
            GrammarType.BinaryOperator,
            GrammarType.UnaryOperator,
            GrammarType.OpenParen,
            GrammarType.OpenBracket,
            GrammarType.Question,
            GrammarType.Colon,
            GrammarType.Comma,
        };
        private Regex _splitRegex;

        /// <summary>
        /// Splits a Jexl expression string into an array of expression elements.
        /// </summary>
        /// <param name="str">A Jexl expression string</param>
        /// <returns>An array of substrings defining the functional elements of the expression.</returns>
        public List<string> GetElements(string str)
        {
            var regex = GetSplitRegex();
            return regex.Split(str).ToList().Where(elem => !string.IsNullOrEmpty(elem)).ToList();
        }

        /// <summary>
        /// Converts an array of expression elements into an array of tokens.  Note that
        /// the resulting array may not equal the element array in length, as any
        /// elements that consist only of whitespace get appended to the previous
        /// token's "raw" property.  For the structure of a token object, please see Lexer.Tokenize.
        /// </summary>
        /// <param name="elements">An array of expression elements</param>
        /// <returns>An array of token objects</returns>
        public List<Token> GetTokens(List<string> elements)
        {
            List<Token> tokens = new List<Token>();
            bool negate = false;
            for (int i = 0; i < elements.Count; i++)
            {
                if (IsWhiteSpace(elements[i]))
                {
                    if (tokens.Count != 0)
                    {
                        tokens[tokens.Count - 1].Raw += elements[i];
                    }
                }
                else if (elements[i] == "-" && IsNegative(tokens))
                {
                    negate = true;
                }
                else
                {
                    if (negate)
                    {
                        elements[i] = "-" + elements[i];
                        negate = false;
                    }
                    tokens.Add(CreateToken(elements[i]));
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
            Token token = new Token(GrammarType.Literal, element, JsonValue.Create(element));
            if (element[0] == '"' || element[0] == '\'')
            {
                token.Value = JsonValue.Create(Unquote(element));
            }
            else if (numericRegex.IsMatch(element))
            {
                token.Value = JsonValue.Create(decimal.Parse(element, CultureInfo.InvariantCulture));
            }
            else if (element == "true")
            {
                token.Value = JsonValue.Create(true);
            }
            else if (element == "false")
            {
                token.Value = JsonValue.Create(false);
            }
            else if (Grammar.Elements.TryGetValue(element, out ElementGrammar value))
            {
                token.Type = value.Type;
            }
            else if (identifierRegex.IsMatch(element))
            {
                token.Type = GrammarType.Identifier;
            }
            else
            {
                throw new Exception($"Invalid expression token: {element}");
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
                escapedString = @"\b" + escapedString + @"\b";
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
                IEnumerable<string> elemArray = Grammar.Elements.Keys
                    .OrderByDescending((a) => a.Length)
                    .Select(EscapeRegex);
                string preOpRegexString = string.Join("|", preOpRegexElems.Select(c => c.ToString()));
                string elemArrayString = string.Join("|", elemArray);
                string postOpRegexString = string.Join("|", postOpRegexElems.Select(c => c.ToString()));
                _splitRegex = new Regex($"({preOpRegexString}|{elemArrayString}|{postOpRegexString})");
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
            if (!tokens.Any()) return true;
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
        private static string Unquote(string str)
        {
            char quote = str[0];
            return str.Substring(1, str.Length - 2).Replace(@"\\", @"\").Replace(@"\\""", @"\""").Replace($@"\{quote}", $"{quote}");
        }
    }
}
