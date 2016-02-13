namespace SassSharp.Model.Css
{
    internal class CssProperty : CssNode
    {
        public CssProperty(string name, string value, int level)
            : base(level)
        {
            Level = level;
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }
}