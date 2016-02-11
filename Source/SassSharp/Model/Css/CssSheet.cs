using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crondale.SassSharp.Model.Css
{
    class CssSheet
    {
        public List<CssSelector> Selectors { get; }

        public CssSheet()
        {
            Selectors = new List<CssSelector>();
        }
    }
}
