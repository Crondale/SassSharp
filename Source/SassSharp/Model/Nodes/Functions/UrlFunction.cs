using SassSharp.Model.Expressions;

namespace SassSharp.Model.Nodes.Functions
{
    internal class UrlFunction : FunctionNode
    {
        public UrlFunction()
            : base("url", new[]
            {
                new VariableNode("path")
            })
        {
        }

        public override ExpressionNode Execute(ValueList args)
        {
            return new ValueNode($"url({args[0]})");
        }
    }
}