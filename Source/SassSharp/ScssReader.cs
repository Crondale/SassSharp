using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SassSharp.IO;
using SassSharp.Model;
using SassSharp.Model.Expressions;
using SassSharp.Model.Nodes;

namespace SassSharp
{
    internal class ScssReader : StreamReader
    {
        public PathFile File { get; set; }

        private int _lineNumber = 0;

        public ScssReader(PathFile file) : base(file.GetStream())
        {
            File = file;
        }

        public override int Read()
        {
            int c = base.Read();

            //Ignore CR
            while ((char)c == '\r')
            {
                c = base.Read();
            }

            if ((char)c == '\n')
                ++_lineNumber;

            return c;
        }

        internal ScssPackage ReadTree()
        {
            var package = new ScssPackage(File);
            ScopeNode currentScope = package;
            currentScope = ReadScopeContent(currentScope);

            return package;
        }

        private ScopeNode ReadScopeContent(ScopeNode currentScope)
        {
            var buffer = new StringBuilder();

            //States
            var inCommentStart = false;
            var paranthesesLevel = 0;
            var inDoubleQuotes = false;

            while (!EndOfStream)
            {
                var c = (char)Read();

                if (inDoubleQuotes)
                {
                    buffer.Append(c);

                    if (c == '"')
                    {
                        inDoubleQuotes = false;
                    }
                }
                else
                {

                    switch (c)
                    {
                        case ' ':
                            //Ignore starting white space
                            if (buffer.Length == 0)
                                break;
                            goto default;
                        case '/':
                            if (inCommentStart)
                            {
                                ReadInlineComment();

                                inCommentStart = false;

                                //Remove the first slash from the buffer
                                buffer.Length--;

                            }
                            else
                            {
                                buffer.Append(c);
                                inCommentStart = true;
                            }
                            break;
                        case '*':
                            if (inCommentStart)
                            {
                                var node = ReadComment();
                                currentScope.Add(node);
                                inCommentStart = false;

                                //Remove the first slash from the buffer
                                buffer.Length--;
                                break;
                            }
                            goto default;
                        case ';':
                            {
                                var node = ParseStatementNode(buffer.ToString());
                                currentScope.Add(node);
                                buffer.Clear();
                                inCommentStart = false;
                            }
                            break;
                        case ':':
                            char pc = (char) Peek();
                            if (char.IsLetter(pc))//hover etc.
                            {
                                buffer.Append(c);
                            }
                            else
                            {
                                var expr = ReadValue();

                                if (buffer[0] == '$')
                                {
                                    var node = new VariableNode();
                                    node.Name = buffer.ToString().Trim().TrimStart('$');
                                    node.Expression = new Expression(expr);
                                    buffer.Clear();
                                    currentScope.Add(node);
                                    
                                    if ((char)Read() != ';')
                                        throw new Exception("Excpected ;");
                                }
                                else { 
                                    var pn = new PropertyNode();
                                    pn.Name = buffer.ToString().Trim();
                                    pn.Expression = new Expression(expr);
                                    buffer.Clear();

                                    char pc2 = (char) Read();
                                    if (pc2 == ';')
                                    {
                                        currentScope.Add(pn);
                                    }
                                    else
                                    {
                                        var result = new NamespaceNode(pn);
                                        currentScope.Add(result);
                                        currentScope = result;
                                    }
                                }
                            }
                            break;
                        case '"':
                            inDoubleQuotes = true;
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
                        case '\n':
                            inCommentStart = false;
                            break;
                        default:
                            inCommentStart = false;
                            buffer.Append(c);
                            break;
                    }
                }
            }

            return currentScope;
        }

