using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SassSharp.Model.Expressions;

namespace SassSharp.Model.Nodes
{
    class EachNode : ScopeNode
    {
        public EachNode(ValueList variables, ValueList list)
        {
            foreach (var variable in variables)
            {
                if (!(variable is ReferenceNode))
                    throw new Exception("Only variable names allowed in each");

                ReferenceNode referenceNode = (ReferenceNode) variable;

                Variables.Add(new VariableNode(referenceNode.VariableName));
            }


            List = list;
        }

        public ValueList List { get; }
        public List<VariableNode> Variables { get; } = new List<VariableNode>();
    }
}