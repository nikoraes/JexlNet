namespace JexlNet;

public interface IExpression
{
    public Expression? Compile();
    public Task<dynamic?> EvalAsync(Dictionary<string, dynamic>? context = null);
    public dynamic? Eval(Dictionary<string, dynamic>? context = null);
}

public class Expression : IExpression
{
    public Expression(string exprStr)
    {
        Grammar = new Grammar();
        ExprStr = exprStr;
    }
    public Expression(Grammar grammar, string exprStr)
    {
        Grammar = grammar;
        ExprStr = exprStr;
    }
    private readonly Grammar Grammar;
    private readonly string ExprStr;
    private Node? _ast = null;

    /// <summary>
    /// Forces a compilation of the expression string that this Expression object
    /// was constructed with.This function can be called multiple times; useful
    /// if the language elements of the associated Jexl instance change.
    /// </summary>
    /// <returns>this Expression instance, for convenience</returns>
    public virtual Expression? Compile()
    {
        var lexer = new Lexer(Grammar);
        var parser = new Parser(Grammar);
        var tokens = lexer.Tokenize(ExprStr);
        parser.AddTokens(tokens);
        _ast = parser.Complete();
        return this;
    }

    /// <summary>
    /// Asynchronously evaluates the expression within an optional context.
    /// </summary>
    /// <param name="context">A mapping of variables to values, which will be
    /// made accessible to the Jexl expression when evaluating it.</param>
    /// <returns>Resolves with the resulting value of the expression.</returns>
    public async Task<dynamic?> EvalAsync(Dictionary<string, dynamic>? context = null)
    {
        var evaluator = new Evaluator(Grammar, context);
        return await evaluator.EvalAsync(GetAst());
    }


    /// <summary>
    /// Asynchronously evaluates the expression within an optional context.
    /// </summary>
    /// <param name="context">A mapping of variables to values, which will be
    /// made accessible to the Jexl expression when evaluating it.</param>
    /// <returns>Resolves with the resulting value of the expression.</returns>
    public dynamic? Eval(Dictionary<string, dynamic>? context = null)
    {
        var evaluator = new Evaluator(Grammar, context);
        var task = evaluator.EvalAsync(GetAst());
        return task.GetAwaiter().GetResult();
    }

    private Node? GetAst()
    {
        if (_ast == null) Compile();
        return _ast;
    }
}