using System;
using System.Collections.Generic;

namespace JexlNet
{
    internal static class ParserHandlers
    {
        ///<summary>
        ///Handles a subexpression that's used to define a transform argument's value.
        ///</summary>
        ///<param name="parser"></param>
        ///<param name="node">The subexpression tree</param>
        internal static void ArgumentValue(this Parser parser, Node node)
        {
            if (node == null || parser.Cursor?.Args == null) return;
            parser.Cursor.Args.Add(node);
        }

        ///<summary>
        ///Handles new array literals by adding them as a new node in the AST,
        ///initialized with an empty array.
        ///</summary>
        ///<param name="parser"></param>
        internal static void ArrayStart(this Parser parser, Node node)
        {
            parser.PlaceAtCursor(new Node(GrammarType.ArrayLiteral)
            {
                Array = new List<Node>()
            });
        }

        ///<summary>
        ///Handles a subexpression representing an element of an array literal.
        ///</summary>
        ///<param name="parser"></param>
        ///<param name="node">The subexpression tree</param>
        internal static void ArrayValue(this Parser parser, Node node)
        {
            if (parser.Cursor?.Array == null) throw new Exception("ParserHandlers.ArrayValue: cursor.Array is null");
            if (node == null) return;
            parser.Cursor.Array.Add(node);
        }

