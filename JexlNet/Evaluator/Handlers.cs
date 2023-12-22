namespace JexlNet;

public static class EvaluatorHandlers
{
    /// <summary>
    /// Evaluates an ArrayLiteral by returning its value, with each element
    /// independently run through the evaluator.
    /// </summary>
    /// <param name="evaluator"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public static Task<dynamic?> ArrayLiteral(Evaluator evaluator, Node? node)
    {
        return evaluator.EvalArray(node?.Value);
    }

    ///<summary>
    ///Evaluates a BinaryExpression node by running the Grammar's evaluator for
    ///the given operator. Note that binary expressions support two types of
    ///evaluators: `eval` is called with the left and right operands pre-evaluated.
    ///`evalOnDemand`, if it exists, will be called with the left and right operands
    ///each individually wrapped in an object with an "eval" function that returns
    ///a promise with the resulting value. This allows the binary expression to
    ///evaluate the operands conditionally.
    ///</summary>
    ///<param name="evaluator"></param>
    ///<param name="node">An expression tree with a BinaryExpression as the top node</param>
    ///<returns>resolves with the value of the BinaryExpression.</returns>
    public static Task<dynamic?> BinaryExpression(Evaluator evaluator, Node? node)
    {
        if (node?.Operator == null)
        {
            throw new Exception("BinaryExpression node has no operator");
        }
        var grammarOp = evaluator.Grammar.Elements[node.Operator];
        if (grammarOp == null || grammarOp.Evaluate == null)
        {
            throw new Exception($"BinaryExpression node has unknown operator: {node.Operator}");
        }
        /* if (grammarOp.EvalOnDemand == true && node.Left != null && node.Right != null)
        {
            var wrap = new Func<Node?, dynamic>((subAst) => new { Eval = new Func<Task<dynamic>>(() => evaluator.Eval(subAst)) });
            return Task.FromResult(grammarOp.Evaluate?.Invoke([wrap(node?.Left), wrap(node?.Right)]));
        } */
        return Task
            .WhenAll([evaluator.Eval(node?.Left), evaluator.Eval(node?.Right)])
            .ContinueWith((arr) => grammarOp.Evaluate([arr.Result[0], arr.Result[1]]));
    }

    ///<summary>
    ///Evaluates a ConditionalExpression node by first evaluating its test branch,
    ///and resolving with the consequent branch if the test is truthy, or the
    ///alternate branch if it is not. If there is no consequent branch, the test
    ///result will be used instead.
    ///</summary>
    ///<param name="evaluator"></param>
    ///<param name="node">An expression tree with a ConditionalExpression as the top node</param>
    public static Task<dynamic?> ConditionalExpression(Evaluator evaluator, Node? node)
    {
        if (node?.Test == null)
        {
            throw new Exception("ConditionalExpression node has no test");
        }
        return evaluator.Eval(node?.Test)
            .ContinueWith((res) =>
            {
                if (res.Result)
                {
                    if (node?.Consequent != null)
                    {
                        return evaluator.Eval(node?.Consequent);
                    }
                    return res.Result;
                }
                return evaluator.Eval(node?.Alternate);
            });
    }

    ///<summary>
    ///Evaluates a FilterExpression by applying it to the subject value.
    ///</summary>
    ///<param name="evaluator"></param>
    ///<param name="node">An expression tree with a FilterExpression as the top node</param>
    ///<returns>resolves with the value of the FilterExpression.</returns>
    public static Task<dynamic?> FilterExpression(Evaluator evaluator, Node? node)
    {
        if (node?.Subject == null)
        {
            throw new Exception("FilterExpression node has no subject");
        }
        return evaluator.Eval(node?.Subject)
            .ContinueWith((subject) =>
            {
                var subjectResult = subject.Result;
                if (node?.Relative == true)
                {
                    return (dynamic?)evaluator.FilterRelative(subjectResult, node?.Expr);
                }
                return evaluator.FilterStatic(subjectResult, node?.Expr);
            });
    }

