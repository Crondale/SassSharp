using SassSharp.Model.Expressions;

namespace SassSharp.Model.Nodes
{
    internal class VariableNode : CodeNode
    {

        public VariableNode(string name, Expression expression)
        {
            Name = name;
            Expression = expression;
        }

        public VariableNode(string name, ExpressionNode expression)
        {
            Name = name;
            Expression = new Expression(expression);
        }

        public VariableNode(string name)
            :this(name, (ExpressionNode) null)
        {
        }

        public string Name { get; set; }

        public Expression Expression { get; set; }
    }
}