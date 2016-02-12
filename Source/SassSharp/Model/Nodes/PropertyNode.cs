namespace SassSharp.Model.Nodes
{
    internal class PropertyNode : CodeNode
    {
        public string Name { get; set; }

        public Expression Expression { get; set; }
    }
}