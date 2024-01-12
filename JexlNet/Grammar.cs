namespace JexlNet;
public static class GrammarType
{
    public const string
        Dot = "dot",
        OpenBracket = "openBracket",
        CloseBracket = "closeBracket",
        Pipe = "pipe",
        OpenCurl = "openCurl",
        CloseCurl = "closeCurl",
        Colon = "colon",
        Comma = "comma",
        OpenParen = "openParen",
        CloseParen = "closeParen",
        Question = "question",
        BinaryOperator = "binaryOp",
        UnaryOperator = "unaryOp";
}

public class ElementGrammar
{
    public ElementGrammar(string type)
    {
        Type = type;
    }
    public ElementGrammar(
        string type,
        int precedence,
        Func<dynamic?[], dynamic?> evaluate,
        Func<Func<Task<dynamic?>>[], Task<dynamic?>>? evalOnDemand = null)
    {
        Type = type;
        Precedence = precedence;
        Evaluate = evaluate;
        EvaluateOnDemandAsync = evalOnDemand;
    }
    public string Type { get; set; }
    public int Precedence { get; set; }
    public Func<dynamic?[], dynamic?>? Evaluate { get; set; }
    public Func<Func<Task<dynamic?>>[], Task<dynamic?>>? EvaluateOnDemandAsync { get; set; }
}

public class BinaryOperatorGrammar : ElementGrammar
{
    public BinaryOperatorGrammar(
        int precedence,
        Func<dynamic?[], dynamic?> evaluate,
        Func<Func<Task<dynamic?>>[], Task<dynamic?>>? evalOnDemand = null
        ) : base(
            GrammarType.BinaryOperator,
            precedence,
            evaluate,
            evalOnDemand)
    {
    }
}

public class UnaryOperatorGrammar : ElementGrammar
{
    public UnaryOperatorGrammar(
        int precedence,
        Func<dynamic?[], dynamic?> evaluate,
        Func<Func<Task<dynamic?>>[], Task<dynamic?>>? evalOnDemand = null
        ) : base(
            GrammarType.UnaryOperator,
            precedence,
            evaluate,
            evalOnDemand)
    {
    }
}