    ///<summary>
    ///Evaluates an Identifier by either stemming from the evaluated 'from'
    ///expression tree or accessing the context provided when this Evaluator was
    ///constructed.
    ///</summary>
    ///<param name="evaluator"></param>
    ///<param name="node">An expression tree with an Identifier as the top node</param>
    ///<returns>either the identifier's value, or a Promise that will resolve with the identifier's value.</returns>
    public static Task<dynamic?> Identifier(Evaluator evaluator, Node? node)
    {
        if (node?.From == null)
        {
            return Task.FromResult<dynamic?>(node?.Relative == true ? evaluator.RelContext?[node?.Value] : evaluator.Context?[node?.Value]);
        }
        return evaluator.Eval(node?.From)
            .ContinueWith((from) =>
            {
                var fromResult = from.Result;
                if (fromResult == null)
                {
                    return null;
                }
                if (fromResult is List<dynamic> list)
                {
                    fromResult = list[0];
                }
                return fromResult[node?.Value];
            });
    }

    ///<summary>
    ///Evaluates a Literal by returning its value property.
    ///</summary>
    ///<param name="evaluator"></param>
    ///<param name="node">An expression tree with a Literal as its only node</param>
    ///<returns>The value of the Literal node</returns>
    public static Task<dynamic?> Literal(Evaluator evaluator, Node? node)
    {
        return Task.FromResult<dynamic?>(node?.Value);
    }

    ///<summary>
    ///Evaluates an ObjectLiteral by returning its value, with each key
    ///independently run through the evaluator.
    ///</summary>
    ///<param name="evaluator"></param>
    ///<param name="node">An expression tree with a Literal as its only node</param>
    ///<returns>The value of the Literal node</returns>
    public static Task<dynamic?> ObjectLiteral(Evaluator evaluator, Node? node)
    {
        return evaluator.EvalMap(node?.Value);
    }

    ///<summary>
    ///Evaluates a FunctionCall node by applying the supplied arguments to a
    ///function defined in one of the grammar's function pools.
    ///</summary>
    ///<param name="evaluator"></param>
    ///<param name="node">An expression tree with a FunctionCall as the top node</param>
    ///<returns>the value of the function call, or a Promise that will resolve with the resulting value.</returns>
    public static Task<dynamic?> FunctionCall(Evaluator evaluator, Node? node)
    {

        if (node?.Pool == null)
        {
            throw new Exception("FunctionCall node has no pool");
        }
        if (node.Name == null)
        {
            throw new Exception("Function or transform not defined");
        }
        if (node.Args == null)
        {
            throw new Exception("Arguments not defined");
        }
        if (node.Pool == "functions" && evaluator.Grammar.Functions.TryGetValue(node.Name, out var func))
        {
            return evaluator.EvalArray(node.Args)
                .ContinueWith((args) => func(args.Result));
        }
        else if (node.Pool == "transforms" && evaluator.Grammar.Transforms.TryGetValue(node.Name, out var transform))
        {
            return evaluator.EvalArray(node.Args)
                .ContinueWith((args) => transform(args.Result));
        }
        else
        {
            throw new Exception($"Function or transform not found: {node.Pool}.{node.Name}");
        }
    }

    ///<summary>
    ///Evaluates a Unary expression by passing the right side through the
    ///operator's eval function.
    ///</summary>
    ///<param name="evaluator"></param>
    ///<param name="node">An expression tree with a UnaryExpression as the top node</param>
    ///<returns>resolves with the value of the UnaryExpression.</returns>
    public static Task<dynamic?> UnaryExpression(Evaluator evaluator, Node? node)
    {
        if (node?.Operator == null)
        {
            throw new Exception("UnaryExpression node has no operator");
        }
        if (node?.Right == null)
        {
            throw new Exception("UnaryExpression node has no right");
        }
        var grammarOp = evaluator.Grammar.Elements[node.Operator];
        if (grammarOp == null || grammarOp.Evaluate == null)
        {
            throw new Exception($"UnaryExpression node has unknown operator: {node.Operator}");
        }
        return evaluator.Eval(node?.Right)
            .ContinueWith<dynamic?>((right) => grammarOp.Evaluate(right.Result));
    }


    public static readonly Dictionary<string, Func<Evaluator, Node?, Task<dynamic?>>> Handlers = new()
    {
        { "ArrayLiteral", ArrayLiteral },
        { "BinaryExpression", BinaryExpression },
        { "ConditionalExpression", ConditionalExpression },
        { "FilterExpression", FilterExpression },
        { "Identifier", Identifier },
        { "Literal", Literal },
        { "ObjectLiteral", ObjectLiteral },
        { "FunctionCall", FunctionCall },
        { "UnaryExpression", UnaryExpression }
    };
}
