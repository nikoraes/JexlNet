namespace JexlNet;

internal static class ParserHandlers
{
    ///<summary>
    ///Handles a subexpression that's used to define a transform argument's value.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">The subexpression tree</param>
    internal static void ArgumentValue(this Parser parser, Node? node)
    {
        if (node != null && parser.Cursor != null && parser.Cursor.Args != null)
        {
            parser.Cursor.Args.Add(node);
        }
    }

    ///<summary>
    ///Handles new array literals by adding them as a new node in the AST,
    ///initialized with an empty array.
    ///</summary>
    ///<param name="parser"></param>
    internal static void ArrayStart(this Parser parser, Node? node)
    {
        parser.PlaceAtCursor(new Node("ArrayLiteral") { Value = new List<Node>() });
    }

    ///<summary>
    ///Handles a subexpression representing an element of an array literal.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">The subexpression tree</param>
    internal static void ArrayValue(this Parser parser, Node? node)
    {
        if (node != null && parser.Cursor != null && parser.Cursor.Value is List<Node> list)
        {
            list.Add(node);
        }
    }

    ///<summary>
    ///Handles tokens of type 'binaryOp', indicating an operation that has two
    ///inputs: a left side and a right side.
    ///</summary>   
    ///<param name="parser"></param>
    ///<param name="token">A token object</param>
    internal static void BinaryOp(this Parser parser, Node? node)
    {
        var precedence = parser.Grammar.Elements[node?.Value].Precedence ?? 0;
        var parent = parser.Cursor?.Parent;
        while (parent != null && parent.Operator != null && parser.Grammar.Elements[parent.Operator].Precedence >= precedence)
        {
            parser.Cursor = parent;
            parent = parser.Cursor?.Parent;
        }
        var newNode = new Node("BinaryExpression")
        {
            Operator = node?.Value,
            Left = parser.Cursor
        };
        Parser.SetParent(parser.Cursor, newNode);
        parser.Cursor = parent;
        parser.PlaceAtCursor(newNode);
    }

    ///<summary>
    ///Handles successive nodes in an identifier chain.  More specifically, it
    ///sets values that determine how the following identifier gets placed in the
    ///AST.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">A token object</param>
    internal static void Dot(this Parser parser, Node? node)
    {
        parser.NextIdentEncapsulate =
            parser.Cursor != null &&
            parser.Cursor.Type != "UnaryExpression" &&
            (parser.Cursor.Type != "BinaryExpression" ||
                (parser.Cursor.Type == "BinaryExpression" && parser.Cursor.Right != null));
        parser.NextIdentRelative = parser.Cursor == null || (parser.Cursor != null && parser.NextIdentEncapsulate != true);
        if (parser.NextIdentRelative == true)
        {
            parser.Relative = true;
        }
    }

    ///<summary>
    ///Handles a subexpression used for filtering an array returned by an
    ///identifier chain.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">The subexpression tree</param>
    internal static void Filter(this Parser parser, Node? node)
    {
        parser.PlaceBeforeCursor(new Node("FilterExpression")
        {
            Expr = node,
            Relative = parser.SubParser?.IsRelative(),
            Subject = parser.Cursor
        });
    }

    ///<summary>
    ///Handles identifier tokens when used to indicate the name of a function to
    ///be called.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">A token object</param>
    internal static void FunctionCall(this Parser parser, Node? node)
    {
        parser.PlaceBeforeCursor(new Node("FunctionCall")
        {
            Name = parser.Cursor?.Value,
            Args = new(),
            Pool = "functions"
        });
    }

    ///<summary>
    ///Handles identifier tokens by adding them as a new node in the AST.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">A token object</param>
    internal static void Identifier(this Parser parser, Node? node)
    {
        var newNode = new Node("Identifier") { Value = node?.Value };
        if (parser.NextIdentEncapsulate == true)
        {
            newNode.From = parser.Cursor;
            parser.PlaceBeforeCursor(newNode);
            parser.NextIdentEncapsulate = false;
        }
        else
        {
            if (parser.NextIdentRelative == true)
            {
                newNode.Relative = true;
                parser.NextIdentRelative = false;
            }
            parser.PlaceAtCursor(newNode);
        }
    }

