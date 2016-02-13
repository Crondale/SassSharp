using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SassSharp.Model.Css
{
    class CssComment:CssNode
    {
        public CssComment(string comment, int level)
            :base(level)
        {
            Comment = comment;
        }

        public string Comment { get; set; }


    }
}
