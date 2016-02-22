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
            package.LoadBuiltInFunctions();

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
                        case '$':
                            string name = ReadUntil(':');
                            VariableNode vn = new VariableNode(name);
                            vn.Expression = new Expression(ReadValueList(';'));
                            Expect(';');
                            currentScope.Add(vn);
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
                                throw new Exception("Unexpected ;");
                            }
                        case ':':
                            char pc = (char) Peek();
                            if (char.IsLetter(pc))//hover etc.
                            {
                                buffer.Append(c);
                            }
                            else
                            {
                                var pn = new PropertyNode();
                                pn.Name = buffer.ToString().Trim();
                                pn.Expression = new Expression(ReadValueList(';', '{'));
                                buffer.Clear();

                                char pc2 = (char)Read();
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

                                /*
                                var expr = ReadValue(false);

                                if (buffer[0] == '$')
                                {
                                    var node = new VariableNode(
                                        buffer.ToString().Trim().TrimStart('$'),
                                        new Expression(expr));
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
                                }*/
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
                case "function":
                    ReadFunction(currentScope);
                    break;
                case "return":
                    ReadReturn(currentScope);
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
            Expect(';');

            var result = new IncludeNode(name, args);
            currentScope.Add(result);
        }

        private void ReadFunction(ScopeNode currentScope)
        {
            string name = ReadName();

            var args = ReadArgumentDefinition();

            var node = new FunctionNode(name, args.ToArray());
            Expect('{');
            ReadScopeContent(node);
            currentScope.Add(node);

        }

        private void ReadReturn(ScopeNode currentScope)
        {
            ExpressionNode value = ReadValueList(';');
            Expect(';');
            var result = new ReturnNode(new Expression(value));
            currentScope.Add(result);

        }

        private ValueList ReadArgumentCall()
        {

            Expect('(');
            var result = ReadValueList(')');
            Expect(')');
            return result;

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

                        var node = new VariableNode(name);
                        result.Add(node);

                        SkipWhitespace();
                        c = (char) Peek();
                        if (c == ':')
                        {
                            Read();
                            node.Expression = new Expression(ReadValueList(',', ')'));
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

        private ValueList ReadValueList(params char[] expectedEndCharaters)
        {
            ValueList commaList = null;
            ValueList spaceList = null;
            string key = null;
            
            List<ExpressionNode> tempNodes = new List<ExpressionNode>();
            char op = '+';

            while (!EndOfStream)
            {
                bool afterSpace = SkipWhitespace();
                char c = (char) Peek();

                if (expectedEndCharaters.Contains(c))
                {
                    var node = Expression.CalculateTree(tempNodes.ToArray());

                    if (commaList != null)
                    {
                        if (spaceList != null)
                        {
                            spaceList.Add(node);
                            commaList.Add(spaceList);
                            spaceList = null;
                        }
                        else
                        {
                            commaList.Add(node);
                        }

                        return commaList;
                    }
                    else
                    {
                        if (spaceList == null)
                        {
                            spaceList = new ValueList();
                        }
                        spaceList.Add(node);
                        return spaceList;
                    }
                }
                
                switch (c)
                {
                    case ')':
                    case '{':
                    case ';':
                    {
                        throw new Exception("Unexpected end character");
                    }
                    case '-':
                    case '+':
                    case '*':
                    case '/':
                    case '<':
                    case '>':
                    case '=':
                        op = c;
                        Read();
                        break;
                    case ',':
                    {
                        Read();

                        if (commaList == null)
                        {
                            commaList = new ValueList();
                            commaList.PreferComma = true;
                        }

                        var node = Expression.CalculateTree(tempNodes.ToArray());
                        tempNodes.Clear();

                        if (spaceList != null)
                        {
                            spaceList.Add(node);
                            commaList.Add(spaceList);
                            spaceList = null;
                        }
                        else
                        {
                            if (key != null)
                            {
                                commaList.Add(key, node);
                                key = null;
                            }
                            else
                            {
                                commaList.Add(node);
                            }
                            
                        }
                    }
                        break;
                    default:
                        if (afterSpace)
                        {
                            if (spaceList == null)
                            {
                                spaceList = new ValueList();
                            }

                            spaceList.Add(Expression.CalculateTree(tempNodes.ToArray()));
                            tempNodes.Clear();
                        }
                        break;
                }

                SkipWhitespace();

                c = (char)Peek();

                //Expect expression node
                ExpressionNode expressionNode = null;
                
                switch (c)
                {
                    case '(':
                        Read();
                        expressionNode = ReadValueList(')');
                        Expect(')');
                        break;
                    case ')': // This only happens with: ",)"
                        continue;
                    case '$':
                        Read();
                        string name = ReadName();
                        expressionNode = new ReferenceNode(name);
                        break;
                    default:
                        var val = ReadValueListItem();
                        expressionNode = val.Value;

                        if (val.Key != null)
                        {
                            if(tempNodes.Count > 0)
                                throw new Exception("Unexpected key");

                            key = val.Key;
                        }
                        break;
                }

                expressionNode.Operator = op;
                tempNodes.Add(expressionNode);
            }

            throw new Exception("Could not find end of expression");
        }

        private KeyValuePair<string, ExpressionNode> ReadValueListItem()
        {
            StringBuilder sb = new StringBuilder();
            string key = null;
            
            while (!EndOfStream)
            {
                char c = (char) Peek();

                switch (c)
                {
                    case '\'':
                    case '"':
                        ReadString(sb);
                        break;
                    case '(':
                        Read();
                        var n = ReadValueList(')');
                        Expect(')');
                        return new KeyValuePair<string, ExpressionNode>(
                            key,
                            new FunctionCallNode(sb.ToString(), n));
                    case ':':
                        Read();
                        key = sb.ToString().Trim('\"');
                        sb.Clear();
                        SkipWhitespace();
                        break;
                    case ' ':
                    case ')':
                    case '{':
                    case ';':
                    case ',':
                    case '+':
                    case '*':
                    case '/':
                    case '<':
                    case '>':
                    case '=':
                        return new KeyValuePair<string, ExpressionNode>(
                            key,
                            new ValueNode(sb.ToString()));
                    //Will - come into play?  Maybe in eg: 10-1
                    //TODO fix this
                    /*case '-':
                        return new ValueNode(sb.ToString());*/
                    default:
                        Read();
                        sb.Append(c);
                        break;
                }
            }

            throw new Exception();
        }

        private void ReadString(StringBuilder buffer)
        {
            char quote = (char)Read();
            bool escape = false;
            while (!EndOfStream)
            {
                char c = (char) Read();
                buffer.Append(c);

                if(!escape && c == quote)
                    return;

                escape = false;

                if (c == '\\')
                    escape = true;
            }
        }

        private ValueList ReadValue(bool returnOnComma)
        {
            throw new Exception("Not in use anymore");

            ValueList result = new ValueList();
            List<ExpressionNode> tempNodes = new List<ExpressionNode>();
            var buffer = new StringBuilder();
            bool afterSpace = false;
            bool afterOperator = false;
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

                    if (c == ')' || c == '{' || c == ';' || (c == ',' && returnOnComma))
                    {
                        if (buffer.Length > 0)
                        {
                            tempNodes.Add(ParseExpressionNode(buffer.ToString(), op));
                        }

                        if (tempNodes.Count != 0)
                        {
                            result.Add(key, Expression.CalculateTree(tempNodes.ToArray()));
                        }

                        if (result.Count == 0)
                            return null;

                        return result;
                    }

                    c = (char) Read();

                    switch (c)
                    {
                        case ',':
                            if (!afterOperator)
                                afterSpace = true;
                            result.PreferComma = true;
                            break;
                        case ' ':
                            if (!afterOperator)
                                afterSpace = true;
                            break;
                        case '(':
                            ExpressionNode inner = ReadValue(false);

                            Expect(')');

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
                        case '<':
                        case '>':
                        case '=':
                            if (c == '-' && !afterSpace)
                                goto default;

                            if (buffer.Length > 0)
                            {
                                tempNodes.Add(ParseExpressionNode(buffer.ToString(), op));
                                buffer.Clear();
                            }
                            op = c;

                            afterSpace = false;
                            afterOperator = true;
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
                                if (buffer.Length != 0)
                                {
                                    tempNodes.Add(ParseExpressionNode(buffer.ToString(), op));
                                    op = ' ';
                                    buffer.Clear();
                                }

                                if (tempNodes.Count > 0)
                                {
                                    result.Add(key, Expression.CalculateTree(tempNodes.ToArray()));
                                    tempNodes.Clear();
                                }

                                afterSpace = false;
                                key = null;
                            }
                            buffer.Append(c);
                            afterSpace = false;
                            afterOperator = false;
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

        
    }
}