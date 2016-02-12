using System.Collections.Generic;

namespace SassSharp.Model.Css
{
    internal class CssSelector
    {
        public CssSelector(string selector, int level)
        {
            Selector = selector;
            Level = level;
            Properties = new List<CssProperty>();
        }

        public string Selector { get; set; }


        public int Level { get; set; }

        public List<CssProperty> Properties { get; }
    }
}