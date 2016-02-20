using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SassSharp.Model.Expressions;

namespace SassSharp.Model.Nodes
{
    class ReturnNode:CodeNode
    {
        public Expression Expression { get; set; }

        public ReturnNode(Expression expression)
        {
            Expression = expression;
        }
    }
}
