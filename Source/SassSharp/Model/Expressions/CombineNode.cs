using System;
using SassSharp.Model.Nodes;

namespace SassSharp.Model.Expressions
{
    internal class CombineNode : ExpressionNode
    {
        public CombineNode(ExpressionNode a, ExpressionNode b, char op)
        {
            A = a;
            B = b;
            CombineOperator = op;
        }

        public char CombineOperator { get; set; }

        public ExpressionNode A { get; set; }

        public ExpressionNode B { get; set; }

        public override ValueNode Resolve(ScopeNode scope)
        {
            var a = A.Resolve(scope);
            var b = B.Resolve(scope);

            switch (CombineOperator)
            {
                case ' ':
                    return new ValueNode(a.Value + " " + b.Value);
            }

            throw new Exception($"Invalid operator: {Operator}");
        }
    }
}