using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Crondale.SassSharp.Model;

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
                                var node = ParseStatementNode(buffer.ToString(), currentScope);
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

        private CodeNode ParseStatementNode(string source, ScopeNode package)
        {
            var m = Regex.Match(source, @"^\s*\$(?<name>[^:\s]+):\s*(?<value>[^;]+)\s*$");

            if (m.Success)
            {
                var result = new VariableNode();
                result.Name = m.Groups["name"].Value;
                result.Value = m.Groups["value"].Value;

                return result;
            }

            return ParsePropertyNode(source, package);
        }

        private CodeNode ParsePropertyNode(string source, ScopeNode package)
        {

            var m = Regex.Match(source, @"^\s*(?<name>[^:\s]+):\s*(?<value>[^;]+)\s*$");

            if (!m.Success)
                throw new Exception("Failed to parse property");

            var result = new PropertyNode();
            result.Name = m.Groups["name"].Value;
            result.Value = ResolveValue(m.Groups["value"].Value, package);

            return result;
        }

        private string ResolveValue(string source, ScopeNode package)
        {
            return Regex.Replace(source, @"\$(?<name>[a-zA-Z0-9_-]+)", new MatchEvaluator(
                m => package.GetVariable(m.Groups["name"].Value).Value
            ));
        }

        private SelectorNode ParseScopeNode(string source)
        {

            source = source.Trim();

            SelectorNode result = new SelectorNode();
            result.Selector = source;

            return result;
        }
    }
}
