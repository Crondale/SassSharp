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

        public override string Value
        {
            get { return $"{A.Value} {CombineOperator} {B.Value}"; }
        }

        public override ExpressionNode Resolve(ScopeNode scope)
        {
            var a = A.Resolve(scope) as ValueNode;
            var b = B.Resolve(scope) as ValueNode;

            if (a == null || b == null)
                throw new InvalidOperationException("Can only combine values");

            switch (CombineOperator)
            {
                case '*':
                    return a*b;
                case '+':
                    return a + b;
                case '-':
                    return a - b;
                case '/':
                    return a/b;
                case '=':
                    return ValueNode.ValueEquals(a, b);
                case '<':
                    return a < b;
                case '>':
                    return a > b;
            }

            throw new Exception($"Invalid operator: {CombineOperator}");
        }
    }
}