using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SassSharp.Model.Nodes
{
    class ExtendNode : ScopeNode
    {
        public ExtendNode(string selector)
        {
            Selector = selector;
        }

        public string Selector { get; set; }
    }
}