public class Grammar
{
    /**
    * A map of all expression elements to their properties. Note that changes
    * here may require changes in the Lexer or Parser.
    */
    public readonly Dictionary<string, ElementGrammar> Elements = new()
        {
            { ".", new ElementGrammar(GrammarType.Dot)},
            { "[", new ElementGrammar(GrammarType.OpenBracket) },
            { "]", new ElementGrammar(GrammarType.CloseBracket) },
            { "|", new ElementGrammar(GrammarType.Pipe) },
            { "{", new ElementGrammar(GrammarType.OpenCurl) },
            { "}", new ElementGrammar(GrammarType.CloseCurl) },
            { ":", new ElementGrammar(GrammarType.Colon) },
            { ",", new ElementGrammar(GrammarType.Comma) },
            { "(", new ElementGrammar(GrammarType.OpenParen) },
            { ")", new ElementGrammar(GrammarType.CloseParen) },
            { "?", new ElementGrammar(GrammarType.Question) },
            {
                "+", new BinaryOperatorGrammar(30, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for + operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a + b;
                })
            },
            {
                "-", new BinaryOperatorGrammar(30, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for - operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a - b;
                })
            },
            {
                "*", new BinaryOperatorGrammar(40, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for * operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a * b;
                })
            },
            {
                "/", new BinaryOperatorGrammar(40, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for / operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a / b;
                })
            },
            {
                "//", new BinaryOperatorGrammar(40, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for // operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return Math.Floor(a / b);
                })
            },
            {
                "%", new BinaryOperatorGrammar(40, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for % operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a % b;
                })
            },
            {
                "^", new BinaryOperatorGrammar(50, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for ^ operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return Math.Pow(a, b);
                })
            },
            {
                "==", new BinaryOperatorGrammar(20, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for == operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a == b;
                })
            },
            {
                "!=", new BinaryOperatorGrammar(20, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for != operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a != b;
                })
            },
            {
                ">", new BinaryOperatorGrammar(20, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for > operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a > b;
                })
            },
            {
                ">=", new BinaryOperatorGrammar(20, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for >= operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a >= b;
                })
            },
            {
                "<", new BinaryOperatorGrammar(20, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for < operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a < b;
                })
            },
            {
                "<=", new BinaryOperatorGrammar(20, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for <= operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a <= b;
                })
            },
            {
                "&&", new BinaryOperatorGrammar(10, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for && operator");
                    }
                    // If it's a string, we consider it truthy if it's non-empty
                    // If it's a decimal, we consider it truthy it's non-zero
                    // Align with behaviour in Javascript
                    var a = args[0]?.GetType() == typeof(string)
                        ? !string.IsNullOrEmpty(args[0])
                        : (args[0]?.GetType() == typeof(decimal) ? args[0] != 0 : args[0]);
                    var b = args[1]?.GetType() == typeof(string)
                        ? !string.IsNullOrEmpty(args[1])
                        : (args[1]?.GetType() == typeof(decimal) ? args[1] != 0 : args[1]);
                    if (a is bool || b is bool)
                    {
                        return (bool)a && (bool)b;
                    }
                    else
                    {
                        throw new Exception("Unsupported type for && operator");
                    }
                }, async (wrapperFunctions) =>
                {
                    var leftVal = await wrapperFunctions[0]();
                    if (
                        (leftVal is string && string.IsNullOrEmpty(leftVal)) ||
                        (leftVal is decimal && leftVal == 0) ||
                        (leftVal is bool && !leftVal)
                    )
                    {
                        return false;
                    }
                    else
                    {
                        var rightVal = await wrapperFunctions[1]();
                        return (
                            (rightVal is string && !string.IsNullOrEmpty(rightVal)) ||
                            (rightVal is decimal && rightVal != 0) ||
                            (rightVal is bool && rightVal)
                        );
                    }


                })
            },
            {
                "||", new BinaryOperatorGrammar(10, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for || operator");
                    }
                    // If it's a string, we consider it truthy if it's non-empty
                    // If it's a decimal, we consider it truthy it's non-zero
                    // Align with behaviour in Javascript
                    var a = args[0]?.GetType() == typeof(string)
                        ? !string.IsNullOrEmpty(args[0])
                        : (args[0]?.GetType() == typeof(decimal) ? args[0] != 0 : args[0]);
                    var b = args[1]?.GetType() == typeof(string)
                        ? !string.IsNullOrEmpty(args[1])
                        : (args[1]?.GetType() == typeof(decimal) ? args[1] != 0 : args[1]);
                    if (a is bool || b is bool)
                    {
                        return (bool)a || (bool)b;
                    }
                    else
                    {
                        throw new Exception("Unsupported type for || operator");
                    }
                }, async (wrapperFunctions) =>
                {
                    var leftVal = await wrapperFunctions[0]();
                    if (
                        (leftVal is string && !string.IsNullOrEmpty(leftVal)) ||
                        (leftVal is decimal && leftVal != 0) ||
                        (leftVal is bool && leftVal)
                    )
                    {
                        return leftVal;
                    }
                    else
                    {
                        var rightVal = await wrapperFunctions[1]();
                        return (
                            (rightVal is string && !string.IsNullOrEmpty(rightVal)) ||
                            (rightVal is decimal && rightVal != 0) ||
                            (rightVal is bool && rightVal)
                        );
                    }
                })
            },
            {
                "in", new BinaryOperatorGrammar(20, (args) =>
                {
                    if (args.Length != 2)
                    {
                        throw new Exception("Unsupported number of arguments for in operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    if (b is IEnumerable<dynamic> enumerable)
                    {
                        return enumerable.Any(elem => elem == a);
                    }
                    else if (b is string str)
                    {
                        return str.Contains(a);
                    }
                    else
                    {
                        throw new Exception("Unsupported type for in operator");
                    }
                })
            },
            {
                "!", new UnaryOperatorGrammar(100, (args) =>
                {
                    if (args.Length != 1)
                    {
                        throw new Exception("Unsupported number of arguments for ! operator");
                    }
                    var a = args[0];
                    if (a is bool v)
                    {
                        return !v;
                    }
                    else
                    {
                        throw new Exception("Unsupported type for ! operator");
                    }
                })
            }
        };

    ///<summary>
    ///Adds a binary operator to Jexl at the specified precedence. The higher the
    ///precedence, the earlier the operator is applied in the order of operations.
    ///For example, * has a higher precedence than +, because multiplication comes
    ///before division.
    ///
    ///Please see Grammar.cs for a listing of all default operators and their
    ///precedence values in order to choose the appropriate precedence for the
    ///new operator.
    ///</summary>
    ///<param name="op">The operator string to be added</param>
    ///<param name="precedence">The operator's precedence</param>
    ///<param name="evaluate">A function to run to calculate the result. The function
    ///will be called with two arguments: left and right, denoting the values
    ///on either side of the operator. It should return either the resulting
    ///value, or a Promise that resolves with the resulting value.</param>
    public void AddBinaryOperator(string op, int precedence, Func<dynamic?[], dynamic?> evaluate)
    {
        Elements.Add(op, new BinaryOperatorGrammar(precedence, evaluate));
    }

    ///<summary>
    ///Adds a unary operator to Jexl. Unary operators are currently only supported
    ///on the left side of the value on which it will operate.
    ///</summary>
    ///<param name="op">The operator string to be added</param>
    ///<param name="evaluate">A function to run to calculate the result.</param>
    public void AddUnaryOperator(string op, Func<dynamic?[], dynamic?> evaluate)
    {
        Elements.Add(op, new UnaryOperatorGrammar(100, evaluate));
    }

    ///<summary>
    ///A map of function names to C# methods. A Jexl function
    ///takes zero ore more arguemnts:
    ///
    ///    - {*} ...args: A variable number of arguments passed to this function.
    ///    All of these are pre-evaluated to their actual values before calling
    ///    the function.
    ///    
    ///The Jexl function should return either the transformed value, or
    ///a Task object that resolves with the value and rejects
    ///or throws only when an unrecoverable error occurs. Functions should
    ///generally return undefined when they don't make sense to be used on the
    ///given value type, rather than throw/reject. An error is only
    ///appropriate when the function would normally return a value, but
    ///cannot due to some other failure.
    ///</summary>
    public readonly Dictionary<string, Func<List<dynamic?>, Task<object>>> Functions = new();

    ///<summary>
    ///A map of transform names to transform functions. A transform function
    ///takes one ore more arguemnts:
    ///
    ///    - {*} val: A value to be transformed
    ///    - {*} ...args: A variable number of arguments passed to this transform.
    ///    All of these are pre-evaluated to their actual values before calling
    ///    the function.
    ///    
    ///The transform function should return either the transformed value, or
    ///a Promises/A+ Promise object that resolves with the value and rejects
    ///or throws only when an unrecoverable error occurs. Transforms should
    ///generally return undefined when they don't make sense to be used on the
    ///given value type, rather than throw/reject. An error is only
    ///appropriate when the transform would normally return a value, but
    ///cannot due to some other failure.
    ///</summary>
    public readonly Dictionary<string, Func<List<dynamic?>, Task<object>>> Transforms = new();

    private void AddFunctionCall<TInput, TResult>(string poolName, string name, Func<TInput, TResult> func)
    {
        Dictionary<string, Func<List<dynamic?>, Task<object>>> pool;
        if (poolName == "functions")
        {
            pool = Functions;
        }
        else if (poolName == "transforms")
        {
            pool = Transforms;
        }
        else throw new Exception("Invalid pool name");

        Type inputType = typeof(TInput);
        Type outputType = typeof(TResult);
        // Match the signature of the Func to the signature of the Func in the dictionary
        if (inputType == typeof(List<dynamic?>) && outputType == typeof(Task<object>))
        {
            if (func is not Func<List<dynamic?>, Task<object>> newFunc)
            {
                throw new Exception("Async function must return a Task");
            }
            pool.Add(name, newFunc);
        }
        else if (typeof(Task).IsAssignableFrom(typeof(TResult)))
        // Async function which needs to be awaited
        {
            pool.Add(name, async args =>
            {
                if (inputType.IsArray)
                {
                    return (object)(await ((dynamic)func)(args.ToArray()));
                }
                else if (typeof(TInput) == typeof(List<dynamic?>))
                {
                    return (object)(await ((dynamic)func)(args));
                }
                else
                {
                    return (object)(await ((dynamic)func)(args.First()));
                }
            });
        }
        else
        // Synchronous function
        {
            pool.Add(name, args =>
            {
                Type argsType = args.GetType();
                if (inputType.IsArray)
                {
                    return Task.FromResult((object)((dynamic)func)(args.ToArray()));
                }
                else if (typeof(TInput) == typeof(List<dynamic?>))
                {
                    return Task.FromResult((object)((dynamic)func)(args));
                }
                else
                {
                    return Task.FromResult((object)((dynamic)func)(args.First()));
                }
            });
        }
    }
    private void AddFunctionCall<TResult>(string poolName, string name, Func<TResult> func)
    {
        Dictionary<string, Func<List<dynamic?>, Task<object>>> pool;
        if (poolName == "functions")
        {
            pool = Functions;
        }
        // Transforms always need an input
        else throw new Exception("Invalid pool name");
        if (typeof(Task).IsAssignableFrom(typeof(TResult)))
        {
            pool.Add(name, args => (Task<object>)((dynamic)func)());
        }
        else
        {
            pool.Add(name, args => Task.FromResult((object)((dynamic)func)()));
        }
    }
    private void AddFunctionCall<TInput, TInput2, TResult>(string poolName, string name, Func<TInput, TInput2, TResult> func)
    {
        // Define a new function that takes a single input parameter (array)
        TResult newFunc(object[] args)
        {
            TInput input1 = (TInput)args[0];
            TInput2 input2;
            if (func.Method.GetParameters()[1].ParameterType.IsArray)
            {
                input2 = (TInput2)((dynamic)args.Skip(1).ToArray());
            }
            else
            {
                input2 = (TInput2)args[1];
            }

            // Call the original function with both inputs joined as an array
            return func(input1, input2);
        }

        // Call the existing method with the new function
        AddFunctionCall(poolName, name, (Func<object[], TResult>)newFunc);
    }
    private void AddFunctionCalls<TInput, TResult>(string poolName, Dictionary<string, Func<TInput, TResult>> funcsDict)
    {
        foreach (var kv in funcsDict)
        {
            AddFunctionCall(poolName, kv.Key, kv.Value);
        }
    }
    private void AddFunctionCalls<TResult>(string poolName, Dictionary<string, Func<TResult>> funcsDict)
    {
        foreach (var kv in funcsDict)
        {
            AddFunctionCall(poolName, kv.Key, kv.Value);
        }
    }

    public void AddFunction<TInput, TResult>(string name, Func<TInput, TResult> func) => AddFunctionCall("functions", name, func);
    public void AddFunction<TResult>(string name, Func<TResult> func) => AddFunctionCall("functions", name, func);
    public void AddFunctions<TInput, TResult>(Dictionary<string, Func<TInput, TResult>> funcsDict) => AddFunctionCalls("functions", funcsDict);
    public void AddFunctions<TResult>(Dictionary<string, Func<TResult>> funcsDict) => AddFunctionCalls("functions", funcsDict);

    public void AddTransform<TInput, TResult>(string name, Func<TInput, TResult> func) => AddFunctionCall("transforms", name, func);
    public void AddTransform<TInput, TInput2, TResult>(string name, Func<TInput, TInput2, TResult> func) => AddFunctionCall("transforms", name, func);
    public void AddTransforms<TInput, TResult>(Dictionary<string, Func<TInput, TResult>> funcsDict) => AddFunctionCalls("transforms", funcsDict);
}
