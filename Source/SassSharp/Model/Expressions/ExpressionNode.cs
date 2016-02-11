using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crondale.SassSharp.Model.Nodes;

namespace Crondale.SassSharp.Model.Expressions
{
    abstract class ExpressionNode
    {
        public char Operator { get; set; }

        public abstract ValueNode Resolve(ScopeNode scope);
    }
}
