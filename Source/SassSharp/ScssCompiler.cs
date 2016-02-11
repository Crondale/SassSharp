using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Crondale.SassSharp.Model;
using Crondale.SassSharp.Model.Css;
using Crondale.SassSharp.Model.Expressions;
using Crondale.SassSharp.Model.Nodes;

namespace Crondale.SassSharp
{
    public class ScssCompiler
    {

        public string Compile(string source)
        {

            using (var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(source)))
            using (var destination = new MemoryStream())
            {
                using (var writer = new StreamWriter(destination))
                using (var reader = new StreamReader(sourceStream))
                {
                    Compile(reader, writer);
                }

                return Encoding.UTF8.GetString(destination.ToArray());
            }

        }

        public void Compile(StreamReader source, StreamWriter destination)
        {
            var reader = new ScssReader();

            var tree = reader.ReadTree(source);
            CssSheet sheet = new CssSheet();

            ProcessTree(tree, sheet);

            var writer = new CssWriter();

            writer.Write(sheet, destination);
        }

        private void ProcessTree(ScssPackage tree, CssSheet sheet)
        {
            ProcessScope(tree, sheet, null, -1);
        }

        private string ExpandSelector(string root, string selector)
        {
            string[] rootSplit = root.Split(',');
            string[] selectorSplit = selector.Split(',');

            List<string> resultSplit = new List<string>();

            foreach (var r in rootSplit)
            {
                foreach (var s in selectorSplit)
                {
                    if(s.Contains("&"))
                        resultSplit.Add(s.Replace("&", r));
                    else
                        resultSplit.Add(r + " " + s);
                }
            }

            string result = String.Join(", ", resultSplit);
            
            result = result.Replace(" &", "");
            result = result.Replace("  ", " ");

            return result;
        }

        private void ProcessScope(ScopeNode scope, CssSheet sheet, CssSelector selector, int level)
        {
            if (scope is SelectorNode)
            {
                var snode = (SelectorNode) scope;
                var s = snode.Selector;
                if (selector != null)
                {
                    s = ExpandSelector(selector.Selector, s);
                }
                
                selector = new CssSelector(s, level);
                sheet.Selectors.Add(selector);
            }

            foreach (var node in scope.Nodes)
            {
                var useNode = node;

                if (node is PropertyNode)
                {
                    var n = (PropertyNode)node;

                    string value = n.Expression.Resolve(scope).Value;

                    selector.Properties.Add(new CssProperty(n.Name, value));

                }
                else if (node is VariableNode)
                {
                    var n = (VariableNode)node;

                    scope.SetVariable(n);
                }
                else if (node is MixinNode)
                {
                    var n = (MixinNode)node;

                    scope.SetMixin(n);
                }
                else if (node is IncludeNode)
                {
                    var n = (IncludeNode)node;
                    
                    MixinNode mn = scope.GetMixin(n.MixinName);


                    mn.Initialize(n.Arguments);

                    ProcessScope(mn, sheet, selector, level + 1);
                }
                else if (useNode is SelectorNode)
                {
                    ProcessScope((ScopeNode)node, sheet, selector, level + 1);
                }
            }
        }
    }
}
