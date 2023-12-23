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
    [InlineData("7 // 2", 3)]
    [InlineData("(\t2\n+\n3) *\n4\n\r\n", 20)]
    public async void EvaluateExpression_ReturnDecimal(string input, decimal expected)
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(input);
        var result = await _evaluator.EvalAsync(ast);
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
        var result = await _evaluator.EvalAsync(ast);
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
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal(expected, result);
    }

    [Fact]
    public async void EvaluateExpression_WithContext()
    {
        var context = new Dictionary<string, dynamic?>
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
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal("tek", result);
    }

    [Fact]
    public async void EvaluateExpression_AppliesTransforms()
    {
        var context = new Dictionary<string, dynamic?> { { "foo", 10 } };
        var grammar = new Grammar();
        grammar.AddTransform("half", (dynamic? val) => val / 2);
        Evaluator _evaluator = new(grammar, context);
        var ast = ToTree("foo|half + 3");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal((decimal)8, result);
    }

    [Fact]
    public async void EvaluateExpression_AppliesFunctions()
    {
        var context = new Dictionary<string, dynamic?> { { "foo", 10 } };
        var grammar = new Grammar();
        grammar.AddFunction("half", (dynamic? val) => val / 2);
        Evaluator _evaluator = new(grammar, context);
        var ast = ToTree("half(foo) + 3");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal((decimal)8, result);
    }

    [Fact]
    public async void EvaluateExpression_AppliesAsyncFunctions()
    {
        var context = new Dictionary<string, dynamic?> { { "foo", 10 } };
        var grammar = new Grammar();
        grammar.AddFunction("half", async (dynamic? val) =>
        {
            await Task.Delay(100);
            return val / 2;
        });
        Evaluator _evaluator = new(grammar, context);
        var ast = ToTree("half(foo) + 3");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal((decimal)8, result);
    }

    [Fact]
    public async void EvaluateExpression_FiltersArrays()
    {
        var context = new Dictionary<string, dynamic?>
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
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal(new List<Dictionary<string, dynamic>> { new() { { "tek", "baz" } } }, result);

        _evaluator = new(new Grammar(), context);
        ast = ToTree("foo.bar[.tek == \"baz\"][0]");
        result = await _evaluator.EvalAsync(ast);
        Assert.Equal(new Dictionary<string, dynamic> { { "tek", "baz" } }, result);

        _evaluator = new(new Grammar(), context);
        ast = ToTree("foo.bar[1]");
        result = await _evaluator.EvalAsync(ast);
        Assert.Equal(new Dictionary<string, dynamic> { { "tek", "baz" } }, result);

        _evaluator = new(new Grammar(), context);
        ast = ToTree("foo.bar[.tek == \"baz\"][0].tek");
        result = await _evaluator.EvalAsync(ast);
        Assert.Equal("baz", result);

        _evaluator = new(new Grammar(), context);
        ast = ToTree("foo.bar[.tek == \"baz\"].tek");
        result = await _evaluator.EvalAsync(ast);
        Assert.Equal("baz", result);

        _evaluator = new(new Grammar(), context);
        ast = ToTree("foo.bar[1].tek");
        result = await _evaluator.EvalAsync(ast);
        Assert.Equal("baz", result);
    }

    [Fact]
    public async void EvaluateExpression_AllowFiltersToSelectObjectProperties()
    {
        var context = new Dictionary<string, dynamic?>
        { { "foo", new Dictionary<string, dynamic>
            { { "baz", new Dictionary<string, dynamic>
                        { { "bar", "tek" } }
            } }
        }};
        Evaluator _evaluator = new(new Grammar(), context);
        var ast = ToTree(@"foo[""ba"" + ""z""].bar");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal("tek", result);
    }

    [Fact]
    public async void EvaluateExpression_ThrowsWhenTransformDoesntExist()
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(@"""hello""|world");
        await Assert.ThrowsAsync<Exception>(async () => await _evaluator.EvalAsync(ast));
    }

    [Fact]
    public async void EvaluateExpression_ObjectLiteral()
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(@"{foo: {bar: ""tek""}}");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal(new Dictionary<string, dynamic>
        { { "foo", new Dictionary<string, dynamic>
            { { "bar", "tek" } }
        }}, result);
    }

    [Fact]
    public async void EvaluateExpression_EmptyObjectLiteral()
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(@"{}");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal(new Dictionary<string, dynamic>(), result);
    }

    [Fact]
    public async void EvaluateExpression_DotNotationForObjectLiterals()
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(@"{foo: ""bar""}.foo");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal("bar", result);
    }

    [Fact]
    public async void EvaluateExpression_AllowAccessToLiteralProperties()
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(@"""foo"".Length");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal("foo".Length, result);
    }

    [Fact]
    public async void EvaluateExpression_AllowAccessToEmptyLiteralProperties()
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(@""""".Length");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal("".Length, result);
    }

    [Fact]
    public async void EvaluateExpression_AppliesTransformsWithMultipleArgs()
    {
        var grammar = new Grammar();
        grammar.AddTransform("concat", (dynamic?[] args) => args[0] + ": " + args[1] + args[2] + args[3]);
        Evaluator _evaluator = new(grammar);
        var ast = ToTree(@"""foo""|concat(""baz"", ""bar"", ""tek"")");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal("foo: bazbartek", result);
    }

    [Fact]
    public async void EvaluateExpression_AllowAddMultipleTransforms()
    {
        var grammar = new Grammar();
        grammar.AddTransforms(new Dictionary<string, Func<List<dynamic?>, object?>>
        {
            { "concat", (List<dynamic?> args) => args[0] + ": " + args[1] + args[2] + args[3] },
            { "concat2", (List<dynamic?> args) => args[0] + ": " + args[1] + args[2] + args[3] }
        });
        Evaluator _evaluator = new(grammar);
        var ast = ToTree(@"""foo""|concat(""baz"", ""bar"", ""tek"")|concat2(""baz"", ""bar"", ""tek"")");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal("foo: bazbartek: bazbartek", result);
    }

    [Fact]
    public async void EvaluateExpression_ArrayLiteral()
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(@"[""foo"", 1+2]");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal(new List<dynamic> { "foo", (decimal)3 }, result);
    }

    [Theory]
    [InlineData(@"""bar"" in ""foobartek""", true)]
    [InlineData(@"""baz"" in ""foobartek""", false)]
    [InlineData(@"""bar"" in [""foo"",""bar"",""tek""]", true)]
    [InlineData(@"""baz"" in [""foo"",""bar"",""tek""]", false)]
    public async void EvaluateExpression_AppliesInOperator(string input, bool expected)
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(input);
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(@"""foo"" ? 1 : 2", 1)]
    [InlineData(@""""" ? 1 : 2", 2)]
    public async void EvaluateExpression_Conditional(string input, decimal expected)
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(input);
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(@"""foo"" ?: ""bar""", "foo")]
    [InlineData(@""""" ?: ""bar""", "bar")]
    public async void EvaluateExpression_AllowsMissingConsequentInTernary(string input, string expected)
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(input);
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal(expected, result);
    }

    [Fact]
    public async void EvaluateExpression_ReturnsEmptyArrayWhenApplyingFilterToUndefined()
    {
        var context = new Dictionary<string, dynamic?>
        {
            { "a", new Dictionary<string, dynamic>() },
            { "b", 4 }
        };
        Evaluator _evaluator = new(new Grammar(), context);
        var ast = ToTree(@"a.b[.c == d]");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal(new List<dynamic>(), result);
        Assert.Empty(result);
    }

    [Fact]
    public async void EvaluateExpression_WithDollarIdentifiers()
    {
        var context = new Dictionary<string, dynamic?>
        {
            { "$", 5 },
            { "$foo", 6 },
            { "$foo$bar", 7 },
            { "$bar", 8 }
        };
        Evaluator _evaluator = new(new Grammar(), context);
        var ast = ToTree(@"$+$foo+$foo$bar+$bar");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal((decimal)26, result);
    }

    [Fact]
    public async void EvaluateExpression_BinaryEvaluateOnDemand()
    {
        var grammar = new Grammar();
        bool toTrueEvaluated = false;
        grammar.AddTransform("toTrue", (dynamic? val) =>
        {
            toTrueEvaluated = true;
            return true;
        });
        Evaluator _evaluator = new(grammar);
        var ast = ToTree(@"true && ""foo""|toTrue");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal(true, result);
        Assert.True(toTrueEvaluated);
    }

    [Fact]
    public async void EvaluateExpression_BinaryEvaluateOnDemand2()
    {
        var grammar = new Grammar();
        bool toTrueEvaluated = false;
        grammar.AddTransform("toTrue", (dynamic? val) =>
        {
            toTrueEvaluated = true;
            return true;
        });
        Evaluator _evaluator = new(grammar);
        var ast = ToTree("true || foo|toTrue");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal(true, result);
        Assert.False(toTrueEvaluated);
    }
}