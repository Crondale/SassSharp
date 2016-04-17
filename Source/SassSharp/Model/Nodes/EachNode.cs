using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SassSharp.Model.Nodes
{
    class EachNode : ScopeNode
    {
        public EachNode(VariableNode variable, ValueList list)
        {
            Variable = variable;
            List = list;
        }

        public ValueList List { get; }
        public VariableNode Variable { get; }
    }
}