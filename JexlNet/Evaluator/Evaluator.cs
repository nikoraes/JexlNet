using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace JexlNet
{
    public class Evaluator
    {
        public Evaluator(Grammar grammar, JsonObject context = null, JsonObject subject = null)
        {
            Grammar = grammar;
            Context = context;
            RelContext = subject;
        }
        internal readonly Grammar Grammar;
        internal readonly JsonObject Context;
        internal readonly JsonObject RelContext;

        /// <summary>
        /// Evaluates an expression tree within the configured context.
        /// </summary>
        /// <param name="ast">An expression tree object</param>
        /// <returns>Resolves with the resulting value of the expression.</returns>
        public async Task<JsonNode> EvalAsync(Node ast, CancellationToken cancellationToken = default)
        {
            if (ast == null) return await Task.FromResult<JsonNode>(null);
            EvaluatorHandlers.Handlers.TryGetValue(ast.Type, out var handleFunc);
            if (handleFunc == null) return await Task.FromResult<JsonNode>(null);
            return await handleFunc.Invoke(this, ast, cancellationToken);
        }

        ///<summary>
        ///Simultaneously evaluates each expression within an array, and delivers the
        ///response as an array with the resulting values at the same indexes as their
        ///originating expressions.
        ///</summary>
        ///<param name="arr">An array of expression strings to be evaluated</param>
        ///<returns>resolves with the result array</returns>
        internal async Task<JsonArray> EvalArrayAsync(List<Node> arr, CancellationToken cancellationToken = default)
        {
            JsonArray result = new JsonArray();
            foreach (var val in arr)
            {
                JsonNode elemResult = await EvalAsync(val, cancellationToken);
                // Not possible for JsonNode to be a Task
                // if (elemResult is Task) await elemResult;
                result.Add(elemResult?.DeepClone());
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
        internal async Task<JsonObject> EvalMapAsync(Dictionary<string, Node> map, CancellationToken cancellationToken = default)
        {
            JsonObject result = new JsonObject();
            foreach (var kv in map)
            {
                result[kv.Key] = await EvalAsync(kv.Value, cancellationToken);
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
        public async Task<JsonArray> FilterRelativeAsync(JsonNode subj, Node expr, CancellationToken cancellationToken = default)
        {
            if (subj != null && !(subj is JsonArray))
            {
                subj = new JsonArray() { subj };
            }
            JsonArray results = new JsonArray();
            if (subj is JsonArray arr)
            {
                foreach (JsonObject elem in arr.Cast<JsonObject>())
                {
                    Evaluator elementEvaluator = new Evaluator(Grammar, Context, elem);
                    JsonNode shouldInclude = await elementEvaluator.EvalAsync(expr, cancellationToken);
                    if (shouldInclude?.GetValueKind() == JsonValueKind.True)
                    {
                        results.Add(elem?.DeepClone());
                    }
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
        public async Task<JsonNode> FilterStaticAsync(JsonNode subj, Node expr, CancellationToken cancellationToken = default)
        {
            JsonNode result = await EvalAsync(expr, cancellationToken);
            if (result == null || subj == null)
            {
                return new JsonArray();
            }
            else if (result.GetValueKind() == JsonValueKind.True)
            {
                return subj;
            }
            else if (result.GetValueKind() == JsonValueKind.Number && result is JsonValue resultValue)
            {
                return subj[resultValue.ToInt32()];
            }
            else if (result.GetValueKind() == JsonValueKind.String)
            {
                return subj[result.GetValue<string>()];
            }
            else
            {
                return null;
            }
            // TODO: if the result is an array or object, maybe we can support some mapping like jsonata
        }
    }
}