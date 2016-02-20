namespace SassSharp.Model.Nodes
{
    internal class SelectorNode : ScopeNode
    {
        public string Selector { get; set; }


        public override string ToString()
        {
            return Selector;
        }
    }
}