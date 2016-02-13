using System.Collections.Generic;

namespace SassSharp.Model.Css
{
    internal class CssSelector:CssNode
    {
        public CssSelector(string selector, int level)
            : base(level)
        {
            Selector = selector;
        }

        public string Selector { get; set; }



    }
}