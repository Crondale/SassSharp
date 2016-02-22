namespace SassSharp.Model.Css
{
    internal class CssImport : CssNode
    {
        public CssImport(string path, int level) : base(level)
        {
            Path = path;
        }

        public string Path { get; set; }
    }
}