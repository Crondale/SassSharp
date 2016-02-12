namespace SassSharp.Model.Nodes
{
    internal class VariableNode : CodeNode
    {
        public string Name { get; set; }

        public Expression Expression { get; set; }
    }
}