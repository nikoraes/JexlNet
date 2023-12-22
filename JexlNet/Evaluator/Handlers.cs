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
    public static Task<dynamic> ArrayLiteral(Evaluator evaluator, Node? node)
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
    public static Task<dynamic> BinaryExpression(Evaluator evaluator, Node? node)
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
        if (grammarOp.EvalOnDemand == true && node.Left != null && node.Right != null)
        {
            var wrap = new Func<Node?, dynamic>((subAst) => new { Eval = new Func<Task<dynamic>>(() => evaluator.Eval(subAst)) });
            return Task.FromResult(grammarOp.Evaluate?.Invoke([wrap(node?.Left), wrap(node?.Right)]));
        }
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
    public static Task<dynamic> ConditionalExpression(Evaluator evaluator, Node? node)
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
    public static Task<dynamic> FilterExpression(Evaluator evaluator, Node? node)
    {
        if (node?.Subject == null)
        {
            throw new Exception("FilterExpression node has no subject");
        }
        return evaluator.Eval(node?.Subject)
            .ContinueWith((subject) =>
            {
                if (node?.Relative == true)
                {
                    return evaluator.FilterRelative(subject.Result, node?.Expr);
                }
                return evaluator.FilterStatic(subject.Result, node?.Expr);
            });
    }

    /**
 * Evaluates an Identifier by either stemming from the evaluated 'from'
 * expression tree or accessing the context provided when this Evaluator was
 * constructed.
 * @param {{type: 'Identifier', value: <string>, [from]: {}}} ast An expression
 *      tree with an Identifier as the top node
 * @returns {Promise<*>|*} either the identifier's value, or a Promise that
 *      will resolve with the identifier's value.
 * @private
 */
    /* exports.Identifier = function(ast)
    {
        if (!ast.from)
        {
            return ast.relative ? this._relContext[ast.value] : this._context[ast.value]
        }
        return this.eval(ast.from).then((context) =>
        {
            if (context === undefined || context === null)
            {
                return undefined
            }
            if (Array.isArray(context))
            {
                context = context[0]
            }
            return context[ast.value]
        })
    } */

    ///<summary>
    ///Evaluates an Identifier by either stemming from the evaluated 'from'
    ///expression tree or accessing the context provided when this Evaluator was
    ///constructed.
    ///</summary>
    ///<param name="evaluator"></param>
    ///<param name="node">An expression tree with an Identifier as the top node</param>
    ///<returns>either the identifier's value, or a Promise that will resolve with the identifier's value.</returns>
    public static Task<dynamic> Identifier(Evaluator evaluator, Node? node)
    {
        throw new NotImplementedException();

        /* if (node?.From == null)
        {
            return Task.FromResult(node?.Relative == true ? evaluator.RelContext?[node?.Value] : evaluator.Context?[node?.Value]);
        } */
        /* return evaluator.Eval(node?.From)
            .ContinueWith((context) =>
            {
                if (context.Result == null)
                {
                    return null;
                }
                if (context.Result is List<dynamic> list)
                {
                    context.Result = list[0];
                }
                return context.Result[node?.Value];
            }); */
    }

    /**
 * Evaluates a Literal by returning its value property.
 * @param {{type: 'Literal', value: <string|number|boolean>}} ast An expression
 *      tree with a Literal as its only node
 * @returns {string|number|boolean} The value of the Literal node
 * @private
 */
    /* exports.Literal = function(ast)
    {
        return ast.value
    } */

    ///<summary>
    ///Evaluates a Literal by returning its value property.
    ///</summary>
    ///<param name="evaluator"></param>
    ///<param name="node">An expression tree with a Literal as its only node</param>
    ///<returns>The value of the Literal node</returns>
    public static Task<dynamic> Literal(Evaluator evaluator, Node? node)
    {
        return Task.FromResult<dynamic>(node?.Value);
    }


    public static readonly Dictionary<string, Func<Evaluator, Node?, Task<dynamic>>> Handlers = new()
    {
        { "ArrayLiteral", ArrayLiteral },
        { "BinaryExpression", BinaryExpression },
        { "ConditionalExpression", ConditionalExpression },
        { "FilterExpression", FilterExpression },
        { "Identifier", Identifier },
        { "Literal", Literal }
    };
}
