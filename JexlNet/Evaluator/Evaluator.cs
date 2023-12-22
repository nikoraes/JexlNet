namespace JexlNet;

public class Evaluator(
    Grammar grammar,
    Dictionary<string, dynamic>? context = null,
    dynamic? subject = null)
{
    public Grammar Grammar { get; set; } = grammar;
    public Dictionary<string, dynamic>? Context { get; set; } = context;
    public dynamic? RelContext { get; set; } = subject;


    /// <summary>
    /// Evaluates an expression tree within the configured context.
    /// </summary>
    /// <param name="ast">An expression tree object</param>
    /// <returns>Resolves with the resulting value of the expression.</returns>
    public async Task<dynamic?> Eval(Node? ast)
    {
        if (ast == null) return await Task.FromResult<dynamic?>(null);
        EvaluatorHandlers.Handlers.TryGetValue(ast.Type, out var handleFunc);
        if (handleFunc == null) return await Task.FromResult<dynamic?>(null);
        return await handleFunc.Invoke(this, ast);
    }

    ///<summary>
    ///Simultaneously evaluates each expression within an array, and delivers the
    ///response as an array with the resulting values at the same indexes as their
    ///originating expressions.
    ///</summary>
    ///<param name="arr">An array of expression strings to be evaluated</param>
    ///<returns>resolves with the result array</returns>
    public async Task<List<dynamic?>> EvalArray(List<Node> arr)
    {
        var result = new List<dynamic?>();
        foreach (var val in arr)
        {
            var elemResult = await Eval(val);
            if (elemResult is Task) await elemResult;
            result.Add(elemResult);
        }
        return result;
        // return await Task.WhenAll(arr.Select(async (item) => await Eval(item)));
    }

    ///<summary>
    ///Simultaneously evaluates each expression within a map, and delivers the
    ///response as a map with the same keys, but with the evaluated result for each
    ///as their value.
    ///</summary>
    ///<param name="map">A map of expression names to expression trees to be evaluated</param>
    ///<returns>resolves with the result map.</returns>
    public async Task<Dictionary<string, dynamic?>> EvalMap(Dictionary<string, Node> map)
    {
        var result = new Dictionary<string, dynamic?>();
        foreach (var kv in map)
        {
            result[kv.Key] = await Eval(kv.Value);
        }
        return result;
    }

    ///<summary>
    ///Applies a filter expression with static identifier elements to a subject.
    ///The intent is for the subject to be an array of subjects that will be
    ///individually used as the static context against the provided expression
    ///tree. Only the elements whose expressions result in a truthy value will be
    ///included in the resulting array.
    ///
    ///If the subject is not an array of values, it will be converted to a single-
    ///element array before running the filter.
    ///</summary>
    ///<param name="subject">The value to be filtered usually an array. If this value is
    ///not an array, it will be converted to an array with this value as the
    ///only element.</param>
    ///<param name="expr">The expression tree to run against each subject. If the
    ///tree evaluates to a truthy result, then the value will be included in
    ///the returned array otherwise, it will be eliminated.</param>
    ///<returns>resolves with an array of values that passed the
    ///expression filter.</returns>
    public async Task<List<dynamic?>> FilterRelative(dynamic subj, Node expr)
    {
        if ((subj as System.Collections.IEnumerable) == null)
        {
            subj = subj == null ? [] : new List<dynamic>() { subj };
        }
        List<dynamic?> results = [];
        foreach (var elem in subj)
        {
            Evaluator elementEvaluator = new(Grammar, Context, elem);
            bool shouldInclude = await elementEvaluator.Eval(expr);
            if (shouldInclude)
            {
                results.Add(elem);
            }
        }
        return results;
    }

    ///<summary>
    ///Applies a static filter expression to a subject value.  If the filter
    ///expression evaluates to boolean true, the subject is returned if false,
    ///undefined.
    /// 
    ///For any other resulting value of the expression, this function will attempt
    ///to respond with the property at that name or index of the subject.
    ///</summary>
    ///<param name="subject">The value to be filtered.  Usually an Array (for which
    ///the expression would generally resolve to a numeric index) or an
    ///Object (for which the expression would generally resolve to a string
    ///indicating a property name)</param>
    ///<param name="expr">The expression tree to run against the subject</param>
    ///<returns>resolves with the value of the drill-down.</returns>
    public async Task<dynamic?> FilterStatic(dynamic subject, Node expr)
    {
        var result = await Eval(expr);
        if (result is bool)
        {
            return result ? subject : null;
        }
        else if (result is decimal)
        {
            return subject[decimal.ToInt32(result)];
        }
        else if (result is string)
        {
            return subject[result];
        }
        else
        {
            return null;
        }
    }
}