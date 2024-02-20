using System.Text.Json.Nodes;

namespace JexlNet.Test;

public class ParserUnitTest
{
    private readonly Lexer _lexer = new(new Grammar());

    [Fact]
    public void ConstructsASTForOnePlusTwo()
    {
        Parser _parser = new(new Grammar());
        var tokens = _lexer.Tokenize("1 + 2");
        _parser.AddTokens(tokens);
        var result = _parser.Complete();
        Node expected = new(GrammarType.BinaryExpression)
        {
            Operator = "+",
            Left = new(GrammarType.Literal, JsonValue.Create((decimal)1)),
            Right = new(GrammarType.Literal, JsonValue.Create((decimal)2))
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ConstructsASTForMinusOnePlusTwo()
    {
        Parser _parser = new(new Grammar());
        var tokens = _lexer.Tokenize("-1 + 2");
        _parser.AddTokens(tokens);
        var result = _parser.Complete();
        Node expected = new(GrammarType.BinaryExpression)
        {
            Operator = "+",
            Left = new(GrammarType.Literal, JsonValue.Create((decimal)-1)),
            Right = new(GrammarType.Literal, JsonValue.Create((decimal)2))
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AddsHeavierOperationsToTheRight()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize("2+3*4"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.BinaryExpression)
        {
            Operator = "+",
            Left = new(GrammarType.Literal, (decimal)2),
            Right = new(GrammarType.BinaryExpression)
            {
                Operator = "*",
                Left = new(GrammarType.Literal, (decimal)3),
                Right = new(GrammarType.Literal, (decimal)4)
            },
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EncapsulatesLighterOperations()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize("2*3+4"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.BinaryExpression)
        {
            Operator = "+",
            Left = new(GrammarType.BinaryExpression)
            {
                Operator = "*",
                Left = new(GrammarType.Literal, (decimal)2),
                Right = new(GrammarType.Literal, (decimal)3)
            },
            Right = new(GrammarType.Literal, (decimal)4)
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesSubtreeEncapsulation()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize("2+3*4==5/6-7"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.BinaryExpression)
        {
            Operator = "==",
            Left = new(GrammarType.BinaryExpression)
            {
                Operator = "+",
                Left = new(GrammarType.Literal, (decimal)2),
                Right = new(GrammarType.BinaryExpression)
                {
                    Operator = "*",
                    Left = new(GrammarType.Literal, (decimal)3),
                    Right = new(GrammarType.Literal, (decimal)4)
                },
            },
            Right = new(GrammarType.BinaryExpression)
            {
                Operator = "-",
                Left = new(GrammarType.BinaryExpression)
                {
                    Operator = "/",
                    Left = new(GrammarType.Literal, (decimal)5),
                    Right = new(GrammarType.Literal, (decimal)6)
                },
                Right = new(GrammarType.Literal, (decimal)7)
            }
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesUnaryOperator()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize("1*!!true-2"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.BinaryExpression)
        {
            Operator = "-",
            Left = new(GrammarType.BinaryExpression)
            {
                Operator = "*",
                Left = new(GrammarType.Literal, (decimal)1),
                Right = new(GrammarType.UnaryExpression)
                {
                    Operator = "!",
                    Right = new(GrammarType.UnaryExpression)
                    {
                        Operator = "!",
                        Right = new(GrammarType.Literal, true)
                    }
                }
            },
            Right = new(GrammarType.Literal, (decimal)2)
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesSubexpression()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize("(2+3)*4"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.BinaryExpression)
        {
            Operator = "*",
            Left = new(GrammarType.BinaryExpression)
            {
                Operator = "+",
                Left = new(GrammarType.Literal, (decimal)2),
                Right = new(GrammarType.Literal, (decimal)3)
            },
            Right = new(GrammarType.Literal, (decimal)4)
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesNestedSubexpressions()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize("(4*(2+3))/5"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.BinaryExpression)
        {
            Operator = "/",
            Left = new(GrammarType.BinaryExpression)
            {
                Operator = "*",
                Left = new(GrammarType.Literal, (decimal)4),
                Right = new(GrammarType.BinaryExpression)
                {
                    Operator = "+",
                    Left = new(GrammarType.Literal, (decimal)2),
                    Right = new(GrammarType.Literal, (decimal)3)
                }
            },
            Right = new(GrammarType.Literal, (decimal)5)
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesObjectLiterals()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize(@"{foo: ""bar"", tek: 1+2}"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.ObjectLiteral)
        {
            Object = new Dictionary<string, Node>
            {
                { "foo", new(GrammarType.Literal, "bar") },
                { "tek", new(GrammarType.BinaryExpression)
                    {
                        Operator = "+",
                        Left = new(GrammarType.Literal, (decimal)1),
                        Right = new(GrammarType.Literal, (decimal)2)
                    }
                }
            }
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesDashesInKey()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize(@"{'with-dash': ""bar"", tek: 1+2}"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.ObjectLiteral)
        {
            Object = new Dictionary<string, Node>
            {
                { "with-dash", new(GrammarType.Literal, "bar") },
                { "tek", new(GrammarType.BinaryExpression)
                    {
                        Operator = "+",
                        Left = new(GrammarType.Literal, (decimal)1),
                        Right = new(GrammarType.Literal, (decimal)2)
                    }
                }
            }
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesNestedObjectLiterals()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize(@"{foo: {bar: ""tek""}}"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.ObjectLiteral)
        {
            Object = new Dictionary<string, Node>
            {
                { "foo", new(GrammarType.ObjectLiteral)
                    {
                        Object = new Dictionary<string, Node>
                        {
                            { "bar", new(GrammarType.Literal, "tek") }
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
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize(@"{}"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.ObjectLiteral)
        {
            Object = new Dictionary<string, Node>()
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesArrayLiterals()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize(@"[""foo"", 1+2]"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.ArrayLiteral)
        {
            Array = new List<Node>
            {
                new(GrammarType.Literal, "foo"),
                new(GrammarType.BinaryExpression)
                {
                    Operator = "+",
                    Left = new(GrammarType.Literal, (decimal)1),
                    Right = new(GrammarType.Literal, (decimal)2)
                }
            }
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesNestedArrayLiterals()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize(@"[""foo"", [""bar"", ""tek""]]"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.ArrayLiteral)
        {
            Array = new List<Node>
            {
                new(GrammarType.Literal, "foo"),
                new(GrammarType.ArrayLiteral)
                {
                    Array = new List<Node>
                    {
                        new(GrammarType.Literal, "bar"),
                        new(GrammarType.Literal, "tek")
                    }
                }
            }
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesEmptyArrayLiterals()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize(@"[]"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.ArrayLiteral)
        {
            Array = new List<Node>()
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ChainsTraversedIdentifiers()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize(@"foo.bar.baz + 1"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.BinaryExpression)
        {
            Operator = "+",
            Left = new(GrammarType.Identifier, "baz")
            {
                From = new(GrammarType.Identifier, "bar")
                {
                    From = new(GrammarType.Identifier, "foo")
                }
            },
            Right = new(GrammarType.Literal, (decimal)1)
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AppliesTransformsAndArguments()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize(@"foo|tr1|tr2.baz|tr3({bar:""tek""})"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.FunctionCall)
        {
            Name = "tr3",
            Pool = Grammar.PoolType.Transforms,
            Args =
            [
                new(GrammarType.Identifier, "baz")
                {
                    From = new(GrammarType.FunctionCall)
                    {
                        Name = "tr2",
                        Pool = Grammar.PoolType.Transforms,
                        Args =
                        [
                            new(GrammarType.FunctionCall)
                            {
                                Name = "tr1",
                                Pool = Grammar.PoolType.Transforms,
                                Args =
                                [
                                    new(GrammarType.Identifier,"foo")
                                ]
                            }
                        ]
                    }
                },
                new(GrammarType.ObjectLiteral)
                {
                    Object = new Dictionary<string, Node>
                    {
                        { "bar", new(GrammarType.Literal, "tek") }
                    }
                }
            ]
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesMultipleArgumentsInTransforms()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize(@"foo|bar(""tek"", -5, true)"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.FunctionCall)
        {
            Name = "bar",
            Pool = Grammar.PoolType.Transforms,
            Args =
            [
                new(GrammarType.Identifier, "foo"),
                new(GrammarType.Literal, "tek"),
                new(GrammarType.Literal, (decimal)-5),
                new(GrammarType.Literal, true)
            ]
        };
        Assert.Equal(expected, result);
    }


    [Fact]
    public void AppliesFiltersToIdentifiers()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize(@"foo[1][.bar[0]==""tek""].baz"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.Identifier, "baz")
        {
            From = new(GrammarType.FilterExpression)
            {
                Relative = true,
                Expr = new(GrammarType.BinaryExpression)
                {
                    Operator = "==",
                    Left = new(GrammarType.FilterExpression)
                    {
                        Relative = false,
                        Expr = new(GrammarType.Literal, 0),
                        Subject = new(GrammarType.Identifier, "bar")
                        {
                            Relative = true,
                        }
                    },
                    Right = new(GrammarType.Literal, "tek")
                },
            },
            Subject = new(GrammarType.FilterExpression)
            {
                Relative = true,
                Expr = new(GrammarType.Literal, (decimal)1),
                Subject = new(GrammarType.Identifier, "foo")
            }
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AllowsMixingRelativeAndNonRelativeIdentifiers()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize(@"foo[.bar.baz == tek]"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.FilterExpression)
        {
            Relative = true,
            Expr = new(GrammarType.BinaryExpression)
            {
                Operator = "==",
                Left = new(GrammarType.Identifier, "baz")
                {
                    From = new(GrammarType.Identifier, "bar")
                    {
                        Relative = true,
                    }
                },
                Right = new(GrammarType.Identifier, "tek")
            },
            Subject = new(GrammarType.Identifier, "foo")
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AllowsMixingRelativeAndNonRelativeComplex()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize(@"foo.bar[.baz == tek.tak]"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.FilterExpression)
        {
            Relative = true,
            Expr = new(GrammarType.BinaryExpression)
            {
                Operator = "==",
                Left = new(GrammarType.Identifier, "baz")
                {
                    Relative = true,
                },
                Right = new(GrammarType.Identifier, "tak")
                {
                    From = new(GrammarType.Identifier, "tek")
                }
            },
            Subject = new(GrammarType.Identifier, "bar")
            {
                From = new(GrammarType.Identifier, "foo")
            }
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AllowsDotNotationForAllOperands()
    {
        Parser _parser = new(new Grammar());
        var tokens = _lexer.Tokenize(@"""foo"".length + {foo: ""bar""}.foo");
        _parser.AddTokens(tokens);
        var result = _parser.Complete();
        Node expected = new(GrammarType.BinaryExpression)
        {
            Operator = "+",
            Left = new(GrammarType.Identifier, "length")
            {
                From = new(GrammarType.Literal, "foo")
            },
            Right = new(GrammarType.Identifier, "foo")
            {
                From = new(GrammarType.ObjectLiteral)
                {
                    Object = new Dictionary<string, Node>
                    {
                        { "foo", new(GrammarType.Literal, "bar") }
                    }
                }
            }
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AllowsDotNotationOnSubexpressions()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize(@"(""foo"" + ""bar"").length"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.Identifier, "length")
        {
            From = new(GrammarType.BinaryExpression)
            {
                Operator = "+",
                Left = new(GrammarType.Literal, "foo"),
                Right = new(GrammarType.Literal, "bar")
            }
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AllowsDotNotationOnArrays()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize(@"[""foo"", ""bar""].length"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.Identifier, "length")
        {
            From = new(GrammarType.ArrayLiteral)
            {
                Array = new List<Node>
                {
                    new(GrammarType.Literal, "foo"),
                    new(GrammarType.Literal, "bar")
                }
            }
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesTernaryExpression()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize(@"foo ? 1 : 0"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.ConditionalExpression)
        {
            Test = new(GrammarType.Identifier, "foo"),
            Consequent = new(GrammarType.Literal, (decimal)1),
            Alternate = new(GrammarType.Literal, (decimal)0)
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesNestedAndGroupedTernaryExpressions()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize(@"foo ? (bar ? 1 : 2) : 3"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.ConditionalExpression)
        {
            Test = new(GrammarType.Identifier, "foo"),
            Consequent = new(GrammarType.ConditionalExpression)
            {
                Test = new(GrammarType.Identifier, "bar"),
                Consequent = new(GrammarType.Literal, (decimal)1),
                Alternate = new(GrammarType.Literal, (decimal)2)
            },
            Alternate = new(GrammarType.Literal, (decimal)3)
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesNestedNonGroupedTernaryExpressions()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize(@"foo ? bar ? 1 : 2 : 3"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.ConditionalExpression)
        {
            Test = new(GrammarType.Identifier, "foo"),
            Consequent = new(GrammarType.ConditionalExpression)
            {
                Test = new(GrammarType.Identifier, "bar"),
                Consequent = new(GrammarType.Literal, (decimal)1),
                Alternate = new(GrammarType.Literal, (decimal)2)
            },
            Alternate = new(GrammarType.Literal, (decimal)3)
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesTernaryExpressionWithObjects()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize(@"foo ? {bar: ""tek""} : ""baz"""));
        var result = _parser.Complete();
        Node expected = new(GrammarType.ConditionalExpression)
        {
            Test = new(GrammarType.Identifier, "foo"),
            Consequent = new(GrammarType.ObjectLiteral)
            {
                Object = new Dictionary<string, Node>
                {
                    { "bar", new(GrammarType.Literal, "tek") }
                }
            },
            Alternate = new(GrammarType.Literal, "baz")
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesTransformsInTernary()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize(@"foo|avg > 5 ? {bar: ""tek""} : ""baz"""));
        var result = _parser.Complete();

        Node expected = new(GrammarType.ConditionalExpression)
        {
            Test = new(GrammarType.BinaryExpression)
            {
                Operator = ">",
                Left = new(GrammarType.FunctionCall)
                {
                    Name = "avg",
                    Pool = Grammar.PoolType.Transforms,
                    Args =
                    [
                        new(GrammarType.Identifier, "foo")
                    ]
                },
                Right = new(GrammarType.Literal, (decimal)5)
            },
            Consequent = new(GrammarType.ObjectLiteral)
            {
                Object = new Dictionary<string, Node>
                {
                    { "bar", new(GrammarType.Literal, "tek") }
                }
            },
            Alternate = new(GrammarType.Literal, "baz")
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesTransformsInTernary2()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize(@"value.age > array|avg ? accumulator|append(value) : accumulator"));
        var result = _parser.Complete();

        Node expected = new(GrammarType.ConditionalExpression)
        {
            Test = new(GrammarType.BinaryExpression)
            {
                Operator = ">",
                Left = new(GrammarType.Identifier, "age")
                {
                    From = new(GrammarType.Identifier, "value")
                },
                Right = new(GrammarType.FunctionCall)
                {
                    Name = "avg",
                    Pool = Grammar.PoolType.Transforms,
                    Args =
                    [
                        new(GrammarType.Identifier, "array")
                    ]
                },
            },
            Consequent = new(GrammarType.FunctionCall)
            {
                Name = "append",
                Pool = Grammar.PoolType.Transforms,
                Args =
                    [
                        new(GrammarType.Identifier, "accumulator"),
                        new(GrammarType.Identifier, "value")
                    ]
            },
            Alternate = new(GrammarType.Identifier, "accumulator")
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesFunctionsInTernary()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize(@"value.age > avg(array) ? accumulator|append(value) : accumulator"));
        var result = _parser.Complete();

        Node expected = new(GrammarType.ConditionalExpression)
        {
            Test = new(GrammarType.BinaryExpression)
            {
                Operator = ">",
                Left = new(GrammarType.Identifier, "age")
                {
                    From = new(GrammarType.Identifier, "value")
                },
                Right = new(GrammarType.FunctionCall)
                {
                    Name = "avg",
                    Pool = Grammar.PoolType.Functions,
                    Args =
                    [
                        new(GrammarType.Identifier, "array")
                    ]
                },
            },
            Consequent = new(GrammarType.FunctionCall)
            {
                Name = "append",
                Pool = Grammar.PoolType.Transforms,
                Args =
                    [
                        new(GrammarType.Identifier, "accumulator"),
                        new(GrammarType.Identifier, "value")
                    ]
            },
            Alternate = new(GrammarType.Identifier, "accumulator")
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void BalancesBinaryBetweenComplexIdentifiers()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize(@"a.b == c.d"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.BinaryExpression)
        {
            Operator = "==",
            Left = new(GrammarType.Identifier, "b")
            {
                From = new(GrammarType.Identifier, "a")
            },
            Right = new(GrammarType.Identifier, "d")
            {
                From = new(GrammarType.Identifier, "c")
            }
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HandlesWhiteSpaceInExpression()
    {
        Parser _parser = new(new Grammar());
        _parser.AddTokens(_lexer.Tokenize("\t2\r\n+\n\r3\n\n"));
        var result = _parser.Complete();
        Node expected = new(GrammarType.BinaryExpression)
        {
            Operator = "+",
            Left = new(GrammarType.Literal, (decimal)2),
            Right = new(GrammarType.Literal, (decimal)3)
        };
        Assert.Equal(expected, result);
    }
}