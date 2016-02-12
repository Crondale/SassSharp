using SassSharp.Model.Nodes;

namespace SassSharp.Model.Expressions
{
    internal abstract class ExpressionNode
    {
        public char Operator { get; set; }

        public abstract ValueNode Resolve(ScopeNode scope);
    }
}