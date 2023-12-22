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
    public Task<dynamic?> Eval(Node? ast)
    {
        if (ast == null) return Task.FromResult<dynamic?>(null);
        EvaluatorHandlers.Handlers.TryGetValue(ast.Type, out var handleFunc);
        if (handleFunc == null) return Task.FromResult<dynamic?>(null);
        return handleFunc.Invoke(this, ast);
    }

    ///<summary>
    ///Simultaneously evaluates each expression within an array, and delivers the
    ///response as an array with the resulting values at the same indexes as their
    ///originating expressions.
    ///</summary>
    ///<param name="arr">An array of expression strings to be evaluated</param>
    ///<returns>resolves with the result array</returns>
    public Task<dynamic?[]> EvalArray(List<Node> arr)
    {
        return Task.WhenAll(arr.Select(async (item) => await Eval(item).ConfigureAwait(true)));
    }

    ///<summary>
    ///Simultaneously evaluates each expression within a map, and delivers the
    ///response as a map with the same keys, but with the evaluated result for each
    ///as their value.
    ///</summary>
    ///<param name="map">A map of expression names to expression trees to be evaluated</param>
    ///<returns>resolves with the result map.</returns>
    public Task<Dictionary<string, dynamic?>> EvalMap(Dictionary<string, Node> map)
    {
        var keys = map.Keys;
        var result = new Dictionary<string, dynamic?>();
        var asts = keys.Select((key) => Eval(map[key]));
        return Task.WhenAll(asts).ContinueWith((vals) =>
        {
            var idx = 0;
            foreach (var val in vals.Result)
            {
                result[keys.ElementAt(idx)] = val;
                idx++;
            }
            return result;
        });
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
    public Task<List<dynamic?>> FilterRelative(dynamic subject, Node expr)
    {
        var promises = new List<Task<dynamic?>>();
        if (subject is not List<dynamic?>)
        {
            subject = subject == null ? [] : new List<dynamic>() { subject };
        }
        foreach (var elem in subject)
        {
            var evalInst = new Evaluator(Grammar, Context, elem);
            promises.Add(evalInst.Eval(expr));
        }
        return Task.WhenAll(promises).ContinueWith((values) =>
        {
            var results = new List<dynamic?>();
            var idx = 0;
            foreach (var value in values.Result)
            {
                if (value)
                {
                    results.Add(subject[idx]);
                }
                idx++;
            }
            return results;
        });
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
    public Task<dynamic?> FilterStatic(dynamic subject, Node expr)
    {
        return Eval(expr).ContinueWith((res) =>
        {
            return res.Result is bool ? res.Result ? subject : null : subject[res.Result];
        });
    }
}