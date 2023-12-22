
namespace JexlNet;

public class Node : Token, IEquatable<Node>
{
    public Node() { }
    public Node(string type) { Type = type; }
    public Node(string type, dynamic value)
    {
        Type = type;
        Value = value;
    }
    public Node(Token token)
    {
        Raw = token.Raw;
        Type = token.Type;
        Value = token.Value;
    }
    public Node? Left { get; set; }
    public Node? Right { get; set; }
    public Node? Parent { get; set; }
    public bool? Writable { get; set; }
    public List<Node>? Args { get; set; }
    public string? Operator { get; set; }
    public Node? Expr { get; set; }
    public bool? Relative { get; set; }
    public Node? Subject { get; set; }
    public string? Name { get; set; }
    public string? Pool { get; set; }
    public Node? From { get; set; } // Used for chained identifiers
    public Node? Alternate { get; set; }
    public Node? Consequent { get; set; }
    public Node? Test { get; set; }

    public bool Equals(Node? other)
    {
        if (other == null) return false;
        bool equalValue;
        if (Value is Dictionary<string, Node> dictionary && other.Value is Dictionary<string, Node> dictionary1)
        {
            if (dictionary.Count != dictionary1.Count)
            {
                return false;
            }
            equalValue = true;
            foreach (var key in dictionary.Keys)
            {
                if (!dictionary1.ContainsKey(key))
                {
                    equalValue = false;
                    break;
                }
                if (!dictionary[key].Equals(dictionary1[key]))
                {
                    equalValue = false;
                    break;
                }
            }
        }
        else if (Value is List<Node> list && other.Value is List<Node> list1)
        {
            equalValue = list.SequenceEqual(list1);
        }
        else
        {
            equalValue = Value == other.Value;
        }
        return
            Type == other.Type &&
            equalValue &&
            Operator == other.Operator &&
            Relative == other.Relative &&
            ((Left == null && other.Left == null) || (Left != null && Left.Equals(other.Left))) &&
            ((Right == null && other.Right == null) || (Right != null && Right.Equals(other.Right))) &&
            ((From == null && other.From == null) || (From != null && From.Equals(other.From))) &&
            Name == other.Name &&
            Pool == other.Pool &&
            ((Args == null && other.Args == null) || (Args != null && other.Args != null && Args.SequenceEqual(other.Args)));
    }
}

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
public class Parser(Grammar grammar, string prefix = "", Dictionary<string, string>? stopMap = null)
{
    public readonly Grammar grammar = grammar;
    private readonly ParserStates _parserStates = new();
    private string _state = "expectOperand";
    private string _exprStr = prefix ?? "";
    public bool relative = false;
    private readonly Dictionary<string, string> _stopMap = stopMap ?? [];
    private bool _parentStop = false;
    public Node? tree = null;
    public Node? cursor;
    private Parser? _subParser;
    public bool? nextIdentEncapsulate;
    public bool? nextIdentRelative;
    public dynamic? cursorObjectKey;

    ///<summary>
    ///Processes a new token into the AST and manages the transitions of the state
    ///machine.
    ///</summary>
    ///<param name="token">A token object, as provided by the {@link Lexer#tokenize} function.</param>
    ///<returns>the stopState value if this parser encountered a token 
    ///in the stopState mapb 'false' if tokens can continue.</returns>
    public string AddToken(Node token)
    {
        if (_state == "complete")
        {
            throw new Exception("Cannot add a new token to a completed parser.");
        }
        var state = _parserStates[_state];
        var startExpr = _exprStr;
        _exprStr += token.Raw;
        if (state.SubHandler != null)
        {
            if (_subParser == null)
            {
                StartSubExpression(startExpr);
            }
            var stopState = _subParser!.AddToken(token);
            if (stopState != "stop")
            {
                EndSubExpression();
                if (_parentStop)
                {
                    return stopState;
                }
                _state = stopState;
            }
        }
        else if (token.Type != null && state.TokenTypes!.TryGetValue(token.Type, out var typeOpts))
        {
            // var handleFunc = ParserHandlers.Handlers[token.Type!];
            ParserHandlers.Handlers.TryGetValue(token.Type, out var handleFunc);
            if (typeOpts.Handler != null)
            {
                handleFunc = typeOpts.Handler;
            }
            handleFunc?.Invoke(this, token);
            if (typeOpts.ToState != null)
            {
                _state = typeOpts.ToState;
            }
        }
        else if (_stopMap.TryGetValue(token.Type!, out string? value))
        {
            return value;
        }
        else
        {
            throw new Exception($"Token {token.Raw} ({token.Type}) unexpected in expression: {_exprStr}");
        }

        return "stop";
    }

