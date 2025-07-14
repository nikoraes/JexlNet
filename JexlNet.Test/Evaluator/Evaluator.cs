using System.Text.Json.Nodes;

namespace JexlNet.Test;

public class EvaluatorUnitTest
{
    private readonly Lexer _lexer = new(new Grammar());
    private readonly Evaluator _evaluator = new(new Grammar());

    private Node ToTree(string input)
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
    [InlineData(@"""foo"" && 6 >= 6 && 0 + 1 && true && 20", 20)]
    public async void EvaluateExpression_ReturnDecimal(string input, decimal expected)
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(input);
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal(expected, result?.GetValue<decimal>());
    }

    [Theory]
    [InlineData(@"""Hello"" + (4+4) + ""Wo\""rld""", @"Hello8Wo""rld")]
    [InlineData(@"'Hello' + (4+4) + 'Wo\'rld'", @"Hello8Wo'rld")]
    [InlineData(@"""Hello"" + (4+4) + 'Wo\'rld'", @"Hello8Wo'rld")]
    [InlineData(@"!!{foo:'a'}.bar || !!{bar:'a'}.baz || {tek:'a'}.tek", @"a")]
    public async void EvaluateExpression_ReturnString(string input, string expected)
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(input);
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal(expected, result?.GetValue<string>());
    }

    [Theory]
    [InlineData(@"2 > 1", true)]
    [InlineData(@"2 <= 1", false)]
    [InlineData(@"""foo"" && 6 >= 6 && 0 + 1 && true", true)]
    [InlineData(@"!!'foo'", true)]
    [InlineData(@"!!{foo:'a'}.bar || !!{bar:'a'}.baz || !!{tek:'a'}.tek", true)]
    [InlineData(@"!!{foo:'a'}.baz || !!{bar:'a'}.baz || !!{tek:'a'}.baz", false)]
    public async void EvaluateExpression_ReturnBoolean(string input, bool expected)
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(input);
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal(expected, result?.GetValue<bool>());
    }

    [Theory]
    [InlineData(@"{foo:{}}.foo.bar[0]")]
    [InlineData(@"{foo:{}}.foo.bar[0].bar")]
    public async void EvaluateExpression_ReturnNull(string input)
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(input);
        var result = await _evaluator.EvalAsync(ast);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("foo.baz.bar", "tek")]
    [InlineData("$['foo'].baz.bar", "tek")]
    [InlineData("$[{bar:'foo'}.bar].baz.bar", "tek")]
    [InlineData("$[{$:'foo'}.$].baz.bar", "tek")]
    [InlineData("(foo.bar ?: foo.baz).bar", "tek")]
    public async void EvaluateExpression_WithContext(string input, string expected)
    {
        JsonObject context =
            new()
            {
                {
                    "foo",
                    new JsonObject
                    {
                        {
                            "baz",
                            new JsonObject { { "bar", "tek" } }
                        }
                    }
                }
            };
        Evaluator _evaluator = new(new Grammar(), context);
        var ast = ToTree(input);
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal(expected, result?.ToString());
    }

    [Fact]
    public async void EvaluateExpression_AppliesTransforms()
    {
        JsonObject context = new() { { "foo", 10 } };
        var grammar = new Grammar();
        grammar.AddTransform(
            "half",
            (JsonValue val) => JsonValue.Create(Convert.ToDecimal(val?.ToString()) / 2)
        );
        Evaluator _evaluator = new(grammar, context);
        var ast = ToTree("foo|half + 3");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal((decimal)8, result?.GetValue<decimal>());
    }

    [Fact]
    public async void EvaluateExpression_AppliesFunctions()
    {
        JsonObject context = new() { { "foo", 10 } };
        var grammar = new Grammar();
        grammar.AddFunction("half", (JsonValue val) => val?.ToDecimal() / 2);
        Evaluator _evaluator = new(grammar, context);
        var ast = ToTree("half(foo) + 3");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal((decimal)8, result?.GetValue<decimal>());
    }

    [Fact]
    public async void EvaluateExpression_AppliesAsyncFunctions()
    {
        JsonObject context = new() { { "foo", 10 } };
        var grammar = new Grammar();
        grammar.AddFunction(
            "half",
            async (JsonValue val) =>
            {
                await Task.Delay(100);
                return val?.ToDecimal() / 2;
            }
        );
        Evaluator _evaluator = new(grammar, context);
        var ast = ToTree("half(foo) + 3");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal((decimal)8, result?.GetValue<decimal>());
    }

    [Fact]
    public async void EvaluateExpression_FiltersArrays()
    {
        JsonObject context =
            new()
            {
                {
                    "foo",
                    new JsonObject
                    {
                        {
                            "bar",
                            new JsonArray
                            {
                                new JsonObject { { "tek", "hello" } },
                                new JsonObject { { "tek", "baz" } },
                                new JsonObject { { "tok", "baz" } }
                            }
                        }
                    }
                }
            };
        Evaluator _evaluator = new(new Grammar(), context);
        var ast = ToTree("foo.bar[.tek == \"baz\"]");
        var result = await _evaluator.EvalAsync(ast);
        var expected = new JsonArray { new JsonObject { { "tek", "baz" } } };
        Assert.True(JsonNode.DeepEquals(expected, result));

        _evaluator = new(new Grammar(), context);
        ast = ToTree("foo.bar[.tek == \"baz\"][0]");
        result = await _evaluator.EvalAsync(ast);
        var expected2 = new JsonObject { { "tek", "baz" } };
        Assert.True(JsonNode.DeepEquals(expected2, result));

        _evaluator = new(new Grammar(), context);
        ast = ToTree("foo.bar[1]");
        result = await _evaluator.EvalAsync(ast);
        var expected3 = new JsonObject { { "tek", "baz" } };
        Assert.True(JsonNode.DeepEquals(expected3, result));

        _evaluator = new(new Grammar(), context);
        ast = ToTree("foo.bar[.tek == \"baz\"][0].tek");
        result = await _evaluator.EvalAsync(ast);
        Assert.Equal("baz", result?.ToString());
    }

    [Fact]
    public async void EvaluateExpression_FiltersArrays2()
    {
        JsonObject context =
            new()
            {
                {
                    "foo",
                    new JsonObject
                    {
                        {
                            "bar",
                            new JsonArray
                            {
                                new JsonObject { { "tek", "hello" } },
                                new JsonObject { { "tek", "baz" } },
                                new JsonObject { { "tok", "baz" } }
                            }
                        }
                    }
                }
            };
        Evaluator _evaluator;

        _evaluator = new(new Grammar(), context);
        var ast = ToTree("foo.bar[.tek == \"baz\"].tek");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal("baz", result?.ToString());

        _evaluator = new(new Grammar(), context);
        ast = ToTree("foo.bar[1].tek");
        result = await _evaluator.EvalAsync(ast);
        Assert.Equal("baz", result?.ToString());
    }

    [Fact]
    public async void EvaluateExpression_FiltersArrays3()
    {
        JsonObject context =
            new()
            {
                {
                    "foo",
                    new JsonObject
                    {
                        {
                            "bar",
                            new JsonArray
                            {
                                new JsonObject { { "tek", "hello" }, { "tok", "olleh" } },
                                new JsonObject { { "tek", "baz" }, { "tok", "olleh" } },
                                new JsonObject { { "tok", "baz" }, { "tak", "olleh" } }
                            }
                        }
                    }
                }
            };
        Evaluator _evaluator;

        _evaluator = new(new Grammar(), context);
        var ast = ToTree("foo.bar[.tek == \"baz\" && .tok == 'olleh'].tek");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal("baz", result?.ToString());
    }

    [Fact]
    public async void EvaluateExpression_AllowFiltersToSelectObjectProperties()
    {
        JsonObject context =
            new()
            {
                {
                    "foo",
                    new JsonObject
                    {
                        {
                            "baz",
                            new JsonObject { { "bar", "tek" } }
                        }
                    }
                }
            };
        Evaluator _evaluator = new(new Grammar(), context);
        var ast = ToTree(@"foo[""ba"" + ""z""].bar");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal("tek", result?.ToString());
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
        var expected = new JsonObject
        {
            {
                "foo",
                new JsonObject { { "bar", "tek" } }
            }
        };
        Assert.True(JsonNode.DeepEquals(expected, result));
    }

    [Fact]
    public async void EvaluateExpression_EmptyObjectLiteral()
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(@"{}");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal(new JsonObject { }, result);
    }

    [Fact]
    public async void EvaluateExpression_DotNotationForObjectLiterals()
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(@"{foo: ""bar""}.foo");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal("bar", result?.ToString());
    }

    /* [Fact]
    public async void EvaluateExpression_AllowAccessToLiteralProperties()
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(@"""foo"".Length");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal(JsonValue.Create("foo".Length), result);
    } */

    /* [Fact]
    public async void EvaluateExpression_AllowAccessToEmptyLiteralProperties()
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(@""""".Length");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal(JsonValue.Create("".Length), result);
    } */

    [Fact]
    public async void EvaluateExpression_AppliesTransformsWithMultipleArgs()
    {
        var grammar = new Grammar();
        grammar.AddTransform(
            "concat",
            (JsonNode[] args) => args[0] + ": " + args[1] + args[2] + args[3]
        );
        Evaluator _evaluator = new(grammar);
        var ast = ToTree(@"""foo""|concat(""baz"", ""bar"", ""tek"")");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal("foo: bazbartek", result?.ToString());
    }

    [Fact]
    public async void EvaluateExpression_AllowAddMultipleTransforms()
    {
        var grammar = new Grammar();
        grammar.AddTransforms(
            new Dictionary<string, Func<JsonNode[], JsonNode>>
            {
                { "concat", (JsonNode[] args) => args[0] + ": " + args[1] + args[2] + args[3] },
                { "concat2", (JsonNode[] args) => args[0] + ": " + args[1] + args[2] + args[3] }
            }
        );
        Evaluator _evaluator = new(grammar);
        var ast = ToTree(
            @"""foo""|concat(""baz"", ""bar"", ""tek"")|concat2(""baz"", ""bar"", ""tek"")"
        );
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal("foo: bazbartek: bazbartek", result?.ToString());
    }

    [Fact]
    public async void EvaluateExpression_ArrayLiteral()
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(@"[""foo"", 1+2]");
        var result = await _evaluator.EvalAsync(ast);
        JsonArray expected = new() { "foo", 3 };
        Assert.True(JsonNode.DeepEquals(expected, result));
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
        Assert.Equal(expected, result?.GetValue<bool>());
    }

    [Theory]
    [InlineData(@"""foo"" ? 1 : 2", 1)]
    [InlineData(@""""" ? 1 : 2", 2)]
    public async void EvaluateExpression_Conditional(string input, decimal expected)
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(input);
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal(expected, result?.GetValue<decimal>());
    }

    [Theory]
    [InlineData(@"""foo"" ?: ""bar""", "foo")]
    [InlineData(@""""" ?: ""bar""", "bar")]
    [InlineData(@"{foo:'bar'}.baz ?: 'tek'", "tek")]
    [InlineData(@"{foo:[]}.foo.bar ?: 'tek'", "tek")]
    [InlineData(@"{foo:{baz:{}}}.foo.bar ?: 'tek'", "tek")]
    public async void EvaluateExpression_AllowsMissingConsequentInTernary(
        string input,
        string expected
    )
    {
        Evaluator _evaluator = new(new Grammar());
        var ast = ToTree(input);
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal(expected, result?.ToString());
    }

    [Fact]
    public async void EvaluateExpression_ReturnsEmptyArrayWhenApplyingFilterToUndefined()
    {
        JsonObject context = new() { { "a", new JsonObject() }, { "b", 4 } };
        Evaluator _evaluator = new(new Grammar(), context);
        var ast = ToTree(@"a.b[.c == d]");
        var result = await _evaluator.EvalAsync(ast);
        var expected = new JsonArray { };
        Assert.True(JsonNode.DeepEquals(expected, result));
        Assert.Empty(result!.AsArray());
    }

    [Fact]
    public async void EvaluateExpression_WithDollarIdentifiers()
    {
        JsonObject context = new() { { "$", 5 }, { "$foo", 6 }, { "$foo$bar", 7 }, { "$bar", 8 } };
        Evaluator _evaluator = new(new Grammar(), context);
        var ast = ToTree(@"$+$foo+$foo$bar+$bar");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal((decimal)26, result?.GetValue<decimal>());
    }

    [Fact]
    public async void EvaluateExpression_BinaryEvaluateOnDemand()
    {
        var grammar = new Grammar();
        bool toTrueEvaluated = false;
        grammar.AddTransform(
            "toTrue",
            (JsonNode[] args) =>
            {
                toTrueEvaluated = true;
                return true;
            }
        );
        Evaluator _evaluator = new(grammar);
        var ast = ToTree(@"true && ""foo""|toTrue");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal(true, result?.GetValue<bool>());
        Assert.True(toTrueEvaluated);
    }

    [Fact]
    public async void EvaluateExpression_BinaryEvaluateOnDemand2()
    {
        var grammar = new Grammar();
        bool toTrueEvaluated = false;
        grammar.AddTransform(
            "toTrue",
            (JsonNode[] args) =>
            {
                toTrueEvaluated = true;
                return true;
            }
        );
        Evaluator _evaluator = new(grammar);
        var ast = ToTree("true || foo|toTrue");
        var result = await _evaluator.EvalAsync(ast);
        Assert.Equal(true, result?.GetValue<bool>());
        Assert.False(toTrueEvaluated);
    }
}
