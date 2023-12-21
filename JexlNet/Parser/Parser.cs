
namespace JexlNet;

/**
 * The Parser is a state machine that converts tokens from the {@link Lexer}
 * into an Abstract Syntax Tree (AST), capable of being evaluated in any
 * context by the {@link Evaluator}.  The Parser expects that all tokens
 * provided to it are legal and typed properly according to the grammar, but
 * accepts that the tokens may still be in an invalid order or in some other
 * unparsable configuration that requires it to throw an Error.
 * @param {{}} grammar The grammar object to use to parse Jexl strings
 * @param {string} [prefix] A string prefix to prepend to the expression string
 *      for error messaging purposes.  This is useful for when a new Parser is
 *      instantiated to parse an subexpression, as the parent Parser's
 *      expression string thus far can be passed for a more user-friendly
 *      error message.
 * @param {{}} [stopMap] A mapping of token types to any truthy value. When the
 *      token type is encountered, the parser will return the mapped value
 *      instead of boolean false.
 */

///<summary>
///The Parser is a state machine that converts tokens from the {@link Lexer}
///into an Abstract Syntax Tree (AST), capable of being evaluated in any
///context by the {@link Evaluator}. The Parser expects that all tokens
///provided to it are legal and typed properly according to the grammar, but
///accepts that the tokens may still be in an invalid order or in some other
///unparsable configuration that requires it to throw an Error.
///</summary>
///<param name="grammar">The grammar object to use to parse Jexl strings</param>
///<param name="prefix">A string prefix to prepend to the expression string
///for error messaging purposes. This is useful for when a new Parser is
///instantiated to parse an subexpression, as the parent Parser's
///expression string thus far can be passed for a more user-friendly
///error message.</param>
///<param name="stopMap">A mapping of token types to any truthy value. When the
///token type is encountered, the parser will return the mapped value
///instead of boolean false.</param>
public class Parser
{
    private readonly Grammar _grammar;
    private string _state;
    private string _exprStr;
    private bool _relative;
    private Dictionary<string, bool> _stopMap;


    public Parser(Grammar grammar, string prefix = "", Dictionary<string, bool>? stopMap = null)
    {
        _grammar = grammar;
        _state = "expectOperand";
        // _tree = null;
        _exprStr = prefix ?? "";
        _relative = false;
        _stopMap = stopMap ?? [];
    }

    ///<summary>
    ///Processes a new token into the AST and manages the transitions of the state
    ///machine.
    ///</summary>
    ///<param name="token">A token object, as provided by the {@link Lexer#tokenize} function.</param>
    ///<returns>the stopState value if this parser encountered a token 
    ///in the stopState mapb false if tokens can continue.</returns>
    public bool AddToken(Token token)
    {
        if (_state == "complete")
        {
            throw new Exception("Cannot add a new token to a completed parser.");
        }
    }
}