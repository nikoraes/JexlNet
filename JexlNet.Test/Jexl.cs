using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace JexlNet.Test;

public class JexlUnitTest
{
    [Fact]
    public void AllowsFunctionsToBeAdded()
    {
        var jexl = new Jexl();
        var sayHi = new Func<string>(() => "hi");
        jexl.Grammar.AddFunction("sayHi", sayHi);
        var result = jexl.Eval("sayHi()");
        Assert.Equal("hi", result);

        jexl.Grammar.AddFunction("sayHi2", sayHi);
        var result2 = jexl.Eval("sayHi2()");
        Assert.Equal("hi", result2);

        jexl.Grammar.AddFunction("concat", (List<dynamic> args) => string.Concat(args));
        var result3 = jexl.Eval("concat('a', 'b', 'c')");
        Assert.Equal("abc", result3);

        jexl.Grammar.AddFunction("concat2", (dynamic[] args) => string.Concat(args));
        var result4 = jexl.Eval("concat2('a', 'b', 'c')");
        Assert.Equal("abc", result4);

        jexl.Grammar.AddFunction("print", (dynamic arg) => arg);
        var result5 = jexl.Eval("print('a')");
        Assert.Equal("a", result5);

        jexl.Grammar.AddFunction("min", (List<dynamic?> args) => args.Min());
        var result6 = jexl.Eval("min(4, 2, 19)");
        Assert.Equal(2, result6);
    }

    [Fact]
    public async void AllowsAsyncFunctionsToBeAdded()
    {
        var jexl = new Jexl();
        var sayHello = new Func<Task<object>>(async () =>
        {
            await Task.Delay(100);
            return "hello";
        });
        jexl.Grammar.AddFunction("sayHello", sayHello);
        var result2 = await jexl.EvalAsync("sayHello()");
        Assert.Equal("hello", result2);

        var result = jexl.Eval("sayHello()");
        Assert.Equal("hello", result);

        jexl.Grammar.AddFunction("concat", async (List<dynamic> args) =>
        {
            await Task.Delay(100);
            return string.Concat(args);
        });
        var result3 = jexl.Eval("concat('a', 'b', 'c')");
        Assert.Equal("abc", result3);

        jexl.Grammar.AddFunction("concat2", async (dynamic?[] args) =>
        {
            await Task.Delay(100);
            return string.Concat(args);
        });
        var result4 = jexl.Eval("concat2('a', 'b', 'c')");
        Assert.Equal("abc", result4);

        jexl.Grammar.AddFunction("print", async (dynamic arg) =>
        {
            await Task.Delay(100);
            return arg;
        });
        var result5 = jexl.Eval("print('a')");
        Assert.Equal("a", result5);
    }

    [Fact]
    public void AllowsFunctionsToBeAddedInBatch()
    {
        var jexl = new Jexl();
        var sayHi = new Func<string>(() => "hi");
        jexl.Grammar.AddFunctions(new Dictionary<string, Func<object>> {
            { "sayHi", sayHi },
            { "sayHo", () => "ho" }
        });
        var result = jexl.Eval("sayHi() + sayHo()");
        Assert.Equal("hiho", result);
    }

