using System.Text.Json.Nodes;

namespace JexlNet.Test;

public class JexlUnitTest
{
    [Fact]
    public void AllowsFunctionsToBeAdded()
    {
        var jexl = new Jexl();
        var sayHi = new Func<JsonValue>(() => (JsonValue)"hi");
        jexl.Grammar.AddFunction("sayHi", sayHi);
        var result = jexl.Eval("sayHi()");
        Assert.Equal("hi", result?.ToString());

        jexl.Grammar.AddFunction("sayHi2", sayHi);
        var result2 = jexl.Eval("sayHi2()");
        Assert.Equal("hi", result2?.ToString());

        jexl.Grammar.AddFunction("concat", (JsonNode?[] args) => string.Concat(args.Select(x => x?.ToString())));
        var result3 = jexl.Eval("concat('a', 'b', 'c')");
        Assert.Equal("abc", result3?.ToString());


        jexl.Grammar.AddFunction("print", (JsonNode? arg) => arg);
        var result5 = jexl.Eval("print('a')");
        Assert.Equal("a", result5?.ToString());

        jexl.Grammar.AddFunction("min", (JsonNode?[] args) => args.Select(x => x?.AsValue().ToDecimal()).Min());
        var result6 = jexl.Eval("min(4, 2, 19)");
        Assert.Equal(2, result6?.AsValue()?.ToInt32());
    }

    [Fact]
    public async void AllowsAsyncFunctionsToBeAdded()
    {
        var jexl = new Jexl();
        var sayHello = new Func<Task<JsonNode?>>(async () =>
        {
            await Task.Delay(100);
            return "hello";
        });
        jexl.Grammar.AddFunction("sayHello", sayHello);
        var result2 = await jexl.EvalAsync("sayHello()");
        Assert.Equal("hello", result2?.ToString());

        var result = jexl.Eval("sayHello()");
        Assert.Equal("hello", result?.ToString());

        jexl.Grammar.AddFunction("concat", async (JsonNode?[] args) =>
        {
            await Task.Delay(100);
            return string.Concat(args.Select(x => x?.ToString()));
        });
        var result3 = jexl.Eval("concat('a', 'b', 'c')");
        Assert.Equal("abc", result3?.ToString());

        jexl.Grammar.AddFunction("concat2", async (JsonNode?[] args) =>
        {
            await Task.Delay(100);
            return string.Concat(args.Select(x => x?.ToString()));
        });
        var result4 = jexl.Eval("concat2('a', 'b', 'c')");
        Assert.Equal("abc", result4?.ToString());

        jexl.Grammar.AddFunction("print", async (JsonNode? arg) =>
        {
            await Task.Delay(100);
            return arg;
        });
        var result5 = jexl.Eval("print('a')");
        Assert.Equal("a", result5?.ToString());
    }

    [Fact]
    public void AllowsFunctionsToBeAddedInBatch()
    {
        var jexl = new Jexl();
        var sayHi = new Func<JsonNode?>(() => "hi");
        jexl.Grammar.AddFunctions(new Dictionary<string, Func<JsonNode?>> {
            { "sayHi", sayHi },
            { "sayHo", () => "ho" }
        });
        var result = jexl.Eval("sayHi() + sayHo()");
        Assert.Equal("hiho", result?.ToString());
    }

