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
    internal class ScssReader : StreamReader
    {
        public ScssReader(Stream stream) : base(stream)
        {
        }

        internal ScssPackage ReadTree()
        {
            var package = new ScssPackage();
            ScopeNode currentScope = package;

            var buffer = new StringBuilder();
            var commentBuffer = new StringBuilder();
            var paranthesesLevel = 0;
            var inQuotes = false;
            var inCommentStart = false;
            var inCommentEnd = false;
            var inComment = false;
            var inLineComment = false;


            while (!EndOfStream)
            {
                var c = (char) Read();

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
                    switch (c)
                    {
                        case '*':
                            if (!inLineComment)
                                inCommentEnd = true;
                            goto default;
                        case '/':
                            if (inCommentEnd)
                            {
                                inComment = false;

                                commentBuffer.Append(c);

                                var node = new CommentNode(commentBuffer.ToString());
                                currentScope.Add(node);
                                commentBuffer.Clear();
                                inCommentEnd = false;
                                break;
                            }
                            goto default;
                        case '\n':
                            if (inLineComment)
                            {
                                inLineComment = false;
                                inComment = false;
                                commentBuffer.Clear(); // Ignore line comments
                                break;
                            }
                            goto default;
                        case '\r':
                            break;
                        default:
                            commentBuffer.Append(c);
                            break;
                    }
                }
                else
                {
                    inCommentEnd = false;

                    switch (c)
                    {
                        case ' ':
                            goto default;
                        case '/':
                            if (inCommentStart)
                            {
                                inLineComment = true;
                                inComment = true;
                                inCommentStart = false;

                                //Move comment start to comment buffer
                                buffer.Length--;
                                commentBuffer.Append("//");
                                break;
                            }
                            inCommentStart = true;
                            goto default;
                        case '*':
                            if (inCommentStart)
                            {
                                inComment = true;

                                //Move comment start to comment buffer
                                buffer.Length--;
                                commentBuffer.Append("/*");
                                break;
                            }
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
                                buffer.Clear();
                            }
                            break;
                        case '}':
                            currentScope = currentScope.Parent;

                            if (currentScope == null)
                                throw new Exception("Unexpected }");
                            break;
                        case '\r':
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
            var op = ' ';

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

            //Check for extended properties
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