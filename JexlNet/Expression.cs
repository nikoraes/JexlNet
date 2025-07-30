using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace JexlNet
{
    public interface IExpression
    {
        Expression Compile();
        Task<JsonNode> EvalAsync(
            JsonObject context = null,
            CancellationToken cancellationToken = default
        );
        JsonNode Eval(JsonObject context = null, CancellationToken cancellationToken = default);
    }

    public class Expression : IExpression
    {
        public Expression(string exprStr)
        {
            Grammar = new Grammar();
            ExprStr = exprStr;
        }

        public Expression(Grammar grammar, string exprStr)
        {
            Grammar = grammar;
            ExprStr = exprStr;
        }

        // Pass precompiled AST
        public Expression(Node ast)
        {
            Grammar = new Grammar();
            _ast = ast;
        }

        public Expression(Grammar grammar, Node ast)
        {
            Grammar = grammar;
            _ast = ast;
        }

        private readonly Grammar Grammar;
        private readonly string ExprStr;
        private Node _ast = null;

        /// <summary>
        /// Forces a compilation of the expression string that this Expression object
        /// was constructed with.This function can be called multiple times; useful
        /// if the language elements of the associated Jexl instance change.
        /// </summary>
        /// <returns>this Expression instance, for convenience</returns>
        public virtual Expression Compile()
        {
            var lexer = new Lexer(Grammar);
            var parser = new Parser(Grammar);
            var tokens = lexer.Tokenize(ExprStr);
            parser.AddTokens(tokens);
            _ast = parser.Complete();
            return this;
        }

        /// <summary>
        /// Asynchronously evaluates the expression within an optional context.
        /// </summary>
        /// <param name="context">A mapping of variables to values, which will be
        /// made accessible to the Jexl expression when evaluating it.</param>
        /// <returns>Resolves with the resulting value of the expression.</returns>
        public async Task<JsonNode> EvalAsync(
            JsonObject context = null,
            CancellationToken cancellationToken = default
        )
        {
            Evaluator evaluator = new Evaluator(Grammar, context);
            return await evaluator.EvalAsync(GetAst(), cancellationToken);
        }

        /// <summary>
        /// Asynchronously evaluates the expression within an optional context.
        /// </summary>
        /// <param name="context">A mapping of variables to values, which will be
        /// made accessible to the Jexl expression when evaluating it.</param>
        /// <returns>Resolves with the resulting value of the expression.</returns>
        public JsonNode Eval(
            JsonObject context = null,
            CancellationToken cancellationToken = default
        )
        {
            Evaluator evaluator = new Evaluator(Grammar, context);
            Task<JsonNode> task = evaluator.EvalAsync(GetAst(), cancellationToken);
            return task?.GetAwaiter().GetResult();
        }

        private Node GetAst()
        {
            if (_ast == null)
                Compile();
            return _ast;
        }
    }
}
