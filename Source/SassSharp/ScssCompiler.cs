﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SassSharp.Exceptions;
using SassSharp.IO;
using SassSharp.Model;
using SassSharp.Model.Css;
using SassSharp.Model.Expressions;
using SassSharp.Model.Nodes;

namespace SassSharp
{
    public class ScssCompiler
    {
        public string CompileFile(string path)
        {
            var file = new PathFile(new RealFileManager(), path);
            //using (var sourceStream = file.GetStream())
            using (var destination = new MemoryStream())
            {
                Compile(file, destination);
                return Encoding.UTF8.GetString(destination.ToArray());
            }
        }

        //public string Compile(string source)
        //{
        //    using (var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(source)))
        //    using (var destination = new MemoryStream())
        //    {
        //        Compile(sourceStream, destination);

        //        return Encoding.UTF8.GetString(destination.ToArray());
        //    }
        //}

        public void Compile(PathFile source, Stream destination)
        {
            var tree = TreeFromFile(source);
            var sheet = new CssSheet();

            ProcessTree(tree, sheet);

            using (var writer = new CssWriter(destination))
            {
                writer.Write(sheet);
            }
        }

        private ScssPackage TreeFromFile(PathFile file)
        {
            using (var reader = new ScssReader(file))
            {
                return reader.ReadTree();
            }
        }

        private void ProcessTree(ScssPackage tree, CssSheet sheet)
        {
            ProcessScope(tree, tree, sheet, null, -1);
            ProcessExtensions(tree, sheet);
        }

        private void ProcessImport(string path, ScssPackage fromPackage, ScopeNode scope, CssRoot root,
            CssSelector selector, int level, string nspace = "")
        {
            var file = fromPackage.File.SolveReference(path);
            var tree = TreeFromFile(file);

            ProcessScope(tree, tree, root, selector, level, nspace);
        }

        private void ProcessExtensions(ScssPackage package, CssRoot root)
        {
            foreach (var cssNode in root.Nodes)
            {
                if (!(cssNode is CssSelector))
                    continue;

                CssSelector selector = (CssSelector) cssNode;
                string orgSelector = selector.Selector;
                foreach (var extension in package.Extensions)
                {
                    if (selector.Selector.Contains(extension.Super))
                    {
                        selector.Selector = selector.Selector + ", " +
                                            orgSelector.Replace(extension.Super, extension.Selector);
                    }
                }
            }
        }

        private void ProcessScope(ScssPackage package, ScopeNode scope, CssRoot root, CssSelector selector, int level,
            string nspace = "")
        {
            if (scope is SelectorNode)
            {
                var snode = (SelectorNode) scope;
                var s = snode.Selector.Resolve(scope);
                if (selector != null)
                {
                    s = ExpandSelector(selector.Selector, s);
                }

                selector = new CssSelector(s, level);
                root.Add(selector);
            }

            foreach (var node in scope.Nodes)
            {
                var useNode = node;

                if (node is PropertyNode)
                {
                    var n = (PropertyNode) node;

                    var value = n.Expression.Resolve(scope).Value;

                    selector.Add(new CssProperty(nspace + n.Name.Resolve(scope), value, level + 1));
                }
                else if (node is MediaNode)
                {
                    var n = (MediaNode) node;
                    var s = new CssMedia(level + 1)
                    {
                        Definition = n.Definition
                    };

                    root.Add(s);

                    int nextLevel = level + 1;

                    if (selector != null)
                    {
                        selector = new CssSelector(selector.Selector, level + 2);
                        s.Add(selector);

                        nextLevel++;
                    }

                    ProcessScope(package, (ScopeNode) n, s, selector, nextLevel, nspace);
                }
                else if (node is CommentNode)
                {
                    var n = (CommentNode) node;
                    CssNode p = root;

                    if (selector != null)
                        p = selector;

                    p.Add(new CssComment(n.Comment, level + 1));
                }
                else if (node is ImportNode)
                {
                    var n = (ImportNode) node;

                    if (n.Path.Contains(".css")
                        || n.Path.Contains("http://")
                        || n.Path.Contains("url(")
                        || n.Path.Contains(" "))
                    {
                        root.Add(new CssImport(n.Path, level + 1));
                    }
                    else
                    {
                        var path = n.Path.Trim('\"');
                        ProcessImport(path, package, scope, root, selector, level, nspace);
                    }
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
                else if (node is ContentNode)
                {
                    var sn = scope.GetContent();
                    ProcessScope(package, sn, root, selector, level - 1, nspace);
                }
                else if (node is IfNode)
                {
                    var n = (IfNode) node;
                    var sn = n.GetActiveScope(scope);

                    if (sn != null)
                        ProcessScope(package, sn, root, selector, level, nspace);
                }
                else if (node is EachNode)
                {
                    var n = (EachNode) node;
                    var var = n.Variables;

                    foreach (var value in n.List)
                    {
                        if (var.Count == 1)
                        {
                            var[0].Expression = new Expression(value);
                            n.SetVariable(var[0]);
                        }
                        else
                        {
                            if (!(value is ValueList))
                                throw new Exception("This must be a list");

                            var subList = (ValueList) value;

                            // There is a bug in the reader causing unecessary levels of value lists
                            // TODO UNDONE
                            if (subList.Count == 1)
                                subList = (ValueList) subList[0];

                            for (int i = 0; i < var.Count; i++)
                            {
                                var[i].Expression = new Expression(subList[i]);
                                n.SetVariable(var[i]);
                            }
                        }

                        ProcessScope(package, n, root, selector, level, nspace);
                    }
                }
                else if (node is ForNode)
                {
                    var n = (ForNode) node;
                    var var = n.Variable;
                    int from = int.Parse(n.From.Resolve(scope).Value);

                    int to = 0;
                    string toStr = n.Through.Resolve(scope).Value;

                    if (!int.TryParse(toStr, out to))
                        throw new Exception("Failed to parse to value");

                    for (int i = from; i <= to; i++)
                    {
                        var value = new ValueNode(i.ToString());

                        var.Expression = new Expression(value);
                        n.SetVariable(var);
                        ProcessScope(package, n, root, selector, level, nspace);
                    }
                }
                else if (node is ExtendNode)
                {
                    var n = (ExtendNode) node;

                    package.AddExtension(new ScssExtension()
                    {
                        Selector = selector.Selector,
                        Super = ((ExtendNode) node).Selector
                    });
                }
                else if (node is FunctionNode)
                {
                    var n = (FunctionNode) node;

                    scope.SetFunction(n);
                }
                else if (node is NamespaceNode)
                {
                    var subLevel = level;

                    var n = (NamespaceNode) node;
                    string header = n.Header.Name.Resolve(scope);
                    if (!n.Header.Expression.Empty)
                    {
                        var value = n.Header.Expression.Resolve(scope).Value;
                        selector.Add(new CssProperty(header, value, level + 1));

                        subLevel++;
                    }

                    ProcessScope(package, (ScopeNode) node, root, selector, subLevel, header + "-");
                }
                else if (node is IncludeNode)
                {
                    var n = (IncludeNode) node;

                    var mn = scope.GetMixin(n.MixinName);


                    mn.Initialize(n);

                    ProcessScope(package, mn, root, selector, level);
                }
                else if (useNode is SelectorNode)
                {
                    ProcessScope(package, (ScopeNode) node, root, selector, level + 1);
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