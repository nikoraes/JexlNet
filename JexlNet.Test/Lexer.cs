namespace JexlNet.Test;

public class LexerUnitTest
{
    private readonly Lexer _lexer = new(new Grammar());

    [Theory]
    [InlineData(@"""foo""")]
    [InlineData(@"'foo'")]
    [InlineData(@"""f\""oo""")]
    [InlineData(@"""foo\""""")]
    [InlineData(@"'foo\''")]
    [InlineData(@"'f\'oo'")]
    [InlineData(@"""Wo\""rld""")]
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

    [Theory]
    [InlineData(@"+""Wo\""rld""", 2)]
    public void GetElements_Multiple(string input, int count)
    {
        var elements = _lexer.GetElements(input);
        Assert.Equal(count, elements.Count);
    }

    [Fact]
    public void GetTokens_UnDoubleQuotesString_ReturnsToken()
    {
        List<string> elements = [@"""foo \""bar\\"""];
        var tokens = _lexer.GetTokens(elements);
        Assert.Single(tokens);
        Assert.Equal(new Token("literal", @"foo ""bar\", @"""foo \""bar\\"""), tokens[0]);
    }

    [Fact]
    public void GetTokens_UnSingleQuotesString_ReturnsToken()
    {
        List<string> elements = [@"'foo \'bar\\'"];
        var tokens = _lexer.GetTokens(elements);
        Assert.Single(tokens);
        Assert.Equal(new Token("literal", @"foo 'bar\", @"'foo \'bar\\'"), tokens[0]);
    }

    [Fact]
    public void GetTokens_UnSingleQuotesString_ReturnsTokenComplex()
    {
        List<string> elements = [@"""Hello8Wo\""rld"""];
        var tokens = _lexer.GetTokens(elements);
        Assert.Single(tokens);
        Assert.Equal(new Token("literal", @"Hello8Wo""rld", @"""Hello8Wo\""rld"""), tokens[0]);
    }

    [Fact]
    public void GetTokens_RecognizesBooleans()
    {
        List<string> elements = ["true", "false"];
        var tokens = _lexer.GetTokens(elements);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(new Token("literal", true, "true"), tokens[0]);
        Assert.Equal(new Token("literal", false, "false"), tokens[1]);
    }

    [Fact]
    public void GetTokens_RecognizesNumerics()
    {
        List<string> elements = ["-7.6", "20"];
        var tokens = _lexer.GetTokens(elements);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(new Token("literal", (decimal)-7.6, "-7.6"), tokens[0]);
        Assert.Equal(new Token("literal", (decimal)20, "20"), tokens[1]);
    }

    [Fact]
    public void GetTokens_RecognizesBinaryOp()
    {
        List<string> elements = ["+"];
        var tokens = _lexer.GetTokens(elements);
        Assert.Single(tokens);
        Assert.Equal(new Token("binaryOp", "+", "+"), tokens[0]);
    }

    [Fact]
    public void GetTokens_RecognizesUnaryOp()
    {
        List<string> elements = ["!"];
        var tokens = _lexer.GetTokens(elements);
        Assert.Single(tokens);
        Assert.Equal(new Token("unaryOp", "!", "!"), tokens[0]);
    }

    [Fact]
    public void GetTokens_RecognizesControlCharacters()
    {
        List<string> elements = ["(", ")"];
        var tokens = _lexer.GetTokens(elements);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(new Token("openParen", "(", "("), tokens[0]);
        Assert.Equal(new Token("closeParen", ")", ")"), tokens[1]);
    }

    [Fact]
    public void GetTokens_RecognizesIdentifiers()
    {
        List<string> elements = ["_foo9_bar"];
        var tokens = _lexer.GetTokens(elements);
        Assert.Single(tokens);
        Assert.Equal(new Token("identifier", "_foo9_bar", "_foo9_bar"), tokens[0]);
    }

    [Fact]
    public void GetTokens_ThrowsExceptionOnInvalidToken()
    {
        Assert.Throws<Exception>(() => _lexer.GetTokens(["9foo"]));
    }

    [Fact]
    public void Tokenize_FullExpression()
    {
        var tokens = _lexer.Tokenize(@"6+x -  -17.55*y<= !foo.bar[""baz\""foz""]");
        Assert.Equal(15, tokens.Count);
        Assert.Equal(new Token("literal", (decimal)6, "6"), tokens[0]);
        Assert.Equal(new Token("binaryOp", "+", "+"), tokens[1]);
        Assert.Equal(new Token("identifier", "x", "x "), tokens[2]);
        Assert.Equal(new Token("binaryOp", "-", "-  "), tokens[3]);
        Assert.Equal(new Token("literal", (decimal)-17.55, "-17.55"), tokens[4]);
        Assert.Equal(new Token("binaryOp", "*", "*"), tokens[5]);
        Assert.Equal(new Token("identifier", "y", "y"), tokens[6]);
        Assert.Equal(new Token("binaryOp", "<=", "<= "), tokens[7]);
        Assert.Equal(new Token("unaryOp", "!", "!"), tokens[8]);
        Assert.Equal(new Token("identifier", "foo", "foo"), tokens[9]);
        Assert.Equal(new Token("dot", ".", "."), tokens[10]);
        Assert.Equal(new Token("identifier", "bar", "bar"), tokens[11]);
        Assert.Equal(new Token("openBracket", "[", "["), tokens[12]);
        Assert.Equal(new Token("literal", @"baz""foz", @"""baz\""foz"""), tokens[13]);
        Assert.Equal(new Token("closeBracket", "]", "]"), tokens[14]);
    }

    [Fact]
    public void Tokenize_ConsidersMinusNegative()
    {
        var tokens = _lexer.Tokenize(@"-1?-2:-3");
        // Assert.Equal(5, tokens.Count);
        Assert.Equal(new Token("literal", (decimal)-1, "-1"), tokens[0]);
        Assert.Equal(new Token("question", "?", "?"), tokens[1]);
        Assert.Equal(new Token("literal", (decimal)-2, "-2"), tokens[2]);
        Assert.Equal(new Token("colon", ":", ":"), tokens[3]);
        Assert.Equal(new Token("literal", (decimal)-3, "-3"), tokens[4]);
    }


}