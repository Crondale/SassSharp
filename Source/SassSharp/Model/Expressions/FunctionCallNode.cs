using SassSharp.Model.Nodes;

namespace SassSharp.Model.Expressions
{
    internal class FunctionCallNode : ExpressionNode
    {
        private readonly ValueList _args;

        public FunctionCallNode(string name, ValueList args)
        {
            _args = args;
            Name = name;
        }

        public string Name { get; set; }

        public override string Value
        {
            get { return $"{Name}({_args.Value})"; }
        }


        public override ExpressionNode Resolve(ScopeNode scope)
        {
            var args = ValueList.From(_args.Resolve(scope));

            switch (Name)
            {
                case "nth":
                {
                    var vs = ValueList.From(args[0]);

                    var index = (int) args[1];

                    return vs[index - 1];
                }
                case "map-get":
                {
                    var vs = ValueList.From(args[0]);

                    var key = (string) args[1];

                    return vs[key];
                }
            }

            var function = scope.GetFunction(Name);

            var result = function.Execute(args);

            return result;
        }
    }
}