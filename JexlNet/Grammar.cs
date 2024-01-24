using System.Text.Json;
using System.Text.Json.Nodes;

namespace JexlNet;

public enum GrammarType
{
    Literal,
    Identifier,
    Dot,
    OpenBracket,
    CloseBracket,
    Pipe,
    OpenCurl,
    CloseCurl,
    Colon,
    Comma,
    OpenParen,
    CloseParen,
    Question,
    BinaryOperator,
    UnaryOperator,
    ObjectLiteral,
    ArrayLiteral,
    ExpectOperand,
    ExpectBinaryOperator,
    ExpectTransform,
    ExpectObjectKey,
    ExpectKeyValueSeperator,
    PostTransform,
    PostArgs,
    Traverse,
    Filter,
    SubExpression,
    ArgumentValue,
    ObjectValue,
    ArrayValue,
    TernaryMid,
    TernaryEnd,
    BinaryExpression,
    UnaryExpression,
    FilterExpression,
    ConditionalExpression,
    FunctionCall,
    Complete,
    Stop,
    ArrayStart,
    ObjectKey,
    ObjectStart,
    TernaryStart,
    Transform,
}

public class ElementGrammar
{
    public ElementGrammar(GrammarType type)
    {
        Type = type;
    }
    public ElementGrammar(
        GrammarType type,
        int precedence,
        Func<JsonArray, JsonNode> evaluate,
        Func<Func<Task<JsonNode?>>[], Task<JsonNode>>? evalOnDemand = null)
    {
        Type = type;
        Precedence = precedence;
        Evaluate = evaluate;
        EvaluateOnDemandAsync = evalOnDemand;
    }
    public GrammarType Type { get; set; }
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
                        return a.ToString() + b.ToString();
                    }
                    else if (a.GetValueKind() == JsonValueKind.Number && b.GetValueKind() == JsonValueKind.Number && a is JsonValue aValue && b is JsonValue bValue)
                    {
                        return aValue.ToDecimal() + bValue.ToDecimal();
                    }
                    else if (a is JsonArray aArr && b is JsonArray bArr)
                    {
                        return new JsonArray([.. aArr, .. bArr]);
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
                    else if (a.GetValueKind() == JsonValueKind.Number && a.GetValueKind() == JsonValueKind.Number && a is JsonValue aValue && b is JsonValue bValue)
                    {
                        return aValue.ToDecimal() + bValue.ToDecimal();
                    }
                    else
                    {
                        throw new Exception("Unsupported type for - operator");
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
                    else if (a.GetValueKind() == JsonValueKind.Number && a.GetValueKind() == JsonValueKind.Number && a is JsonValue aValue && b is JsonValue bValue)
                    {
                        return aValue.ToDecimal() * bValue.ToDecimal();
                    }
                    else
                    {
                        throw new Exception("Unsupported type for * operator");
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
                    else if (a.GetValueKind() == JsonValueKind.Number && a.GetValueKind() == JsonValueKind.Number && a is JsonValue aValue && b is JsonValue bValue)
                    {
                        return aValue.ToDecimal() / bValue.ToDecimal();
                    }
                    else
                    {
                        throw new Exception("Unsupported type for / operator");
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
                    else if (a.GetValueKind() == JsonValueKind.Number && a.GetValueKind() == JsonValueKind.Number && a is JsonValue aValue && b is JsonValue bValue)
                    {
                        return Math.Floor(aValue.ToDecimal() / bValue.ToDecimal());
                    }
                    else
                    {
                        throw new Exception("Unsupported type for // operator");
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
                    else if (a.GetValueKind() == JsonValueKind.Number && a.GetValueKind() == JsonValueKind.Number && a is JsonValue aValue && b is JsonValue bValue)
                    {
                        return aValue.ToDecimal() % bValue.ToDecimal();
                    }
                    else
                    {
                        throw new Exception("Unsupported type for % operator");
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
                    else if (a.GetValueKind() == JsonValueKind.Number && a.GetValueKind() == JsonValueKind.Number && a is JsonValue aValue && b is JsonValue bValue)
                    {
                        return (decimal)Math.Pow(aValue.ToDouble(), bValue.ToDouble());
                    }
                    else
                    {
                        throw new Exception("Unsupported type for ^ operator");
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
                    if (a == null && b == null)
                    {
                        return true;
                    }
                    else if (a == null || b == null)
                    {
                        return false;
                    }
                    else if (a.GetValueKind() == JsonValueKind.String && b.GetValueKind() == JsonValueKind.String)
                    {
                        return a.GetValue<string>() == b.GetValue<string>();
                    }
                    else if (a.GetValueKind() == JsonValueKind.Number && b.GetValueKind() == JsonValueKind.Number && a is JsonValue aValue && b is JsonValue bValue)
                    {
                        return aValue.ToDecimal() == aValue.ToDecimal();
                    }
                    else if (a.GetValueKind() == JsonValueKind.True && b.GetValueKind() == JsonValueKind.True)
                    {
                        return true;
                    }
                    else if (a.GetValueKind() == JsonValueKind.False && b.GetValueKind() == JsonValueKind.False)
                    {
                        return true;
                    }
                    else
                    {
                        return JsonNode.DeepEquals(a, b);
                    }
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
                    if (a == null && b == null)
                    {
                        return false;
                    }
                    else if (a == null || b == null)
                    {
                        return true;
                    }
                    else if (a.GetValueKind() == JsonValueKind.String && b.GetValueKind() == JsonValueKind.String)
                    {
                        return a.GetValue<string>() != b.GetValue<string>();
                    }
                    else if (a.GetValueKind() == JsonValueKind.Number && b.GetValueKind() == JsonValueKind.Number && a is JsonValue aValue && b is JsonValue bValue)
                    {
                        return aValue.ToDecimal() != aValue.ToDecimal();
                    }
                    else if (a.GetValueKind() == JsonValueKind.True && b.GetValueKind() == JsonValueKind.True)
                    {
                        return false;
                    }
                    else if (a.GetValueKind() == JsonValueKind.False && b.GetValueKind() == JsonValueKind.False)
                    {
                        return false;
                    }
                    else
                    {
                        return !JsonNode.DeepEquals(a, b);
                    }
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
                    if (a == null || b == null)
                    {
                        throw new Exception("Binary operator cannot be applied to null values");
                    }
                    else if (a.GetValueKind() == JsonValueKind.Number && a.GetValueKind() == JsonValueKind.Number && a is JsonValue aValue && b is JsonValue bValue)
                    {
                        return aValue.ToDecimal() > bValue.ToDecimal();
                    }
                    else
                    {
                        throw new Exception("Unsupported type for > operator");
                    }
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
                    if (a == null || b == null)
                    {
                        throw new Exception("Binary operator cannot be applied to null values");
                    }
                    else if (a.GetValueKind() == JsonValueKind.Number && a.GetValueKind() == JsonValueKind.Number && a is JsonValue aValue && b is JsonValue bValue)
                    {
                        return aValue.ToDecimal() >= bValue.ToDecimal();
                    }
                    else
                    {
                        throw new Exception("Unsupported type for >= operator");
                    }
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
                    if (a == null || b == null)
                    {
                        throw new Exception("Binary operator cannot be applied to null values");
                    }
                    else if (a.GetValueKind() == JsonValueKind.Number && a.GetValueKind() == JsonValueKind.Number && a is JsonValue aValue && b is JsonValue bValue)
                    {
                        return aValue.ToDecimal() < bValue.ToDecimal();
                    }
                    else
                    {
                        throw new Exception("Unsupported type for < operator");
                    }
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
                    if (a == null || b == null)
                    {
                        throw new Exception("Binary operator cannot be applied to null values");
                    }
                    else if (a.GetValueKind() == JsonValueKind.Number && a.GetValueKind() == JsonValueKind.Number && a is JsonValue aValue && b is JsonValue bValue)
                    {
                        return aValue.ToDecimal() <= bValue.ToDecimal();
                    }
                    else
                    {
                        throw new Exception("Unsupported type for <= operator");
                    }
                })
            },
            {
                "&&", new BinaryOperatorGrammar(10, (args) =>
                {
                    if (args.Count == 2 && args[0] is JsonValue aValue && args[1] is JsonValue bValue)
                    {
                        // If it's a string, we consider it truthy if it's non-empty
                        // If it's a decimal, we consider it truthy it's non-zero
                        // Align with behaviour in Javascript
                        bool a = aValue.GetValueKind() == JsonValueKind.String
                            ? !string.IsNullOrEmpty(aValue.GetValue<string>())
                            : (
                                aValue.GetValueKind() == JsonValueKind.Number
                                ? aValue.ToDecimal() != 0
                                : aValue.GetValueKind() == JsonValueKind.True
                            );
                        bool b = bValue.GetValueKind() == JsonValueKind.String
                            ? !string.IsNullOrEmpty(bValue.GetValue<string>())
                            : (
                                bValue.GetValueKind() == JsonValueKind.Number
                                ? bValue.ToDecimal() != 0
                                : bValue.GetValueKind() == JsonValueKind.True
                            );
                        return a && b;
                    }
                    else
                    {
                        throw new Exception("Unsupported number of arguments for && operator");
                    }

                }, async (wrapperFunctions) =>
                {
                    JsonNode? leftVal = await wrapperFunctions[0]();
                    if (leftVal == null ||
                        (leftVal.GetValueKind() == JsonValueKind.String && string.IsNullOrEmpty(leftVal.GetValue<string>())) ||
                        (leftVal.GetValueKind() == JsonValueKind.Number && leftVal is JsonValue leftValue && leftValue.ToDecimal() == 0) ||
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
                            (rightVal.GetValueKind() == JsonValueKind.Number && rightVal is JsonValue rightValue && rightValue.ToDecimal() != 0) ||
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
                    if (args.Count == 2 && args[0] is JsonValue aValue && args[1] is JsonValue bValue)
                    {
                        // If it's a string, we consider it truthy if it's non-empty
                        // If it's a decimal, we consider it truthy it's non-zero
                        // Align with behaviour in Javascript
                        bool a = aValue.GetValueKind() == JsonValueKind.String
                            ? !string.IsNullOrEmpty(aValue.GetValue<string>())
                            : (
                                aValue.GetValueKind() == JsonValueKind.Number
                                ? aValue.ToDecimal() != 0
                                : aValue.GetValueKind() == JsonValueKind.True
                            );
                        bool b = bValue.GetValueKind() == JsonValueKind.String
                            ? !string.IsNullOrEmpty(bValue.GetValue<string>())
                            : (
                                bValue.GetValueKind() == JsonValueKind.Number
                                ? bValue.ToDecimal() != 0
                                : bValue.GetValueKind() == JsonValueKind.True
                            );
                        return a || b;
                    }
                    else
                    {
                        throw new Exception("Unsupported number of arguments for || operator");
                    }
                }, async (wrapperFunctions) =>
                {
                    JsonNode? leftVal = await wrapperFunctions[0]();
                    if (leftVal != null &&
                        ((leftVal.GetValueKind() == JsonValueKind.String && !string.IsNullOrEmpty(leftVal.GetValue<string>())) ||
                        (leftVal.GetValueKind() == JsonValueKind.Number && leftVal is JsonValue leftValue && leftValue.ToDecimal() != 0) ||
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
                            (rightVal.GetValueKind() == JsonValueKind.Number && rightVal is JsonValue rightValue && rightValue.ToDecimal() == 0) ||
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
                        JsonValueKind aValueKind = a.GetValueKind();
                        return arr.Any(x => x?.GetValueKind() == aValueKind
                            && aValueKind switch
                            {
                                JsonValueKind.String => x.GetValue<string>() == a.GetValue<string>(),
                                JsonValueKind.Number => x.GetValue<decimal>() == a.GetValue<decimal>(),
                                _ => JsonNode.DeepEquals(x, a)
                            });
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
    public void AddUnaryOperator(string op, Func<JsonNode, JsonNode> evaluate)
    {
        Elements.Add(op, new UnaryOperatorGrammar(100, evaluate));
    }

    public enum PoolType
    {
        Functions,
        Transforms
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

    private Dictionary<string, Func<JsonArray, Task<JsonNode>>> GetPool(PoolType pool)
    {
        return pool switch
        {
            PoolType.Functions => Functions,
            PoolType.Transforms => Transforms,
            _ => throw new Exception("Invalid pool"),
        };
    }

    ///<summary>
    ///Add a single function call with no arguments to the grammar (only for functions).
    ///</summary>
    private void AddFunctionCall(PoolType poolName, string name, Func<Task<JsonNode>> func)
    {
        var pool = GetPool(poolName);
        pool.Add(name, async args => await func());
    }
    ///<summary>
    ///Add a single function call with no arguments to the grammar (only for functions).
    ///</summary>
    private void AddFunctionCall(PoolType poolName, string name, Func<JsonNode> func)
    {
        var pool = GetPool(poolName);
        pool.Add(name, args => Task.FromResult(func()));
    }
    ///<summary>
    ///Add a single function call with a single argument (can be an array or list) to the grammar.
    ///</summary>
    private void AddFunctionCall(PoolType poolName, string name, Func<JsonArray, Task<JsonNode>> func)
    {
        var pool = GetPool(poolName);
        pool.Add(name, func);
    }
    ///<summary>
    ///Add a single function call with a single argument (can be an array or list) to the grammar.
    ///</summary>
    private void AddFunctionCall(PoolType poolName, string name, Func<JsonArray, JsonNode> func)
    {
        var pool = GetPool(poolName);
        pool.Add(name, args => Task.FromResult(func(args)));
    }
    ///<summary>
    ///Add a single function call with a single argument (can be an array or list) to the grammar.
    ///</summary>
    private void AddFunctionCall(PoolType poolName, string name, Func<JsonValue, Task<JsonNode>> func)
    {
        // Define a new function that takes a single input parameter (array)
        Task<JsonNode> newFunc(JsonArray args)
        {
            if (args.Count == 1 && args[0] is JsonValue input1)
            {
                return func(input1);
            }
            else
            {
                throw new Exception("Unsupported arguments");
            }
        }
        AddFunctionCall(poolName, name, newFunc);
    }
    ///<summary>
    ///Add a single function call with a single argument (can be an array or list) to the grammar.
    ///</summary>
    private void AddFunctionCall(PoolType poolName, string name, Func<JsonValue, JsonNode> func)
    {
        // Define a new function that takes a single input parameter (array)
        JsonNode newFunc(JsonArray args)
        {
            if (args.Count == 1 && args[0] is JsonValue input1)
            {
                return func(input1);
            }
            else
            {
                throw new Exception("Unsupported arguments");
            }
        }
        AddFunctionCall(poolName, name, newFunc);
    }
    ///<summary>
    ///Add a single function call with a single argument (can be an array or list) to the grammar.
    ///</summary>
    private void AddFunctionCall(PoolType poolName, string name, Func<JsonObject, Task<JsonNode>> func)
    {
        // Define a new function that takes a single input parameter (array)
        Task<JsonNode> newFunc(JsonArray args)
        {
            if (args.Count == 1 && args[0] is JsonObject input1)
            {
                return func(input1);
            }
            else
            {
                throw new Exception("Unsupported arguments");
            }
        }
        AddFunctionCall(poolName, name, newFunc);
    }
    ///<summary>
    ///Add a single function call with a single argument (can be an array or list) to the grammar.
    ///</summary>
    private void AddFunctionCall(PoolType poolName, string name, Func<JsonObject, JsonNode> func)
    {
        // Define a new function that takes a single input parameter (array)
        JsonNode newFunc(JsonArray args)
        {
            if (args.Count == 1 && args[0] is JsonObject input1)
            {
                return func(input1);
            }
            else
            {
                throw new Exception("Unsupported arguments");
            }
        }
        AddFunctionCall(poolName, name, newFunc);
    }
    ///<summary>
    ///Add a single function call with a a first argument and a second argument (can be an array or list) to the grammar (only for transforms).
    ///</summary>
    private void AddFunctionCall(PoolType poolName, string name, Func<JsonNode, JsonNode, Task<JsonNode>> func)
    {
        // Define a new function that takes a single input parameter (array)
        Task<JsonNode> newFunc(JsonArray args)
        {
            if (args.Count < 2)
            {
                throw new Exception("Unsupported number of arguments for transform");
            }
            JsonNode input1 = args[0]!;
            JsonNode input2 = args[1]!;

            // Call the original function with both inputs joined as an array
            return func(input1, input2);
        }

        // Call the existing method with the new function
        AddFunctionCall(poolName, name, newFunc);
    }
    ///<summary>
    ///Add a single function call with a a first argument and a second argument (can be an array or list) to the grammar (only for transforms).
    ///</summary>
    private void AddFunctionCall(PoolType poolName, string name, Func<JsonNode, JsonNode, JsonNode> func)
    {
        // Define a new function that takes a single input parameter (array)
        JsonNode newFunc(JsonArray args)
        {
            if (args.Count < 2)
            {
                throw new Exception("Unsupported number of arguments for transform");
            }
            JsonNode input1 = args[0]!;
            JsonNode input2 = args[1]!;

            // Call the original function with both inputs joined as an array
            return func(input1, input2);
        }

        // Call the existing method with the new function
        AddFunctionCall(poolName, name, newFunc);
    }
    ///<summary>
    ///Add a single function call with a a first argument and a second argument (can be an array or list) to the grammar (only for transforms).
    ///</summary>
    private void AddFunctionCall(PoolType poolName, string name, Func<JsonNode, JsonArray, Task<JsonNode>> func)
    {
        // Define a new function that takes a single input parameter (array)
        Task<JsonNode> newFunc(JsonArray args)
        {
            if (args.Count < 2)
            {
                throw new Exception("Unsupported number of arguments for transform");
            }
            JsonNode input1 = args[0]!;
            JsonArray input2 = new(args.Skip(1).ToArray());

            // Call the original function with both inputs joined as an array
            return func(input1, input2);
        }

        // Call the existing method with the new function
        AddFunctionCall(poolName, name, newFunc);
    }
    ///<summary>
    ///Add a single function call with a a first argument and a second argument (can be an array or list) to the grammar (only for transforms).
    ///</summary>
    private void AddFunctionCall(PoolType poolName, string name, Func<JsonNode, JsonArray, JsonNode> func)
    {
        // Define a new function that takes a single input parameter (array)
        JsonNode newFunc(JsonArray args)
        {
            if (args.Count < 2)
            {
                throw new Exception("Unsupported number of arguments for transform");
            }
            JsonNode input1 = args[0]!;
            JsonArray input2 = new(args.Skip(1).ToArray());

            // Call the original function with both inputs joined as an array
            return func(input1, input2);
        }

        // Call the existing method with the new function
        AddFunctionCall(poolName, name, newFunc);
    }

    ///<summary>
    ///Add a dictionary of function calls with no arguments to the grammar (only for functions).
    ///</summary>
    private void AddFunctionCalls(PoolType poolName, Dictionary<string, Func<Task<JsonNode>>> funcsDict)
    {
        foreach (var kv in funcsDict)
        {
            AddFunctionCall(poolName, kv.Key, kv.Value);
        }
    }
    ///<summary>
    ///Add a dictionary of function calls with no arguments to the grammar (only for functions).
    ///</summary>
    private void AddFunctionCalls(PoolType poolName, Dictionary<string, Func<JsonNode>> funcsDict)
    {
        foreach (var kv in funcsDict)
        {
            AddFunctionCall(poolName, kv.Key, kv.Value);
        }
    }
    ///<summary>
    ///Add a dictionary of function calls with a single argument (can be an array or list) to the grammar.
    ///</summary>
    private void AddFunctionCalls(PoolType poolName, Dictionary<string, Func<JsonArray, Task<JsonNode>>> funcsDict)
    {
        foreach (var kv in funcsDict)
        {
            AddFunctionCall(poolName, kv.Key, kv.Value);
        }
    }
    ///<summary>
    ///Add a dictionary of function calls with a single argument (can be an array or list) to the grammar.
    ///</summary>
    private void AddFunctionCalls(PoolType poolName, Dictionary<string, Func<JsonArray, JsonNode>> funcsDict)
    {
        foreach (var kv in funcsDict)
        {
            AddFunctionCall(poolName, kv.Key, kv.Value);
        }
    }
    ///<summary>
    ///Add a dictionary of function calls with a a first argument and a second argument (can be an array or list) to the grammar (only for transforms).
    ///</summary>
    private void AddFunctionCalls(PoolType poolName, Dictionary<string, Func<JsonNode, JsonNode, Task<JsonNode>>> funcsDict)
    {
        foreach (var kv in funcsDict)
        {
            AddFunctionCall(poolName, kv.Key, kv.Value);
        }
    }
    ///<summary>
    ///Add a dictionary of function calls with a a first argument and a second argument (can be an array or list) to the grammar (only for transforms).
    ///</summary>
    private void AddFunctionCalls(PoolType poolName, Dictionary<string, Func<JsonNode, JsonNode, JsonNode>> funcsDict)
    {
        foreach (var kv in funcsDict)
        {
            AddFunctionCall(poolName, kv.Key, kv.Value);
        }
    }
    ///<summary>
    ///Add a dictionary of function calls with a a first argument and a second argument (can be an array or list) to the grammar (only for transforms).
    ///</summary>
    private void AddFunctionCalls(PoolType poolName, Dictionary<string, Func<JsonNode, JsonArray, Task<JsonNode>>> funcsDict)
    {
        foreach (var kv in funcsDict)
        {
            AddFunctionCall(poolName, kv.Key, kv.Value);
        }
    }
    ///<summary>
    ///Add a dictionary of function calls with a a first argument and a second argument (can be an array or list) to the grammar (only for transforms).
    ///</summary>
    private void AddFunctionCalls(PoolType poolName, Dictionary<string, Func<JsonNode, JsonArray, JsonNode>> funcsDict)
    {
        foreach (var kv in funcsDict)
        {
            AddFunctionCall(poolName, kv.Key, kv.Value);
        }
    }

    ///<summary> Add a function to the grammar with no arguments. </summary>
    public void AddFunction(string name, Func<Task<JsonNode>> func) => AddFunctionCall(PoolType.Functions, name, func);
    ///<summary> Add a function to the grammar with no arguments. </summary>
    public void AddFunction(string name, Func<JsonNode> func) => AddFunctionCall(PoolType.Functions, name, func);
    ///<summary> Add a function to the grammar with a single argument (can be an array or list). </summary>
    ///<summary> Add a function to the grammar with no arguments. </summary>
    public void AddFunction(string name, Func<JsonValue, Task<JsonNode>> func) => AddFunctionCall(PoolType.Functions, name, func);
    ///<summary> Add a function to the grammar with no arguments. </summary>
    public void AddFunction(string name, Func<JsonValue, JsonNode> func) => AddFunctionCall(PoolType.Functions, name, func);
    ///<summary> Add a function to the grammar with no arguments. </summary>
    public void AddFunction(string name, Func<JsonObject, Task<JsonNode>> func) => AddFunctionCall(PoolType.Functions, name, func);
    ///<summary> Add a function to the grammar with no arguments. </summary>
    public void AddFunction(string name, Func<JsonObject, JsonNode> func) => AddFunctionCall(PoolType.Functions, name, func);
    ///<summary> Add a function to the grammar with a single argument (can be an array or list). </summary>
    public void AddFunction(string name, Func<JsonArray, Task<JsonNode>> func) => AddFunctionCall(PoolType.Functions, name, func);
    ///<summary> Add a function to the grammar with a single argument (can be an array or list). </summary>
    public void AddFunction(string name, Func<JsonArray, JsonNode> func) => AddFunctionCall(PoolType.Functions, name, func);
    ///<summary> Add a dictionary of functions with no arguments to the grammar. </summary>
    public void AddFunctions(Dictionary<string, Func<JsonNode>> funcsDict) => AddFunctionCalls(PoolType.Functions, funcsDict);
    ///<summary> Add a dictionary of functions with a single argument (can be an array or list) to the grammar. </summary>
    public void AddFunctions(Dictionary<string, Func<JsonArray, JsonNode>> funcsDict) => AddFunctionCalls(PoolType.Functions, funcsDict);

    ///<summary> Add a transform with a single argument to the grammar. </summary>
    public void AddTransform(string name, Func<JsonValue, Task<JsonNode>> func) => AddFunctionCall(PoolType.Transforms, name, func);
    ///<summary> Add a transform with a single argument to the grammar. </summary>
    public void AddTransform(string name, Func<JsonValue, JsonNode> func) => AddFunctionCall(PoolType.Transforms, name, func);
    ///<summary> Add a transform with a single argument to the grammar. </summary>
    public void AddTransform(string name, Func<JsonObject, Task<JsonNode>> func) => AddFunctionCall(PoolType.Transforms, name, func);
    ///<summary> Add a transform with a single argument to the grammar. </summary>
    public void AddTransform(string name, Func<JsonObject, JsonNode> func) => AddFunctionCall(PoolType.Transforms, name, func);
    ///<summary> Add a transform with a single argument to the grammar. </summary>
    public void AddTransform(string name, Func<JsonArray, Task<JsonNode>> func) => AddFunctionCall(PoolType.Transforms, name, func);
    ///<summary> Add a transform with a single argument to the grammar. </summary>
    public void AddTransform(string name, Func<JsonArray, JsonNode> func) => AddFunctionCall(PoolType.Transforms, name, func);
    ///<summary> Add a transform with a a first argument and a second argument (can be an array or list) to the grammar. </summary>
    public void AddTransform(string name, Func<JsonNode, JsonNode, Task<JsonNode>> func) => AddFunctionCall(PoolType.Transforms, name, func);
    ///<summary> Add a transform with a a first argument and a second argument (can be an array or list) to the grammar. </summary>
    public void AddTransform(string name, Func<JsonNode, JsonNode, JsonNode> func) => AddFunctionCall(PoolType.Transforms, name, func);
    ///<summary> Add a transform with a a first argument and a second argument (can be an array or list) to the grammar. </summary>
    public void AddTransform(string name, Func<JsonNode, JsonArray, Task<JsonNode>> func) => AddFunctionCall(PoolType.Transforms, name, func);
    ///<summary> Add a transform with a a first argument and a second argument (can be an array or list) to the grammar. </summary>
    public void AddTransform(string name, Func<JsonNode, JsonArray, JsonNode> func) => AddFunctionCall(PoolType.Transforms, name, func);
    ///<summary> Add a dictionary of transforms with a single argument (can be an array or list) to the grammar. </summary>
    public void AddTransforms(Dictionary<string, Func<JsonArray, JsonNode>> funcsDict) => AddFunctionCalls(PoolType.Transforms, funcsDict);
    ///<summary> Add a dictionary of transforms with a a first argument and a second argument (can be an array or list) to the grammar. </summary>
    public void AddTransforms(Dictionary<string, Func<JsonNode, JsonNode, JsonNode>> funcsDict) => AddFunctionCalls(PoolType.Transforms, funcsDict);
    ///<summary> Add a dictionary of transforms with a a first argument and a second argument (can be an array or list) to the grammar. </summary>
    public void AddTransforms(Dictionary<string, Func<JsonNode, JsonArray, JsonNode>> funcsDict) => AddFunctionCalls(PoolType.Transforms, funcsDict);
}
