using System.Text.Json.Nodes;

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
        var expected = new Token(GrammarType.Literal, @"""foo \""bar\\""", JsonValue.Create(@"foo ""bar\"));
        Assert.Equal(expected, tokens[0]);
    }

    [Fact]
    public void GetTokens_UnSingleQuotesString_ReturnsToken()
    {
        List<string> elements = [@"'foo \'bar\\'"];
        var tokens = _lexer.GetTokens(elements);
        Assert.Single(tokens);
        Assert.Equal(new Token(GrammarType.Literal, @"'foo \'bar\\'", JsonValue.Create(@"foo 'bar\")), tokens[0]);
    }

    [Fact]
    public void GetTokens_UnSingleQuotesString_ReturnsTokenComplex()
    {
        List<string> elements = [@"""Hello8Wo\""rld"""];
        var tokens = _lexer.GetTokens(elements);
        Assert.Single(tokens);
        Assert.Equal(new Token(GrammarType.Literal, @"""Hello8Wo\""rld""", JsonValue.Create(@"Hello8Wo""rld")), tokens[0]);
    }

    [Fact]
    public void GetTokens_RecognizesBooleans()
    {
        List<string> elements = ["true", "false"];
        var tokens = _lexer.GetTokens(elements);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(new Token(GrammarType.Literal, "true", JsonValue.Create(true)), tokens[0]);
        Assert.Equal(new Token(GrammarType.Literal, "false", JsonValue.Create(false)), tokens[1]);
    }

    [Fact]
    public void GetTokens_RecognizesNumerics()
    {
        List<string> elements = ["-7.6", "20"];
        var tokens = _lexer.GetTokens(elements);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(new Token(GrammarType.Literal, "-7.6", JsonValue.Create((decimal)-7.6)), tokens[0]);
        Assert.Equal(new Token(GrammarType.Literal, "20", JsonValue.Create((decimal)20)), tokens[1]);
    }

    [Fact]
    public void GetTokens_RecognizesBinaryOp()
    {
        List<string> elements = ["+"];
        var tokens = _lexer.GetTokens(elements);
        Assert.Single(tokens);
        Assert.Equal(new Token(GrammarType.BinaryOperator, "+", JsonValue.Create("+")), tokens[0]);
    }

    [Fact]
    public void GetTokens_RecognizesUnaryOp()
    {
        List<string> elements = ["!"];
        var tokens = _lexer.GetTokens(elements);
        Assert.Single(tokens);
        Assert.Equal(new Token(GrammarType.UnaryOperator, "!", JsonValue.Create("!")), tokens[0]);
    }

    [Fact]
    public void GetTokens_RecognizesControlCharacters()
    {
        List<string> elements = ["(", ")"];
        var tokens = _lexer.GetTokens(elements);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(new Token(GrammarType.OpenParen, "(", JsonValue.Create("(")), tokens[0]);
        Assert.Equal(new Token(GrammarType.CloseParen, ")", JsonValue.Create(")")), tokens[1]);
    }

    [Fact]
    public void GetTokens_RecognizesIdentifiers()
    {
        List<string> elements = ["_foo9_bar"];
        var tokens = _lexer.GetTokens(elements);
        Assert.Single(tokens);
        Assert.Equal(new Token(GrammarType.Identifier, "_foo9_bar", JsonValue.Create("_foo9_bar")), tokens[0]);
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
        Assert.Equal(new Token(GrammarType.Literal, "6", JsonValue.Create((decimal)6)), tokens[0]);
        Assert.Equal(new Token(GrammarType.BinaryOperator, "+", JsonValue.Create("+")), tokens[1]);
        Assert.Equal(new Token(GrammarType.Identifier, "x ", JsonValue.Create("x")), tokens[2]);
        Assert.Equal(new Token(GrammarType.BinaryOperator, "-  ", JsonValue.Create("-")), tokens[3]);
        Assert.Equal(new Token(GrammarType.Literal, "-17.55", JsonValue.Create((decimal)-17.55)), tokens[4]);
        Assert.Equal(new Token(GrammarType.BinaryOperator, "*", JsonValue.Create("*")), tokens[5]);
        Assert.Equal(new Token(GrammarType.Identifier, "y", JsonValue.Create("y")), tokens[6]);
        Assert.Equal(new Token(GrammarType.BinaryOperator, "<= ", JsonValue.Create("<=")), tokens[7]);
        Assert.Equal(new Token(GrammarType.UnaryOperator, "!", JsonValue.Create("!")), tokens[8]);
        Assert.Equal(new Token(GrammarType.Identifier, "foo", JsonValue.Create("foo")), tokens[9]);
        Assert.Equal(new Token(GrammarType.Dot, ".", JsonValue.Create(".")), tokens[10]);
        Assert.Equal(new Token(GrammarType.Identifier, "bar", JsonValue.Create("bar")), tokens[11]);
        Assert.Equal(new Token(GrammarType.OpenBracket, "[", JsonValue.Create("[")), tokens[12]);
        Assert.Equal(new Token(GrammarType.Literal, @"""baz\""foz""", JsonValue.Create(@"baz""foz")), tokens[13]);
        Assert.Equal(new Token(GrammarType.CloseBracket, "]", JsonValue.Create("]")), tokens[14]);
    }

    [Fact]
    public void Tokenize_ConsidersMinusNegative()
    {
        var tokens = _lexer.Tokenize(@"-1?-2:-3");
        // Assert.Equal(5, tokens.Count);
        Assert.Equal(new Token(GrammarType.Literal, "-1", JsonValue.Create((decimal)-1)), tokens[0]);
        Assert.Equal(new Token(GrammarType.Question, "?", JsonValue.Create("?")), tokens[1]);
        Assert.Equal(new Token(GrammarType.Literal, "-2", JsonValue.Create((decimal)-2)), tokens[2]);
        Assert.Equal(new Token(GrammarType.Colon, ":", JsonValue.Create(":")), tokens[3]);
        Assert.Equal(new Token(GrammarType.Literal, "-3", JsonValue.Create((decimal)-3)), tokens[4]);
    }


}