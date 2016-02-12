using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crondale.SassSharp.Model;
using Crondale.SassSharp.Model.Css;
using Crondale.SassSharp.Model.Nodes;

namespace Crondale.SassSharp
{
    class CssWriter
    {
        internal void Write(CssSheet sheet, StreamWriter sw)
        {
            WriteSheet(sheet, sw);
            sw.Write("\n");
        }

        private void WriteSheet(CssSheet sheet, StreamWriter sw)
        {
            int lastLevel = -1;
            foreach (var selector in sheet.Selectors)
            {

                if (selector.Level == 0 && lastLevel != -1)
                    sw.Write("\n");

                for (int i = 0; i < selector.Level; i++)
                    sw.Write("  ");

                sw.Write(selector.Selector);
                sw.Write(" {");
                WriteProperties(selector, sw);
                sw.Write(" }\n");

                lastLevel = selector.Level;
                
            }

        }

        private void WriteProperties(CssSelector selector, StreamWriter sw)
        {
            foreach (var property in selector.Properties)
            {

                sw.Write("\n");

                for(int i = 0; i < property.Level; i++)
                    sw.Write("  ");

                sw.Write(property.Name);
                sw.Write(": ");
                sw.Write(property.Value);
                sw.Write(";");

            }

        }
    }
}
