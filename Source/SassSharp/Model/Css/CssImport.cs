using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SassSharp.Model.Css
{
    class CssImport:CssNode
    {
        public string Path { get; set; }

        public CssImport(string path, int level) : base(level)
        {
            Path = path;
        }
    }
}
