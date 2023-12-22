using System.Security.Cryptography.X509Certificates;

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

    [Fact]
    public void HandlesDashesInKey()
    {
        _parser.AddTokens(_lexer.Tokenize(@"{'with-dash': ""bar"", tek: 1+2}"));
        var result = _parser.Complete();
        Node expected = new("ObjectLiteral")
        {
            Value = new Dictionary<string, Node>
            {
                { "with-dash", new("Literal", "bar") },
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

    [Fact]
    public void HandlesNestedObjectLiterals()
    {
        _parser.AddTokens(_lexer.Tokenize(@"{foo: {bar: ""tek""}}"));
        var result = _parser.Complete();
        Node expected = new("ObjectLiteral")
        {
            Value = new Dictionary<string, Node>
            {
                { "foo", new("ObjectLiteral")
                    {
                        Value = new Dictionary<string, Node>
                        {
                            { "bar", new("Literal", "tek") }
                        }
                    }
                }
            }
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesEmptyObjectLiterals()
    {
        _parser.AddTokens(_lexer.Tokenize(@"{}"));
        var result = _parser.Complete();
        Node expected = new("ObjectLiteral")
        {
            Value = new Dictionary<string, Node>()
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesArrayLiterals()
    {
        _parser.AddTokens(_lexer.Tokenize(@"[""foo"", 1+2]"));
        var result = _parser.Complete();
        Node expected = new("ArrayLiteral")
        {
            Value = new List<Node>
            {
                new("Literal", "foo"),
                new("BinaryExpression")
                {
                    Operator = "+",
                    Left = new("Literal", (decimal)1),
                    Right = new("Literal", (decimal)2)
                }
            }
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesNestedArrayLiterals()
    {
        _parser.AddTokens(_lexer.Tokenize(@"[""foo"", [""bar"", ""tek""]]"));
        var result = _parser.Complete();
        Node expected = new("ArrayLiteral")
        {
            Value = new List<Node>
            {
                new("Literal", "foo"),
                new("ArrayLiteral")
                {
                    Value = new List<Node>
                    {
                        new("Literal", "bar"),
                        new("Literal", "tek")
                    }
                }
            }
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesEmptyArrayLiterals()
    {
        _parser.AddTokens(_lexer.Tokenize(@"[]"));
        var result = _parser.Complete();
        Node expected = new("ArrayLiteral")
        {
            Value = new List<Node>()
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ChainsTraversedIdentifiers()
    {
        _parser.AddTokens(_lexer.Tokenize(@"foo.bar.baz + 1"));
        var result = _parser.Complete();
        Node expected = new("BinaryExpression")
        {
            Operator = "+",
            Left = new("Identifier", "baz")
            {
                From = new("Identifier", "bar")
                {
                    From = new("Identifier", "foo")
                }
            },
            Right = new("Literal", (decimal)1)
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AppliesTransformsAndArguments()
    {
        _parser.AddTokens(_lexer.Tokenize(@"foo|tr1|tr2.baz|tr3({bar:""tek""})"));
        var result = _parser.Complete();
        Node expected = new("FunctionCall")
        {
            Name = "tr3",
            Pool = "transforms",
            Args =
            [
                new("Identifier", "baz")
                {
                    From = new("FunctionCall")
                    {
                        Name = "tr2",
                        Pool = "transforms",
                        Args =
                        [
                            new("FunctionCall")
                            {
                                Name = "tr1",
                                Pool = "transforms",
                                Args =
                                [
                                    new("Identifier","foo")
                                ]
                            }
                        ]
                    }
                },
                new("ObjectLiteral")
                {
                    Value = new Dictionary<string, Node>
                    {
                        { "bar", new("Literal", "tek") }
                    }
                }
            ]
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesMultipleArgumentsInTransforms()
    {
        _parser.AddTokens(_lexer.Tokenize(@"foo|bar(""tek"", 5, true)"));
        var result = _parser.Complete();
        Node expected = new("FunctionCall")
        {
            Name = "bar",
            Pool = "transforms",
            Args =
            [
                new("Identifier", "foo"),
                new("Literal", "tek"),
                new("Literal", (decimal)5),
                new("Literal", true)
            ]
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AppliesFiltersToIdentifiers()
    {
        _parser.AddTokens(_lexer.Tokenize(@"foo[1][.bar[0]==""tek""].baz"));
        var result = _parser.Complete();
        Node expected = new("Identifier", "baz")
        {
            From = new("FilterExpression")
            {
                Relative = true,
                Expr = new("BinaryExpression")
                {
                    Operator = "==",
                    Left = new("FilterExpression")
                    {
                        Relative = false,
                        Expr = new("Literal", 0),
                        Subject = new("Identifier", "bar")
                        {
                            Relative = true,
                        }
                    },
                    Right = new("Literal", "tek")
                },
            },
            Subject = new("FilterExpression")
            {
                Relative = true,
                Expr = new("Literal", (decimal)1),
                Subject = new("Identifier", "foo")
            }
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AllowsMixingRelativeAndNonRelativeIdentifiers()
    {
        _parser.AddTokens(_lexer.Tokenize(@"foo[.bar.baz == tek]"));
        var result = _parser.Complete();
        Node expected = new("FilterExpression")
        {
            Relative = true,
            Expr = new("BinaryExpression")
            {
                Operator = "==",
                Left = new("Identifier", "baz")
                {
                    From = new("Identifier", "bar")
                    {
                        Relative = true,
                    }
                },
                Right = new("Identifier", "tek")
            },
            Subject = new("Identifier", "foo")
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AllowsMixingRelativeAndNonRelativeComplex()
    {
        _parser.AddTokens(_lexer.Tokenize(@"foo.bar[.baz == tek.tak]"));
        var result = _parser.Complete();
        Node expected = new("FilterExpression")
        {
            Relative = true,
            Expr = new("BinaryExpression")
            {
                Operator = "==",
                Left = new("Identifier", "baz")
                {
                    Relative = true,
                },
                Right = new("Identifier", "tak")
                {
                    From = new("Identifier", "tek")
                }
            },
            Subject = new("Identifier", "bar")
            {
                From = new("Identifier", "foo")
            }
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AllowsDotNotationForAllOperands()
    {
        _parser.AddTokens(_lexer.Tokenize(@"""foo"".length + {foo: ""bar""}.foo"));
        var result = _parser.Complete();
        Node expected = new("BinaryExpression")
        {
            Operator = "+",
            Left = new("Identifier", "length")
            {
                From = new("Literal", "foo")
            },
            Right = new("Identifier", "foo")
            {
                From = new("ObjectLiteral")
                {
                    Value = new Dictionary<string, Node>
                    {
                        { "foo", new("Literal", "bar") }
                    }
                }
            }
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AllowsDotNotationOnSubexpressions()
    {
        _parser.AddTokens(_lexer.Tokenize(@"(""foo"" + ""bar"").length"));
        var result = _parser.Complete();
        Node expected = new("Identifier", "length")
        {
            From = new("BinaryExpression")
            {
                Operator = "+",
                Left = new("Literal", "foo"),
                Right = new("Literal", "bar")
            }
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AllowsDotNotationOnArrays()
    {
        _parser.AddTokens(_lexer.Tokenize(@"[""foo"", ""bar""].length"));
        var result = _parser.Complete();
        Node expected = new("Identifier", "length")
        {
            From = new("ArrayLiteral")
            {
                Value = new List<Node>
                {
                    new("Literal", "foo"),
                    new("Literal", "bar")
                }
            }
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesTernaryExpression()
    {
        _parser.AddTokens(_lexer.Tokenize(@"foo ? 1 : 0"));
        var result = _parser.Complete();
        Node expected = new("ConditionalExpression")
        {
            Test = new("Identifier", "foo"),
            Consequent = new("Literal", (decimal)1),
            Alternate = new("Literal", (decimal)0)
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesNestedAndGroupedTernaryExpressions()
    {
        _parser.AddTokens(_lexer.Tokenize(@"foo ? (bar ? 1 : 2) : 3"));
        var result = _parser.Complete();
        Node expected = new("ConditionalExpression")
        {
            Test = new("Identifier", "foo"),
            Consequent = new("ConditionalExpression")
            {
                Test = new("Identifier", "bar"),
                Consequent = new("Literal", (decimal)1),
                Alternate = new("Literal", (decimal)2)
            },
            Alternate = new("Literal", (decimal)3)
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesNestedNonGroupedTernaryExpressions()
    {
        _parser.AddTokens(_lexer.Tokenize(@"foo ? bar ? 1 : 2 : 3"));
        var result = _parser.Complete();
        Node expected = new("ConditionalExpression")
        {
            Test = new("Identifier", "foo"),
            Consequent = new("ConditionalExpression")
            {
                Test = new("Identifier", "bar"),
                Consequent = new("Literal", (decimal)1),
                Alternate = new("Literal", (decimal)2)
            },
            Alternate = new("Literal", (decimal)3)
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesTernaryExpressionWithObjects()
    {
        _parser.AddTokens(_lexer.Tokenize(@"foo ? {bar: ""tek""} : ""baz"""));
        var result = _parser.Complete();
        Node expected = new("ConditionalExpression")
        {
            Test = new("Identifier", "foo"),
            Consequent = new("ObjectLiteral")
            {
                Value = new Dictionary<string, Node>
                {
                    { "bar", new("Literal", "tek") }
                }
            },
            Alternate = new("Literal", "baz")
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void BalancesBinaryBetweenComplexIdentifiers()
    {
        _parser.AddTokens(_lexer.Tokenize(@"a.b == c.d"));
        var result = _parser.Complete();
        Node expected = new("BinaryExpression")
        {
            Operator = "==",
            Left = new("Identifier", "b")
            {
                From = new("Identifier", "a")
            },
            Right = new("Identifier", "d")
            {
                From = new("Identifier", "c")
            }
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesWhiteSpaceInExpression()
    {
        _parser.AddTokens(_lexer.Tokenize("\t2\r\n+\n\r3\n\n"));
        var result = _parser.Complete();
        Node expected = new("BinaryExpression")
        {
            Operator = "+",
            Left = new("Literal", (decimal)2),
            Right = new("Literal", (decimal)3)
        };
        Assert.Equal(expected, result);
    }
}