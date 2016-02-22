namespace SassSharp.Model.Nodes
{
    internal class ReturnNode : CodeNode
    {
        public ReturnNode(Expression expression)
        {
            Expression = expression;
        }

        public Expression Expression { get; set; }
    }
}