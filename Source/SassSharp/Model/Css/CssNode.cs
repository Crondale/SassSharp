using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SassSharp.Model.Css
{
    abstract class CssNode
    {
        private readonly List<CssNode> _nodes = new List<CssNode>();
        
        public int Level { get; set; }

        protected CssNode(int level)
        {
            Level = level;
        }

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
