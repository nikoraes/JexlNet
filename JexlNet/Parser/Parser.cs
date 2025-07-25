using System;
using System.Collections.Generic;

namespace JexlNet
{
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
        public Parser(
            Grammar grammar,
            string prefix = "",
            Dictionary<GrammarType, GrammarType> stopMap = null
        )
        {
            Grammar = grammar;
            State = GrammarType.ExpectOperand;
            ExpressionString = prefix;
            Relative = false;
            StopMap = stopMap ?? new Dictionary<GrammarType, GrammarType>();
            ParentStop = false;
            Tree = null;
            Cursor = null;
            SubParser = null;
            NextIdentEncapsulate = null;
            NextIdentRelative = null;
            CursorObjectKey = null;
        }

        internal readonly Grammar Grammar;
        internal GrammarType State;
        internal string ExpressionString;
        internal bool Relative;
        internal readonly Dictionary<GrammarType, GrammarType> StopMap;
        internal bool ParentStop;
        internal Node Tree;
        internal Node Cursor;
        internal Parser SubParser;
        internal bool? NextIdentEncapsulate;
        internal bool? NextIdentRelative;
        internal string CursorObjectKey;

        ///<summary>
        ///Processes a new token into the AST and manages the transitions of the state
        ///machine.
        ///</summary>
        ///<param name="node">A token object, as provided by the {@link Lexer#tokenize} function.</param>
        ///<returns>the stopState value if this parser encountered a token
        ///in the stopState map 'false' if tokens can continue.</returns>
        internal GrammarType AddToken(Node node)
        {
            if (State == GrammarType.Complete)
            {
                throw new Exception("Cannot add a new token to a completed parser.");
            }
            ParserState state = ParserStates.States[State];
            string startExpr = ExpressionString;
            ExpressionString += node.Raw;
            if (state.SubHandler != null)
            {
                if (SubParser == null)
                {
                    StartSubExpression(startExpr);
                }
                GrammarType stopState = SubParser.AddToken(node);
                if (stopState != GrammarType.Stop)
                {
                    EndSubExpression();
                    if (ParentStop)
                    {
                        return stopState;
                    }
                    State = stopState;
                }
            }
            else if (state.TokenTypes.TryGetValue(node.Type, out ParserStateTokenType typeOpts))
            {
                // var handleFunc = ParserHandlers.Handlers[token.Type!];
                ParserHandlers.Handlers.TryGetValue(node.Type, out var handleFunc);
                if (typeOpts.Handler != null)
                {
                    handleFunc = typeOpts.Handler;
                }
                handleFunc?.Invoke(this, node);
                if (typeOpts.ToState != null)
                {
                    State = (GrammarType)typeOpts.ToState;
                }
            }
            else if (StopMap.TryGetValue(node.Type, out GrammarType value))
            {
                return value;
            }
            else
            {
                throw new Exception(
                    $"Token {node.Raw} ({node.Type}) unexpected in expression: {ExpressionString}"
                );
            }

            return GrammarType.Stop;
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
        public Node Complete()
        {
            if (
                Cursor != null
                && (
                    !ParserStates.States.TryGetValue(State, out var state)
                    || state.Completable != true
                )
            )
            {
                throw new Exception($"Unexpected end of expression: {ExpressionString}");
            }
            if (SubParser != null)
            {
                EndSubExpression();
            }
            State = GrammarType.Complete;
            return Cursor != null ? Tree : null;
        }

        ///<summary>
        ///Indicates whether the expression tree contains a relative path identifier.
        ///</summary>
        ///<returns>true if a relative identifier exists false otherwise.</returns>
        internal bool IsRelative()
        {
            return Relative;
        }

        ///<summary>
        ///Ends a subexpression by completing the subParser and passing its result
        ///to the subHandler configured in the current state.
        ///</summary>
        internal void EndSubExpression()
        {
            if (
                ParserStates.States.TryGetValue(State, out var state)
                && state.SubHandler != null
                && SubParser != null
            )
            {
                state.SubHandler(this, SubParser.Complete());
            }
            SubParser = null;
        }

        ///<summary>
        ///Places a new tree node at the current position of the cursor (to the 'right'
        ///property) and then advances the cursor to the new node. This function also
        ///handles setting the parent of the new node.
        ///</summary>
        ///<param name="node">A node to be added to the AST</param>
        internal void PlaceAtCursor(Node node)
        {
            if (Cursor == null)
            {
                Tree = node;
            }
            else
            {
                Cursor.Right = node;
                SetParent(node, Cursor);
            }
            Cursor = node;
        }

        ///<summary>
        ///Places a tree node before the current position of the cursor, replacing
        ///the node that the cursor currently points to. This should only be called in
        ///cases where the cursor is known to exist, and the provided node already
        ///contains a pointer to what's at the cursor currently.
        ///</summary>
        ///<param name="node">A node to be added to the AST</param>
        internal void PlaceBeforeCursor(Node node)
        {
            Cursor = Cursor.Parent;
            PlaceAtCursor(node);
        }

        ///<summary>
        ///Sets the parent of a node by creating a non-enumerable Parent property
        ///that points to the supplied parent argument.
        ///</summary>
        ///<param name="node">A node of the AST on which to set a new parent</param>
        ///<param name="parent">An existing node of the AST to serve as the parent of the new node</param>
        internal static void SetParent(Node node, Node parent)
        {
            if (node == null || parent == null)
                return;
            node.Parent = parent; // new Node() { Value = parent, Writable = true };
        }

        ///<summary>
        ///Prepares the Parser to accept a subexpression by (re)instantiating the
        ///subParser.
        ///</summary>
        ///<param name="exprStr">The expression string to prefix to the new Parser</param>
        internal void StartSubExpression(string exprStr)
        {
            var endStates = ParserStates.States[State].EndStates;
            if (endStates == null)
            {
                ParentStop = true;
                endStates = StopMap;
            }
            SubParser = new Parser(Grammar, exprStr, endStates);
        }
    }
}
