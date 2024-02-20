# JexlNet

Powerful context-based expression parser and evaluator in C# using JEXL (Javascript Expression Language) expressions.

NOTE: This library handles the JEXL from [TomFrost's JEXL library](https://github.com/TomFrost/Jexl). It does NOT handle the similarly-named Apache Commons JEXL language.

## Quick start

Expressions can be evaluated synchronously or asynchronously by using the `Eval` and `EvalAsync` methods respectively.
Context should be a JsonObject (or a string which will internally be converted to a JsonObject). The output is always a JsonNode (can be JsonObject, JsonArray or JsonValue).

The Grammar can be expanded by adding new operators, functions and transforms.

```csharp
// Define context as a JsonObject (System.Text.Json)
var context = new JsonObject {
    { "name", new JsonObject {
        { "first", "Sterling" },
        { "last", "Archer" }
    }},
    { "assoc", new JsonArray {
        new JsonObject {
            { "first", "Lana" },
            { "last", "Kane" }
        },
        new JsonObject {
            { "first", "Cyril" },
            { "last", "Figgis" }
        },
        new JsonObject {
            { "first", "Pam" },
            { "last", "Poovey" }
        }
    }},
    { "age", 36 }
};

// Or as a JSON string
string context = 
@"{
    ""name"": {
        ""first"": ""Sterling"",
        ""last"": ""Archer""
    },
    ""assoc"": [
        {
            ""first"": ""Lana"",
            ""last"": ""Kane""
        },
        {
            ""first"": ""Cyril"",
            ""last"": ""Figgis""
        },
        {
            ""first"": ""Pam"",
            ""last"": ""Poovey""
        }
    ],
    ""age"": 36
}";

// Initialize Jexl
var jexl = new Jexl();

// Use it with asynchronously or synchronously:

// Filter an array asynchronously...
await jexl.EvalAsync(@"assoc[.first == ""Lana""].last", context);
// Kane

// Or synchronously!
jexl.Eval(@"assoc[.first == ""Lana""].last", context);
// Kane

// Do math
await jexl.Eval(@"age * (3 - 1)", context);
// 72

// Concatenate
await jexl.EvalAsync(@"name.first + "" "" + name[""la"" + ""st""]", context);
// "Sterling Archer"

// Compound
await jexl.EvalAsync(
  'assoc[.last == "Figgis"].first == "Cyril" && assoc[.last == "Poovey"].first == "Pam"',
  context
)
// true

// Use array indexes and return objects
await jexl.EvalAsync(@"assoc[1]", context);
// new JsonObject {
//             { "first", "Cyril" },
//             { "last", "Figgis" }
//         }
// 

// Use conditional logic
await jexl.EvalAsync(@"age > 62 ? ""retired"" : ""working""", context);
// "working"

// Transform
jexl.Grammar.AddTransform("upper", (JsonValue val) => val?.ToString().ToUpper());
await jexl.EvalAsync(@"""duchess""|upper + "" "" + name.last|upper", context);
// "DUCHESS ARCHER"

// Transform asynchronously, with arguments
jexl.Grammar.AddTransform("getStat", async (JsonNode[] args) => await DbSelectByLastName(args[0], args[1]));
await jexl.EvalAsync(@"name.last|getStat(""weight"")", context);
// Output: 184

// Functions too, sync or async, args or no args
jexl.Grammar.AddFunction("getOldestAgent", GetOldestAgent);
await jexl.EvalAsync(@"age == getOldestAgent().age", context);
// false

// Add your own (a)synchronous operators
// Here's a case-insensitive string equality
jexl.Grammar.AddBinaryOperator("_=", 20, (JsonNode[] args) => args[0]?.ToString().ToLower() == args[1]?.ToString().ToLower());
await jexl.EvalAsync(@"""Guest"" _= ""gUeSt""");
// true

// Compile your expression once, evaluate many times!
const { expr } = jexl
var danger = jexl.CreateExpression(@"""Danger "" + place");
await danger.EvalAsync(new JsonObject { { "place", "Zone" } }); // Danger zone
await danger.EvalAsync(new JsonObject { { "place", "ZONE!!!" } }); // Danger ZONE!!! (Doesn't recompile the expression!)
```

## Play with it

- [Jexl Playground](https://czosel.github.io/jexl-playground/) - An interactive Jexl sandbox by Christian Zosel [@czosel](https://github.com/czosel).

## Installation

Install from NuGet:

```powershell
    Install-Package JexlNet
```

Add using statement:

```csharp
    using JexlNet;
```

And use it:

```csharp
var jexl = new Jexl();
var result = jexl.Eval("1 + 1");
```

## Async vs Sync: Which to use

There is little performance difference between `EvalAsync` and `Eval`. Both support async functions and transforms. The only difference is that `EvalAsync` returns a `Task<JsonNode>` and `Eval` returns a `JsonNode`.

## All the details

### Unary Operators

| Operation | Symbol |
| --------- | :----: |
| Negate    |   !    |

### Binary Operators

| Operation        |    Symbol    |
| ---------------- | :----------: |
| Add, Concat      |      +       |
| Subtract         |      -       |
| Multiply         |      *       |
| Divide           |      /       |
| Divide and floor |      //      |
| Modulus          |      %       |
| Power of         |      ^       |
| Logical AND      |      &&      |
| Logical OR       | &#124;&#124; |

### Comparisons

| Comparison                 | Symbol |
| -------------------------- | :----: |
| Equal                      |   ==   |
| Not equal                  |   !=   |
| Greater than               |   >    |
| Greater than or equal      |   >=   |
| Less than                  |   <    |
| Less than or equal         |   <=   |
| Element in array or string |   in   |

#### A note about `in`

The `in` operator can be used to check for a substring:
`"Cad" in "Ron Cadillac"`, and it can be used to check for an array element:
`"coarse" in ['fine', 'medium', 'coarse']`. However, the `==` operator is used
behind-the-scenes to search arrays, so it should not be used with arrays of
objects. The following expression returns false: `{a: 'b'} in [{a: 'b'}]`.

### Ternary operator

Conditional expressions check to see if the first segment evaluates to a truthy
value. If so, the consequent segment is evaluated. Otherwise, the alternate
is. If the consequent section is missing, the test result itself will be used
instead.

| Expression                        | Result |
| --------------------------------- | ------ |
| "" ? "Full" : "Empty"             | Empty  |
| "foo" in "foobar" ? "Yes" : "No"  | Yes    |
| {agent: "Archer"}.agent ?: "Kane" | Archer |

### Native Types

| Type     |            Examples            |
| -------- | :----------------------------: |
| Booleans |        `true`, `false`         |
| Strings  | "Hello \"user\"", 'Hey there!' |
| Numerics |      6, -7.2, 5, -3.14159      |
| Objects  |       {hello: "world!"}        |
| Arrays   |      ['hello', 'world!']       |

### Groups

Parentheses work just how you'd expect them to:

| Expression                          | Result |
| ----------------------------------- | :----- |
| (83 + 1) / 2                        | 42     |
| 1 < 3 && (4 > 2 &#124;&#124; 2 > 4) | true   |

### Identifiers

Access variables in the context object by just typing their name. Objects can
be traversed with dot notation, or by using brackets to traverse to a dynamic
property name.

Example context:

```csharp
var context = new JsonObject 
{
    { "name", new JsonObject {
        { "first", "Malory" },
        { "last", "Archer" }
    }},
    { "exes", new List<string> {
        "Nikolai Jakov",
        "Len Trexler",
        "Burt Reynolds"
    }},
    { "lastEx", 2 }
};
```

| Expression        | Result        |
| ----------------- | ------------- |
| name.first        | Malory        |
| name['la' + 'st'] | Archer        |
| exes[2]           | Burt Reynolds |
| exes[lastEx - 1]  | Len Trexler   |

### Collections

Collections, or arrays of objects, can be filtered by including a filter
expression in brackets. Properties of each collection can be referenced by
prefixing them with a leading dot. The result will be a list of the objects
for which the filter expression resulted in a truthy value.

Example context:

```csharp
var context = new JsonObject
{
    {
        "employees", new List<dynamic>
        {
            new JsonObject { { "first", "Sterling" }, { "last", "Archer" }, { "age", 36 } },
            new JsonObject { { "first", "Malory" }, { "last", "Archer" }, { "age", 75 } },
            new JsonObject { { "first", "Lana" }, { "last", "Kane" }, { "age", 33 } },
            new JsonObject { { "first", "Cyril" }, { "last", "Figgis" }, { "age", 45 } },
            new JsonObject { { "first", "Cheryl" }, { "last", "Tunt" }, { "age", 28 } }
        }
    },
    { "retireAge", 62 }
};
```

| Expression                                    | Result                                                                                |
| --------------------------------------------- | ------------------------------------------------------------------------------------- |
| employees[.first == 'Sterling']               | [{first: 'Sterling', last: 'Archer', age: 36}]                                        |
| employees[.age >= 30 && .age < 40]            | [{first: 'Sterling', last: 'Archer', age: 36},{first: 'Lana', last: 'Kane', age: 33}] |
| employees[.age >= 30 && .age < 40][.age < 35] | [{first: 'Lana', last: 'Kane', age: 33}]                                              |
| employees[.last == 'Tu' + 'nt'][0].first      | Cheryl                                                                                |
| employees[.last == 'Tu' + 'nt'].first         | Cheryl                                                                                |
| employees[.age >= retireAge][0].first         | Malory                                                                                |
| employees[.age >= retireAge].first            | Malory                                                                                |

#### Note about collection filters

The collection filter returns a filtered collection, but if you add a dot and identifier after the filter, it will return the first matching object's property. If you want to return a collection of properties, use the `map` transform in the [Extended Grammar](#extended-grammar).

### Transforms

The power of Jexl is in transforming data, synchronously or asynchronously.
Transform functions take one argument or an array or list of more arguments.
The first argument is the value to be transformed, and the rest are any other
arguments passed to the transform in the expression. They must return either
the transformed value, or a Promise that resolves with the transformed
value. Add them with `jexl.AddTransform(name, function)`.
Arguments can be `JsonNode` or `JsonNode[]`. In case of
enumerables the first element is the value to be transformed and the rest
are the arguments. It is also possible to use a first `JsonNode` argument as
the value to be transformed and the rest as the arguments.

```csharp
jexl.Grammar.AddTransform("lower", (JsonNode val) => val?.ToString().ToLower());
jexl.Grammar.AddTransform("split", (JsonNode[] args) => new JsonArray((args[0]?.ToString().Split(args[1]?.ToString()) ?? []).Select(x => (JsonNode)x).ToArray()));
jexl.Grammar.AddTransform("split", (JsonNode arg0, JsonNode[] args) => new JsonArray((arg0?.ToString().Split(args[0]?.ToString()) ?? []).Select(x => (JsonNode)x).ToArray()));
```

| Expression                                 | Result                |
| ------------------------------------------ | --------------------- |
| "Pam Poovey"&#124;lower&#124;split[' '](1) | poovey                |
| "password==guest"&#124;split('=' + '=')    | ['password', 'guest'] |

Many examples can be found in the [Extended Grammar](#extended-grammar) [code](/JexlNet.ExtendedGrammar/ExtendedGrammar.cs) and [tests](/JexlNet.Test/ExtendedGrammar.cs).

### Functions

While Transforms are the preferred way to change one value into another value,
Jexl also allows top-level expression functions to be defined. Use these to
provide access to functions that either don't require an input, or require
multiple equally-important inputs. They can be added with
`jexl.AddFunction(name, function)`. Like transforms, functions can return a
value, or a Promise that resolves to the resulting value.
For functions, arguments are not required, but if they are defined,
they must be `JsonNode` or `JsonNode[]`.

```csharp
jexl.Grammar.AddFunction("getTrue", () => true);
jexl.Grammar.AddFunction("min", (JsonNode[] args) => args.Select(x => x?.AsValue().ToDecimal()).Min());
jexl.Grammar.AddFunction("expensiveQuery", Db.RunExpensiveQuery);
```

| Expression                                    | Result                    |
| --------------------------------------------- | ------------------------- |
| false || getTrue()                            | true                      |
| min(4, 2, 19)                                 | 2                         |
| counts.missions &#124;&#124; expensiveQuery() | Query only runs if needed |

Many examples can be found in the [Extended Grammar](#extended-grammar) [code](/JexlNet.ExtendedGrammar/ExtendedGrammar.cs) and [tests](/JexlNet.Test/ExtendedGrammar.cs).

## Extended Grammar

By installing the `JexlNet.ExtendedGrammar` package, you get access to a large number of additional builtin functions and transforms.

Install from NuGet:

```powershell
    Install-Package JexlNet.ExtendedGrammar
```

Add using statement and use extended grammar instead of default grammar:

```csharp
    using JexlNet;
    using JexlNet.ExtendedGrammar;
```

```csharp
    // Initialize Jexl with extended grammar
    var jexl = new Jexl(new ExtendedGrammar());
```

See the [Extended Grammar code](/JexlNet.ExtendedGrammar/ExtendedGrammar.cs) for a list of all the builtin functions and transforms and the [Extended Grammar tests](/JexlNet.Test/ExtendedGrammar.cs) for examples of how to use the extended grammar.

## Other implementations

- [Jexl](https://github.com/TomFrost/Jexl) - The original JavaScript implementation of JEXL.
- [jexl-rs](https://github.com/mozilla/jexl-rs) - A Rust-based JEXL parser and evaluator.
- [PyJEXL](https://github.com/mozilla/pyjexl) - A Python-based JEXL parser and evaluator.

## License

JexlNet is licensed under the MIT license. Please see `LICENSE` for full details.

## Credits

This library is a port of [TomFrost's JEXL library](https://github.com/TomFrost/Jexl) so all credit goes to the author and the contributors of that library.
