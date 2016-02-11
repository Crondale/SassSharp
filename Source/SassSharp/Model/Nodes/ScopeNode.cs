using System.Collections.Generic;

namespace Crondale.SassSharp.Model.Nodes
{
    class ScopeNode : CodeNode
    {
        private readonly List<CodeNode> _nodes = new List<CodeNode>();
        

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
            node.Parent = this;
            _nodes.Add(node);
        }

        public virtual void SetVariable(VariableNode node)
        {
            Parent.SetVariable(node);
        }

        public virtual VariableNode GetVariable(string name)
        {

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

    }
}
