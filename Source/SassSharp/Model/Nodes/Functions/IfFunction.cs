using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SassSharp.Model.Expressions;

namespace SassSharp.Model.Nodes.Functions
{
    class IfFunction:FunctionNode
    {
        public IfFunction() 
            : base("if", new VariableNode[]
        {
            new VariableNode("test"),
            new VariableNode("success"),
            new VariableNode("fail")
        })
        {
        }

        public override ExpressionNode Execute(ValueList args)
        {
            if (args[0].Value == "1")
            {
                return args[1];
            }
            else
            {
                return args[2];
            }
        }
    }
}
