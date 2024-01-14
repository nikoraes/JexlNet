using System.Text.Json;
using System.Text.Json.Nodes;

namespace JexlNet;

public interface IJexl
{
    public Expression CreateExpression(string exprStr);
    public Task<JsonNode> EvalAsync(string expression, JsonObject? context = null);
    public JsonNode Eval(string expression, JsonObject? context = null);
}

public class Jexl : IJexl
{
    public Jexl()
    {
        Grammar = new Grammar();
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
    public async Task<JsonNode> EvalAsync(string expression, JsonObject? context = null)
    {
        var expressionObj = new Expression(Grammar, expression);
        return await expressionObj.EvalAsync(context);
    }

    /* public async Task<JsonElement> EvalAsync(string expression, JsonDocument context)
    {
        if (context.RootElement.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("Context must be an object", nameof(context));
        var expressionObj = new Expression(Grammar, expression);
        var result = await expressionObj.EvalAsync(JsonObject.Create(context.RootElement));
        return JsonElement.ParseValue(result);
    } */

    ///<summary>
    ///Synchronously evaluates a Jexl string within an optional context.
    ///</summary>
    ///<param name="expression">The Jexl expression to be evaluated</param>
    ///<param name="context">A mapping of variables to values, which will be
    ///made accessible to the Jexl expression when evaluating it</param>
    ///<returns>The result of the evaluation.</returns>
    public JsonNode Eval(string expression, JsonObject? context = null)
    {
        var expressionObj = new Expression(Grammar, expression);
        return expressionObj.Eval(context);
    }
}