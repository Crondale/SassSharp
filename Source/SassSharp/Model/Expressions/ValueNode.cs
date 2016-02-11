using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crondale.SassSharp.Model.Nodes;

namespace Crondale.SassSharp.Model.Expressions
{
    class ValueNode:ExpressionNode
    {

        public ValueNode(string value)
        {
            Value = value;
        }

        public string Value { get; set; }

        public override ValueNode Resolve(ScopeNode scope)
        {
            return this;
        }
    }
}
