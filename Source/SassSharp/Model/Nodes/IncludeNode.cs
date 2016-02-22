namespace SassSharp.Model.Nodes
{
    internal class IncludeNode : ScopeNode
    {
        public IncludeNode(string mixinName, ValueList args)
        {
            MixinName = mixinName;
            Arguments = args;
        }

        public string MixinName { get; set; }

        public ValueList Arguments { get; set; }
    }
}