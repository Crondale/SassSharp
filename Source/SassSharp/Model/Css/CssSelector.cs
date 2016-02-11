using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crondale.SassSharp.Model.Css
{
    class CssSelector
    {

        public string Selector { get; set; }


        public int Level { get; set; }

        public List<CssProperty> Properties { get; }

        public CssSelector(string selector, int level)
        {
            Selector = selector;
            Level = level;
            Properties = new List<CssProperty>();
        }

    }
}
