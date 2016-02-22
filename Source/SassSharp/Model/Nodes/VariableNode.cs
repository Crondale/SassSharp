using SassSharp.Model.Expressions;

namespace SassSharp.Model.Nodes
{
    internal class VariableNode : CodeNode
    {
        private Expression _expression;

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
            : this(name, (ExpressionNode) null)
        {
        }

        public bool Default { get; set; }

        public string Name { get; set; }

        public Expression Expression
        {
            get { return _expression; }
            set
            {
                if (value.Root is ValueList)
                {
                    var list = (ValueList) value.Root;
                    if (list[list.Count - 1].Value == "!default")
                    {
                        Default = true;

                        list.RemoveAt(list.Count - 1);
                    }
                }

                _expression = value;
            }
        }
    }
}