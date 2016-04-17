namespace SassSharp.Model.Nodes
{
    internal class MixinNode : ScopeNode
    {
        private readonly VariableNode[] _args;

        public MixinNode(string name, VariableNode[] args)
        {
            Name = name;
            _args = args;
        }


        public string Name { get; set; }

        public void Initialize(IncludeNode includeNode)
        {
            SetContent(includeNode);
            var args = includeNode.Arguments;
            for (var i = 0; i < args.Count; i++)
            {
                SetVariable(new VariableNode(_args[i].Name, args[i]));
            }
        }

        public override void SetVariable(VariableNode node)
        {
            _variables[node.Name] = node;
        }

        public override string ToString()
        {
            return $"@mixin {Name}";
        }
    }
}