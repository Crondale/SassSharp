using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SassSharp.Model;
using SassSharp.Model.Expressions;
using SassSharp.Model.Nodes;

namespace SassSharp
{
    internal class ScssReader
    {
        internal ScssPackage ReadTree(StreamReader sr)
        {
            var package = new ScssPackage();
            ScopeNode currentScope = package;

            var attributeBuffer = new List<string>();

            var buffer = new StringBuilder();
            var paranthesesLevel = 0;
            var inQuotes = false;
            var inCommentStart = false;
            var inComment = false;


            while (!sr.EndOfStream)
            {
                var c = (char) sr.Read();

                if (inQuotes)
                {
                    buffer.Append(c);

                    if (c == '"')
                    {
                        inQuotes = false;
                    }
                }
                else if (inComment)
                {
                    buffer.Append(c);
                }
                else
                {
                    switch (c)
                    {
                        case ' ':
                            goto default;
                        case '[':
                            break;
                        case ']':
                            attributeBuffer.Add(buffer.ToString().Trim());
                            buffer.Clear();
                            break;
                        case '/':
                            if (inCommentStart)
                            {
                                inComment = true;
                                inCommentStart = false;
                            }
                            else
                            {
                                inCommentStart = true;
                            }
                            goto default;
                        case '\n':
                            goto default;
                        case ';':
                        {
                            var node = ParseStatementNode(buffer.ToString());
                            currentScope.Add(node);
                            buffer.Clear();
                        }
                            break;
                        case '"':
                            inQuotes = true;
                            goto default;
                        case '(':
                            paranthesesLevel++;
                            goto default;
                        case ')':
                            paranthesesLevel--;
                            goto default;
                        case '{':
                            if (paranthesesLevel == 0)
                            {
                                var node = ParseScopeNode(buffer.ToString());
                                currentScope.Add(node);
                                currentScope = node;
                                attributeBuffer.Clear();
                                buffer.Clear();
                            }
                            break;
                        case '}':
                            currentScope = currentScope.Parent;

                            if (currentScope == null)
                                throw new Exception("Unexpected }");
                            break;
                        default:
                            buffer.Append(c);
                            break;
                    }
                }
            }

            return package;
        }

        private CodeNode ParseStatementNode(string source)
        {
            var m = Regex.Match(source, @"^\s*\$(?<name>[^:\s]+):\s*(?<value>[^;]+)\s*$");

            if (m.Success)
            {
                var result = new VariableNode();
                result.Name = m.Groups["name"].Value;
                result.Expression = ParseExpression(m.Groups["value"].Value);

                return result;
            }

            m = Regex.Match(source, @"^\s*@include (?<name>[^:\s]+)\s*\((?<arg>[^,)]+,?)*\)\s*$");

            if (m.Success)
            {
                var argsGroup = m.Groups["arg"];
                var args = new Expression[argsGroup.Captures.Count];

                for (var i = 0; i < argsGroup.Captures.Count; i++)
                {
                    args[i] = ParseExpression(argsGroup.Captures[i].Value);
                }

                var result = new IncludeNode(m.Groups["name"].Value, args);


                return result;
            }

            return ParsePropertyNode(source);
        }

        [Obsolete]
        private IEnumerable<Expression> ParseValue(string source)
        {
            var m = Regex.Match(source, @"^(\s*(?:[a-zA-Z0-9_$%#-]+)\s*(?:(?:[+-/*])\s*(?:[a-zA-Z0-9_%#-]+)\s*)*)+\s*$");

            if (!m.Success)
                throw new Exception("Could not recognize value");

            foreach (Capture capture in m.Groups[1].Captures)
            {
                yield return ParseExpression(capture.Value);
            }
        }

        private Expression ParseExpression(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return null;

            var m = Regex.Match(source,
                @"^\s*(?<first>[a-zA-Z0-9_$%#,-]+)(?:(?<op>\s*[+-/*]\s*|\s+)(?<other>[a-zA-Z0-9_$%#,-]+)\s*)*\s*$");

            if (!m.Success)
                throw new Exception("Could not recognize expression");

            var firstGroup = m.Groups["first"];
            var othersGroup = m.Groups["other"];
            var opGroup = m.Groups["op"];

            var nodes = new ExpressionNode[othersGroup.Captures.Count + 1];

            nodes[0] = ParseExpressionNode(firstGroup.Value, "+");

            for (var i = 0; i < othersGroup.Captures.Count; i++)
            {
                nodes[i + 1] = ParseExpressionNode(othersGroup.Captures[i].Value, opGroup.Captures[i].Value);
            }

            var e = new Expression(nodes);

            return e;
        }

        private ExpressionNode ParseExpressionNode(string source, string opSource)
        {
            char op = ' ';

            opSource = opSource.Trim();

            if (opSource.Length > 0)
                op = opSource[0];

            if (source.StartsWith("$"))
            {
                return new ReferenceNode(source.Substring(1))
                {
                    Operator = op
                };
            }

            return new ValueNode(source)
            {
                Operator = op
            };
        }

        private PropertyNode ParsePropertyNode(string source)
        {
            var m = Regex.Match(source, @"^\s*(?<name>[^:\s]+)\s*:\s*(?<value>[^;]+)\s*$");

            if (!m.Success)
                throw new Exception("Failed to parse property");

            var result = new PropertyNode();
            result.Name = m.Groups["name"].Value;
            result.Expression = ParseExpression(m.Groups["value"].Value);

            return result;
        }

        private ScopeNode ParseScopeNode(string source)
        {
            var m = Regex.Match(source, @"^\s*@mixin (?<name>[^:\s]+)\s*\((?:\s*\$(?<arg>[^,)]+),?)*\)\s*$");

            if (m.Success)
            {
                var argsGroup = m.Groups["arg"];
                var args = new VariableNode[argsGroup.Captures.Count];

                for (var i = 0; i < argsGroup.Captures.Count; i++)
                {
                    args[i] = new VariableNode
                    {
                        Name = argsGroup.Captures[i].Value
                    };
                }

                var result = new MixinNode(m.Groups["name"].Value, args);

                return result;
            }

            m = Regex.Match(source, @":[^a-z]");
            if (m.Success)
            {
                var pn = ParsePropertyNode(source);

                var result = new NamespaceNode(pn);

                return result;
            }

            {
                source = source.Trim();

                var result = new SelectorNode();
                result.Selector = source;

                return result;
            }
        }
    }
}