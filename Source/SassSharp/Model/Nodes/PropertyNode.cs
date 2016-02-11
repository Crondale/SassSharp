using System;
using System.Collections.Generic;

namespace Crondale.SassSharp.Model.Nodes
{
    class PropertyNode:CodeNode
    {

        public String Name { get; set; }

        public Expression Expression { get; set; }
        
    }
}
