namespace SassSharp.Model.Nodes
{
    internal class PropertyNode : CodeNode
    {
        public ScssString Name { get; set; }

        public Expression Expression { get; set; }
    }
}