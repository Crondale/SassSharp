using SassSharp.Model.Expressions;

namespace SassSharp.Model.Nodes.Functions
{
    internal class IfFunction : FunctionNode
    {
        public IfFunction()
            : base("if", new[]
            {
                new VariableNode("test"),
                new VariableNode("success"),
                new VariableNode("fail")
            })
        {
        }

        public override ExpressionNode Execute(ValueList args)
        {
            if (args[0].Value == "1")
            {
                return args[1];
            }
            return args[2];
        }
    }
}