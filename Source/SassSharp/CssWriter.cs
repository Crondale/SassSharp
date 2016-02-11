using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crondale.SassSharp.Model;

namespace Crondale.SassSharp
{
    class CssWriter
    {
        internal void Write(ScssPackage package, StreamWriter sw)
        {
            WriteScope(package, package, "", 0, sw);
            //sw.Write("\n");
        }

        private void WriteScope(ScssPackage package, ScopeNode scope, string root, int level, StreamWriter sw)
        {
            foreach (var node in scope.Nodes)
            {
                if (node is SelectorNode)
                {
                    
                    var snode = (SelectorNode) node;
                    var selector = root + snode.Selector;
                    selector = selector.Replace(" &", "");

                    for (int i = 0; i < level; i++)
                        sw.Write("  ");

                    sw.Write(selector);
                    sw.Write(" {");
                    WriteProperties(package, snode, level, sw);
                    sw.Write(" }\n");
                    
                    WriteScope(package, snode, selector + " ", level + 1, sw);

                    if(level == 0)
                        sw.Write("\n");
                }
            }

        }

        private void WriteProperties(ScssPackage package, ScopeNode scope, int level, StreamWriter sw)
        {
            foreach (var node in scope.Nodes)
            {
                if (node is PropertyNode)
                {
                    sw.Write("\n");

                    for(int i = 0; i <= level;i++)
                        sw.Write("  ");

                    sw.Write(((PropertyNode)node).Name);
                    sw.Write(": ");
                    sw.Write(((PropertyNode)node).Value);
                    sw.Write(";");
                }
            }

        }
    }
}
