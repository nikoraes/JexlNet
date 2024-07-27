using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace JexlNet
{
    public interface IJexl
    {
        Expression CreateExpression(string exprStr);
        Task<JsonNode> EvalAsync(string expression, JsonObject context = null);
        JsonNode Eval(string expression, JsonObject context = null);
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
        ///Asynchronously evaluates a Jexl string within an optional context.
        ///</summary>
        ///<param name="expression">The Jexl expression to be evaluated</param>
        ///<param name="context">A mapping of variables to values, which will be
        ///made accessible to the Jexl expression when evaluating it</param>
        ///<returns>The result of the evaluation.</returns>
        public async Task<JsonNode> EvalAsync(string expression, JsonObject context = null)
        {
            var expressionObj = new Expression(Grammar, expression);
            return await expressionObj.EvalAsync(context);
        }

        public async Task<JsonNode> EvalAsync(string expression, string context) =>
            await EvalAsync(expression, (JsonObject)JsonNode.Parse(context));

        ///<summary>
        ///Synchronously evaluates a Jexl string within an optional context.
        ///</summary>
        ///<param name="expression">The Jexl expression to be evaluated</param>
        ///<param name="context">A mapping of variables to values, which will be
        ///made accessible to the Jexl expression when evaluating it</param>
        ///<returns>The result of the evaluation.</returns>
        public JsonNode Eval(string expression, JsonObject context = null)
        {
            var expressionObj = new Expression(Grammar, expression);
            return expressionObj.Eval(context);
        }

        public JsonNode Eval(string expression, string context) =>
            Eval(expression, (JsonObject)JsonNode.Parse(context));
    }
}