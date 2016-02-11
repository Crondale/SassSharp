using System;
using System.Collections.Generic;

namespace Crondale.SassSharp.Model
{
    class ScopeNode : CodeNode
    {
        private readonly List<CodeNode> _nodes = new List<CodeNode>();
        Dictionary<string, VariableNode> _variables = new Dictionary<string, VariableNode>();

        public IEnumerable<CodeNode> Nodes
        {
            get { return _nodes; }
        }

        public ScopeNode()
        {
            
        }

        public ScopeNode(params CodeNode[] nodes)
        {
            foreach (var node in nodes)
            {
                Add(node);
            }
        }

        public virtual void Add(CodeNode node)
        {
            if (node is VariableNode)
            {
                SetVariable((VariableNode) node);
            }
            else
            {
                node.Parent = this;
                _nodes.Add(node);
            }
        }

        private void SetVariable(VariableNode node)
        {
            _variables[node.Name] = node;
        }

        public VariableNode GetVariable(string name)
        {
            VariableNode result = null;

            if (!_variables.TryGetValue(name, out result))
            {
                if (Parent == null)
                    throw new Exception($"Could not find variable: {name}");

                return Parent.GetVariable(name);

            }
            
            return _variables[name];
        }

    }
}
