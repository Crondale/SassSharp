namespace SassSharp.Model.Nodes
{
    internal class ImportNode : CodeNode
    {
        public ImportNode(string path)
        {
            Path = path;
        }

        public string Path { get; set; }
    }
}