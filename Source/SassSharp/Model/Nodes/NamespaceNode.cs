using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crondale.SassSharp.Model.Nodes
{
    class NamespaceNode:ScopeNode
    {
        public NamespaceNode(PropertyNode header)
        {
            Header = header;
        }

        public PropertyNode Header { get; set; }


    }
}