    [Fact]
    public void AllowsTransformsToBeAdded()
    {
        var jexl = new Jexl();
        jexl.Grammar.AddTransform("add1", (dynamic? val) => val + 1);
        var result = jexl.Eval("2|add1");
        Assert.Equal(3, result);
        jexl.Grammar.AddTransform("add2", (dynamic? val) => val + 2);
        var result2 = jexl.Eval("2|add1|add2");
        Assert.Equal(5, result2);
        jexl.Grammar.AddTransform("split", (dynamic?[] args) => args[0]?.Split(args[1]));
        jexl.Grammar.AddTransform("lower", (dynamic? val) => val?.ToLower());
        var res3 = jexl.Eval(@"""Pam Poovey""|lower|split(' ')[1]");
        Assert.Equal("poovey", res3);
        var res4 = jexl.Eval(@"""password==guest""|split('=' + '=')");
        Assert.Equal(new List<dynamic?> { "password", "guest" }, res4);
    }


    [Fact]
    public async void FullSample()
    {
        var context = new Dictionary<string, dynamic?> {
            { "name", new Dictionary<string, dynamic> {
                { "first", "Sterling" },
                { "last", "Archer" }
            }},
            { "assoc", new List<dynamic> {
                new Dictionary<string, dynamic> {
                    { "first", "Lana" },
                    { "last", "Kane" }
                },
                new Dictionary<string, dynamic> {
                    { "first", "Cyril" },
                    { "last", "Figgis" }
                },
                new Dictionary<string, dynamic> {
                    { "first", "Pam" },
                    { "last", "Poovey" }
                }
            }},
            { "age", 36 }
        };
        var jexl = new Jexl();
        dynamic? res = await jexl.EvalAsync(@"assoc[.first == ""Lana""].last", context);
        Assert.Equal("Kane", res);
        dynamic? res2 = jexl.Eval(@"assoc[.first == ""Lana""].last", context);
        Assert.Equal("Kane", res2);
        dynamic? res3 = await jexl.EvalAsync(@"age * (3 - 1)", context);
        Assert.Equal(72, res3);
        dynamic? res4 = await jexl.EvalAsync(@"name.first + "" "" + name[""la"" + ""st""]", context);
        Assert.Equal("Sterling Archer", res4);
        dynamic? res5 = await jexl.EvalAsync(@"assoc[.last == ""Figgis""].first == ""Cyril"" && assoc[.last == ""Poovey""].first == ""Pam""", context);
        Assert.Equal(true, res5);
        dynamic? res6 = await jexl.EvalAsync(@"assoc[1]", context);
        Assert.Equal(new Dictionary<string, dynamic> {
            { "first", "Cyril" },
            { "last", "Figgis" }
        }, res6);
        dynamic? res7 = await jexl.EvalAsync(@"age > 62 ? ""retired"" : ""working""", context);
        Assert.Equal("working", res7);
        jexl.Grammar.AddTransform("upper", (dynamic? val) => val?.ToString().ToUpper());
        dynamic? res8 = await jexl.EvalAsync(@"""duchess""|upper + "" "" + name.last|upper", context);
        Assert.Equal("DUCHESS ARCHER", res8);
    }

    [Fact]
    public async void FullSample2()
    {
        var context = new Dictionary<string, dynamic?> {
            { "name", new Dictionary<string, dynamic> {
                { "first", "Sterling" },
                { "last", "Archer" }
            }},
            { "assoc", new List<dynamic> {
                new Dictionary<string, dynamic> {
                    { "first", "Lana" },
                    { "last", "Kane" }
                },
                new Dictionary<string, dynamic> {
                    { "first", "Cyril" },
                    { "last", "Figgis" }
                },
                new Dictionary<string, dynamic> {
                    { "first", "Pam" },
                    { "last", "Poovey" }
                }
            }},
            { "age", 36 }
        };
        var jexl = new Jexl();
        var DbSelectByLastName = new Func<dynamic?, dynamic?, Task<object?>>(async (lastName, stat) =>
        {
            await Task.Delay(100);
            var dict = new Dictionary<string, decimal> {
                { "Archer", 184 },
            };
            if (!dict.ContainsKey(lastName)) throw new Exception("not found");
            return dict[lastName];
        });
        jexl.Grammar.AddTransform("getStat", async (dynamic?[] args) => await DbSelectByLastName(args[0], args[1]));
        dynamic? res9 = await jexl.EvalAsync(@"name.last|getStat(""weight"")", context);
        Assert.Equal(184, res9);
        await Assert.ThrowsAsync<Exception>(async () => await jexl.EvalAsync(@"assoc[1].last|getStat(""weight"")", context));
        var GetOldestAgent = new Func<Task<object?>>(async () =>
        {
            await Task.Delay(100);
            var dict = new Dictionary<string, decimal> {
                { "Archer", 32 },
                { "Poovey", 34 },
                { "Figgis", 45 },
            };
            return dict.OrderByDescending(x => x.Value).First();
        });
        jexl.Grammar.AddFunction("getOldestAgent", GetOldestAgent);
        dynamic? res10 = await jexl.EvalAsync(@"getOldestAgent().Value", context);
        Assert.Equal(45, res10);
        var GetOldestAgent2 = new Func<Task<object?>>(async () =>
        {
            await Task.Delay(100);
            var list = new List<dynamic> {
                new Dictionary<string, object> {{ "lastName", "Archer" }, {"age", 32 }},
                new Dictionary<string, object> {{ "lastName", "Poovey" }, {"age", 34 }},
                new Dictionary<string, object> {{ "lastName", "Figgis" }, {"age", 45 }},
            };
            return list.OrderByDescending(x => x["age"]).First();
        });
        jexl.Grammar.AddFunction("getOldestAgent2", GetOldestAgent2);
        dynamic? res11 = await jexl.EvalAsync(@"getOldestAgent2().age", context);
        Assert.Equal(45, res11);
        dynamic? res12 = await jexl.EvalAsync(@"age == getOldestAgent2().age", context);
        Assert.False(res12);
    }

    [Theory]
    [InlineData("name.first", "Malory")]
    [InlineData("name['la' + 'st']", "Archer")]
    [InlineData("exes[2]", "Burt Reynolds")]
    [InlineData("exes[lastEx - 1]", "Len Trexler")]
    public async void AccessIdentifiers(string input, string expected)
    {
        var context = new Dictionary<string, dynamic?>
        {
            { "name", new Dictionary<string, dynamic?> {
                { "first", "Malory" },
                { "last", "Archer" }
            }},
            { "exes", new List<string> {
                "Nikolai Jakov",
                "Len Trexler",
                "Burt Reynolds"
            }},
            { "lastEx", 2 }
        };
        var jexl = new Jexl();
        var result = await jexl.EvalAsync(input, context);
        Assert.Equal(expected, result);
    }

    [Fact]
    public async void AllowAddBinaryOp()
    {
        var jexl = new Jexl();
        jexl.Grammar.AddBinaryOperator("_=", 20, (dynamic?[] args) => args[0]?.ToLower() == args[1]?.ToLower());
        var res = await jexl.EvalAsync(@"""Guest"" _= ""gUeSt""");
        Assert.True(res);
    }

    [Fact]
    public async void CompileOnlyOnce()
    {
        var jexl = new Jexl();
        var danger = jexl.CreateExpression(@"""Danger "" + place");
        var res1 = await danger.EvalAsync(new Dictionary<string, dynamic?> { { "place", "Zone" } });
        Assert.Equal("Danger Zone", res1);
        var res2 = await danger.EvalAsync(new Dictionary<string, dynamic?> { { "place", "ZONE!!!" } });
        Assert.Equal("Danger ZONE!!!", res2);
    }

    [Fact]
    public async void ResolveDictionaryPromise()
    {
        var func = new Func<Task<object?>>(async () =>
        {
            await Task.Delay(100);
            return "bar";
        });
        var context = new Dictionary<string, dynamic?> {
            { "foo", func }
        };
        var jexl = new Jexl();
        var res = await jexl.EvalAsync(@"foo", context);
        Assert.Equal("bar", res);

    }

    [Fact]
    public async void ResolveDictionaryPromise2()
    {
        var func = new Func<Task<object?>>(async () =>
        {
            await Task.Delay(100);
            return new Dictionary<string, dynamic> {
                { "bar", "baz" }
            };
        });
        var context = new Dictionary<string, dynamic?> {
            { "foo", func }
        };
        var jexl = new Jexl();
        var res = await jexl.EvalAsync(@"foo.bar", context);
        Assert.Equal("baz", res);
    }

    [Fact]
    public async void SupportJsonContext()
    {
        string contextJson =
        @"{
            ""name"": {
                ""first"": ""Sterling"",
                ""last"": ""Archer""
            },
            ""assoc"": [
                {
                    ""first"": ""Lana"",
                    ""last"": ""Kane""
                },
                {
                    ""first"": ""Cyril"",
                    ""last"": ""Figgis""
                },
                {
                    ""first"": ""Pam"",
                    ""last"": ""Poovey""
                }
            ],
            ""age"": 36
        }";
        JsonElement contextJsonElement = JsonDocument.Parse(contextJson).RootElement;
        Dictionary<string, dynamic?> context = ContextHelpers.ConvertJsonElement(contextJsonElement);
        var jexl = new Jexl();
        var res = await jexl.EvalAsync(@"assoc[.first == 'Cyril'].last", context);
        Assert.Equal("Figgis", res);
    }

    [Fact]
    public async void SupportJsonNetContext()
    {
        string contextJson =
        @"{
            ""name"": {
                ""first"": ""Sterling"",
                ""last"": ""Archer""
            },
            ""assoc"": [
                {
                    ""first"": ""Lana"",
                    ""last"": ""Kane""
                },
                {
                    ""first"": ""Cyril"",
                    ""last"": ""Figgis""
                },
                {
                    ""first"": ""Pam"",
                    ""last"": ""Poovey""
                }
            ],
            ""age"": 36
        }";
        JObject contextJsonElement = JObject.Parse(contextJson);
        Dictionary<string, dynamic?>? context = JsonNet.ContextHelpers.ConvertJObject(contextJsonElement);
        var jexl = new Jexl();
        var res = await jexl.EvalAsync(@"assoc[.first == 'Cyril'].last", context);
        Assert.Equal("Figgis", res);
    }
}