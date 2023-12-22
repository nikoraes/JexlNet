
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
    public async void EvaluateExpression_ReturnDecimal(string input, decimal expected)
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(input);
        var result = await _evaluator.Eval(ast);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(@"""Hello"" + (4+4) + ""Wo\""rld""", @"Hello8Wo""rld")]
    [InlineData(@"'Hello' + (4+4) + 'Wo\'rld'", @"Hello8Wo'rld")]
    [InlineData(@"""Hello"" + (4+4) + 'Wo\'rld'", @"Hello8Wo'rld")]
    public async void EvaluateExpression_ReturnString(string input, string expected)
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(input);
        var result = await _evaluator.Eval(ast);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(@"2 > 1", true)]
    [InlineData(@"2 <= 1", false)]
    [InlineData(@"""foo"" && 6 >= 6 && 0 + 1 && true", true)]
    public async void EvaluateExpression_ReturnBoolean(string input, bool expected)
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(input);
        var result = await _evaluator.Eval(ast);
        Assert.Equal(expected, result);
    }

    [Fact]
    public async void EvaluateExpression_WithContext()
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
        var result = await _evaluator.Eval(ast);
        Assert.Equal("tek", result);
    }

    [Fact]
    public async void EvaluateExpression_AppliesTransforms()
    {
        var context = new Dictionary<string, dynamic> { { "foo", 10 } };
        var grammar = new Grammar();
        grammar.AddTransform("half", (dynamic? val) => val / 2);
        /* static object half(dynamic val) => val / 2;
        grammar.AddTransform("half", half); */
        Evaluator _evaluator = new(grammar, context);
        var ast = ToTree("foo|half + 3");
        var result = await _evaluator.Eval(ast);
        Assert.Equal((decimal)8, result);
    }

    [Fact]
    public async void EvaluateExpression_AppliesFunctions()
    {
        var context = new Dictionary<string, dynamic> { { "foo", 10 } };
        var grammar = new Grammar();
        grammar.AddFunction("half", (dynamic? val) => val / 2);
        /* static object half(dynamic val) => val / 2;
        grammar.AddTransform("half", half); */
        Evaluator _evaluator = new(grammar, context);
        var ast = ToTree("half(foo) + 3");
        var result = await _evaluator.Eval(ast);
        Assert.Equal((decimal)8, result);
    }

    [Fact]
    public async void EvaluateExpression_FiltersArrays()
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
        var result = await _evaluator.Eval(ast);
        Assert.Equal(new List<Dictionary<string, dynamic>> { new() { { "tek", "baz" } } }, result);

        _evaluator = new(new Grammar(), context);
        ast = ToTree("foo.bar[.tek == \"baz\"][0]");
        result = await _evaluator.Eval(ast);
        Assert.Equal(new Dictionary<string, dynamic> { { "tek", "baz" } }, result);

        _evaluator = new(new Grammar(), context);
        ast = ToTree("foo.bar[1]");
        result = await _evaluator.Eval(ast);
        Assert.Equal(new Dictionary<string, dynamic> { { "tek", "baz" } }, result);

        _evaluator = new(new Grammar(), context);
        ast = ToTree("foo.bar[.tek == \"baz\"][0].tek");
        result = await _evaluator.Eval(ast);
        Assert.Equal("baz", result);

        _evaluator = new(new Grammar(), context);
        ast = ToTree("foo.bar[.tek == \"baz\"].tek");
        result = await _evaluator.Eval(ast);
        Assert.Equal("baz", result);

        _evaluator = new(new Grammar(), context);
        ast = ToTree("foo.bar[1].tek");
        result = await _evaluator.Eval(ast);
        Assert.Equal("baz", result);
    }
}