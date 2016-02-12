using SassSharp.Model.Nodes;

namespace SassSharp.Model.Expressions
{
    internal class ValueNode : ExpressionNode
    {
        public ValueNode(string value)
        {
            Value = value;
        }

        public string Value { get; set; }

        public override ValueNode Resolve(ScopeNode scope)
        {
            return this;
        }
    }
}