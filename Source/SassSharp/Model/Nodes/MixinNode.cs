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

        public void Initialize(Expression[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                SetVariable(new VariableNode
                {
                    Name = _args[i].Name,
                    Expression = args[i]
                });
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