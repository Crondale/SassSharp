using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SassSharp.Model.Nodes
{
    class ForNode : ScopeNode
    {
        public ForNode(VariableNode variable, Expression from, Expression through)
        {
            Variable = variable;
            From = @from;
            Through = through;
        }

        public VariableNode Variable { get; }
        public Expression From { get; set; }
        public Expression Through { get; set; }
    }
}