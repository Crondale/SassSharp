namespace SassSharp.Model.Css
{
    internal class CssComment : CssNode
    {
        public CssComment(string comment, int level)
            : base(level)
        {
            Comment = comment;
        }

        public string Comment { get; set; }
    }
}