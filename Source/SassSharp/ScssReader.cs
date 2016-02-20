using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

        public override int Peek()
        {
            int c = base.Peek();
            
            //Ignore CR
            while ((char)c == '\r')
            {
                base.Read();
                c = base.Peek();
            }

            return c;
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

        /// <summary>
        /// Skips whitespace
        /// </summary>
        /// <returns>true if whitespace was found</returns>
        private bool SkipWhitespace()
        {
            bool foundWhitespace = false;

            while (!EndOfStream)
            {
                var c = (char)Peek();

                if (char.IsWhiteSpace(c))
                {
                    foundWhitespace = true;
                    Read();
                }
                else
                {
                    return foundWhitespace;
                }
            }

            return false;
        }

        /// <summary>
        /// Reads one character, throwing an exception if not as expected.
        /// </summary>
        /// <param name="c"></param>
        private void Expect(char expected)
        {
            SkipWhitespace();

            if(EndOfStream)
                throw new Exception($"Expected {expected}, end of stream");

            var c = (char)Read();

            if(c != expected)
                throw new Exception($"Expected {expected}, found {c}");
            
        }

        private string ReadUntil(char endChar)
        {
            StringBuilder buffer = new StringBuilder();
            while (!EndOfStream)
            {
                var c = (char) Read();

                if (c == endChar)
                    return buffer.ToString();

                buffer.Append(c);
            }

            throw new Exception($"Expected {endChar}");
        }

        internal ScssPackage ReadTree()
        {
            var package = new ScssPackage(File);
            ScopeNode currentScope = package;
            ReadScopeContent(currentScope);

            return package;
        }

        private void ReadScopeContent(ScopeNode currentScope)
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
                        case '@':
                            if (buffer.Length != 0)
                                throw new Exception("Must be first character");

                            ReadAt(currentScope);

                            break;
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
                                throw new Exception("Why here?");
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
                                var expr = ReadValue(false);

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
                                    else if (pc2 == '{')
                                    {
                                        var result = new NamespaceNode(pn);
                                        ReadScopeContent(result);
                                        currentScope.Add(result);

                                    }
                                    else
                                    {
                                        throw new Exception("Expected { or ;");
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
                            {
                                var node = new SelectorNode();
                                node.Selector = buffer.ToString().Trim();
                                ReadScopeContent(node);
                                currentScope.Add(node);
                                buffer.Clear();
                            }
                            break;
                        case '}':
                            return;
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

            return;
        }

        private void ReadAt(ScopeNode currentScope)
        {
            StringBuilder buffer = new StringBuilder();
            while (!EndOfStream)
            {
                char c = (char) Read();

                if(c == ' ')
                    break;

                buffer.Append(c);
            }

            String type = buffer.ToString();

            SkipWhitespace();

            switch (type)
            {
                case "mixin":
                    ReadMixin(currentScope);
                    break;
                case "include":
                    ReadInclude(currentScope);
                    break;
                case "import":
                    ReadImport(currentScope);
                    break;
                default:
                    throw new Exception($"Could not recognize @{type}");
            }
        }

        private void ReadImport(ScopeNode currentScope)
        {
            string path = ReadUntil(';');

            currentScope.Add(new ImportNode(path));
        }



        private void ReadMixin(ScopeNode currentScope)
        {
            string name = ReadName();

            var args = ReadArgumentDefinition();

            var node = new MixinNode(name, args.ToArray());
            Expect('{');
            ReadScopeContent(node);
            currentScope.Add(node);

        }

        private void ReadInclude(ScopeNode currentScope)
        {
            string name = ReadName();
            var args = ReadArgumentCall();
            var result = new IncludeNode(name, args.ToArray());
            currentScope.Add(result);
        }

        private IEnumerable<Expression> ReadArgumentCall()
        {
            var result = new List<Expression>();

            Expect('(');
            
            while (!EndOfStream)
            {
                SkipWhitespace();
                char c = (char)Peek();

                if (c == ';')
                {
                    Read();
                    return result;
                }

                Expression expr = new Expression(ReadValue(true));
                result.Add(expr);
            }

            throw new Exception("Could not find argument ending");
        }

        private IEnumerable<VariableNode> ReadArgumentDefinition()
        {
            var result = new List<VariableNode>();

            Expect('(');

            while (!EndOfStream)
            {
                char c = (char) Read();

                switch (c)
                {
                    case '$':
                        string name = ReadName();

                        var node = new VariableNode();
                        node.Name = name;
                        result.Add(node);

                        SkipWhitespace();
                        c = (char) Peek();
                        if (c == ':')
                        {
                            Read();
                            node.Expression = new Expression(ReadValue(true));
                        }
                        
                        break;
                    case ',':
                    case ' ':
                        break;
                    case ')':
                        return result;
                }

                //var expr = ReadValue();
            }

            throw new Exception("Could not find argument ending");
        }


        /// <summary>
        /// Reads a name, stopping at (, {, ; and whitespace.  Avoids consuming the last character
        /// </summary>
        /// <returns></returns>
        private string ReadName()
        {
            char firstChar = (char)Peek();

            if (!char.IsLetter(firstChar))
                throw new Exception("First character in name must be a letter");

            StringBuilder buffer = new StringBuilder();
            buffer.Append((char)Read());

            while (!EndOfStream)
            {
                char c = (char)Peek();

                if (!char.IsLetter(c)
                    && !char.IsDigit(c)
                    && c != '-'
                    && c != '_')
                    break;
                
                buffer.Append((char)Read());
            }

            return buffer.ToString();
        }


        private ValueList ReadValue(bool returnOnComma)
        {
            ValueList result = new ValueList();
            List<ExpressionNode> tempNodes = new List<ExpressionNode>();
            var buffer = new StringBuilder();
            bool afterSpace = false;
            char op = '+';
            string key = null;
            bool inDoubleQuotes = false;

            while (!EndOfStream)
            {
                if (inDoubleQuotes)
                {
                    var c = (char) Read();

                    buffer.Append(c);

                    if (c == '"')
                    {
                        inDoubleQuotes = false;
                    }
                    continue;
                }
                else
                {

                    var c = (char) Peek();

                    if (c == '{' || c == ';' || (c == ',' && returnOnComma))
                    {
                        if (buffer.Length > 0)
                        {
                            tempNodes.Add(ParseExpressionNode(buffer.ToString(), op));
                        }

                        if (tempNodes.Count != 0)
                        {
                            result.Add(key, Expression.CalculateTree(tempNodes.ToArray()));
                            key = null;
                        }

                        if (result.Count == 0)
                            return null;

                        return result;
                    }

                    c = (char) Read();

                    if (c == ')')
                    {
                        if (buffer.Length > 0)
                        {
                            tempNodes.Add(ParseExpressionNode(buffer.ToString(), op));
                            buffer.Clear();
                        }

                        if (tempNodes.Count != 0)
                        {
                            result.Add(key, Expression.CalculateTree(tempNodes.ToArray()));
                            key = null;
                        }

                        return result;
                    }

                    switch (c)
                    {
                        case ',':
                            if (buffer.Length != 0)
                                afterSpace = true;
                            result.PreferComma = true;
                            break;
                        case ' ':
                            if (buffer.Length != 0)
                                afterSpace = true;
                            break;
                        case '(':
                            ExpressionNode inner = ReadValue(false);

                            if (buffer.Length != 0)
                            {
                                inner = new FunctionCallNode(buffer.ToString(), (ValueList) inner);
                                buffer.Clear();
                            }

                            inner.Operator = op;
                            tempNodes.Add(inner);

                            break;
                        case '-':
                        case '+':
                        case '*':
                        case '/':
                            if (c == '-' && !afterSpace)
                                goto default;

                            if (buffer.Length > 0)
                            {
                                tempNodes.Add(ParseExpressionNode(buffer.ToString(), op));
                                buffer.Clear();
                            }
                            op = c;

                            afterSpace = false;
                            break;
                        case '\n':
                            break;
                        case '"':
                            inDoubleQuotes = true;
                            goto default;
                        case ':':
                            key = buffer.ToString().Trim('\"', '\'');
                            buffer.Clear();
                            break;
                        default:
                            if (afterSpace)
                            {
                                tempNodes.Add(ParseExpressionNode(buffer.ToString(), op));
                                op = ' ';
                                buffer.Clear();
                                afterSpace = false;

                                result.Add(key, Expression.CalculateTree(tempNodes.ToArray()));
                                tempNodes.Clear();
                                key = null;
                            }
                            buffer.Append(c);
                            break;
                    }

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
                throw new Exception("Not in use");
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
                throw new Exception("Not in use");
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
            throw new Exception("Not in use anymore");
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