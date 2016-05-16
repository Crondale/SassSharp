using SassSharp.Model.Expressions;

namespace SassSharp.Model.Nodes.Functions
{
    internal class CssFunction : FunctionNode
    {
        public CssFunction(string name)
            : base(name, new[]
            {
                new VariableNode("arg0"),
                new VariableNode("arg1"),
                new VariableNode("arg2")
            })
        {
        }

        public override ExpressionNode Execute(ValueList args)
        {
            string arguments = args[0].ToString();

            return new ValueNode($"url({arguments})");
        }
    }
}