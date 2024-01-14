using System.Text.Json;
using System.Text.Json.Nodes;

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
        Func<JsonArray, JsonNode> evaluate,
        Func<Func<Task<JsonNode?>>[], Task<JsonNode>>? evalOnDemand = null)
    {
        Type = type;
        Precedence = precedence;
        Evaluate = evaluate;
        EvaluateOnDemandAsync = evalOnDemand;
    }
    public string Type { get; set; }
    public int Precedence { get; set; }
    public Func<JsonArray, JsonNode>? Evaluate { get; set; }
    public Func<Func<Task<JsonNode?>>[], Task<JsonNode>>? EvaluateOnDemandAsync { get; set; }
}

public class BinaryOperatorGrammar : ElementGrammar
{
    public BinaryOperatorGrammar(
        int precedence,
        Func<JsonArray, JsonNode> evaluate,
        Func<Func<Task<JsonNode?>>[], Task<JsonNode>>? evalOnDemand = null
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
        Func<JsonArray, JsonNode> evaluate,
        Func<Func<Task<JsonNode?>>[], Task<JsonNode>>? evalOnDemand = null
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
                    if (args.Count != 2)
                    {
                        throw new Exception("Unsupported number of arguments for + operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    if (a == null || b == null)
                    {
                        throw new Exception("Binary operator cannot be applied to null values");
                    }
                    else if (a.GetValueKind() == JsonValueKind.String || b.GetValueKind() == JsonValueKind.String)
                    {
                        return a.GetValue<string>() + b.GetValue<string>();
                    }
                    else if (a.GetValueKind() == JsonValueKind.Number && b.GetValueKind() == JsonValueKind.Number)
                    {
                        return a.GetValue<decimal>() + b.GetValue<decimal>();
                    }
                    else if (a is JsonArray aArr && b is JsonArray bArr)
                    {
                        return new JsonArray(aArr.Concat(bArr).ToArray());
                    }
                    else
                    {
                        throw new Exception("Unsupported type for + operator");
                    }
                })
            },
            {
                "-", new BinaryOperatorGrammar(30, (args) =>
                {
                    if (args.Count != 2)
                    {
                        throw new Exception("Unsupported number of arguments for + operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    if (a == null || b == null)
                    {
                        throw new Exception("Binary operator cannot be applied to null values");
                    }
                    else if (a.GetValueKind() == JsonValueKind.Number && a.GetValueKind() == JsonValueKind.Number)
                    {
                        return a.GetValue<decimal>() - b.GetValue<decimal>();
                    }
                    else
                    {
                        throw new Exception("Unsupported type for + operator");
                    }
                })
            },
            {
                "*", new BinaryOperatorGrammar(40, (args) =>
                {
                    if (args.Count != 2)
                    {
                        throw new Exception("Unsupported number of arguments for * operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    if (a == null || b == null)
                    {
                        throw new Exception("Binary operator cannot be applied to null values");
                    }
                    else if (a.GetValueKind() == JsonValueKind.Number && a.GetValueKind() == JsonValueKind.Number)
                    {
                        return a.GetValue<decimal>() * b.GetValue<decimal>();
                    }
                    else
                    {
                        throw new Exception("Unsupported type for + operator");
                    }
                })
            },
            {
                "/", new BinaryOperatorGrammar(40, (args) =>
                {
                    if (args.Count != 2)
                    {
                        throw new Exception("Unsupported number of arguments for / operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    if (a == null || b == null)
                    {
                        throw new Exception("Binary operator cannot be applied to null values");
                    }
                    else if (a.GetValueKind() == JsonValueKind.Number && a.GetValueKind() == JsonValueKind.Number)
                    {
                        return a.GetValue<decimal>() / b.GetValue<decimal>();
                    }
                    else
                    {
                        throw new Exception("Unsupported type for + operator");
                    }
                })
            },
            {
                "//", new BinaryOperatorGrammar(40, (args) =>
                {
                    if (args.Count != 2)
                    {
                        throw new Exception("Unsupported number of arguments for // operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    if (a == null || b == null)
                    {
                        throw new Exception("Binary operator cannot be applied to null values");
                    }
                    else if (a.GetValueKind() == JsonValueKind.Number && a.GetValueKind() == JsonValueKind.Number)
                    {
                        return Math.Floor(a.GetValue<decimal>() / b.GetValue<decimal>());
                    }
                    else
                    {
                        throw new Exception("Unsupported type for + operator");
                    }
                })
            },
            {
                "%", new BinaryOperatorGrammar(40, (args) =>
                {
                    if (args.Count != 2)
                    {
                        throw new Exception("Unsupported number of arguments for % operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    if (a == null || b == null)
                    {
                        throw new Exception("Binary operator cannot be applied to null values");
                    }
                    else if (a.GetValueKind() == JsonValueKind.Number && a.GetValueKind() == JsonValueKind.Number)
                    {
                        return a.GetValue<decimal>() % b.GetValue<decimal>();
                    }
                    else
                    {
                        throw new Exception("Unsupported type for + operator");
                    }
                })
            },
            {
                "^", new BinaryOperatorGrammar(50, (args) =>
                {
                    if (args.Count != 2)
                    {
                        throw new Exception("Unsupported number of arguments for ^ operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    if (a == null || b == null)
                    {
                        throw new Exception("Binary operator cannot be applied to null values");
                    }
                    else if (a.GetValueKind() == JsonValueKind.Number && a.GetValueKind() == JsonValueKind.Number)
                    {
                        return Math.Pow(a.GetValue<double>(), b.GetValue<double>());
                    }
                    else
                    {
                        throw new Exception("Unsupported type for + operator");
                    }
                })
            },
            {
                "==", new BinaryOperatorGrammar(20, (args) =>
                {
                    if (args.Count != 2)
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
                    if (args.Count != 2)
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
                    if (args.Count != 2)
                    {
                        throw new Exception("Unsupported number of arguments for > operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a?.GetValue<decimal>() > b?.GetValue<decimal>();
                })
            },
            {
                ">=", new BinaryOperatorGrammar(20, (args) =>
                {
                    if (args.Count != 2)
                    {
                        throw new Exception("Unsupported number of arguments for >= operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a?.GetValue<decimal>() >= b?.GetValue<decimal>();
                })
            },
            {
                "<", new BinaryOperatorGrammar(20, (args) =>
                {
                    if (args.Count != 2)
                    {
                        throw new Exception("Unsupported number of arguments for < operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a?.GetValue<decimal>() < b?.GetValue<decimal>();
                })
            },
            {
                "<=", new BinaryOperatorGrammar(20, (args) =>
                {
                    if (args.Count != 2)
                    {
                        throw new Exception("Unsupported number of arguments for <= operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    return a?.GetValue<decimal>() <= b?.GetValue<decimal>();
                })
            },
            {
                "&&", new BinaryOperatorGrammar(10, (args) =>
                {
                    if (args.Count != 2)
                    {
                        throw new Exception("Unsupported number of arguments for && operator");
                    }
                    // If it's a string, we consider it truthy if it's non-empty
                    // If it's a decimal, we consider it truthy it's non-zero
                    // Align with behaviour in Javascript
                    bool a = args[0]?.GetValueKind() == JsonValueKind.String
                        ? !string.IsNullOrEmpty(args[0]?.GetValue<string>())
                        : (
                            args[0]?.GetValueKind() == JsonValueKind.Number
                            ? args[0]?.GetValue<decimal>() != 0
                            : args[0]?.GetValueKind() == JsonValueKind.True
                        );
                    bool b = args[1]?.GetValueKind() == JsonValueKind.String
                        ? !string.IsNullOrEmpty(args[1]?.GetValue<string>())
                        : (
                            args[1]?.GetValueKind() == JsonValueKind.Number
                            ? args[1]?.GetValue<decimal>() != 0
                            : args[1]?.GetValueKind() == JsonValueKind.True
                        );
                    return a && b;
                }, async (wrapperFunctions) =>
                {
                    JsonNode? leftVal = await wrapperFunctions[0]();
                    if (leftVal == null ||
                        (leftVal.GetValueKind() == JsonValueKind.String && string.IsNullOrEmpty(leftVal.GetValue<string>())) ||
                        (leftVal.GetValueKind() == JsonValueKind.Number && leftVal.GetValue<decimal>() == 0) ||
                        (leftVal.GetValueKind() == JsonValueKind.False)
                    )
                    {
                        return JsonValue.Create(false);
                    }
                    else
                    {
                        JsonNode? rightVal = await wrapperFunctions[1]();
                        if (rightVal != null &&
                            ((rightVal.GetValueKind() == JsonValueKind.String && !string.IsNullOrEmpty(rightVal.GetValue<string>())) ||
                            (rightVal.GetValueKind() == JsonValueKind.Number && leftVal.GetValue<decimal>() != 0) ||
                            (rightVal.GetValueKind() == JsonValueKind.True))
                        )
                        {
                            return rightVal;
                        }
                        else return JsonValue.Create(false);
                    }
                })
            },
            {
                "||", new BinaryOperatorGrammar(10, (args) =>
                {
                    if (args.Count != 2)
                    {
                        throw new Exception("Unsupported number of arguments for || operator");
                    }
                    // If it's a string, we consider it truthy if it's non-empty
                    // If it's a decimal, we consider it truthy it's non-zero
                    // Align with behaviour in Javascript
                    bool a = args[0]?.GetValueKind() == JsonValueKind.String
                        ? !string.IsNullOrEmpty(args[0]?.GetValue<string>())
                        : (
                            args[0]?.GetValueKind() == JsonValueKind.Number
                            ? args[0]?.GetValue<decimal>() != 0
                            : args[0]?.GetValueKind() == JsonValueKind.True
                        );
                    bool b = args[1]?.GetValueKind() == JsonValueKind.String
                        ? !string.IsNullOrEmpty(args[1]?.GetValue<string>())
                        : (
                            args[1]?.GetValueKind() == JsonValueKind.Number
                            ? args[1]?.GetValue<decimal>() != 0
                            : args[1]?.GetValueKind() == JsonValueKind.True
                        );
                    return a || b;
                }, async (wrapperFunctions) =>
                {
                    JsonNode? leftVal = await wrapperFunctions[0]();
                    if (leftVal != null &&
                        ((leftVal.GetValueKind() == JsonValueKind.String && !string.IsNullOrEmpty(leftVal.GetValue<string>())) ||
                        (leftVal.GetValueKind() == JsonValueKind.Number && leftVal.GetValue<decimal>() != 0) ||
                        (leftVal.GetValueKind() == JsonValueKind.True))
                    )
                    {
                        return leftVal;
                    }
                    else
                    {
                        JsonNode? rightVal = await wrapperFunctions[1]();
                        if (rightVal != null &&
                            ((rightVal.GetValueKind() == JsonValueKind.String && string.IsNullOrEmpty(rightVal.GetValue<string>())) ||
                            (rightVal.GetValueKind() == JsonValueKind.Number && rightVal.GetValue<decimal>() == 0) ||
                            (rightVal.GetValueKind() == JsonValueKind.False))
                        )
                        {
                            return rightVal;
                        }
                        else return JsonValue.Create(false);
                    }
                })
            },
            {
                "in", new BinaryOperatorGrammar(20, (args) =>
                {
                    if (args.Count != 2)
                    {
                        throw new Exception("Unsupported number of arguments for in operator");
                    }
                    var a = args[0];
                    var b = args[1];
                    if (a == null || b == null)
                    {
                        throw new Exception("Binary operator cannot be applied to null values");
                    }
                    if (b is JsonArray arr)
                    {
                        return JsonValue.Create(arr.Contains(a));
                    }
                    else if (b.GetValueKind() == JsonValueKind.String)
                    {
                        return JsonValue.Create(b.GetValue<string>().Contains(a.GetValue<string>()));
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
                    if (args.Count != 1)
                    {
                        throw new Exception("Unsupported number of arguments for ! operator");
                    }
                    var a = args[0];
                    if (a == null)
                    {
                        return JsonValue.Create(true);
                    }
                    else if (a.GetValueKind() == JsonValueKind.String)
                    {
                        return JsonValue.Create(string.IsNullOrEmpty(a.GetValue<string>()));
                    }
                    else if (a.GetValueKind() == JsonValueKind.Number)
                    {
                        return JsonValue.Create(a.GetValue<decimal>() == 0);
                    }
                    else if (a.GetValueKind() == JsonValueKind.Array)
                    {
                        return JsonValue.Create(a.GetValue<JsonArray>().Count == 0);
                    }
                    else if (a.GetValueKind() == JsonValueKind.True || a.GetValueKind() == JsonValueKind.False)
                    {
                        return JsonValue.Create(!a.GetValue<bool>());
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
    public void AddBinaryOperator(string op, int precedence, Func<JsonArray, JsonNode> evaluate)
    {
        Elements.Add(op, new BinaryOperatorGrammar(precedence, evaluate));
    }

    ///<summary>
    ///Adds a unary operator to Jexl. Unary operators are currently only supported
    ///on the left side of the value on which it will operate.
    ///</summary>
    ///<param name="op">The operator string to be added</param>
    ///<param name="evaluate">A function to run to calculate the result.</param>
    public void AddUnaryOperator(string op, Func<JsonArray, JsonNode> evaluate)
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
    public readonly Dictionary<string, Func<JsonArray, Task<JsonNode>>> Functions = new();

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
    public readonly Dictionary<string, Func<JsonArray, Task<JsonNode>>> Transforms = new();

    private Dictionary<string, Func<JsonArray, Task<JsonNode>>> GetPool(string poolName)
    {
        return poolName switch
        {
            "functions" => Functions,
            "transforms" => Transforms,
            _ => throw new Exception("Invalid pool name"),
        };
    }

    ///<summary>
    ///Add a single function call with no arguments to the grammar (only for functions).
    ///</summary>
    private void AddFunctionCall(string poolName, string name, Func<Task<JsonNode>> func)
    {
        var pool = GetPool(poolName);
        pool.Add(name, async args => await func());
    }
    ///<summary>
    ///Add a single function call with no arguments to the grammar (only for functions).
    ///</summary>
    private void AddFunctionCall(string poolName, string name, Func<JsonNode> func)
    {
        var pool = GetPool(poolName);
        pool.Add(name, args => Task.FromResult(func()));
    }
    ///<summary>
    ///Add a single function call with a single argument (can be an array or list) to the grammar.
    ///</summary>
    private void AddFunctionCall(string poolName, string name, Func<JsonArray, Task<JsonNode>> func)
    {
        var pool = GetPool(poolName);
        pool.Add(name, func);
    }
    ///<summary>
    ///Add a single function call with a single argument (can be an array or list) to the grammar.
    ///</summary>
    private void AddFunctionCall(string poolName, string name, Func<JsonArray, JsonNode> func)
    {
        var pool = GetPool(poolName);
        pool.Add(name, args => Task.FromResult(func(args)));
    }
    ///<summary>
    ///Add a single function call with a a first argument and a second argument (can be an array or list) to the grammar (only for transforms).
    ///</summary>
    private void AddFunctionCall(string poolName, string name, Func<JsonNode?, JsonNode?, Task<JsonNode>> func)
    {
        // Define a new function that takes a single input parameter (array)
        Task<JsonNode> newFunc(JsonArray args)
        {
            JsonNode? input1 = args[0];
            JsonNode? input2 = args[1];

            // Call the original function with both inputs joined as an array
            return func(input1, input2);
        }

        // Call the existing method with the new function
        AddFunctionCall(poolName, name, newFunc);
    }
    ///<summary>
    ///Add a single function call with a a first argument and a second argument (can be an array or list) to the grammar (only for transforms).
    ///</summary>
    private void AddFunctionCall(string poolName, string name, Func<JsonNode?, JsonNode?, JsonNode> func)
    {
        // Define a new function that takes a single input parameter (array)
        JsonNode newFunc(JsonArray args)
        {
            JsonNode? input1 = args[0];
            JsonNode? input2 = args[1];

            // Call the original function with both inputs joined as an array
            return func(input1, input2);
        }

        // Call the existing method with the new function
        AddFunctionCall(poolName, name, newFunc);
    }
    ///<summary>
    ///Add a single function call with a a first argument and a second argument (can be an array or list) to the grammar (only for transforms).
    ///</summary>
    private void AddFunctionCall(string poolName, string name, Func<JsonNode?, JsonArray?, Task<JsonNode>> func)
    {
        // Define a new function that takes a single input parameter (array)
        Task<JsonNode> newFunc(JsonArray args)
        {
            JsonNode? input1 = args[0];
            JsonArray? input2 = new(args.Skip(1).ToArray());

            // Call the original function with both inputs joined as an array
            return func(input1, input2);
        }

        // Call the existing method with the new function
        AddFunctionCall(poolName, name, newFunc);
    }
    ///<summary>
    ///Add a single function call with a a first argument and a second argument (can be an array or list) to the grammar (only for transforms).
    ///</summary>
    private void AddFunctionCall(string poolName, string name, Func<JsonNode?, JsonArray?, JsonNode> func)
    {
        // Define a new function that takes a single input parameter (array)
        JsonNode newFunc(JsonArray args)
        {
            JsonNode? input1 = args[0];
            JsonArray? input2 = new(args.Skip(1).ToArray());

            // Call the original function with both inputs joined as an array
            return func(input1, input2);
        }

        // Call the existing method with the new function
        AddFunctionCall(poolName, name, newFunc);
    }

    ///<summary>
    ///Add a dictionary of function calls with no arguments to the grammar (only for functions).
    ///</summary>
    private void AddFunctionCalls(string poolName, Dictionary<string, Func<Task<JsonNode>>> funcsDict)
    {
        foreach (var kv in funcsDict)
        {
            AddFunctionCall(poolName, kv.Key, kv.Value);
        }
    }
    ///<summary>
    ///Add a dictionary of function calls with no arguments to the grammar (only for functions).
    ///</summary>
    private void AddFunctionCalls(string poolName, Dictionary<string, Func<JsonNode>> funcsDict)
    {
        foreach (var kv in funcsDict)
        {
            AddFunctionCall(poolName, kv.Key, kv.Value);
        }
    }
    ///<summary>
    ///Add a dictionary of function calls with a single argument (can be an array or list) to the grammar.
    ///</summary>
    private void AddFunctionCalls(string poolName, Dictionary<string, Func<JsonArray, Task<JsonNode>>> funcsDict)
    {
        foreach (var kv in funcsDict)
        {
            AddFunctionCall(poolName, kv.Key, kv.Value);
        }
    }
    ///<summary>
    ///Add a dictionary of function calls with a single argument (can be an array or list) to the grammar.
    ///</summary>
    private void AddFunctionCalls(string poolName, Dictionary<string, Func<JsonArray, JsonNode>> funcsDict)
    {
        foreach (var kv in funcsDict)
        {
            AddFunctionCall(poolName, kv.Key, kv.Value);
        }
    }
    ///<summary>
    ///Add a dictionary of function calls with a a first argument and a second argument (can be an array or list) to the grammar (only for transforms).
    ///</summary>
    private void AddFunctionCalls(string poolName, Dictionary<string, Func<JsonNode?, JsonNode?, Task<JsonNode>>> funcsDict)
    {
        foreach (var kv in funcsDict)
        {
            AddFunctionCall(poolName, kv.Key, kv.Value);
        }
    }
    ///<summary>
    ///Add a dictionary of function calls with a a first argument and a second argument (can be an array or list) to the grammar (only for transforms).
    ///</summary>
    private void AddFunctionCalls(string poolName, Dictionary<string, Func<JsonNode?, JsonNode?, JsonNode>> funcsDict)
    {
        foreach (var kv in funcsDict)
        {
            AddFunctionCall(poolName, kv.Key, kv.Value);
        }
    }
    ///<summary>
    ///Add a dictionary of function calls with a a first argument and a second argument (can be an array or list) to the grammar (only for transforms).
    ///</summary>
    private void AddFunctionCalls(string poolName, Dictionary<string, Func<JsonNode?, JsonArray?, Task<JsonNode>>> funcsDict)
    {
        foreach (var kv in funcsDict)
        {
            AddFunctionCall(poolName, kv.Key, kv.Value);
        }
    }
    ///<summary>
    ///Add a dictionary of function calls with a a first argument and a second argument (can be an array or list) to the grammar (only for transforms).
    ///</summary>
    private void AddFunctionCalls(string poolName, Dictionary<string, Func<JsonNode?, JsonArray?, JsonNode>> funcsDict)
    {
        foreach (var kv in funcsDict)
        {
            AddFunctionCall(poolName, kv.Key, kv.Value);
        }
    }

    ///<summary> Add a function to the grammar with no arguments. </summary>
    public void AddFunction(string name, Func<Task<JsonNode>> func) => AddFunctionCall("functions", name, func);
    ///<summary> Add a function to the grammar with no arguments. </summary>
    public void AddFunction(string name, Func<JsonNode> func) => AddFunctionCall("functions", name, func);
    ///<summary> Add a function to the grammar with a single argument (can be an array or list). </summary>
    public void AddFunction(string name, Func<JsonArray, Task<JsonNode>> func) => AddFunctionCall("functions", name, func);
    ///<summary> Add a function to the grammar with a single argument (can be an array or list). </summary>
    public void AddFunction(string name, Func<JsonArray, JsonNode> func) => AddFunctionCall("functions", name, func);
    ///<summary> Add a dictionary of functions with no arguments to the grammar. </summary>
    public void AddFunctions(Dictionary<string, Func<JsonNode>> funcsDict) => AddFunctionCalls("functions", funcsDict);
    ///<summary> Add a dictionary of functions with a single argument (can be an array or list) to the grammar. </summary>
    public void AddFunctions(Dictionary<string, Func<JsonArray, JsonNode>> funcsDict) => AddFunctionCalls("functions", funcsDict);

    ///<summary> Add a transform with a single argument to the grammar. </summary>
    public void AddTransform(string name, Func<JsonArray, Task<JsonNode>> func) => AddFunctionCall("transforms", name, func);
    ///<summary> Add a transform with a single argument to the grammar. </summary>
    public void AddTransform(string name, Func<JsonArray, JsonNode> func) => AddFunctionCall("transforms", name, func);
    ///<summary> Add a transform with a a first argument and a second argument (can be an array or list) to the grammar. </summary>
    public void AddTransform(string name, Func<JsonNode?, JsonNode?, Task<JsonNode>> func) => AddFunctionCall("transforms", name, func);
    ///<summary> Add a transform with a a first argument and a second argument (can be an array or list) to the grammar. </summary>
    public void AddTransform(string name, Func<JsonNode?, JsonNode?, JsonNode> func) => AddFunctionCall("transforms", name, func);
    ///<summary> Add a transform with a a first argument and a second argument (can be an array or list) to the grammar. </summary>
    public void AddTransform(string name, Func<JsonNode?, JsonArray?, Task<JsonNode>> func) => AddFunctionCall("transforms", name, func);
    ///<summary> Add a transform with a a first argument and a second argument (can be an array or list) to the grammar. </summary>
    public void AddTransform(string name, Func<JsonNode?, JsonArray?, JsonNode> func) => AddFunctionCall("transforms", name, func);
    ///<summary> Add a dictionary of transforms with a single argument (can be an array or list) to the grammar. </summary>
    public void AddTransforms(Dictionary<string, Func<JsonArray, JsonNode>> funcsDict) => AddFunctionCalls("transforms", funcsDict);
    ///<summary> Add a dictionary of transforms with a a first argument and a second argument (can be an array or list) to the grammar. </summary>
    public void AddTransforms(Dictionary<string, Func<JsonNode?, JsonNode?, JsonNode>> funcsDict) => AddFunctionCalls("transforms", funcsDict);
    ///<summary> Add a dictionary of transforms with a a first argument and a second argument (can be an array or list) to the grammar. </summary>
    public void AddTransforms(Dictionary<string, Func<JsonNode?, JsonArray?, JsonNode>> funcsDict) => AddFunctionCalls("transforms", funcsDict);
}
