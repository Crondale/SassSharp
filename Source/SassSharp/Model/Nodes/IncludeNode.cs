namespace Crondale.SassSharp.Model.Nodes
{
    class IncludeNode:ScopeNode
    {
        public IncludeNode(string mixinName, Expression[] args)
        {
            MixinName = mixinName;
            Arguments = args;
        }

        public string MixinName { get; set; }

        public Expression[] Arguments { get; set; }
    }
}
