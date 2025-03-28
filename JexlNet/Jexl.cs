using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace JexlNet
{
    public interface IJexl
    {
        Expression CreateExpression(string exprStr);
        Task<JsonNode> EvalAsync(string expression, JsonObject context = null, CancellationToken cancellationToken = default);
        JsonNode Eval(string expression, JsonObject context = null, CancellationToken cancellationToken = default);
    }

    public class Jexl : IJexl
    {
        public Jexl()
        {
            Grammar = new Grammar();
        }
        public Jexl(Grammar grammar)
        {
            Grammar = grammar;
        }
        public Grammar Grammar { get; set; }

        ///<summary>
        ///Constructs an Expression object from a Jexl expression string.
        ///</summary>
        ///<param name="expression">The Jexl expression to be wrapped in an
        ///Expression object</param>
        ///<returns>The Expression object representing the given string</returns>
        public Expression CreateExpression(string exprStr)
        {
            return new Expression(Grammar, exprStr);
        }

        ///<summary>
        ///Constructs an Expression object from a precompiled AST.
        ///</summary>
        ///<param name="ast">The Jexl AST to be wrapped in an
        ///Expression object</param>
        ///<returns>The Expression object representing the given string</returns>
        public Expression CreateExpression(Node ast)
        {
            return new Expression(Grammar, ast);
        }

        ///<summary>
        ///Asynchronously evaluates a Jexl string within an optional context.
        ///</summary>
        ///<param name="expression">The Jexl expression to be evaluated</param>
        ///<param name="context">A mapping of variables to values, which will be
        ///made accessible to the Jexl expression when evaluating it</param>
        ///<returns>The result of the evaluation.</returns>
        public async Task<JsonNode> EvalAsync(string expression, JsonObject context = null, CancellationToken cancellationToken = default)
        {
            var expressionObj = new Expression(Grammar, expression);
            return await expressionObj.EvalAsync(context, cancellationToken);
        }

        public async Task<JsonNode> EvalAsync(string expression, string context, CancellationToken cancellationToken = default) =>
            await EvalAsync(expression, (JsonObject)JsonNode.Parse(context), cancellationToken);

        ///<summary>
        ///Synchronously evaluates a Jexl string within an optional context.
        ///</summary>
        ///<param name="expression">The Jexl expression to be evaluated</param>
        ///<param name="context">A mapping of variables to values, which will be
        ///made accessible to the Jexl expression when evaluating it</param>
        ///<returns>The result of the evaluation.</returns>
        public JsonNode Eval(string expression, JsonObject context = null, CancellationToken cancellationToken = default)
        {
            var expressionObj = new Expression(Grammar, expression);
            return expressionObj.Eval(context, cancellationToken);
        }

        public JsonNode Eval(string expression, string context, CancellationToken cancellationToken = default) =>
            Eval(expression, (JsonObject)JsonNode.Parse(context), cancellationToken);
    }
}