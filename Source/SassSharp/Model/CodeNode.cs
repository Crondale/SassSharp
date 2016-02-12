using SassSharp.Model.Nodes;

namespace SassSharp.Model
{
    internal abstract class CodeNode
    {
        public ScopeNode Parent { get; set; }
    }
}