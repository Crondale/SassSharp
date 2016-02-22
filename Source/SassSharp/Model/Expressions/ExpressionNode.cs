using SassSharp.Model.Nodes;

namespace SassSharp.Model.Expressions
{
    internal abstract class ExpressionNode
    {
        public char Operator { get; set; }

        public abstract string Value { get; }

        public abstract ExpressionNode Resolve(ScopeNode scope);

        public static explicit operator int(ExpressionNode v)
        {
            return int.Parse(v.Value);
        }

        public static explicit operator string(ExpressionNode v)
        {
            return v.Value;
        }


        public override string ToString()
        {
            return Value;
        }
    }
}