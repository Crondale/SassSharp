using System.Collections.Generic;

namespace Crondale.SassSharp.Model.Nodes
{
    class VariableNode:CodeNode
    {
        public string Name { get; set; }

        public Expression Expression { get; set; }

    }
}
