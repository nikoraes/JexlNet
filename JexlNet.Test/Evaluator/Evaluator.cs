
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
    [InlineData(@"'Hello' + (4+4) + 'Wo\'rld'", @"Hello8Wo'rld")]
    [InlineData(@"""Hello"" + (4+4) + 'Wo\'rld'", @"Hello8Wo'rld")]
    public void EvaluateExpression_ReturnString(string input, string expected)
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(input);
        var result = _evaluator.Eval(ast);
        Assert.Equal(expected, result.Result);
    }

    [Theory]
    [InlineData(@"2 > 1", true)]
    [InlineData(@"2 <= 1", false)]
    [InlineData(@"""foo"" && 6 >= 6 && 0 + 1 && true", true)]
    public void EvaluateExpression_ReturnBoolean(string input, bool expected)
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(input);
        var result = _evaluator.Eval(ast);
        Assert.Equal(expected, result.Result);
    }

    [Fact]
    public void EvaluateExpression_WithContext()
    {
        var context = new Dictionary<string, dynamic>
        {
            {
                "foo", new Dictionary<string, dynamic>
                {
                    {
                        "baz", new Dictionary<string, dynamic>
                        {
                            { "bar", "tek" }
                        }
                    }
                }
            }
        };
        Evaluator _evaluator = new(new Grammar(), context);
        var ast = ToTree("foo.baz.bar");
        var result = _evaluator.Eval(ast);
        Assert.Equal("tek", result.Result);
    }

    [Fact]
    public void EvaluateExpression_AppliesTransforms()
    {
        var context = new Dictionary<string, dynamic> { { "foo", 10 } };
        var grammar = new Grammar();
        grammar.AddTransform("half", (dynamic? val) => val / 2);
        /* static object half(dynamic val) => val / 2;
        grammar.AddTransform("half", half); */
        Evaluator _evaluator = new(grammar, context);
        var ast = ToTree("foo|half + 3");
        var result = _evaluator.Eval(ast);
        Assert.Equal((decimal)8, result.Result);
    }

    [Fact]
    public void EvaluateExpression_FiltersArrays()
    {
        var context = new Dictionary<string, dynamic>
        {
            {
                "foo", new Dictionary<string, dynamic>
                {
                    {
                        "bar", new List<Dictionary<string, dynamic>>
                        {
                            new() { { "tek", "hello" } },
                            new() { { "tek", "baz" } },
                            new() { { "tok", "baz" } }
                        }
                    }
                }
            }
        };
        Evaluator _evaluator = new(new Grammar(), context);
        var ast = ToTree("foo.bar[.tek == \"baz\"]");
        var result = _evaluator.Eval(ast);
        Assert.Equal(new List<Dictionary<string, dynamic>> { new() { { "tek", "baz" } } }, result.Result);
    }

}