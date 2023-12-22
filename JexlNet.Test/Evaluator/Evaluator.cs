namespace JexlNet.Test;


public class EvaluatorUnitTest
{
    private readonly Lexer _lexer = new(new Grammar());
    private readonly Evaluator _evaluator = new(new Grammar());

    private Node? ToTree(string input)
    {
        Parser _parser = new(new Grammar());
        var tokens = _lexer.Tokenize(input);
        _parser.AddTokens(tokens);
        return _parser.Complete();
    }

    [Theory]
    [InlineData("1 + 2", 3)]
    [InlineData("(2 + 3) * 4", 20)]
    public void EvaluateExpression_ReturnDecimal(string input, decimal expected)
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(input);
        var result = _evaluator.Eval(ast);
        Assert.Equal(expected, result.Result);
    }

    [Theory]
    [InlineData(@"""Hello"" + (4+4) + ""Wo\""rld""", @"Hello8Wo""rld")]
    public void EvaluateExpression_ReturnString(string input, string expected)
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(input);
        var result = _evaluator.Eval(ast);
        Assert.Equal(expected, result.Result);
    }
}