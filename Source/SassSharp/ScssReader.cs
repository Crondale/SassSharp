using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crondale.SassSharp.Model;
using Crondale.SassSharp.Model.Expressions;
using Crondale.SassSharp.Model.Nodes;

namespace Crondale.SassSharp
{
    class ScssReader
    {
        internal ScssPackage ReadTree(StreamReader sr)
        {
            ScssPackage package = new ScssPackage();
            ScopeNode currentScope = package;

            List<string> attributeBuffer = new List<string>();

            StringBuilder buffer = new StringBuilder();
            int paranthesesLevel = 0;
            bool inQuotes = false;
            bool inCommentStart = false;
            bool inComment = false;


            while (!sr.EndOfStream)
            {
                char c = (char)sr.Read();

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

                            if(currentScope == null)
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
                var result = new IncludeNode();
                result.MixinName = m.Groups["name"].Value;

                return result;
            }

            return ParsePropertyNode(source);
        }

        [Obsolete]
        private IEnumerable<Expression> ParseValue(string source)
        {
            Match m = Regex.Match(source, @"^(\s*(?:[a-zA-Z0-9_$%#-]+)\s*(?:(?:[+-/*])\s*(?:[a-zA-Z0-9_%#-]+)\s*)*)+\s*$");

            if (!m.Success)
                throw new Exception("Could not recognize value");

            foreach (Capture capture in m.Groups[1].Captures)
            {
                yield return ParseExpression(capture.Value);
            }
        }

        private Expression ParseExpression(string source)
        {
            Match m = Regex.Match(source, @"^\s*(?<first>[a-zA-Z0-9_$%#,-]+)(?:(?<op>\s*[+-/*]\s*|\s+)(?<other>[a-zA-Z0-9_$%#,-]+)\s*)*\s*$");

            if(!m.Success)
                throw new Exception("Could not recognize expression");

            Group firstGroup = m.Groups["first"];
            Group othersGroup = m.Groups["other"];
            Group opGroup = m.Groups["op"];

            ExpressionNode[] nodes = new ExpressionNode[othersGroup.Captures.Count + 1];
            
            nodes[0] = ParseExpressionNode(firstGroup.Value, "+");

            for (int i = 0; i < othersGroup.Captures.Count; i++)
            {
                nodes[i+1] = ParseExpressionNode(othersGroup.Captures[i].Value, opGroup.Captures[i].Value);
            }

            Expression e = new Expression(nodes);

            return e;

        }

        private ExpressionNode ParseExpressionNode(string source, string opSource)
        {
            if (source.StartsWith("$"))
            {
                return new ReferenceNode(source.Substring(1))
                {
                    Operator = opSource[0]
                };
            }

            return new ValueNode(source)
            {
                Operator = opSource[0]
            };
        }

        private CodeNode ParsePropertyNode(string source)
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
            var m = Regex.Match(source, @"^\s*@mixin (?<name>[^:\s]+)\s*\((?<arg>[^,)]+,?)*\)\s*$");

            if (m.Success)
            {
                var result = new MixinNode();
                result.Name = m.Groups["name"].Value;

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
