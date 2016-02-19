﻿using System;
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
            get { return Root == null; }
        }

        public static ExpressionNode CalculateTree(ExpressionNode[] nodes)
        {
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
                case '+':
                    return 1;
                case '-':
                    return 2;
                case '*':
                    return 3;
                case '/':
                    return 4;
            }

            throw new Exception($"Unexpected operator: {op}");
        }

        public ValueNode Resolve(ScopeNode scope)
        {
            return Root.Resolve(scope);
        }
    }
}