using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SassSharp.Model.Css
{
    abstract class CssRoot : CssNode
    {
        public CssRoot(int level) : base(level)
        {
        }
    }
}