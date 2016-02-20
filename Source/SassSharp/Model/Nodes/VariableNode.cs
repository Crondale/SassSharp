namespace SassSharp.Model.Nodes
{
    internal class VariableNode : CodeNode
    {

        public VariableNode(string name, Expression expression)
        {
            Name = name;
            Expression = expression;
        }

        public VariableNode(string name)
            :this(name, null)
        {
        }

        public string Name { get; set; }

        public Expression Expression { get; set; }
    }
}