    ///<summary>
    ///Handles literal values, such as strings, booleans, and numerics, by adding
    ///them as a new node in the AST.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">A token object</param>
    internal static void Literal(this Parser parser, Node? node)
    {
        parser.PlaceAtCursor(new Node("Literal") { Value = node?.Value });
    }

    /**
     * Queues a new object literal key to be written once a value is collected.
     * @param {{type: <string>}} token A token object
     */
    /* exports.objKey = function(token)
    {
        this._curObjKey = token.value
    } */

    ///<summary>
    ///Queues a new object literal key to be written once a value is collected.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">A token object</param>
    internal static void ObjectKey(this Parser parser, Node? node)
    {
        // TODO: check whether node.Value is a literal that can be used as a key
        parser.CursorObjectKey = node?.Value;
    }

    ///<summary>
    ///Handles a subexpression that's used to define a transform argument's value.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">The subexpression tree</param>
    internal static void ObjectStart(this Parser parser, Node? node)
    {
        parser.PlaceAtCursor(new Node("ObjectLiteral") { Value = new Dictionary<string, Node>() });
    }

    ///<summary>
    ///Handles an object value by adding its AST to the queued key on the object
    ///literal node currently at the cursor.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">The subexpression tree</param>
    internal static void ObjectValue(this Parser parser, Node? node)
    {
        if (parser.Cursor != null && parser.Cursor.Value is Dictionary<string, Node> dict)
        {
            dict[parser.CursorObjectKey!] = node;
        }
    }

    ///<summary>
    ///Handles traditional subexpressions, delineated with the groupStart and
    ///groupEnd elements.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">The subexpression tree</param>
    internal static void SubExpression(this Parser parser, Node? node)
    {
        parser.PlaceAtCursor(node);
    }

    ///<summary>
    ///Handles a completed consequent subexpression of a ternary operator.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">The subexpression tree</param>
    internal static void TernaryEnd(this Parser parser, Node? node)
    {
        parser.Cursor!.Alternate = node;
    }


    /**
     * Handles a completed consequent subexpression of a ternary operator.
     * @param {{type: <string>}} ast The subexpression tree
     */
    /* exports.ternaryMid = function(ast)
    {
        this._cursor.consequent = ast
    } */

    ///<summary>
    ///Handles a completed consequent subexpression of a ternary operator.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">The subexpression tree</param>
    internal static void TernaryMid(this Parser parser, Node? node)
    {
        parser.Cursor!.Consequent = node;
    }

    ///<summary>
    ///Handles the start of a new ternary expression by encapsulating the entire
    ///AST in a ConditionalExpression node, and using the existing tree as the
    ///test element.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">The subexpression tree</param>
    internal static void TernaryStart(this Parser parser, Node? node)
    {
        parser.Tree = new Node("ConditionalExpression")
        {
            Test = parser.Tree
        };
        parser.Cursor = parser.Tree;
    }

    ///<summary>
    ///Handles identifier tokens when used to indicate the name of a transform to
    ///be applied.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">A token object</param>
    internal static void Transform(this Parser parser, Node? node)
    {
        parser.PlaceBeforeCursor(new Node("FunctionCall")
        {
            Name = node?.Value,
            Args = new() { parser.Cursor! },
            Pool = "transforms"
        });
    }

    ///<summary>
    ///Handles token of type 'unaryOp', indicating that the operation has only
    ///one input: a right side.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">A token object</param>
    internal static void UnaryOp(this Parser parser, Node? node)
    {
        parser.PlaceAtCursor(new Node("UnaryExpression")
        {
            Operator = node?.Value
        });
    }

    internal static readonly Dictionary<string, Action<Parser, Node?>> Handlers = new()
    {
        { "argValue", ArgumentValue },
        { "arrayStart", ArrayStart },
        { "arrayVal", ArrayValue },
        { "binaryOp", BinaryOp },
        { "dot", Dot },
        { "filter", Filter },
        { "functionCall", FunctionCall },
        { "identifier", Identifier },
        { "literal", Literal },
        { "objKey", ObjectKey },
        { "objStart", ObjectStart },
        { "objVal", ObjectValue },
        { "subExpression", SubExpression },
        { "ternaryEnd", TernaryEnd },
        { "ternaryMid", TernaryMid },
        { "ternaryStart", TernaryStart },
        { "transform", Transform },
        { "unaryOp", UnaryOp }
    };
}