    [Fact]
    public void AllowsTransformsToBeAdded()
    {
        var jexl = new Jexl();
        jexl.Grammar.AddTransform("add1", (JsonValue? val) => val?.ToDecimal() + 1);
        var result = jexl.Eval("2|add1");
        Assert.Equal(3, result?.AsValue().ToDecimal());
        jexl.Grammar.AddTransform("add2", (JsonNode? val) => Convert.ToInt32(val?.ToString()) + 2);
        var result2 = jexl.Eval("2|add1|add2");
        Assert.Equal(5, result2?.AsValue().ToDecimal());
        jexl.Grammar.AddTransform("split", (JsonNode?[] args) =>
        {
            var str = args[0]?.ToString();
            var splitchar = args[1]?.ToString();
            string[] splitted = str?.Split(splitchar) ?? [];
            return new JsonArray(splitted.Select(x => (JsonNode?)x).ToArray());
        });
        var res3 = jexl.Eval(@"""password==guest""|split('=' + '=')");
        Assert.True(JsonNode.DeepEquals(new JsonArray { "password", "guest" }, res3));
        jexl.Grammar.AddTransform("lower", (JsonNode? val) => val?.ToString().ToLower());
        var res4 = jexl.Eval(@"""Pam Poovey""|lower|split(' ')[1]");
        Assert.Equal("poovey", res4?.ToString());
        jexl.Grammar.AddTransform("split2", (JsonNode? arg0, JsonNode?[] args) => new JsonArray((arg0?.ToString().Split(args[0]?.ToString()) ?? []).Select(x => (JsonNode?)x).ToArray()));
        var res5 = jexl.Eval(@"""Pam Poovey""|lower|split2(' ')[1]");
        Assert.Equal("poovey", res5?.ToString());
        jexl.Grammar.AddTransform("split3", (JsonNode? arg0, JsonNode? splitchar) => new JsonArray((arg0?.ToString().Split(splitchar?.ToString()) ?? []).Select(x => (JsonNode?)x).ToArray()));
        var res6 = jexl.Eval(@"""Pam Poovey""|lower|split3(' ')[1]");
        Assert.Equal("poovey", res6?.ToString());
    }

    [Fact]
    public void AllowsTransformsToBeAddedInBatch()
    {
        var jexl = new Jexl();
        static JsonNode? split(JsonNode?[] args) => new JsonArray((args[0]?.ToString().Split(args[1]?.ToString()) ?? []).Select(x => (JsonNode?)x).ToArray());
        static JsonNode? lower(JsonNode?[] args) => args[0]?.ToString().ToLower();
        jexl.Grammar.AddTransforms(new Dictionary<string, Func<JsonNode?[], JsonNode?>> {
            { "split", split },
            { "lower", lower }
        });
        var result = jexl.Eval(@"""Pam Poovey""|lower|split(' ')[1]");
        Assert.Equal("poovey", result?.ToString());
    }

    [Fact]
    public void AllowsTransformsToBeAddedInBatch2()
    {
        var jexl = new Jexl();
        static JsonNode? split(JsonNode? arg0, JsonNode?[] args) => new JsonArray((arg0?.ToString().Split(args[0]?.ToString()) ?? []).Select(x => (JsonNode?)x).ToArray());
        static JsonNode? lower(JsonNode? arg0, JsonNode?[] args) => arg0?.ToString().ToLower();
        jexl.Grammar.AddTransforms(new Dictionary<string, Func<JsonNode?, JsonNode?[], JsonNode?>> {
            { "split", split },
            { "lower", lower }
        });
        var result = jexl.Eval(@"""Pam Poovey""|lower|split(' ')[1]");
        Assert.Equal("poovey", result?.ToString());
    }

    [Fact]
    public void AllowsTransformsToBeAddedInBatch3()
    {
        var jexl = new Jexl();
        static JsonNode? split(JsonNode? arg0, JsonNode? arg) => new JsonArray((arg0?.ToString().Split(arg?.ToString()) ?? []).Select(x => (JsonNode?)x).ToArray());
        static JsonNode? lower(JsonNode? arg0, JsonNode? arg) => arg0?.ToString().ToLower();
        jexl.Grammar.AddTransforms(new Dictionary<string, Func<JsonNode?, JsonNode?, JsonNode?>> {
            { "split", split },
            { "lower", lower }
        });
        var result = jexl.Eval(@"""Pam Poovey""|lower|split(' ')[1]");
        Assert.Equal("poovey", result?.ToString());
    }

    [Fact]
    public async void FullSample()
    {
        var context = new JsonObject {
            { "name", new JsonObject {
                { "first", "Sterling" },
                { "last", "Archer" }
            }},
            { "assoc", new JsonArray {
                new JsonObject {
                    { "first", "Lana" },
                    { "last", "Kane" }
                },
                new JsonObject {
                    { "first", "Cyril" },
                    { "last", "Figgis" }
                },
                new JsonObject {
                    { "first", "Pam" },
                    { "last", "Poovey" }
                }
            }},
            { "age", 36 }
        };
        var jexl = new Jexl();
        JsonNode? res = await jexl.EvalAsync(@"assoc[.first == ""Lana""].last", context);
        Assert.Equal("Kane", res?.ToString());
        JsonNode? res2 = jexl.Eval(@"assoc[.first == ""Lana""].last", context);
        Assert.Equal("Kane", res2?.ToString());
        JsonNode? res3 = await jexl.EvalAsync(@"age * (3 - 1)", context);
        Assert.Equal(72, res3?.AsValue().ToDecimal());
        JsonNode? res4 = await jexl.EvalAsync(@"name.first + "" "" + name[""la"" + ""st""]", context);
        Assert.Equal("Sterling Archer", res4?.ToString());
        JsonNode? res5 = await jexl.EvalAsync(@"assoc[.last == ""Figgis""].first == ""Cyril"" && assoc[.last == ""Poovey""].first == ""Pam""", context);
        Assert.Equal(true, res5?.GetValue<bool>());
        JsonNode? res6 = await jexl.EvalAsync(@"assoc[1]", context);
        Assert.True(JsonNode.DeepEquals(new JsonObject {
            { "first", "Cyril" },
            { "last", "Figgis" }
        }, res6));
        JsonNode? res7 = await jexl.EvalAsync(@"age > 62 ? ""retired"" : ""working""", context);
        Assert.Equal("working", res7?.ToString());
        jexl.Grammar.AddTransform("upper", (JsonNode? val) => val?.ToString().ToUpper());
        JsonNode? res8 = await jexl.EvalAsync(@"""duchess""|upper + "" "" + name.last|upper", context);
        Assert.Equal("DUCHESS ARCHER", res8?.ToString());
    }

    [Fact]
    public async void FullSample2()
    {
        var context = new JsonObject {
            { "name", new JsonObject {
                { "first", "Sterling" },
                { "last", "Archer" }
            }},
            { "assoc", new JsonArray {
                new JsonObject {
                    { "first", "Lana" },
                    { "last", "Kane" }
                },
                new JsonObject {
                    { "first", "Cyril" },
                    { "last", "Figgis" }
                },
                new JsonObject {
                    { "first", "Pam" },
                    { "last", "Poovey" }
                }
            }},
            { "age", 36 }
        };
        var jexl = new Jexl();
        JsonNode? lastName = await jexl.EvalAsync(@"name.last", context);
        Assert.Equal("Archer", lastName?.ToString());
        var DbSelectByLastName = new Func<JsonNode?, JsonNode?, Task<JsonNode?>>(async (lastName, stat) =>
        {
            await Task.Delay(100);
            var dict = new JsonObject {
                { "Archer", 184 },
            };
            if (lastName == null || !dict.ContainsKey(lastName.ToString())) throw new Exception("not found");
            return dict[lastName.ToString()];
        });
        jexl.Grammar.AddTransform("getStat", async (JsonNode?[] args) => await DbSelectByLastName(args[0], args[1]));
        JsonNode? res9 = await jexl.EvalAsync(@"name.last|getStat(""weight"")", context);
        Assert.Equal(184, res9?.AsValue().ToDecimal());
        await Assert.ThrowsAsync<Exception>(async () => await jexl.EvalAsync(@"assoc[1].last|getStat(""weight"")", context));
        var GetOldestAgent = new Func<Task<JsonNode?>>(async () =>
        {
            await Task.Delay(100);
            var dict = new JsonObject {
                { "Archer", 32 },
                { "Poovey", 34 },
                { "Figgis", 45 },
            };
            // Create new JsonObject that only contains the oldest agent
            JsonObject oldestAgent = new();
            foreach (var kv in dict)
            {
                if (oldestAgent.Count == 0 || kv.Value?.AsValue().ToDecimal() > oldestAgent.First().Value?.AsValue().ToDecimal())
                {
                    oldestAgent.Clear();
                    oldestAgent.Add(kv.Key, kv.Value?.DeepClone());
                }
            }
            // var oldestKv = dict.OrderByDescending(x => x.Value?.AsValue().ToDecimal()).Select(x => new JsonObject { {x.Key, x.Value?.DeepClone()} });
            return oldestAgent;
        });
        jexl.Grammar.AddFunction("getOldestAgent", GetOldestAgent);
        JsonNode? res10 = await jexl.EvalAsync(@"getOldestAgent()", context);
        Assert.True(JsonNode.DeepEquals(res10, new JsonObject {
            { "Figgis", 45 }
        }));
        var GetOldestAgent2 = new Func<Task<JsonNode?>>(async () =>
        {
            await Task.Delay(100);
            var list = new JsonArray {
                new JsonObject {{ "lastName", "Archer" }, {"age", 32 }},
                new JsonObject {{ "lastName", "Poovey" }, {"age", 34 }},
                new JsonObject {{ "lastName", "Figgis" }, {"age", 45 }},
            };
            return list.OrderByDescending(x => x?["age"]?.AsValue().ToDecimal()).First();
        });
        jexl.Grammar.AddFunction("getOldestAgent2", GetOldestAgent2);
        JsonNode? res11 = await jexl.EvalAsync(@"getOldestAgent2().age", context);
        Assert.Equal(45, res11?.AsValue().ToDecimal());
        JsonNode? res11b = await jexl.EvalAsync(@"age", context);
        Assert.Equal(36, res11b?.AsValue().ToDecimal());
        JsonNode? res11c = await jexl.EvalAsync(@"36 == 45");
        Assert.False(res11c?.GetValue<bool>());
        JsonNode? res12 = await jexl.EvalAsync(@"age == getOldestAgent2().age", context);
        Assert.False(res12?.GetValue<bool>());
    }

    [Theory]
    [InlineData("name.first", "Malory")]
    [InlineData("name['la' + 'st']", "Archer")]
    [InlineData("exes[2]", "Burt Reynolds")]
    [InlineData("exes[lastEx - 1]", "Len Trexler")]
    public async void AccessIdentifiers(string input, string expected)
    {
        var context = new JsonObject
        {
            { "name", new JsonObject {
                { "first", "Malory" },
                { "last", "Archer" }
            }},
            { "exes", new JsonArray {
                "Nikolai Jakov",
                "Len Trexler",
                "Burt Reynolds"
            }},
            { "lastEx", 2 }
        };
        var jexl = new Jexl();
        var result = await jexl.EvalAsync(input, context);
        Assert.Equal(expected, result?.ToString());
    }

    [Fact]
    public async void AllowAddBinaryOp()
    {
        var jexl = new Jexl();
        jexl.Grammar.AddBinaryOperator("_=", 20, (JsonNode?[] args) => args[0]?.ToString().ToLower() == args[1]?.ToString().ToLower());
        var res = await jexl.EvalAsync(@"""Guest"" _= ""gUeSt""");
        Assert.True(res?.GetValue<bool>());
    }

    [Fact]
    public async void CompileOnlyOnce()
    {
        var jexl = new Jexl();
        var danger = jexl.CreateExpression(@"""Danger "" + place");
        var res1 = await danger.EvalAsync(new JsonObject { { "place", "Zone" } });
        Assert.Equal("Danger Zone", res1?.ToString());
        var res2 = await danger.EvalAsync(new JsonObject { { "place", "ZONE!!!" } });
        Assert.Equal("Danger ZONE!!!", res2?.ToString());
    }

    // This is not possible when context is a JSON ...
    /* [Fact]
    public async void ResolveDictionaryPromise()
    {
        var func = new Func<Task<object?>>(async () =>
        {
            await Task.Delay(100);
            return "bar";
        });
        var context = new JsonObject {
            { "foo", func }
        };
        var jexl = new Jexl();
        var res = await jexl.EvalAsync(@"foo", context);
        Assert.Equal("bar", res);
    } */

    // This is not possible when context is a JSON ...
    /* [Fact]
    public async void ResolveDictionaryPromise2()
    {
        var func = new Func<Task<object?>>(async () =>
        {
            await Task.Delay(100);
            return new JsonObject {
                { "bar", "baz" }
            };
        });
        var context = new JsonObject {
            { "foo", func }
        }; 
        var jexl = new Jexl();
        var res = await jexl.EvalAsync(@"foo.bar", context);
        Assert.Equal("baz", res);
    } */

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
        JsonObject? contextJsonDocument = (JsonObject?)JsonNode.Parse(contextJson);
        var jexl = new Jexl();
        var res = await jexl.EvalAsync(@"assoc[.first == 'Cyril'].last", contextJsonDocument);
        Assert.Equal("Figgis", res?.ToString());
    }


    [Fact]
    public async void SupportStringContext()
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
        var jexl = new Jexl();
        var res = await jexl.EvalAsync(@"assoc[.first == 'Cyril'].last", contextJson);
        Assert.Equal("Figgis", res?.ToString());
    }

    /* [Fact]
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
        JsonObject? context = JsonNet.ContextHelpers.ConvertJObject(contextJsonElement);
        var jexl = new Jexl();
        var res = await jexl.EvalAsync(@"assoc[.first == 'Cyril'].last", context);
        Assert.Equal("Figgis", res);
    } */
}