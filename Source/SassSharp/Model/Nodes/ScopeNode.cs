using System.Collections.Generic;

namespace SassSharp.Model.Nodes
{
    class ScopeNode : CodeNode
    {
        private readonly List<CodeNode> _nodes = new List<CodeNode>();
        protected readonly Dictionary<string, VariableNode> _variables = new Dictionary<string, VariableNode>();

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


        public IEnumerable<CodeNode> Nodes
        {
            get { return _nodes; }
        }

        public virtual void Add(CodeNode node)
        {
            node.Parent = this;
            _nodes.Add(node);
        }

        public virtual void SetVariable(VariableNode node)
        {
            if (Parent.HasVariable(node.Name))
                Parent.SetVariable(node);
            else
            {
                _variables[node.Name] = node;
            }
        }

        public virtual bool HasVariable(string name)
        {
            if (_variables.ContainsKey(name))
                return true;

            return Parent.HasVariable(name);
        }

        public virtual VariableNode GetVariable(string name)
        {
            VariableNode result = null;

            if (_variables.TryGetValue(name, out result))
                return result;

            return Parent.GetVariable(name);
        }

        public virtual void SetMixin(MixinNode node)
        {
            Parent.SetMixin(node);
        }


        public virtual MixinNode GetMixin(string name)
        {
            return Parent.GetMixin(name);
        }

        public virtual void SetFunction(FunctionNode node)
        {
            Parent.SetFunction(node);
        }

        public virtual FunctionNode GetFunction(string name)
        {
            return Parent.GetFunction(name);
        }
    }
}