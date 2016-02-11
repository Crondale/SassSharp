using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crondale.SassSharp.Model.Nodes;

namespace Crondale.SassSharp.Model.Expressions
{
    class ReferenceNode:ExpressionNode
    {

        public ReferenceNode(string variableName)
        {
            VariableName = variableName;
        }

        public string VariableName { get; set; }

        public override ValueNode Resolve(ScopeNode scope)
        {
            return scope.GetVariable(VariableName).Expression.Resolve(scope);
        }
    }
}
