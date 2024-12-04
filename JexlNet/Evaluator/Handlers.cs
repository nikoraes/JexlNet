using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace JexlNet
{
    public class OnDemandBinaryFunctionWrapper
    {
        public OnDemandBinaryFunctionWrapper(Evaluator evaluator, Node subAst, CancellationToken cancellationToken = default)
        {
            Evaluator = evaluator;
            SubAst = subAst;
            EvalAsync = async () => await Evaluator.EvalAsync(subAst, cancellationToken);
        }
        public Evaluator Evaluator { get; }
        public Node SubAst { get; }
        public Func<Task<JsonNode>> EvalAsync { get; }
    }

    public static class EvaluatorHandlers
    {
        /// <summary>
        /// Evaluates an ArrayLiteral by returning its value, with each element
        /// independently run through the evaluator.
        /// </summary>
        /// <param name="evaluator"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public static async Task<JsonNode> ArrayLiteralAsync(Evaluator evaluator, Node node, CancellationToken cancellationToken = default)
        {
            if (node?.Array == null) throw new Exception("EvaluatorHandlers.ArrayLiteralAsync: node has no array");
            return await evaluator.EvalArrayAsync(node.Array, cancellationToken);
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
        public static async Task<JsonNode> BinaryExpressionAsync(Evaluator evaluator, Node node, CancellationToken cancellationToken = default)
        {
            if (node?.Operator == null)
            {
                throw new Exception("EvaluatorHandlers.BinaryExpression: node has no operator");
            }
            var grammarOp = evaluator.Grammar.Elements[node.Operator];
            if (grammarOp == null)
            {
                throw new Exception($"EvaluatorHandlers.BinaryExpression: node has unknown operator: {node.Operator}");
            }
            // EvaluateOnDemand allows to only conditionally evaluate one side of the binary expression
            // It is always prefered, so that we can avoid evaluating both sides if not necessary
            if (grammarOp.EvaluateOnDemandAsync != null && node.Left != null && node.Right != null)
            {
                var wrappedLeft = new OnDemandBinaryFunctionWrapper(evaluator, node.Left, cancellationToken);
                var wrappedRight = new OnDemandBinaryFunctionWrapper(evaluator, node.Right, cancellationToken);
                var wrap = new Func<Node, Func<Task<JsonNode>>>((subAst) => async () => await evaluator.EvalAsync(subAst, cancellationToken));
                return await grammarOp.EvaluateOnDemandAsync(new OnDemandBinaryFunctionWrapper[] { wrappedLeft, wrappedRight });
            }
            // We don't really need the non ondemand evaluation to be defined on the grammar, but we can use it as a fallback
            if (grammarOp.Evaluate == null)
            {
                var leftResult = await evaluator.EvalAsync(node?.Left, cancellationToken);
                var rightResult = await evaluator.EvalAsync(node?.Right, cancellationToken);
                return grammarOp.Evaluate(new JsonNode[] { leftResult?.DeepClone(), rightResult?.DeepClone() });
            }
            throw new Exception($"EvaluatorHandlers.BinaryExpression: node has no evaluation function: {node.Operator}");
        }

        ///<summary>
        ///Evaluates a ConditionalExpression node by first evaluating its test branch,
        ///and resolving with the consequent branch if the test is truthy, or the
        ///alternate branch if it is not. If there is no consequent branch, the test
        ///result will be used instead.
        ///</summary>
        ///<param name="evaluator"></param>
        ///<param name="node">An expression tree with a ConditionalExpression as the top node</param>
        public static async Task<JsonNode> ConditionalExpression(Evaluator evaluator, Node node, CancellationToken cancellationToken = default)
        {
            if (node?.Test == null)
            {
                throw new Exception("EvaluatorHandlers.ConditionalExpression: node has no test");
            }
            JsonNode testResult = await evaluator.EvalAsync(node?.Test, cancellationToken);
            // If it's a string, we consider it truthy if it's non-empty
            // If it's a decimal, we consider it truthy it's non-zero
            // Align with behaviour in Javascript
            if (testResult != null &&
                ((testResult.GetValueKind() == JsonValueKind.String && !string.IsNullOrEmpty(testResult.GetValue<string>())) ||
                (testResult.GetValueKind() == JsonValueKind.Number && testResult.GetValue<decimal>() != 0) ||
                (testResult.GetValueKind() == JsonValueKind.True))
            )
            {
                if (node?.Consequent != null)
                {
                    return await evaluator.EvalAsync(node?.Consequent, cancellationToken);
                }
                return testResult;
            }
            return await evaluator.EvalAsync(node?.Alternate, cancellationToken);
        }

        ///<summary>
        ///Evaluates a FilterExpression by applying it to the subject value.
        ///</summary>
        ///<param name="evaluator"></param>
        ///<param name="node">An expression tree with a FilterExpression as the top node</param>
        ///<returns>resolves with the value of the FilterExpression.</returns>
        public static async Task<JsonNode> FilterExpression(Evaluator evaluator, Node node, CancellationToken cancellationToken = default)
        {
            if (node?.Subject == null)
            {
                throw new Exception("EvaluatorHandlers.FilterExpression: node has no subject");
            }
            if (node.Expr == null)
            {
                throw new Exception("EvaluatorHandlers.FilterExpression: node has no expression");
            }
            JsonNode subjectResult = await evaluator.EvalAsync(node?.Subject, cancellationToken);
            if (node?.Relative == true)
            {
                return await evaluator.FilterRelativeAsync(subjectResult, node.Expr, cancellationToken);
            }
            else
            {
                return await evaluator.FilterStaticAsync(subjectResult, node.Expr);
            }
        }

        ///<summary>
        ///Evaluates an Identifier by either stemming from the evaluated 'from'
        ///expression tree or accessing the context provided when this Evaluator was
        ///constructed.
        ///</summary>
        ///<param name="evaluator"></param>
        ///<param name="node">An expression tree with an Identifier as the top node</param>
        ///<returns>either the identifier's value, or a Promise that will resolve with the identifier's value.</returns>
        public static async Task<JsonNode> IdentifierAsync(Evaluator evaluator, Node node, CancellationToken cancellationToken = default)
        {
            string nodeValue;
            if (node.Value != null && node.From == null)
            {
                nodeValue = node.Value.GetValue<string>();
                if (node?.Relative == true && evaluator.RelContext != null && evaluator.RelContext.ContainsKey(nodeValue))
                {
                    return evaluator.RelContext[nodeValue];
                }
                else if (evaluator.Context != null && evaluator.Context.ContainsKey(nodeValue))
                {
                    return evaluator.Context?[nodeValue];
                }
                // Return the whole context if the identifier is "$"
                // This functionality doesn't exist in basic javascript JEXL, but allows to dynamically access the root context
                else if (string.Equals(nodeValue, "$") && evaluator.Context != null)
                {
                    return evaluator.Context;
                }
                else return null;
            }
            JsonNode fromResult = await evaluator.EvalAsync(node.From, cancellationToken);
            if (fromResult == null || node.Value == null)
            {
                return null;
            }
            nodeValue = node.Value.GetValue<string>();
            if (fromResult is JsonArray list)
            {
                if (list.Count > 0 && list.First() is JsonObject dict && !string.IsNullOrEmpty(nodeValue))
                {
                    // NOTE: we could also return a mapped list, like in JSONata, but then it won't align to the JS JEXL implementation
                    /* JsonArray result = new();
                    foreach (var item in list)
                    {
                        if (item is JsonObject obj && obj.ContainsKey(nodeValue))
                        {
                            result.Add(obj[nodeValue]);
                        }
                    }
                    return result; */

                    return dict[nodeValue];
                }
                else if (int.TryParse(nodeValue, out int index))
                {
                    return list[index];
                }
                else return null;
            }
            else if (fromResult is JsonObject dict && !string.IsNullOrEmpty(nodeValue) && dict.ContainsKey(nodeValue))
            {
                return dict[nodeValue];
            }
            /* else if ((fromResultType.IsGenericType || fromResultType != null) && node?.Value != null && node!.Value is string)
            {
                // Try to access builtin properties
                PropertyInfo? propertyInfo = fromResultType?.GetProperty($"{node!.Value}");
                return propertyInfo?.GetValue(fromResult);
            } */
            return null;
        }

        ///<summary>
        ///Evaluates a Literal by returning its value property.
        ///</summary>
        ///<param name="evaluator"></param>
        ///<param name="node">An expression tree with a Literal as its only node</param>
        ///<returns>The value of the Literal node</returns>
        public static async Task<JsonNode> LiteralAsync(Evaluator evaluator, Node node, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(node.Value);
        }

        ///<summary>
        ///Evaluates an ObjectLiteral by returning its value, with each key
        ///independently run through the evaluator.
        ///</summary>
        ///<param name="evaluator"></param>
        ///<param name="node">An expression tree with a Literal as its only node</param>
        ///<returns>The value of the Literal node</returns>
        public static async Task<JsonNode> ObjectLiteralAsync(Evaluator evaluator, Node node, CancellationToken cancellationToken = default)
        {
            if (node?.Object == null) throw new Exception("EvaluatorHandlers.ObjectLiteralAsync: node has no object");
            return await evaluator.EvalMapAsync(node.Object, cancellationToken);
        }

        /// <summary>
        /// Evaluates a SequenceLiteral by returning its value, with each element
        /// independently run through the evaluator.
        /// </summary>
        /// <param name="evaluator"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public static async Task<JsonNode> SequenceLiteralAsync(Evaluator evaluator, Node node, CancellationToken cancellationToken = default)
        {
            if (node?.Args == null) throw new Exception("EvaluatorHandlers.SequenceLiteralAsync: node has no args");
            return await evaluator.EvalArrayAsync(node.Args, cancellationToken);
        }

        ///<summary>
        ///Evaluates a FunctionCall node by applying the supplied arguments to a
        ///function defined in one of the grammar's function pools.
        ///</summary>
        ///<param name="evaluator"></param>
        ///<param name="node">An expression tree with a FunctionCall as the top node</param>
        ///<returns>the value of the function call, or a Promise that will resolve with the resulting value.</returns>
        public static async Task<JsonNode> FunctionCallAsync(Evaluator evaluator, Node node, CancellationToken cancellationToken = default)
        {

            if (node?.Pool == null)
            {
                throw new Exception("EvaluatorHandlers.FunctionCallAsync: node has no pool");
            }
            if (node.Name == null)
            {
                throw new Exception("EvaluatorHandlers.FunctionCallAsync: function or transform not defined");
            }
            if (node.Args == null)
            {
                throw new Exception("EvaluatorHandlers.FunctionCallAsync: Arguments not defined");
            }
            if (node.Pool == Grammar.PoolType.Functions && evaluator.Grammar.Functions.TryGetValue(node.Name, out var func))
            {
                // TODO: Certain functions could benefit from not evaluating the arguments before passing them
                // Especially if combined with arrow notation: eg. `map([0,1,2], x => x + 1)`

                JsonArray argsResult = await evaluator.EvalArrayAsync(node.Args, cancellationToken);
                return await func(argsResult.Select((arg) => arg).ToArray(), cancellationToken);
            }
            else if (node.Pool == Grammar.PoolType.Transforms && evaluator.Grammar.Transforms.TryGetValue(node.Name, out var transform))
            {
                // TODO: Certain transforms could benefit from not evaluating the arguments before passing them
                // Especially if combined with arrow notation: eg. `[0,1,2]|map(x => x + 1)`
                JsonArray argsResult = await evaluator.EvalArrayAsync(node.Args, cancellationToken);
                // Convert to array of JsonNode
                return await transform(argsResult.Select((arg) => arg).ToArray(), cancellationToken);
            }
            else
            {
                throw new Exception($"EvaluatorHandlers.FunctionCallAsync: Function or transform not found: {Enum.GetName(typeof(Grammar.PoolType), node.Pool)}.{node.Name}");
            }
        }

        ///<summary>
        ///Evaluates a Unary expression by passing the right side through the
        ///operator's eval function.
        ///</summary>
        ///<param name="evaluator"></param>
        ///<param name="node">An expression tree with a UnaryExpression as the top node</param>
        ///<returns>resolves with the value of the UnaryExpression.</returns>
        public static async Task<JsonNode> UnaryExpressionAsync(Evaluator evaluator, Node node, CancellationToken cancellationToken = default)
        {
            if (node?.Operator == null)
            {
                throw new Exception("EvaluatorHandlers.UnaryExpressionAsync: node has no operator");
            }
            if (node?.Right == null)
            {
                throw new Exception("EvaluatorHandlers.UnaryExpressionAsync: node has no right");
            }
            var grammarOp = evaluator.Grammar.Elements[node.Operator];
            if (grammarOp == null || grammarOp.Evaluate == null)
            {
                throw new Exception($"EvaluatorHandlers.UnaryExpressionAsync: node has unknown operator: {node.Operator}");
            }
            JsonNode rightResult = await evaluator.EvalAsync(node?.Right, cancellationToken);
            return grammarOp.Evaluate(new[] { rightResult });
        }


        public static readonly Dictionary<GrammarType, Func<Evaluator, Node, CancellationToken, Task<JsonNode>>> Handlers = new Dictionary<GrammarType, Func<Evaluator, Node, CancellationToken, Task<JsonNode>>>
        {
            { GrammarType.ArrayLiteral, ArrayLiteralAsync },
            { GrammarType.BinaryExpression, BinaryExpressionAsync },
            { GrammarType.ConditionalExpression, ConditionalExpression },
            { GrammarType.FilterExpression, FilterExpression },
            { GrammarType.Identifier, IdentifierAsync },
            { GrammarType.Literal, LiteralAsync },
            { GrammarType.ObjectLiteral, ObjectLiteralAsync },
            { GrammarType.FunctionCall, FunctionCallAsync },
            { GrammarType.UnaryExpression, UnaryExpressionAsync },
            { GrammarType.SequenceLiteral, SequenceLiteralAsync }
        };
    }


}
