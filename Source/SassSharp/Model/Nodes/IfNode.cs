using System.Collections.Generic;
using SassSharp.Model.Expressions;

namespace SassSharp.Model.Nodes
{
    internal class IfNode : ScopeNode
    {
        private readonly ExpressionNode _condition;

        public IfNode(ValueList condition)
        {
            _condition = condition;
            Elses = new List<ElseNode>();
        }

        public List<ElseNode> Elses { get; }

        public ScopeNode GetActiveScope(ScopeNode currentScope)
        {
            if (Test(currentScope))
                return this;

            foreach (var elseNode in Elses)
            {
                if (elseNode.Test(currentScope))
                    return elseNode;
            }

            return null;
        }

        public bool Test(ScopeNode currentScope)
        {
            var result = _condition.Resolve(currentScope);

            return result.Value == "1";
        }

        public override void SetVariable(VariableNode node)
        {
            _variables[node.Name] = node;
        }

        public override string ToString()
        {
            return $"@if {_condition}";
        }
    }
}