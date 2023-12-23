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
    }
}