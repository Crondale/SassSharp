using System.Collections.Generic;

namespace SassSharp.Model.Css
{
    internal class CssSheet
    {
        public CssSheet()
        {
            Selectors = new List<CssSelector>();
        }

        public List<CssSelector> Selectors { get; }
    }
}