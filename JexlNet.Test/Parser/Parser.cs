namespace JexlNet.Test;

public class ParserUnitTest
{
    private readonly Lexer _lexer = new(new Grammar());
    private readonly Parser _parser = new(new Grammar());

    [Fact]
    public void ConstructsASTForOnePlusTwo()
    {
        _parser.AddTokens(_lexer.Tokenize("1 + 2"));
        var result = _parser.Complete();
        Node expected = new("BinaryExpression")
        {
            Operator = "+",
            Left = new("Literal", (decimal)1),
            Right = new("Literal", (decimal)2)
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AddsHeavierOperationsToTheRight()
    {
        _parser.AddTokens(_lexer.Tokenize("2+3*4"));
        var result = _parser.Complete();
        Node expected = new("BinaryExpression")
        {
            Operator = "+",
            Left = new("Literal", (decimal)2),
            Right = new("BinaryExpression")
            {
                Operator = "*",
                Left = new("Literal", (decimal)3),
                Right = new("Literal", (decimal)4)
            },
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EncapsulatesLighterOperations()
    {
        _parser.AddTokens(_lexer.Tokenize("2*3+4"));
        var result = _parser.Complete();
        Node expected = new("BinaryExpression")
        {
            Operator = "+",
            Left = new("BinaryExpression")
            {
                Operator = "*",
                Left = new("Literal", (decimal)2),
                Right = new("Literal", (decimal)3)
            },
            Right = new("Literal", (decimal)4)
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesSubtreeEncapsulation()
    {
        _parser.AddTokens(_lexer.Tokenize("2+3*4==5/6-7"));
        var result = _parser.Complete();
        Node expected = new("BinaryExpression")
        {
            Operator = "==",
            Left = new("BinaryExpression")
            {
                Operator = "+",
                Left = new("Literal", (decimal)2),
                Right = new("BinaryExpression")
                {
                    Operator = "*",
                    Left = new("Literal", (decimal)3),
                    Right = new("Literal", (decimal)4)
                },
            },
            Right = new("BinaryExpression")
            {
                Operator = "-",
                Left = new("BinaryExpression")
                {
                    Operator = "/",
                    Left = new("Literal", (decimal)5),
                    Right = new("Literal", (decimal)6)
                },
                Right = new("Literal", (decimal)7)
            }
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesUnaryOperator()
    {
        _parser.AddTokens(_lexer.Tokenize("1*!!true-2"));
        var result = _parser.Complete();
        Node expected = new("BinaryExpression")
        {
            Operator = "-",
            Left = new("BinaryExpression")
            {
                Operator = "*",
                Left = new("Literal", (decimal)1),
                Right = new("UnaryExpression")
                {
                    Operator = "!",
                    Right = new("UnaryExpression")
                    {
                        Operator = "!",
                        Right = new("Literal", true)
                    }
                }
            },
            Right = new("Literal", (decimal)2)
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesSubexpression()
    {
        _parser.AddTokens(_lexer.Tokenize("(2+3)*4"));
        var result = _parser.Complete();
        Node expected = new("BinaryExpression")
        {
            Operator = "*",
            Left = new("BinaryExpression")
            {
                Operator = "+",
                Left = new("Literal", (decimal)2),
                Right = new("Literal", (decimal)3)
            },
            Right = new("Literal", (decimal)4)
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesNestedSubexpressions()
    {
        _parser.AddTokens(_lexer.Tokenize("(4*(2+3))/5"));
        var result = _parser.Complete();
        Node expected = new("BinaryExpression")
        {
            Operator = "/",
            Left = new("BinaryExpression")
            {
                Operator = "*",
                Left = new("Literal", (decimal)4),
                Right = new("BinaryExpression")
                {
                    Operator = "+",
                    Left = new("Literal", (decimal)2),
                    Right = new("Literal", (decimal)3)
                }
            },
            Right = new("Literal", (decimal)5)
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesObjectLiterals()
    {
        _parser.AddTokens(_lexer.Tokenize(@"{foo: ""bar"", tek: 1+2}"));
        var result = _parser.Complete();
        Node expected = new("ObjectLiteral")
        {
            Value = new Dictionary<string, Node>
            {
                { "foo", new("Literal", "bar") },
                { "tek", new("BinaryExpression")
                    {
                        Operator = "+",
                        Left = new("Literal", (decimal)1),
                        Right = new("Literal", (decimal)2)
                    }
                }
            }
        };
        Assert.Equal(expected, result);
    }

}