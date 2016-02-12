namespace SassSharp.Model.Nodes
{
    internal class NamespaceNode : ScopeNode
    {
        public NamespaceNode(PropertyNode header)
        {
            Header = header;
        }

        public PropertyNode Header { get; set; }
    }
}