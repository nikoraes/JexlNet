using System.Globalization;
using System.Text.RegularExpressions;

namespace JexlNet;

public static class ParserHandlers
{
    ///<summary>
    ///Handles a subexpression that's used to define a transform argument's value.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">The subexpression tree</param>
    public static void ArgumentValue(this Parser parser, Node? node)
    {
        if (node != null && parser.cursor != null && parser.cursor.Args != null)
        {
            parser.cursor.Args.Add(node);
        }
    }

    ///<summary>
    ///Handles new array literals by adding them as a new node in the AST,
    ///initialized with an empty array.
    ///</summary>
    ///<param name="parser"></param>
    public static void ArrayStart(this Parser parser, Node? node)
    {
        parser.PlaceAtCursor(new Node("ArrayLiteral") { Value = new List<Node>() });
    }

    ///<summary>
    ///Handles a subexpression representing an element of an array literal.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">The subexpression tree</param>
    public static void ArrayValue(this Parser parser, Node? node)
    {
        if (node != null && parser.cursor != null && parser.cursor.Value is List<Node> list)
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
    public static void BinaryOp(this Parser parser, Node? node)
    {
        var precedence = parser.grammar.Elements[node?.Value].Precedence ?? 0;
        var parent = parser.cursor?.Parent;
        while (parent != null && parent.Operator != null && parser.grammar.Elements[parent.Operator].Precedence >= precedence)
        {
            parser.cursor = parent;
            parent = parser.cursor?.Parent;
        }
        var newNode = new Node("BinaryExpression")
        {
            Operator = node?.Value,
            Left = parser.cursor
        };
        Parser.SetParent(parser.cursor, newNode);
        parser.cursor = parent;
        parser.PlaceAtCursor(newNode);
    }

    ///<summary>
    ///Handles successive nodes in an identifier chain.  More specifically, it
    ///sets values that determine how the following identifier gets placed in the
    ///AST.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">A token object</param>
    public static void Dot(this Parser parser, Node? node)
    {
        parser.nextIdentEncapsulate =
            parser.cursor != null &&
            parser.cursor.Type != "UnaryExpression" &&
            (parser.cursor.Type != "BinaryExpression" ||
                (parser.cursor.Type == "BinaryExpression" && parser.cursor.Right != null));
        parser.nextIdentRelative = parser.cursor == null || (parser.cursor != null && parser.nextIdentEncapsulate != true);
        if (parser.nextIdentRelative == true)
        {
            parser.relative = true;
        }
    }

    ///<summary>
    ///Handles a subexpression used for filtering an array returned by an
    ///identifier chain.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">The subexpression tree</param>
    public static void Filter(this Parser parser, Node? node)
    {
        parser.PlaceBeforeCursor(new Node("FilterExpression")
        {
            Expr = node,
            Relative = parser.subParser?.IsRelative(),
            Subject = parser.cursor
        });
    }

    ///<summary>
    ///Handles identifier tokens when used to indicate the name of a function to
    ///be called.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">A token object</param>
    public static void FunctionCall(this Parser parser, Node? node)
    {
        parser.PlaceBeforeCursor(new Node("FunctionCall")
        {
            Name = parser.cursor?.Value,
            Args = [],
            Pool = "functions"
        });
    }

    ///<summary>
    ///Handles identifier tokens by adding them as a new node in the AST.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">A token object</param>
    public static void Identifier(this Parser parser, Node? node)
    {
        var newNode = new Node("Identifier") { Value = node?.Value };
        if (parser.nextIdentEncapsulate == true)
        {
            newNode.From = parser.cursor;
            parser.PlaceBeforeCursor(newNode);
            parser.nextIdentEncapsulate = false;
        }
        else
        {
            if (parser.nextIdentRelative == true)
            {
                newNode.Relative = true;
                parser.nextIdentRelative = false;
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
    public static void Literal(this Parser parser, Node? node)
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
    public static void ObjectKey(this Parser parser, Node? node)
    {
        // TODO: check whether node.Value is a literal that can be used as a key
        parser.cursorObjectKey = node?.Value;
    }

    ///<summary>
    ///Handles a subexpression that's used to define a transform argument's value.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">The subexpression tree</param>
    public static void ObjectStart(this Parser parser, Node? node)
    {
        parser.PlaceAtCursor(new Node("ObjectLiteral") { Value = new Dictionary<string, Node>() });
    }

    ///<summary>
    ///Handles an object value by adding its AST to the queued key on the object
    ///literal node currently at the cursor.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">The subexpression tree</param>
    public static void ObjectValue(this Parser parser, Node? node)
    {
        if (parser.cursor != null && parser.cursor.Value is Dictionary<string, Node> dict)
        {
            dict[parser.cursorObjectKey!] = node;
        }
    }

    ///<summary>
    ///Handles traditional subexpressions, delineated with the groupStart and
    ///groupEnd elements.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">The subexpression tree</param>
    public static void SubExpression(this Parser parser, Node? node)
    {
        parser.PlaceAtCursor(node);
    }

    ///<summary>
    ///Handles a completed consequent subexpression of a ternary operator.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">The subexpression tree</param>
    public static void TernaryEnd(this Parser parser, Node? node)
    {
        parser.cursor!.Alternate = node;
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
    public static void TernaryMid(this Parser parser, Node? node)
    {
        parser.cursor!.Consequent = node;
    }

    /**
     * Handles the start of a new ternary expression by encapsulating the entire
     * AST in a ConditionalExpression node, and using the existing tree as the
     * test element.
     */
    /* exports.ternaryStart = function()
    {
        this._tree = {
        type: 'ConditionalExpression',
    test: this._tree
        }
        this._cursor = this._tree
    } */

    ///<summary>
    ///Handles the start of a new ternary expression by encapsulating the entire
    ///AST in a ConditionalExpression node, and using the existing tree as the
    ///test element.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">The subexpression tree</param>
    public static void TernaryStart(this Parser parser, Node? node)
    {
        parser.tree = new Node("ConditionalExpression")
        {
            Test = parser.tree
        };
        parser.cursor = parser.tree;
    }

    /**
     * Handles identifier tokens when used to indicate the name of a transform to
     * be applied.
     * @param {{type: <string>}} token A token object
     */
    /* exports.transform = function(token)
    {
        this._placeBeforeCursor({
        type: 'FunctionCall',
    name: token.value,
    args: [this._cursor],
    pool: 'transforms'
        }) */

    ///<summary>
    ///Handles identifier tokens when used to indicate the name of a transform to
    ///be applied.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">A token object</param>
    public static void Transform(this Parser parser, Node? node)
    {
        parser.PlaceBeforeCursor(new Node("FunctionCall")
        {
            Name = node?.Value,
            Args = [parser.cursor!],
            Pool = "transforms"
        });
    }

    ///<summary>
    ///Handles token of type 'unaryOp', indicating that the operation has only
    ///one input: a right side.
    ///</summary>
    ///<param name="parser"></param>
    ///<param name="node">A token object</param>
    public static void UnaryOp(this Parser parser, Node? node)
    {
        parser.PlaceAtCursor(new Node("UnaryExpression")
        {
            Operator = node?.Value
        });
    }

    public static readonly Dictionary<string, Action<Parser, Node?>> Handlers = new()
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
