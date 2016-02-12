using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SassSharp.Model.Nodes
{
    class CommentNode:CodeNode
    {
        public string Comment { get; set; }

        public CommentNode(string comment)
        {
            Comment = comment;
        }
    }
}
