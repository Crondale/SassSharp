using System.Collections.Generic;
using System.Linq;

namespace SassSharp.Model.Nodes
{
    internal class SelectorNode : ScopeNode
    {
        public SelectorNode()
        {
        }

        public ScssString Selector { get; set; }

        public override string ToString()
        {
            return Selector.ToString();
        }
    }
}