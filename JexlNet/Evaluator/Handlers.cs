using System.Reflection;

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
    public static async Task<dynamic?> ArrayLiteralAsync(Evaluator evaluator, Node? node)
    {
        return await evaluator.EvalArrayAsync(node?.Value);
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
    public static async Task<dynamic?> BinaryExpressionAsync(Evaluator evaluator, Node? node)
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
        // EvaluateOnDemand allows to only conditionally evaluate one side of the binary expression
        if (grammarOp.EvaluateOnDemandAsync != null && node.Left != null && node.Right != null)
        {
            var wrap = new Func<Node?, Func<Task<dynamic?>>>((subAst) => async () => await evaluator.EvalAsync(subAst));
            return await grammarOp.EvaluateOnDemandAsync([wrap(node?.Left), wrap(node?.Right)]);
        }
        var leftResult = await evaluator.EvalAsync(node?.Left);
        var rightResult = await evaluator.EvalAsync(node?.Right);
        return grammarOp.Evaluate([leftResult, rightResult]);
    }

    ///<summary>
    ///Evaluates a ConditionalExpression node by first evaluating its test branch,
    ///and resolving with the consequent branch if the test is truthy, or the
    ///alternate branch if it is not. If there is no consequent branch, the test
    ///result will be used instead.
    ///</summary>
    ///<param name="evaluator"></param>
    ///<param name="node">An expression tree with a ConditionalExpression as the top node</param>
    public static async Task<dynamic?> ConditionalExpression(Evaluator evaluator, Node? node)
    {
        if (node?.Test == null)
        {
            throw new Exception("ConditionalExpression node has no test");
        }
        var testResult = await evaluator.EvalAsync(node?.Test);
        // If it's a string, we consider it truthy if it's non-empty
        // If it's a decimal, we consider it truthy it's non-zero
        // Align with behaviour in Javascript
        if (testResult?.GetType() == typeof(string)
            ? !string.IsNullOrEmpty(testResult)
            : (testResult?.GetType() == typeof(decimal) ? testResult != 0 : testResult == true))
        {
            if (node?.Consequent != null)
            {
                return await evaluator.EvalAsync(node?.Consequent);
            }
            return testResult;
        }
        return await evaluator.EvalAsync(node?.Alternate);
    }

    ///<summary>
    ///Evaluates a FilterExpression by applying it to the subject value.
    ///</summary>
    ///<param name="evaluator"></param>
    ///<param name="node">An expression tree with a FilterExpression as the top node</param>
    ///<returns>resolves with the value of the FilterExpression.</returns>
    public static async Task<dynamic?> FilterExpression(Evaluator evaluator, Node? node)
    {
        if (node?.Subject == null)
        {
            throw new Exception("FilterExpression node has no subject");
        }
        var subjectResult = await evaluator.EvalAsync(node?.Subject);
        Console.WriteLine($"{subjectResult}");
        if (node?.Relative == true)
        {
            return await evaluator.FilterRelativeAsync(subjectResult, node?.Expr);
        }
        return await evaluator.FilterStaticAsync(subjectResult, node?.Expr);
    }

    ///<summary>
    ///Evaluates an Identifier by either stemming from the evaluated 'from'
    ///expression tree or accessing the context provided when this Evaluator was
    ///constructed.
    ///</summary>
    ///<param name="evaluator"></param>
    ///<param name="node">An expression tree with an Identifier as the top node</param>
    ///<returns>either the identifier's value, or a Promise that will resolve with the identifier's value.</returns>
    public static async Task<dynamic?> IdentifierAsync(Evaluator evaluator, Node? node)
    {
        if (node?.From == null)
        {
            if (node?.Relative == true && evaluator.RelContext != null && evaluator.RelContext?.ContainsKey(node?.Value))
            {
                return evaluator.RelContext?[node?.Value];
            }
            else if (evaluator.Context != null && evaluator.Context?.ContainsKey(node?.Value))
            {
                var result = evaluator.Context?[node?.Value];
                var resultType = result?.GetType();
                if (_invokableTypes.Contains(resultType) || resultType?.IsGenericType && _invokableTypes.Contains(resultType?.GetGenericTypeDefinition()))
                {
                    return await result?.Invoke();
                }
                if (result != null && typeof(Task).IsAssignableFrom(resultType))
                {
                    return await result;
                }
                return result;
            }
            else return null;
        }
        var fromResult = await evaluator.EvalAsync(node?.From);
        Type? fromResultType = fromResult?.GetType();
        if (fromResultType == null || node?.Value == null)
        {
            return null;
        }
        else if (fromResult is List<dynamic> list)
        {
            fromResult = list.First();
        }
        else if (fromResultType!.IsGenericType && fromResultType.GetGenericTypeDefinition() == typeof(List<>))
        {
            fromResult = fromResult?.First();
        }
        else if (fromResult is Dictionary<string, dynamic> dict)
        {
            string key = node!.Value!;
            if (dict.TryGetValue(key, out dynamic? value))
            {
                return value;
            }
            else return null;
        }
        else if ((fromResultType.IsGenericType || fromResultType != null) && node?.Value != null && node!.Value is string)
        {
            // Try to access builtin properties
            PropertyInfo? propertyInfo = fromResultType?.GetProperty($"{node!.Value}");
            return propertyInfo?.GetValue(fromResult);
        }
        return fromResult?[node?.Value];
    }

    ///<summary>
    ///Evaluates a Literal by returning its value property.
    ///</summary>
    ///<param name="evaluator"></param>
    ///<param name="node">An expression tree with a Literal as its only node</param>
    ///<returns>The value of the Literal node</returns>
    public static async Task<dynamic?> LiteralAsync(Evaluator evaluator, Node? node)
    {
        return await Task.FromResult<dynamic?>(node?.Value);
    }

    ///<summary>
    ///Evaluates an ObjectLiteral by returning its value, with each key
    ///independently run through the evaluator.
    ///</summary>
    ///<param name="evaluator"></param>
    ///<param name="node">An expression tree with a Literal as its only node</param>
    ///<returns>The value of the Literal node</returns>
    public static async Task<dynamic?> ObjectLiteralAsync(Evaluator evaluator, Node? node)
    {
        return await evaluator.EvalMapAsync(node?.Value);
    }

    ///<summary>
    ///Evaluates a FunctionCall node by applying the supplied arguments to a
    ///function defined in one of the grammar's function pools.
    ///</summary>
    ///<param name="evaluator"></param>
    ///<param name="node">An expression tree with a FunctionCall as the top node</param>
    ///<returns>the value of the function call, or a Promise that will resolve with the resulting value.</returns>
    public static async Task<dynamic?> FunctionCallAsync(Evaluator evaluator, Node? node)
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
            var argsResult = await evaluator.EvalArrayAsync(node.Args);
            return await func(argsResult);
        }
        else if (node.Pool == "transforms" && evaluator.Grammar.Transforms.TryGetValue(node.Name, out var transform))
        {
            var argsResult = await evaluator.EvalArrayAsync(node.Args);
            return await transform(argsResult);
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
    public static async Task<dynamic?> UnaryExpressionAsync(Evaluator evaluator, Node? node)
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
        var rightResult = await evaluator.EvalAsync(node?.Right);
        return await grammarOp.Evaluate(rightResult);
    }


    public static readonly Dictionary<string, Func<Evaluator, Node?, Task<dynamic?>>> Handlers = new()
    {
        { "ArrayLiteral", ArrayLiteralAsync },
        { "BinaryExpression", BinaryExpressionAsync },
        { "ConditionalExpression", ConditionalExpression },
        { "FilterExpression", FilterExpression },
        { "Identifier", IdentifierAsync },
        { "Literal", LiteralAsync },
        { "ObjectLiteral", ObjectLiteralAsync },
        { "FunctionCall", FunctionCallAsync },
        { "UnaryExpression", UnaryExpressionAsync }
    };

    private static readonly HashSet<Type> _invokableTypes =
     [
         typeof(Action), typeof(Action<>), typeof(Action<,>),    // etc
         typeof(Func<>), typeof(Func<,>), typeof(Func<,,>),      // etc
     ];
}


