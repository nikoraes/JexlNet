using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit.Sdk;

namespace JexlNet.Test;

public class ExtendedGrammarUnitTest
{
    [Theory]
    [InlineData("123456|toString", "123456")]
    [InlineData("{'a':123456}|toString", @"{""a"":123456}")]
    [InlineData("'123456'|string", "123456")]
    public void String(string expression, string expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression)?.ToString();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("'test123'|length", 7)]
    [InlineData("length('test123')", 7)]
    [InlineData("[\"a\",1,\"b\"]|length", 3)]
    [InlineData("$length([\"a\",1,\"b\"])", 3)]
    [InlineData("{foo:[]}.foo|length", 0)]
    [InlineData("{foo:'a',bar:'b'}|length", 2)]
    public void Length(string expression, int expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        Assert.Equal(expected, jexl.Eval(expression)?.AsValue().ToInt32());
    }

    [Theory]
    [InlineData("substring(123456,2,2)", "34")]
    [InlineData("substring('foo',1)", "oo")]
    [InlineData("$substring('test',(-2))", "st")]
    [InlineData("$substring('test',-2)", "st")] // Doesn't work in JS
    [InlineData("$substring($string({'a':123456}, true),0,1)", "{")] // Doesn't work in JS
    public void Substring(string expression, string expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression)?.ToString();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("substringBefore(123456,2)", "1")]
    [InlineData("$substringBefore('test','st')", "te")]
    public void SubstringBefore(string expression, string expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression)?.ToString();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("substringAfter(123456,2)", "3456")]
    [InlineData("$substringAfter('test','es')", "t")]
    public void SubstringAfter(string expression, string expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression)?.ToString();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("'baz'|uppercase", "BAZ")]
    [InlineData("$lowercase('FOObar')", "foobar")]
    public void UpperLower(string expression, string expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression)?.ToString();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("'foo bar'|camelCase", "fooBar")]
    [InlineData("$camelCase('Foo_bar')", "fooBar")]
    [InlineData("'FooBar'|toCamelCase", "fooBar")]
    [InlineData("'foo bar'|toPascalCase", "FooBar")]
    [InlineData("'fooBar'|toPascalCase", "FooBar")]
    [InlineData("'Foo_bar'|toPascalCase", "FooBar")]
    public void CamelPascalCase(string expression, string expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression)?.ToString();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("'baz  '|trim", "baz")]
    [InlineData("'  baz  '|trim", "baz")]
    [InlineData("'__baz--'|trim('-')", "__baz")]
    [InlineData("'foo'|pad(5)", "foo  ")]
    [InlineData("'foo'|pad(-5,0)", "00foo")] // Needs to be "pad("foo",(-5),0)" in TS
    public void TrimPad(string expression, string expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression)?.ToString();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("'foo-bar'|contains('bar')", true)]
    [InlineData("'foo-bar'|contains('baz')", false)]
    [InlineData("['foo-bar']|contains('bar')", false)]
    [InlineData("['foo-bar']|contains('foo-bar')", true)]
    [InlineData("['baz', 'foo', 'bar']|contains('bar')", true)]
    public void Contains(string expression, bool expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression)?.GetValue<bool>();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("replace('foo-bar', '-', '_')", "foo_bar")]
    [InlineData("replace('foo-bar----', '-', '')", "foobar")]
    [InlineData("'123ab123ab123ab'|replace('123')", "ababab")]
    public void Replace(string expression, string expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression);
        Assert.Equal(expected, result?.ToString());
    }

    [Fact]
    public void Split()
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval("'foo-bar'|split('-')");
        var expected = new JsonArray { "foo", "bar" };
        Assert.True(JsonNode.DeepEquals(expected, result));
    }

    [Theory]
    [InlineData("['foo','bar']|join('-')", "foo-bar")]
    [InlineData("'f,b,a,d,e,c'|split(',')|sort|join", "a,b,c,d,e,f")]
    [InlineData("'f,b,a,d,e,c'|split(',')|sort|join('')", "abcdef")]
    [InlineData(
        "'2024-07-08 23:50:00'|split(' ')|join('T') + '.00000+02:00'",
        "2024-07-08T23:50:00.00000+02:00"
    )]
    public void Join(string expression, string expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression);
        Assert.Equal(expected, result?.ToString());
    }

    [Theory]
    [InlineData("'foobar'|base64Encode", "Zm9vYmFy")]
    [InlineData("'Zm9vYmFy'|base64Decode", "foobar")]
    public void ConvertBase64(string expression, string expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression);
        Assert.Equal(expected, result?.ToString());
    }

    [Theory]
    [InlineData("{foo:'bar',baz:'tek'}|formUrlEncoded", "foo=bar&baz=tek")]
    public void ConvertFormUrlEncoded(string expression, string expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression);
        Assert.Equal(expected, result?.ToString());
    }

    [Theory]
    [InlineData("'foobar'|regexMatch('foo')", true)]
    [InlineData("'foobar'|regexMatch('baz')", false)]
    public void RegexMatch(string expression, bool expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression);
        Assert.Equal(expected, result?.GetValue<bool>());
    }

    [Theory]
    [InlineData("'foobar'|regexMatches('foo')|mapField('0')|includes('foo')", "true")]
    [InlineData("'foobar'|regexMatches('baz')|mapField('0')|includes('foo')", "false")]
    [InlineData(
        "'<table><tr><td>foo1</td><td>bar1</td></tr><tr><td>foo2</td><td>bar2</td></tr><tr><td>foo3</td><td>bar3</td></tr></table>'|regexMatches('<tr><td>(?<col1>.*?)</td><td>(?<col2>.*?)</td></tr>')",
        @"[{""0"":""<tr><td>foo1</td><td>bar1</td></tr>"",""col1"":""foo1"",""col2"":""bar1""},{""0"":""<tr><td>foo2</td><td>bar2</td></tr>"",""col1"":""foo2"",""col2"":""bar2""},{""0"":""<tr><td>foo3</td><td>bar3</td></tr>"",""col1"":""foo3"",""col2"":""bar3""}]"
    )]
    [InlineData(
        "'<table><tr><td>foo1</td><td>bar1</td></tr><tr><td>foo2</td><td>bar2</td></tr><tr><td>foo3</td><td>bar3</td></tr></table>'|regexMatches('<tr><td>(?<col1>.*?)</td>(?:<td>(?<col2>.*?)</td>)?</tr>')",
        @"[{""0"":""<tr><td>foo1</td><td>bar1</td></tr>"",""col1"":""foo1"",""col2"":""bar1""},{""0"":""<tr><td>foo2</td><td>bar2</td></tr>"",""col1"":""foo2"",""col2"":""bar2""},{""0"":""<tr><td>foo3</td><td>bar3</td></tr>"",""col1"":""foo3"",""col2"":""bar3""}]"
    )]
    public void RegexMatches(string expression, string expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression);
        var options = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        Assert.Equal(expected, JsonSerializer.Serialize(result, options));
    }

    [Theory]
    [InlineData("$number('1')", 1)]
    [InlineData("$number('1.1')", 1.1)]
    [InlineData("$number('-1.1')", -1.1)]
    [InlineData("$number(-1.1)", -1.1)]
    [InlineData("$number(-1.1)|floor", -2)]
    [InlineData("$number('10.6')|ceil", 11)]
    [InlineData("'5e2'|toNumber", 500)]
    [InlineData("'5.1e+3'|toNumber", 5100)]
    [InlineData("10.123456|round(2)", 10.12)]
    [InlineData("3|power(2)", 9)]
    [InlineData("3|power", 9)]
    [InlineData("9|sqrt", 3)]
    [InlineData("random() < 1 ? 1 : 0", 1)]
    public void Number(string expression, decimal expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression);
        Assert.Equal(expected, result?.AsValue().ToDecimal());
    }

    [Theory]
    [InlineData("16325.62|formatNumber('0,0.000')", "16,325.620")]
    [InlineData("12|formatBase(16)", "c")]
    [InlineData("16325.62|formatInteger('0000000')", "0016325")]
    public void Formatting(string expression, string expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression);
        Assert.Equal(expected, result?.ToString());
    }

    [Theory]
    [InlineData("'16325'|toInt", 16325)]
    [InlineData("(9/2)|toInt", 4)]
    public void Integers(string expression, int expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression);
        Assert.Equal(expected, result?.AsValue().ToInt32());
    }

    [Theory]
    [InlineData("[1,2,3]|sum", 6)]
    [InlineData("sum(1,2,3,4,5)", 15)]
    [InlineData("[1,3]|sum(1,2,3,4,5)", 19)]
    [InlineData("[1,3]|sum([1,2,3,4,5])", 19)]
    [InlineData("[1,3]|max([1,2,3,4,5])", 5)]
    [InlineData("[2,3]|min([1,2,3,4,5])", 1)]
    [InlineData("[4,5,6]|avg", 5)]
    public void NumericAggregations(string expression, decimal expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression);
        Assert.Equal(expected, result?.AsValue().ToDecimal());
    }

    [Theory]
    [InlineData("1|toBoolean", true)]
    [InlineData("3|toBoolean", true)]
    [InlineData("'1'|toBoolean", true)]
    [InlineData("'2'|toBoolean", null)]
    [InlineData("'a'|toBool", null)]
    [InlineData("''|toBool", null)]
    [InlineData("0|toBool", false)]
    [InlineData("0.0|toBool", false)]
    [InlineData("'false'|toBool", false)]
    [InlineData("'False'|toBool", false)]
    [InlineData("'fALSE'|toBool", false)]
    [InlineData("'tRUE       '|toBoolean", true)]
    [InlineData("'False'|toBool|not", true)]
    [InlineData("'TRUE'|toBool|not", false)]
    public void Boolean(string expression, bool? expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression);
        Assert.Equal(expected, result?.GetValue<bool>());
    }

    [Theory]
    [InlineData("2|case(1,'a',2,'b',3,'c')", "b")]
    [InlineData("$case('bar','foo','a','bar','b','baz','c')", "b")]
    [InlineData("'notfound'|case('bar','foo','a','bar','b','baz','c','b','b')", "b")]
    public void Case(string expression, string expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression);
        Assert.Equal(expected, result?.ToString());
    }

    [Theory]
    [InlineData("['foo', 'bar', 'baz'] | append('tek')")]
    [InlineData("['foo', 'bar'] | append(['baz','tek'])")]
    [InlineData("'foo' | append(['bar', 'baz','tek'])")]
    [InlineData("'foo' | append('bar', 'baz','tek')")]
    [InlineData("['tek', 'baz', 'bar', 'foo']|reverse")]
    [InlineData("['tek', 'baz', 'bar', 'foo', 'foo']|reverse|distinct")]
    [InlineData("{'foo':0, bar:1, 'baz':2, tek:3}|keys")] // doesn't work in TS
    // [InlineData("{['foo']:0, bar:1, ['baz']:2, tek:3}|keys")] // doesn't work
    [InlineData("{foo:0, bar:1, baz:2, tek:3}|keys")]
    [InlineData("{a:'foo', b:'bar', c:'baz', d:'tek'}|values")]
    [InlineData("[{name:'foo'}, {name:'bar'}, {name:'baz'}, {name:'tek'}]|mapField('name')")]
    [InlineData(
        "[{name:'tek',age:32}, {name:'bar',age:34}, {name:'baz',age:33}, {name:'foo',age:35}]|sort('age',true)|mapField('name')"
    )]
    [InlineData("['foo']|append(['tek','baz','bar']|sort)")]
    [InlineData(
        "['foo']|append(['tek', 'baz', 'bar', 'foo', 'foo']|filter('value != \\'foo\\'')|sort)"
    )]
    public void Arrays(string expression)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression);
        var expected = new JsonArray { "foo", "bar", "baz", "tek" };
        Assert.True(JsonNode.DeepEquals(expected, result));
    }

    [Theory]
    [InlineData(
        "[{name:'foo'}, {name:'bar'}, {name:'baz'}, {name:'tek'}]|map('value.name')",
        "['foo', 'bar', 'baz', 'tek']"
    )]
    [InlineData("assoc|map('value.age')", "[32,34,45]")]
    [InlineData("assoc|map('value.lastName')", "['Archer','Poovey','Figgis']")]
    [InlineData("assoc|map('value.age + index')", "[32,35,47]")]
    [InlineData("assoc|map('value.age + array[.age <= value.age][0].age + index')", "[64,67,79]")]
    [InlineData("assoc|map('value.age')|avg", "37")]
    public void Map(string expression, string expected)
    {
        var context = new JsonObject
        {
            {
                "assoc",
                new JsonArray
                {
                    new JsonObject { { "lastName", "Archer" }, { "age", 32 } },
                    new JsonObject { { "lastName", "Poovey" }, { "age", 34 } },
                    new JsonObject { { "lastName", "Figgis" }, { "age", 45 } },
                }
            }
        };
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression, context);
        var expectedResult = jexl.Eval(expected);
        Assert.True(JsonNode.DeepEquals(expectedResult, result));
    }

    [Theory]
    [InlineData(
        "[{name:'foo'}, {name:'bar'}, {name:'baz'}, {name:'tek'}]|any('value.name==\\'foo\\'')",
        true
    )]
    [InlineData("assoc|every('value.age>30')", true)]
    [InlineData("assoc|every('value.age>40')", false)]
    [InlineData("assoc|some('value.age>40')", true)]
    [InlineData("assoc|some('value.lastName==\\'Figgis\\'')", true)]
    [InlineData("assoc|map('value.age')|some('value>30')", true)]
    [InlineData("assoc[0]|map('value.age')[0] > 30", true)]
    public void AnyAll(string expression, bool expected)
    {
        var context = new JsonObject
        {
            {
                "assoc",
                new JsonArray
                {
                    new JsonObject { { "lastName", "Archer" }, { "age", 32 } },
                    new JsonObject { { "lastName", "Poovey" }, { "age", 34 } },
                    new JsonObject { { "lastName", "Figgis" }, { "age", 45 } },
                }
            }
        };
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression, context);
        Assert.Equal(expected, result?.GetValue<bool>());
    }

    [Theory]
    [InlineData("assoc|reduce('accumulator + value.age', 0)", "111")]
    [InlineData(
        "assoc|reduce('value.age > array|map(\\'value.age\\')|avg ? accumulator|append(value.age) : accumulator', [])",
        "[45]"
    )]
    [InlineData(
        "assoc|reduce('value.age < array|map(\\'value.age\\')|avg ? accumulator|append(value.age) : accumulator', [])[1]",
        "34"
    )]
    public void Reduce(string expression, string expected)
    {
        var context = new JsonObject
        {
            {
                "assoc",
                new JsonArray
                {
                    new JsonObject { { "lastName", "Archer" }, { "age", 32 } },
                    new JsonObject { { "lastName", "Poovey" }, { "age", 34 } },
                    new JsonObject { { "lastName", "Figgis" }, { "age", 45 } },
                }
            }
        };
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression, context);
        var expectedResult = jexl.Eval(expected);
        Assert.True(JsonNode.DeepEquals(expectedResult, result));
    }

    [Theory]
    [InlineData("$merge({'foo':'bar'},{baz:'tek'})")]
    [InlineData("{'foo':'bar'}|merge({baz:'tek'})")]
    [InlineData("{'foo':'bar'}|merge([{baz:'tek'}])")]
    [InlineData("[{'foo':'bar'}]|merge([{baz:'tek'}])")]
    [InlineData("[{foo:'bar'},{baz:'tek'}]|merge")]
    [InlineData("[['foo','bar'],['baz','tek']]|toObject")]
    public void Objects(string expression)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression);
        var expected = new JsonObject { { "foo", "bar" }, { "baz", "tek" } };
        Assert.True(JsonNode.DeepEquals(expected, result));
    }

    [Theory]
    [InlineData("['foo','bar']|toObject(true)")]
    public void ObjectsWithSingleValue(string expression)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression);
        var expected = new JsonObject { { "foo", true }, { "bar", true } };
        Assert.True(JsonNode.DeepEquals(expected, result));
    }

    [Theory]
    [InlineData("(now()|toMillis / 1000)|ceil == (millis() / 1000)|ceil", true)]
    [InlineData(
        "(((millis() / 1000) | ceil) * 1000) | toDateTime == ((now()|toMillis / 1000) | ceil * 1000) | toDateTime",
        true
    )]
    [InlineData(
        "(((millis() / 1000) | ceil) * 1000) | toDateTime | dateTimeAdd('second',5) == (((now()|toMillis / 1000) + 5) | ceil * 1000) | toDateTime",
        true
    )]
    [InlineData("'22-Feb-24 00:00:00'|toDateTime == '2024-02-22T00:00:00Z'|toDateTime", true)] // this is just a coincidence that this works, it won't in JS
    [InlineData(
        "'02-22-24 00:00:00'|toDateTime('MM-dd-yy HH:mm:ss') == '2024-02-22T00:00:00Z'|toDateTime",
        true
    )]
    [InlineData("(now()|toMillis - 1000) > (millis() - 2000)", true)]
    [InlineData("$toDateTime(1727169975224)|toMillis == 1727169975224", true)]
    public void TimeFunctions(string expression, bool expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression);
        Assert.Equal(expected, result?.GetValue<bool>());
    }

    [Theory]
    [InlineData("'22-Feb-24 00:00:00'|toDateTime", "2024-02-22T00:00:00.0000000+00:00")]
    [InlineData(
        "'02-22-24 00:00:00'|toDateTime('MM-dd-yy HH:mm:ss')",
        "2024-02-22T00:00:00.0000000+00:00"
    )]
    [InlineData(
        "'2024-02-22T00:00:00.000000Z'|dateTimeFormat('dd.MM.yyyy HH:mm:ss')",
        "22.02.2024 00:00:00"
    )]
    public void TimeFunctions2(string expression, string expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression);
        Assert.Equal(expected, result?.ToString());
    }

    [Theory]
    [InlineData("eval(1+2)", "1+2")]
    [InlineData("assoc[0]|eval('age')", "32")]
    [InlineData("assoc[2]|eval(expression)", "45")]
    public void Eval(string expression, string expected)
    {
        var context = new JsonObject
        {
            {
                "assoc",
                new JsonArray
                {
                    new JsonObject { { "lastName", "Archer" }, { "age", 32 } },
                    new JsonObject { { "lastName", "Poovey" }, { "age", 34 } },
                    new JsonObject { { "lastName", "Figgis" }, { "age", 45 } },
                }
            },
            { "expression", "age" }
        };
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression, context);
        var expectedResult = jexl.Eval(expected);
        Assert.True(JsonNode.DeepEquals(expectedResult, result));
    }

    [Fact]
    public void AllowsFunctionsToBeAdded()
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var sayHi = new Func<JsonValue>(() => (JsonValue)"hi");
        jexl.Grammar.AddFunction("sayHi", sayHi);
        var result = jexl.Eval("sayHi()");
        Assert.Equal("hi", result?.ToString());
    }

    [Fact]
    public void SelectivelyUseExtendedGrammar()
    {
        var jexl = new Jexl();
        jexl.Grammar.AddTransform("lower", ExtendedGrammar.Lowercase);
        var result = jexl.Eval("'FOObar'|lower");
        Assert.Equal("foobar", result?.ToString());
    }

    [Fact]
    public void ComplexMapExample1()
    {
        var jexl = new Jexl(new ExtendedGrammar());
        string context =
            """{"timestamp":{"low":1710760508,"high":0,"unsigned":true},"data":{"id":"176971594","tripUpdate":{"trip":{"tripId":"176971594","startTime":"15:18:00","startDate":"20240318","scheduleRelationship":"SCHEDULED","routeId":"90296","directionId":0},"stopTimeUpdate":[{"scheduleRelationship":"SCHEDULED","stopId":"2511612","departure":{"time":"1710771480","delay":0}},{"scheduleRelationship":"SCHEDULED","stopId":"2628927","departure":{"time":"1710771660","delay":0},"arrival":{"time":"1710771660","delay":0}},{"scheduleRelationship":"SCHEDULED","stopId":"2628499","departure":{"time":"1710772080","delay":0},"arrival":{"time":"1710772080","delay":0}},{"scheduleRelationship":"SCHEDULED","stopId":"2510883","departure":{"time":"1710772380","delay":0},"arrival":{"time":"1710772320","delay":0}},{"scheduleRelationship":"SCHEDULED","stopId":"2510417","departure":{"time":"1710772980","delay":0},"arrival":{"time":"1710772920","delay":0}},{"scheduleRelationship":"SCHEDULED","stopId":"2511520","arrival":{"time":"1710773400","delay":0}}]}},"gtfsRealtimeVersion":"1.0","incrementality":0}""";
        string expression =
            """data.tripUpdate.stopTimeUpdate|map(data.tripUpdate.trip.tripId + '+ \'-\' + value.stopId')""";
        var result = jexl.Eval(expression, context);
        var expected = new JsonArray
        {
            "176971594-2511612",
            "176971594-2628927",
            "176971594-2628499",
            "176971594-2510883",
            "176971594-2510417",
            "176971594-2511520"
        };
        Assert.True(JsonNode.DeepEquals(expected, result));
    }

    [Fact]
    public void ComplexMapExample2()
    {
        var jexl = new Jexl(new ExtendedGrammar());
        string context =
            """{"data": [{"id": 18833,"name": "NL-238409_RAW_3296_08","variableId": 167147,"variableName": "3296_08_X", "data": {   "2024-09-05 13:25:00": 1992.5402 } }, {  "id": 18833, "name": "NL-238409_RAW_3296_08", "variableId": 167148, "variableName": "3296_08_Y", "data": {         "2024-09-05 13:25:00": 5090.9231  }  }]}""";
        string expression =
            """data|find('!value.variableName|startsWith(\'d\') && value.variableName|endsWith(\'X\')').data|values[0]""";
        var result = jexl.Eval(expression, context);
        var expected = 1992.5402;
        Assert.Equal(expected, result.AsValue().ToDouble());
    }

    [Fact]
    public void ComplexExample3()
    {
        var jexl = new Jexl(new ExtendedGrammar());
        string context =
            """{"data": "204016                        52172    004   002ID  87362729       J         07162024MX                                        ID  132634661      J54G2   N   90                        07162024                                             ORINWN                                                                                                             "}""";
        Assert.Equal("004", jexl.Eval("data|substring(39,6)|trim", context).ToString());

        string expression =
            """data|substring(0,30)|trim + '-' + data|substring(39,6)|trim + '-' + data|substring(45,3)|trim""";
        var result = jexl.Eval(expression, context);
        var expected = "204016-004-002";
        Assert.Equal(expected, result.ToString());
    }

    [Fact]
    public void ComplexExample4()
    {
        var jexl = new Jexl(new ExtendedGrammar());
        string context =
            @"{  ""fields"": {
    ""_ch1"": ""18.8929"",
    ""_ch2"": ""16.142"",
    ""ch1"": {
            ""field"": ""LEVEL"",
      ""unit"": ""m""
    },
    ""ch2"": {
            ""field"": ""TEMPERATURE"",
      ""unit"": ""°C""
    }
    }
}";
        //"!value[0]|startsWith(\"_\") && " +
        string expression =
            """fields|entries|map('[array|find("value[0]==\""+value[0]|substring(1)+"\"")[1].field,value[1]]')|filter('!!value[0]')|toObject""";
        var result = jexl.Eval(expression, context);
        var expected = new JsonObject { { "LEVEL", "18.8929" }, { "TEMPERATURE", "16.142" } };
        Assert.True(JsonNode.DeepEquals(expected, result));
    }

    [Fact]
    public void ComplexExample5()
    {
        var jexl = new Jexl(new ExtendedGrammar());
        string context =
            @"{    ""evalInput"": {
    ""data"": {
      ""measurementTimeDefault"": ""2024-12-05T14:05:00Z"",
      ""measuredValue"": [
    {
                ""@index"": ""6"",
            ""@xsi: type"": ""_SiteMeasurementsIndexMeasuredValue"",
            ""measuredValue"": {
                    ""@xsi: type"": ""MeasuredValue"",
                ""basicData"": {
                        ""@xsi: type"": ""TrafficFlow"",
                    ""vehicleFlow"": {
                            ""@numberOfInputValuesUsed"": ""3"",
                        ""vehicleFlowRate"": ""180""
                    }
                    }
                }
            },{
                ""@index"": ""7"",
            ""@xsi: type"": ""_SiteMeasurementsIndexMeasuredValue"",
            ""vehicleFlowRate"": ""50""
            }
    ]
    } } }";

        string expression =
            """(5 + {a:5}|eval(evalInput.data.measuredValue|filter('value["@index"]=="6"')[0].measuredValue.basicData.vehicleFlow.vehicleFlowRate + ' - a'))|toString""";
        var result = jexl.Eval(expression, context);
        Assert.Equal("180", result.ToString());
    }

    [Fact]
    public void ComplexExample6()
    {
        var jexl = new Jexl(new ExtendedGrammar());
        string context =
            @"{ ""data"": {
      ""weirdDate"": ""9-2-2025 00:00:00""
      } }";

        string expression =
            """data.weirdDate|split(' ')[0]|split('-')|map(a=>a|pad(-2,'0'))|reverse|join('-') + 'T' + data.weirdDate|split(' ')[1] + '.000000Z'""";
        var result = jexl.Eval(expression, context);
        Assert.Equal("2025-02-09T00:00:00.000000Z", result.ToString());
    }

    [Theory]
    [InlineData("['f','o','o']|map((v,i) => v + i)|join('')", "f0o1o2")]
    [InlineData(
        "['foo','bar','baz','tek']|reduce((acc,v) => acc + '-' + v,'abc')",
        "abc-foo-bar-baz-tek"
    )]
    [InlineData("['foo','bar','baz','tek']|map((v) => ({'a':v}))[0].a", "foo")]
    public void ArrowOperatorString(string expression, string expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression)?.ToString();
        Assert.Equal(expected, result);
    }
}
