namespace SassSharp.Model.Css
{
    internal class CssProperty
    {
        public CssProperty(string name, string value, int level)
        {
            Level = level;
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }

        public int Level { get; set; }
    }
}