    ///<summary>
    ///Processes an array of tokens iteratively through the {@link #addToken}
    ///function.
    ///</summary>
    ///<param name="tokens">An array of token objects, as provided by the {@link Lexer#tokenize} function.</param>
    public void AddTokens(List<Node> tokens)
    {
        foreach (var token in tokens)
        {
            AddToken(token);
        }
    }

    ///<summary>
    ///Processes an array of tokens iteratively through the {@link #addToken}
    ///function.
    ///</summary>
    ///<param name="tokens">An array of token objects, as provided by the {@link Lexer#tokenize} function.</param>
    public void AddTokens(List<Token> tokens)
    {
        foreach (var token in tokens)
        {
            AddToken(new Node(token));
        }
    }

    ///<summary>
    ///Marks this Parser instance as completed and retrieves the full AST.
    ///</summary>
    ///<returns>A full expression tree, ready for evaluation by the
    ///{@link Evaluator#eval} function, or null if no tokens were passed to
    ///the parser before complete was called</returns>
    ///<exception cref="Error">if the parser is not in a state where it's legal to end
    ///the expression, indicating that the expression is incomplete</exception>
    public Node? Complete()
    {
        if (cursor != null && (!_parserStates.TryGetValue(_state, out var state) || state.Completable != true))
        {
            throw new Exception($"Unexpected end of expression: {_exprStr}");
        }
        if (_subParser != null)
        {
            EndSubExpression();
        }
        _state = "complete";
        return cursor != null ? tree : null;
    }

    ///<summary>
    ///Indicates whether the expression tree contains a relative path identifier.
    ///</summary>
    ///<returns>true if a relative identifier exists false otherwise.</returns>
    public bool IsRelative()
    {
        return relative;
    }

    ///<summary>
    ///Ends a subexpression by completing the subParser and passing its result
    ///to the subHandler configured in the current state.
    ///</summary>
    private void EndSubExpression()
    {
        if (_parserStates.TryGetValue(_state, out var state) && state.SubHandler != null && _subParser != null)
        {
            state.SubHandler(this, _subParser.Complete());
        }
        _subParser = null;
    }

    ///<summary>
    ///Places a new tree node at the current position of the cursor (to the 'right'
    ///property) and then advances the cursor to the new node. This function also
    ///handles setting the parent of the new node.
    ///</summary>
    ///<param name="node">A node to be added to the AST</param>
    public void PlaceAtCursor(Node? node)
    {
        if (cursor == null)
        {
            tree = node;
        }
        else
        {
            cursor.Right = node;
            SetParent(node, cursor);
        }
        cursor = node;
    }

    ///<summary>
    ///Places a tree node before the current position of the cursor, replacing
    ///the node that the cursor currently points to. This should only be called in
    ///cases where the cursor is known to exist, and the provided node already
    ///contains a pointer to what's at the cursor currently.
    ///</summary>
    ///<param name="node">A node to be added to the AST</param>
    public void PlaceBeforeCursor(Node node)
    {
        cursor = cursor!.Parent;
        PlaceAtCursor(node);
    }


    ///<summary>
    ///Sets the parent of a node by creating a non-enumerable _parent property
    ///that points to the supplied parent argument.
    ///</summary>
    ///<param name="node">A node of the AST on which to set a new parent</param>
    ///<param name="parent">An existing node of the AST to serve as the parent of the new node</param>
    public static void SetParent(Node? node, Node? parent)
    {
        if (node == null || parent == null) return;
        node.Parent = parent;// new Node() { Value = parent, Writable = true };
    }

    ///<summary>
    ///Prepares the Parser to accept a subexpression by (re)instantiating the
    ///subParser.
    ///</summary>
    ///<param name="exprStr">The expression string to prefix to the new Parser</param>
    private void StartSubExpression(string exprStr)
    {
        var endStates = _parserStates[_state].EndStates;
        if (endStates == null)
        {
            _parentStop = true;
            endStates = _stopMap;
        }
        _subParser = new Parser(grammar, exprStr, endStates);
    }



}