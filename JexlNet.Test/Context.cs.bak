using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
// using JexlNet.JsonNet;

namespace JexlNet.Test;

public class ContextUnitTest
{
    [Fact]
    public void ConvertJsonElement()
    {
        string contextJson = @"{ ""name"": ""Sterling"" }";
        JsonElement contextJsonElement = JsonDocument.Parse(contextJson).RootElement;
        Dictionary<string, dynamic?> context = ContextHelpers.ConvertJsonElement(contextJsonElement);
        var expected = new Dictionary<string, dynamic?> {
            { "name", "Sterling" }
        };
        Assert.Equal(expected, context);
    }

    [Fact]
    public void ConvertJsonElementComplex()
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
        var expected = new Dictionary<string, dynamic?> {
            { "name", new Dictionary<string, dynamic?> {
                { "first", "Sterling" },
                { "last", "Archer" }
            }},
            { "assoc", new List<dynamic?> {
                new Dictionary<string, dynamic?> {
                    { "first", "Lana" },
                    { "last", "Kane" }
                },
                new Dictionary<string, dynamic?> {
                    { "first", "Cyril" },
                    { "last", "Figgis" }
                },
                new Dictionary<string, dynamic?> {
                    { "first", "Pam" },
                    { "last", "Poovey" }
                }
            }},
            { "age", 36 }
        };
        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(expected), System.Text.Json.JsonSerializer.Serialize(context));
    }

    [Fact]
    public void ConvertJObjectElement()
    {
        string contextJson = @"{ ""name"": ""Sterling"" }";
        JObject contextJsonElement = JObject.Parse(contextJson);
        Dictionary<string, dynamic?>? context = JexlNet.JsonNet.ContextHelpers.ConvertJObject(contextJsonElement);
        var expected = new Dictionary<string, dynamic?> {
            { "name", "Sterling" }
        };
        Assert.Equal(expected, context);
    }

    [Fact]
    public void ConvertJObjectComplex()
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
        Dictionary<string, dynamic?>? context = JexlNet.JsonNet.ContextHelpers.ConvertJObject(contextJsonElement);
        var expected = new Dictionary<string, dynamic?> {
            { "name", new Dictionary<string, dynamic?> {
                { "first", "Sterling" },
                { "last", "Archer" }
            }},
            { "assoc", new List<dynamic?> {
                new Dictionary<string, dynamic?> {
                    { "first", "Lana" },
                    { "last", "Kane" }
                },
                new Dictionary<string, dynamic?> {
                    { "first", "Cyril" },
                    { "last", "Figgis" }
                },
                new Dictionary<string, dynamic?> {
                    { "first", "Pam" },
                    { "last", "Poovey" }
                }
            }},
            { "age", 36 }
        };
        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(expected), System.Text.Json.JsonSerializer.Serialize(context));
        // Json.Net converts adds decimals to 'decimal' values, resulting in 36.0 as age, instead of 36.
        // So this is meant to fail.
        Assert.NotEqual(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(context));
    }
}