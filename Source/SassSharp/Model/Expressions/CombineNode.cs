using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crondale.SassSharp.Model.Nodes;

namespace Crondale.SassSharp.Model.Expressions
{

    class CombineNode:ExpressionNode
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
            ValueNode a = A.Resolve(scope);
            ValueNode b = B.Resolve(scope);

            switch (CombineOperator)
            {
                case ' ':
                    return new ValueNode(a.Value + " " + b.Value);
            }

            throw new Exception($"Invalid operator: {Operator}");

        }
    }
}
