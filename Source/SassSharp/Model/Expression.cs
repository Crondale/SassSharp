using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crondale.SassSharp.Model.Expressions;
using Crondale.SassSharp.Model.Nodes;

namespace Crondale.SassSharp.Model
{
    class Expression
    {
        public Expression(ExpressionNode[] nodes)
        {
            Root = CalculateTree(nodes);
        }

        private ExpressionNode CalculateTree(ExpressionNode[] nodes)
        {
            if (nodes.Length == 1) return nodes[0];

            if (nodes.Length == 2) return new CombineNode(nodes[0], nodes[1], nodes[1].Operator);

            int president = int.MaxValue;

            for (int i = 1; i < nodes.Length; i++)
            {
                ExpressionNode filter = nodes[i];
                int sortIndex = getOperatorIndex(filter.Operator);

                if (sortIndex < president) president = sortIndex;
            }

            int index = 0;
            char op = ' ';
            for (index = 1; index < nodes.Length; index++)
            {
                ExpressionNode filter = nodes[index];
                if (getOperatorIndex(filter.Operator) == president)
                {
                    op = filter.Operator;
                    break;
                }
            }

            ExpressionNode a = CalculateTree(nodes.Take(index).ToArray());
            ExpressionNode b = CalculateTree(nodes.Skip(index).ToArray());

            return new CombineNode(a, b, op);
        }

        private int getOperatorIndex(char op)
        {
            switch (op)
            {
                case '+': return 1;
            }

            throw new Exception($"Unexpected operator: {op}");
        }

        public ExpressionNode Root { get; set; }

        public ValueNode Resolve(ScopeNode scope)
        {
            return Root.Resolve(scope);
        }
    }
}
