namespace SassSharp.Model.Nodes
{
    internal class CommentNode : CodeNode
    {
        public CommentNode(string comment)
        {
            Comment = comment;
        }

        public string Comment { get; set; }

        public override string ToString()
        {
            return Comment;
        }
    }
}