using System;
using System.Linq;
using SassSharp.Model.Expressions;
using SassSharp.Model.Nodes;

namespace SassSharp.Model
{
    internal class Expression
    {
        public Expression(ExpressionNode root)
        {
            Root = root;
        }

        public ExpressionNode Root { get; set; }

        public bool Empty
        {
            get { return Root == null || (Root is ValueList && ((ValueList) Root).Count == 0); }
        }

        public static ValueList CalculateList(ExpressionNode[] nodes)
        {
            if (nodes.Length == 0)
                return new ValueList();

            for (var i = 1; i < nodes.Length; i++)
            {
                var filter = nodes[i];

                if (filter.Operator == ',')
                    return CalculateCommaList(nodes);
            }

            for (var i = 1; i < nodes.Length; i++)
            {
                var filter = nodes[i];

                if (filter.Operator == ' ')
                    return CalculateSpaceList(nodes);
            }

            return new ValueList(CalculateTree(nodes));
        }

        private static ValueList CalculateCommaList(ExpressionNode[] nodes)
        {
            var result = new ValueList();
            result.PreferComma = true;
            var skip = 0;
            for (var i = 1; i < nodes.Length; i++)
            {
                var filter = nodes[i];

                if (filter.Operator == ',')
                {
                    result.Add(CalculateList(nodes.Skip(skip).Take(i - skip).ToArray()));
                    skip = i;
                }
            }

            result.Add(CalculateList(nodes.Skip(skip).ToArray()));

            return result;
        }

        private static ValueList CalculateSpaceList(ExpressionNode[] nodes)
        {
            var result = new ValueList();
            var skip = 0;
            for (var i = 1; i < nodes.Length; i++)
            {
                var filter = nodes[i];

                if (filter.Operator == ' ')
                {
                    var a = CalculateList(nodes.Skip(skip).Take(i - skip).ToArray());
                    result.Add(a);
                    skip = i;
                }
            }

            result.Add(CalculateList(nodes.Skip(skip).ToArray()));

            return result;
        }

        public static ExpressionNode CalculateTree(ExpressionNode[] nodes)
        {
            if (nodes.Length == 0)
                return null;

            if (nodes.Length == 1) return nodes[0];

            if (nodes.Length == 2) return new CombineNode(nodes[0], nodes[1], nodes[1].Operator);

            var president = int.MaxValue;

            for (var i = 1; i < nodes.Length; i++)
            {
                var filter = nodes[i];
                var sortIndex = getOperatorIndex(filter.Operator);

                if (sortIndex < president) president = sortIndex;
            }

            var index = 0;
            var op = '+';
            for (index = 1; index < nodes.Length; index++)
            {
                var filter = nodes[index];
                if (getOperatorIndex(filter.Operator) == president)
                {
                    op = filter.Operator;
                    break;
                }
            }

            var a = CalculateTree(nodes.Take(index).ToArray());
            var b = CalculateTree(nodes.Skip(index).ToArray());

            return new CombineNode(a, b, op);
        }

        private static int getOperatorIndex(char op)
        {
            switch (op)
            {
                case '=':
                    return 1;
                case '<':
                    return 2;
                case '>':
                    return 3;
                case '+':
                    return 11;
                case '-':
                    return 12;
                case '*':
                    return 13;
                case '/':
                    return 14;
            }

            throw new Exception($"Unexpected operator: {op}");
        }

        public ExpressionNode Resolve(ScopeNode scope)
        {
            return Root.Resolve(scope);
        }
    }
}