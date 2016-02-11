using System;
using System.Collections.Generic;

namespace Crondale.SassSharp.Model.Nodes
{
    class MixinNode:ScopeNode
    {
        readonly Dictionary<string, VariableNode> _variables = new Dictionary<string, VariableNode>();
        private readonly VariableNode[] _args;

        public MixinNode(string name, VariableNode[] args)
        {
            Name = name;
            _args = args;
        }

        public void Initialize(Expression[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                SetVariable(new VariableNode()
                {
                    Name = _args[i].Name,
                    Expression = args[i]
                });
            }
        }

        public override void SetVariable(VariableNode node)
        {
            _variables[node.Name] = node;
        }

        public override VariableNode GetVariable(string name)
        {
            VariableNode result = null;

            if (!_variables.TryGetValue(name, out result))
                return Parent.GetVariable(name);

            return result;
        }

        public string Name { get; set; }
    }
}
