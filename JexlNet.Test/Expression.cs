using System.Text.Json.Nodes;

namespace JexlNet.Test;

public class ExpressionUnitTest
{
    [Fact]
    public void ReturnsParentInstance()
    {
        var jexl = new Jexl();
        var expr = jexl.CreateExpression("2/2");
        Assert.IsType<Expression>(expr);
        var compiled = expr.Compile();
        Assert.Equal(expr, compiled);
    }

    [Fact]
    public void ThrowsOnInvalidExpression()
    {
        var jexl = new Jexl();
        var expr = jexl.CreateExpression("2 & 2");
        Assert.Throws<Exception>(expr.Compile);
    }

    [Fact]
    public void PassesContext()
    {
        var jexl = new Jexl();
        var expr = jexl.CreateExpression("foo");
        var result = expr.Eval(new JsonObject { { "foo", "bar" } });
        Assert.Equal("bar", result?.ToString());
    }

    [Fact]
    public async void PassesContextAsync()
    {
        var jexl = new Jexl();
        var expr = jexl.CreateExpression("foo");
        var result = await expr.EvalAsync(new JsonObject { { "foo", "bar" } });
        Assert.Equal("bar", result?.ToString());
    }

    public class TestExpression(string exprStr) : Expression(exprStr)
    {
        public int CompileCallCount { get; private set; }
        public override Expression Compile()
        {
            CompileCallCount++;
            return base.Compile();
        }
    }

    [Fact]
    public void CompilesMoreThanOnceIfRequested()
    {
        var expr = new TestExpression("2 * 2");
        expr.Compile();
        expr.Compile();
        Assert.Equal(2, expr.CompileCallCount);
    }

    [Fact]
    public void CompilesOnceIfEvaluatedTwice()
    {
        var expr = new TestExpression("2 * 2");
        expr.Eval();
        expr.Eval();
        Assert.Equal(1, expr.CompileCallCount);
    }

    [Fact]
    public async void CompilesOnceIfEvaluatedAsyncTwice()
    {
        var expr = new TestExpression("2 * 2");
        await expr.EvalAsync();
        await expr.EvalAsync();
        Assert.Equal(1, expr.CompileCallCount);
    }
}