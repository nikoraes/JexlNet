using System.Text.Json.Nodes;

namespace JexlNet.Test;

public class ExtendedGrammarUnitTest
{
    [Theory]
    [InlineData("123456|toString", "123456")]
    [InlineData("{'a':123456}|toString", @"{""a"":123456}")]
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
    public void Length(string expression, int expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        Assert.Equal(expected, jexl.Eval(expression)?.AsValue().ToInt32());
    }

    [Theory]
    [InlineData("substring(123456,2,2)", "34")]
    [InlineData("$substring('test',(-2))", "st")]
    [InlineData("$substring('test',-2)", "st")]
    [InlineData("$substring($string({'a':123456}, true),0,1)", "{")]
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
    [InlineData("'baz  '|trim", "baz")]
    [InlineData("'  baz  '|trim", "baz")]
    [InlineData("'foo'|pad(5)", "foo  ")]
    [InlineData("'foo'|pad(-5,0)", "00foo")]
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

    [Fact]
    public void Split()
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval("'foo-bar'|split('-')");
        var expected = new JsonArray { "foo", "bar" };
        Assert.True(JsonNode.DeepEquals(expected, result));
    }

    [Fact]
    public void Join()
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval("['foo','bar']|join('-')");
        Assert.Equal("foo-bar", result?.ToString());
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
    [InlineData("$number('1')", 1)]
    [InlineData("$number('1.1')", 1.1)]
    [InlineData("$number('-1.1')", -1.1)]
    [InlineData("$number(-1.1)", -1.1)]
    [InlineData("$number(-1.1)|floor", -2)]
    [InlineData("$number('10.6')|ceil", 11)]
    [InlineData("10.123456|round(2)", 10.12)]
    [InlineData("3|power(2)", 9)]
    [InlineData("9|sqrt", 3)]
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
    [InlineData("['foo', 'bar', 'baz'] | append('tek')")]
    [InlineData("['foo', 'bar'] | append(['baz','tek'])")]
    [InlineData("'foo' | append(['bar', 'baz','tek'])")]
    [InlineData("'foo' | append('bar', 'baz','tek')")]
    [InlineData("['tek', 'baz', 'bar', 'foo']|reverse")]
    [InlineData("['tek', 'baz', 'bar', 'foo', 'foo']|reverse|distinct")]
    [InlineData("{'foo':0, bar:1, 'baz':2, tek:3}|keys")]
    [InlineData("{a:'foo', b:'bar', c:'baz', d:'tek'}|values")]
    [InlineData("[{name:'foo'}, {name:'bar'}, {name:'baz'}, {name:'tek'}]|mapField('name')")]
    public void Arrays(string expression)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression);
        var expected = new JsonArray { "foo", "bar", "baz", "tek" };
        Assert.True(JsonNode.DeepEquals(expected, result));
    }

    [Theory]
    [InlineData("$merge({'foo':'bar'},{baz:'tek'})")]
    [InlineData("{'foo':'bar'}|merge({baz:'tek'})")]
    public void Objects(string expression)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression);
        var expected = new JsonObject { { "foo", "bar" }, { "baz", "tek" } };
        Assert.True(JsonNode.DeepEquals(expected, result));
    }


    [Theory]
    [InlineData("(now()|toMillis / 1000)|ceil == (millis() / 1000)|ceil", true)]
    [InlineData("(((millis() / 1000) | ceil) * 1000) | toDateTime == ((now()|toMillis / 1000) | ceil * 1000) | toDateTime", true)]
    public void TimeFunctions(string expression, bool expected)
    {
        var jexl = new Jexl(new ExtendedGrammar());
        var result = jexl.Eval(expression);
        Assert.Equal(expected, result?.GetValue<bool>());
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
}
