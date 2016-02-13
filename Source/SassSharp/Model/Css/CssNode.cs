using System.Collections.Generic;

namespace SassSharp.Model.Css
{
    internal abstract class CssNode
    {
        private readonly List<CssNode> _nodes = new List<CssNode>();

        protected CssNode(int level)
        {
            Level = level;
        }

        public int Level { get; set; }

        public IEnumerable<CssNode> Nodes
        {
            get { return _nodes; }
        }

        public void Add(CssNode node)
        {
            _nodes.Add(node);
        }
    }
}