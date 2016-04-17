using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SassSharp.Model.Css
{
    class CssMedia : CssRoot
    {
        public CssMedia(int level) : base(level)
        {
        }

        public string Definition { get; set; }
    }
}