        ///<summary>
        ///Handles tokens of type 'binaryOp', indicating an operation that has two
        ///inputs: a left side and a right side.
        ///</summary>   
        ///<param name="parser"></param>
        ///<param name="token">A token object</param>
        internal static void BinaryOperator(this Parser parser, Node node)
        {
            string gramarElementKey = node?.Value?.GetValue<string>() ?? throw new ApplicationException("node.Value is null");
            ElementGrammar grammarElement = parser.Grammar.Elements.TryGetValue(gramarElementKey, out ElementGrammar value) ? value : throw new ApplicationException($"Grammar element {gramarElementKey} not found");
            int precedence = grammarElement.Precedence;
            Node parent = parser.Cursor?.Parent;
            while (parent != null && parent.Operator != null && parser.Grammar.Elements[parent.Operator].Precedence >= precedence)
            {
                parser.Cursor = parent;
                parent = parser.Cursor?.Parent;
            }
            var newNode = new Node(GrammarType.BinaryExpression)
            {
                Operator = gramarElementKey,
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
        internal static void Dot(this Parser parser, Node node)
        {
            parser.NextIdentEncapsulate =
                parser.Cursor != null &&
                parser.Cursor.Type != GrammarType.UnaryExpression &&
                (parser.Cursor.Type != GrammarType.BinaryExpression ||
                    (parser.Cursor.Type == GrammarType.BinaryExpression && parser.Cursor.Right != null));
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
        internal static void Filter(this Parser parser, Node node)
        {
            parser.PlaceBeforeCursor(new Node(GrammarType.FilterExpression)
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
        internal static void FunctionCall(this Parser parser, Node node)
        {
            if (parser.Cursor?.Value == null) throw new Exception("ParserHandlers.FunctionCall: cursor.Value is null");
            parser.PlaceBeforeCursor(new Node(GrammarType.FunctionCall)
            {
                Name = parser.Cursor.Value.GetValue<string>(),
                Args = new List<Node>(),
                Pool = Grammar.PoolType.Functions
            });
        }

        ///<summary>
        ///Handles identifier tokens by adding them as a new node in the AST.
        ///</summary>
        ///<param name="parser"></param>
        ///<param name="node">A token object</param>
        internal static void Identifier(this Parser parser, Node node)
        {
            if (node?.Value == null) throw new Exception("ParserHandlers.Identifier: node is null");
            Node newNode = new Node(GrammarType.Identifier, node.Value);
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
        internal static void Literal(this Parser parser, Node node)
        {
            if (node?.Value == null) throw new Exception("ParserHandlers.Literal: node is null");
            parser.PlaceAtCursor(new Node(GrammarType.Literal, node.Value));
        }

        ///<summary>
        ///Queues a new object literal key to be written once a value is collected.
        ///</summary>
        ///<param name="parser"></param>
        ///<param name="node">A token object</param>
        internal static void ObjectKey(this Parser parser, Node node)
        {
            if (node?.Value == null) throw new Exception("ParserHandlers.ObjectKey: node is null");
            parser.CursorObjectKey = node.Value.GetValue<string>();
        }

        ///<summary>
        ///Handles a subexpression that's used to define a transform argument's value.
        ///</summary>
        ///<param name="parser"></param>
        ///<param name="node">The subexpression tree</param>
        internal static void ObjectStart(this Parser parser, Node node)
        {
            parser.PlaceAtCursor(new Node(GrammarType.ObjectLiteral)
            {
                Object = new Dictionary<string, Node>()
            });
        }

        ///<summary>
        ///Handles an object value by adding its AST to the queued key on the object
        ///literal node currently at the cursor.
        ///</summary>
        ///<param name="parser"></param>
        ///<param name="node">The subexpression tree</param>
        internal static void ObjectValue(this Parser parser, Node node)
        {
            if (node == null) throw new Exception("ParserHandlers.ObjectValue: node is null");
            if (parser.Cursor?.Object == null) throw new Exception("ParserHandlers.ObjectValue: cursor.Object is null");
            if (parser.CursorObjectKey == null) throw new Exception("ParserHandlers.ObjectValue: cursorObjectKey is null");
            parser.Cursor.Object[parser.CursorObjectKey] = node;
        }

        ///<summary>
        ///Handles traditional subexpressions, delineated with the groupStart and
        ///groupEnd elements.
        ///</summary>
        ///<param name="parser"></param>
        ///<param name="node">The subexpression tree</param>
        internal static void SubExpression(this Parser parser, Node node)
        {
            parser.PlaceAtCursor(node);
        }

        ///<summary>
        ///Handles a completed consequent subexpression of a ternary operator.
        ///</summary>
        ///<param name="parser"></param>
        ///<param name="node">The subexpression tree</param>
        internal static void TernaryEnd(this Parser parser, Node node)
        {
            if (parser.Cursor == null) throw new Exception("ParserHandlers.TernaryEnd: cursor is null");
            parser.Cursor.Alternate = node;
        }

        ///<summary>
        ///Handles a completed consequent subexpression of a ternary operator.
        ///</summary>
        ///<param name="parser"></param>
        ///<param name="node">The subexpression tree</param>
        internal static void TernaryMid(this Parser parser, Node node)
        {
            if (parser.Cursor == null) throw new Exception("ParserHandlers.TernaryMid: cursor is null");
            parser.Cursor.Consequent = node;
        }

        ///<summary>
        ///Handles the start of a new ternary expression by encapsulating the entire
        ///AST in a ConditionalExpression node, and using the existing tree as the
        ///test element.
        ///</summary>
        ///<param name="parser"></param>
        ///<param name="node">The subexpression tree</param>
        internal static void TernaryStart(this Parser parser, Node node)
        {
            parser.Tree = new Node(GrammarType.ConditionalExpression)
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
        internal static void Transform(this Parser parser, Node node)
        {
            if (node?.Value == null) throw new Exception("ParserHandlers.Transform: node.Value is null");
            if (parser.Cursor == null) throw new Exception("ParserHandlers.Transform: cursor is null");
            parser.PlaceBeforeCursor(new Node(GrammarType.FunctionCall)
            {
                Name = node.Value.GetValue<string>(),
                Args = new List<Node> { parser.Cursor },
                Pool = Grammar.PoolType.Transforms
            });
        }

        ///<summary>
        ///Handles token of type 'unaryOp', indicating that the operation has only
        ///one input: a right side.
        ///</summary>
        ///<param name="parser"></param>
        ///<param name="node">A token object</param>
        internal static void UnaryOperator(this Parser parser, Node node)
        {
            if (node?.Value == null) throw new Exception("ParserHandlers.UnaryOp: node.Value is null");
            parser.PlaceAtCursor(new Node(GrammarType.UnaryExpression)
            {
                Operator = node.Value.GetValue<string>()
            });
        }
        internal static readonly Dictionary<GrammarType, Action<Parser, Node>> Handlers = new Dictionary<GrammarType, Action<Parser, Node>>()
        {
            { GrammarType.ArgumentValue, ArgumentValue },
            { GrammarType.ArrayStart, ArrayStart },
            { GrammarType.ArrayValue, ArrayValue },
            { GrammarType.BinaryOperator, BinaryOperator },
            { GrammarType.Dot, Dot },
            { GrammarType.Filter, Filter },
            { GrammarType.FunctionCall, FunctionCall },
            { GrammarType.Identifier, Identifier },
            { GrammarType.Literal, Literal },
            { GrammarType.ObjectKey, ObjectKey },
            { GrammarType.ObjectStart, ObjectStart },
            { GrammarType.ObjectValue, ObjectValue },
            { GrammarType.SubExpression, SubExpression },
            { GrammarType.TernaryEnd, TernaryEnd },
            { GrammarType.TernaryMid, TernaryMid },
            { GrammarType.TernaryStart, TernaryStart },
            { GrammarType.Transform, Transform },
            { GrammarType.UnaryOperator, UnaryOperator }
        };
    }
}
