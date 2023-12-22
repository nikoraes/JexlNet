namespace JexlNet;

public class Expression
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
    private Grammar Grammar { get; set; }
    private string ExprStr { get; set; }
    private Node? _ast = null;

    /// <summary>
    /// Forces a compilation of the expression string that this Expression object
    /// was constructed with.This function can be called multiple times; useful
    /// if the language elements of the associated Jexl instance change.
    /// </summary>
    /// <returns>this Expression instance, for convenience</returns>
    public Expression? Compile()
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
        return await evaluator.Eval(GetAst());
    }

    public Task<dynamic?> Eval(Dictionary<string, dynamic>? context = null) => EvalAsync(context);


    /// <summary>
    /// Asynchronously evaluates the expression within an optional context.
    /// </summary>
    /// <param name="context">A mapping of variables to values, which will be
    /// made accessible to the Jexl expression when evaluating it.</param>
    /// <returns>Resolves with the resulting value of the expression.</returns>
    public dynamic? EvalSync(Dictionary<string, dynamic>? context = null)
    {
        var evaluator = new Evaluator(Grammar, context);
        var task = evaluator.Eval(GetAst());
        return task.GetAwaiter().GetResult();
    }

    private Node? GetAst()
    {
        if (_ast == null) Compile();
        return _ast;
    }
}