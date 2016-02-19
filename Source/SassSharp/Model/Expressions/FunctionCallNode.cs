using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SassSharp.Model.Nodes;

namespace SassSharp.Model.Expressions
{
    class FunctionCallNode:ExpressionNode
    {
        private readonly ValueList _args;

        public string Name { get; set; }

        public FunctionCallNode(string name, ValueList args)
        {
            _args = args;
            Name = name;
        }


        public override ExpressionNode Resolve(ScopeNode scope)
        {
            var args = ValueList.From(_args.Resolve(scope));

            switch (Name)
            {
                case "nth":
                    var vs = ValueList.From(args[0]);

                    int index = (int)args[1];
                    
                    return vs[index - 1];
            }


            throw new NotImplementedException();
        }

        public override string Value
        {
            get { return $"{Name}({_args.Value})"; }
        }
    }
}
