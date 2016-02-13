using System.Collections.Generic;
using System.IO;
using System.Text;
using SassSharp.Model;
using SassSharp.Model.Css;
using SassSharp.Model.Nodes;

namespace SassSharp
{
    public class ScssCompiler
    {
        public string Compile(string source)
        {
            using (var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(source)))
            using (var destination = new MemoryStream())
            {
                Compile(sourceStream, destination);

                return Encoding.UTF8.GetString(destination.ToArray());
            }
        }

        public void Compile(Stream source, Stream destination)
        {
            var reader = new ScssReader(source);

            var tree = reader.ReadTree();
            var sheet = new CssSheet();

            ProcessTree(tree, sheet);

            using (var writer = new CssWriter(destination))
            {
                writer.Write(sheet);
            }

                
        }

        private void ProcessTree(ScssPackage tree, CssSheet sheet)
        {
            ProcessScope(tree, sheet, null, -1);
        }


        private void ProcessScope(ScopeNode scope, CssSheet sheet, CssSelector selector, int level, string nspace = "")
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
                sheet.Add(selector);
            }

            foreach (var node in scope.Nodes)
            {
                var useNode = node;

                if (node is PropertyNode)
                {
                    var n = (PropertyNode) node;

                    var value = n.Expression.Resolve(scope).Value;

                    selector.Add(new CssProperty(nspace + n.Name, value, level + 1));
                }
                else if (node is CommentNode)
                {
                    if(selector == null)
                        sheet.Add(new CssComment(((CommentNode)node).Comment, level + 1));
                    else
                        selector.Add(new CssComment(((CommentNode)node).Comment, level + 1));
                }
                else if (node is VariableNode)
                {
                    var n = (VariableNode) node;

                    scope.SetVariable(n);
                }
                else if (node is MixinNode)
                {
                    var n = (MixinNode) node;

                    scope.SetMixin(n);
                }
                else if (node is NamespaceNode)
                {
                    var subLevel = level;

                    var n = (NamespaceNode) node;
                    if (n.Header.Expression != null)
                    {
                        var value = n.Header.Expression.Resolve(scope).Value;
                        selector.Add(new CssProperty(n.Header.Name, value, level + 1));

                        subLevel++;
                    }

                    ProcessScope((ScopeNode) node, sheet, selector, subLevel, n.Header.Name + "-");
                }
                else if (node is IncludeNode)
                {
                    var n = (IncludeNode) node;

                    var mn = scope.GetMixin(n.MixinName);


                    mn.Initialize(n.Arguments);

                    ProcessScope(mn, sheet, selector, level);
                }
                else if (useNode is SelectorNode)
                {
                    ProcessScope((ScopeNode) node, sheet, selector, level + 1);
                }
            }
        }

        private string ExpandSelector(string root, string selector)
        {
            var rootSplit = root.Split(',');
            var selectorSplit = selector.Split(',');

            var resultSplit = new List<string>();

            foreach (var r in rootSplit)
            {
                foreach (var s in selectorSplit)
                {
                    if (s.Contains("&"))
                        resultSplit.Add(s.Replace("&", r));
                    else
                        resultSplit.Add(r + " " + s);
                }
            }

            var result = string.Join(", ", resultSplit);

            result = result.Replace(" &", "");
            result = result.Replace("  ", " ");

            return result;
        }
    }
}