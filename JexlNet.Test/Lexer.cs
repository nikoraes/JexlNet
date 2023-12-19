namespace JexlNet.Test;

public class LexerUnitTest
{
    private readonly Lexer _lexer = new(new Grammar());

    [Theory]
    [InlineData(@"""foo""")]
    [InlineData(@"'foo'")]
    [InlineData(@"""f\\""oo""")]
    [InlineData(@"""foo\\""""")]
    [InlineData(@"'foo\\''")]
    [InlineData(@"'f\\'oo'")]
    public void GetElements_InputIsSingleString_Return1(string input)
    {
        var elements = _lexer.GetElements(input);
        Assert.Single(elements);
        Assert.Equal(input, elements[0]);
    }

    [Theory]
    [InlineData(@"alpha12345")]
    [InlineData(@"inString")]
    [InlineData(@"$my$Var")]
    [InlineData(@"ÄmyäVarÖö")]
    [InlineData(@"Проверка")]
    public void GetElements_InputIsIdentifier_Return1(string input)
    {
        var elements = _lexer.GetElements(input);
        Assert.Equal(input, elements[0]);
    }

    [Fact]
    public void GetTokens_UnDoubleQuotesString_ReturnsToken()
    {
        string[] elements = [@"""foo \""bar\\"""];
        var tokens = _lexer.GetTokens(elements);
        Assert.Single(tokens);
        Assert.Equal("literal", tokens[0].Type);
        Assert.Equal(@"foo ""bar\", tokens[0].Value);
        Assert.Equal(@"""foo \""bar\\""", tokens[0].Raw);
    }

    [Fact]
    public void GetTokens_UnSingleQuotesString_ReturnsToken()
    {
        string[] elements = [@"'foo \'bar\\'"];
        var tokens = _lexer.GetTokens(elements);
        Assert.Single(tokens);
        Assert.Equal("literal", tokens[0].Type);
        Assert.Equal(@"foo 'bar\", tokens[0].Value);
        Assert.Equal(@"'foo \'bar\\'", tokens[0].Raw);
    }
}