        private ValueList ReadValue()
        {
            ValueList result = new ValueList();
            List<ExpressionNode> tempNodes = new List<ExpressionNode>();
            var buffer = new StringBuilder();
            bool afterSpace = false;
            char op = '+';

            while (!EndOfStream)
            {
                var c = (char)Peek();

                if (c == '{' || c == ';')
                {
                    if (buffer.Length > 0)
                    {
                        tempNodes.Add(ParseExpressionNode(buffer.ToString(), op));
                        op = c;
                    }
                    
                    if (tempNodes.Count != 0)
                        result.Add(Expression.CalculateTree(tempNodes.ToArray()));

                    if (result.Count == 0)
                        return null;

                    return result;
                }

                c = (char)Read();

                if (c == ')')
                {
                    if (buffer.Length > 0)
                    {
                        tempNodes.Add(ParseExpressionNode(buffer.ToString(), op));
                        op = c;
                        buffer.Clear();
                    }
                    
                    if (tempNodes.Count != 0)
                        result.Add(Expression.CalculateTree(tempNodes.ToArray()));

                    return result;
                }

                switch (c)
                {
                    case ',':
                        afterSpace = true;
                        result.PreferComma = true;
                        break;
                    case ' ':
                        if (buffer.Length != 0)
                            afterSpace = true;
                        break;
                    case '(':
                        ExpressionNode inner = ReadValue();

                        if (buffer.Length != 0)
                        {
                            inner = new FunctionCallNode(buffer.ToString(), (ValueList)inner);
                            buffer.Clear();
                        }

                        if (op == ' ')
                        {
                            result.Add(inner);
                        }
                        else
                        {
                            tempNodes.Add(inner);
                        }

                        break;
                    case '-':
                    case '+':
                    case '*':
                    case '/':
                        if(c == '-' && !afterSpace)
                            goto default;

                        if (buffer.Length > 0)
                        {
                            tempNodes.Add(ParseExpressionNode(buffer.ToString(), op));
                            buffer.Clear();
                        }
                        op = c;
                        
                        afterSpace = false;
                        break;
                    default:
                        if (afterSpace)
                        {
                            tempNodes.Add(ParseExpressionNode(buffer.ToString(), op));
                            op = ' ';
                            buffer.Clear();
                            afterSpace = false;

                            result.Add(Expression.CalculateTree(tempNodes.ToArray()));
                            tempNodes.Clear();
                        }
                        buffer.Append(c);
                        break;
                }
            }

            throw new Exception("Could not find en of expression");
        }
        
        private void ReadInlineComment()
        {
            var buffer = new StringBuilder("//");

            while (!EndOfStream)
            {
                var c = (char)Read();

                if(c == '\n')
                    return; //Ignore single line comments for now
                else
                {
                    buffer.Append(c);
                }
            }
        }

        private CommentNode ReadComment()
        {
            var buffer = new StringBuilder("/*");
            var inCommentEnd = false;

            while (!EndOfStream)
            {
                var c = (char) Read();

                switch (c)
                {
                    case '*':
                        inCommentEnd = true;
                        buffer.Append(c);
                        break;
                    case '/':
                        if (inCommentEnd)
                        {
                            buffer.Append(c);

                            return new CommentNode(buffer.ToString());
                        }
                        goto default;
                    default:
                        inCommentEnd = false;
                        buffer.Append(c);
                        break;
                }
            }

            throw new Exception("Could not find comment end");
        }

        private CodeNode ParseStatementNode(string source)
        {
            var m = Regex.Match(source, @"^\s*\$(?<name>[^:\s]+):\s*(?<value>[^;]+)\s*$");

            if (m.Success)
            {
                throw new Exception("Not in use anymore");
                /*var result = new VariableNode();
                result.Name = m.Groups["name"].Value;
                result.Expression = ParseExpression(m.Groups["value"].Value);

                return result;*/
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

            m = Regex.Match(source, @"^\s*@import (?<path>[^;]+)\s*$");

            if (m.Success)
            {
                var result = new ImportNode(m.Groups["path"].Value);
                
                return result;
            }

            return ParsePropertyNode(source);
        }

        private Expression ParseExpression(string source)
        {
            //throw new Exception("Not in use anymore");
            if (string.IsNullOrWhiteSpace(source))
                return null;
            
            var m = Regex.Match(source,
                @"^\s*(?<first>[a-zA-Z0-9_!$%#,-]+)(?:(?<op>\s*[+-/*]\s*|\s+)(?<other>[a-zA-Z0-9_!$%#,-]+)\s*)*\s*$");

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

            var e = new Expression(Expression.CalculateTree(nodes));

            return e;
        }

        private ExpressionNode ParseExpressionNode(string source, string opSource)
        {
            //throw new Exception("Not in use anymore");
            var op = ' ';

            opSource = opSource.Trim();

            if (opSource.Length > 0)
                op = opSource[0];

            return ParseExpressionNode(source, op);
        }

        private ExpressionNode ParseExpressionNode(string source, char op)
        {
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
            throw new Exception("Not in use anymore");
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
                throw new Exception("Not in use anymore");
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