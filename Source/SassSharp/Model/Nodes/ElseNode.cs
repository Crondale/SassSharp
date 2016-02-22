using SassSharp.Model.Expressions;

namespace SassSharp.Model.Nodes
{
    internal class ElseNode : ScopeNode
    {
        private readonly ExpressionNode _condition;

        public ElseNode(ValueList condition)
        {
            _condition = condition;
        }

        public ElseNode()
        {
            _condition = new ValueNode("true");
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
            return $"@else if {_condition}";
        }
    }
}