using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SassSharp.IO;
using SassSharp.Model;
using SassSharp.Model.Expressions;
using SassSharp.Model.Nodes;

namespace SassSharp
{
    internal class ScssReader : StreamReader
    {
        private int _lineNumber;


        public ScssReader(PathFile file) : base(file.GetStream())
        {
            File = file;
        }

        public PathFile File { get; set; }


        public override int Peek()
        {
            var c = base.Peek();

            //Ignore CR
            while ((char) c == '\r')
            {
                base.Read();
                c = base.Peek();
            }

            return c;
        }

        public override int Read()
        {
            var c = base.Read();

            //Ignore CR
            while ((char) c == '\r')
            {
                c = base.Read();
            }

            if ((char) c == '\n')
                ++_lineNumber;

            return c;
        }

        /// <summary>
        ///     Skips whitespace
        /// </summary>
        /// <returns>true if whitespace was found</returns>
        private bool SkipWhitespace()
        {
            var foundWhitespace = false;

            while (!EndOfStream)
            {
                var c = (char) Peek();

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
        ///     Reads one character, throwing an exception if not as expected.
        /// </summary>
        /// <param name="c"></param>
        private void Expect(char expected)
        {
            SkipWhitespace();

            if (EndOfStream)
                throw new Exception($"Expected {expected}, end of stream");

            var c = (char) Read();

            if (c != expected)
                throw new Exception($"Expected {expected}, found {c}");
        }

        private string ReadUntil(char endChar)
        {
            var buffer = new StringBuilder();
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
                var c = (char) Read();

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
                            var name = ReadUntil(':');
                            var vn = new VariableNode(name);
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
                            var pc = (char) Peek();
                            if (char.IsLetter(pc)) //hover etc.
                            {
                                buffer.Append(c);
                            }
                            else
                            {
                                var pn = new PropertyNode();
                                pn.Name = buffer.ToString().Trim();
                                pn.Expression = new Expression(ReadValueList(';', '{'));
                                buffer.Clear();

                                var pc2 = (char) Read();
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
        }

        private void ReadAt(ScopeNode currentScope)
        {
            var buffer = new StringBuilder();
            while (!EndOfStream)
            {
                var c = (char) Read();

                if (c == ' ')
                    break;
                if (c == ';')
                    break;

                buffer.Append(c);
            }

            var type = buffer.ToString();

            SkipWhitespace();

            switch (type)
            {
                case "media":
                    ReadMedia(currentScope);
                    break;
                case "mixin":
                    ReadMixin(currentScope);
                    break;
                case "content":
                    ReadContent(currentScope);
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
                case "if":
                    ReadIf(currentScope);
                    break;
                case "else":
                    ReadElse(currentScope);
                    break;
                default:
                    throw new Exception($"Could not recognize @{type}");
            }
        }


        private void ReadIf(ScopeNode currentScope)
        {
            var val = ReadValueList('{');
            Expect('{');

            var ifNode = new IfNode(val);
            currentScope.Add(ifNode);

            ReadScopeContent(ifNode);
        }

        private void ReadElse(ScopeNode currentScope)
        {
            ElseNode elseNode;

            if (Peek() == 'i') //Assume if
            {
                Read();
                Expect('f');
                SkipWhitespace();

                var val = ReadValueList('{');
                Expect('{');

                elseNode = new ElseNode(val);
            }
            else
            {
                elseNode = new ElseNode();
            }

            var parentIf = currentScope.Nodes.Last() as IfNode;

            if (parentIf == null)
                throw new Exception("Else without if");

            parentIf.Elses.Add(elseNode);

            ReadScopeContent(elseNode);
        }


        private void ReadImport(ScopeNode currentScope)
        {
            var path = ReadUntil(';');

            currentScope.Add(new ImportNode(path));
        }


        private void ReadMedia(ScopeNode currentScope)
        {
            var def = ReadUntil('{');

            MediaNode node = new MediaNode()
            {
                Definition = def.TrimEnd()
            };


            ReadScopeContent(node);
            currentScope.Add(node);
        }


        private void ReadMixin(ScopeNode currentScope)
        {
            var name = ReadName();

            VariableNode[] args = new VariableNode[0];

            SkipWhitespace();

            if (Peek() == '(')
                args = ReadArgumentDefinition().ToArray();

            var node = new MixinNode(name, args);
            Expect('{');
            ReadScopeContent(node);
            currentScope.Add(node);
        }

        private void ReadContent(ScopeNode currentScope)
        {
            currentScope.Add(new ContentNode());
        }

        private void ReadInclude(ScopeNode currentScope)
        {
            var name = ReadName();
            ValueList args = new ValueList();

            SkipWhitespace();

            if (Peek() == '(')
                args = ReadArgumentCall();

            SkipWhitespace();

            var result = new IncludeNode(name, args);
            currentScope.Add(result);

            if (Peek() == '{')
            {
                Expect('{');
                ReadScopeContent(result);
            }
            else
                Expect(';');
        }

        private void ReadFunction(ScopeNode currentScope)
        {
            var name = ReadName();

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
                var c = (char) Read();

                switch (c)
                {
                    case '$':
                        var name = ReadName();

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
        ///     Reads a name, stopping at (, {, ; and whitespace.  Avoids consuming the last character
        /// </summary>
        /// <returns></returns>
        private string ReadName()
        {
            var firstChar = (char) Peek();

            if (!char.IsLetter(firstChar))
                throw new Exception("First character in name must be a letter");

            var buffer = new StringBuilder();
            buffer.Append((char) Read());

            while (!EndOfStream)
            {
                var c = (char) Peek();

                if (!char.IsLetter(c)
                    && !char.IsDigit(c)
                    && c != '-'
                    && c != '_')
                    break;

                buffer.Append((char) Read());
            }

            return buffer.ToString();
        }

        private ValueList ReadValueList(params char[] expectedEndCharaters)
        {
            ValueList commaList = null;
            ValueList spaceList = null;
            string key = null;

            var tempNodes = new List<ExpressionNode>();
            var op = '+';

            while (!EndOfStream)
            {
                var afterSpace = SkipWhitespace();
                var c = (char) Peek();

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
                    if (spaceList == null)
                    {
                        spaceList = new ValueList();
                    }
                    spaceList.Add(node);
                    return spaceList;
                }

                switch (c)
                {
                    case ')':
                    case '{':
                    case ';':
                    {
                        throw new Exception("Unexpected end character");
                    }
                    case '!':
                        Read();
                        c = (char) Peek();
                        if (c == '=')
                        {
                            op = '!';
                            Read();
                            break;
                        }
                        var name = ReadName();

                        if (spaceList == null)
                        {
                            spaceList = new ValueList();
                        }

                        spaceList.Add(Expression.CalculateTree(tempNodes.ToArray()));
                        tempNodes.Clear();

                        //Make a special modifierNode?
                        tempNodes.Add(new ValueNode($"!{name}")
                        {
                            Operator = '+'
                        });

                        continue;
                    case '=':
                        op = c;
                        Read();
                        Expect('=');
                        break;
                    case '-':
                    case '+':
                    case '*':
                    case '/':
                    case '<':
                    case '>':
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

                c = (char) Peek();

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
                        var name = ReadName();
                        expressionNode = new ReferenceNode(name);
                        break;
                    default:
                        var val = ReadValueListItem();
                        expressionNode = val.Value;

                        if (val.Key != null)
                        {
                            if (tempNodes.Count > 0)
                                throw new Exception("Unexpected key");

                            key = val.Key;
                        }
                        break;
                }

                //TODO Fix this opening slash hack
                if (expressionNode is ValueNode && ((ValueNode) expressionNode).Type == ValueNode.ValueNodeType.Text &&
                    op == '/')
                {
                    var vn = ((ValueNode) expressionNode);
                    expressionNode = new ValueNode("/" + vn.Value);
                }
                else
                    expressionNode.Operator = op;

                tempNodes.Add(expressionNode);
            }

            throw new Exception("Could not find end of expression");
        }

        private KeyValuePair<string, ExpressionNode> ReadValueListItem()
        {
            var sb = new StringBuilder();
            string key = null;

            while (!EndOfStream)
            {
                var c = (char) Peek();

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
            var quote = (char) Read();
            var escape = false;

            buffer.Append(quote);

            while (!EndOfStream)
            {
                var c = (char) Read();
                buffer.Append(c);

                if (!escape && c == quote)
                    return;

                escape = false;

                if (c == '\\')
                    escape = true;
            }
        }

        private void ReadInlineComment()
        {
            var buffer = new StringBuilder("//");

            while (!EndOfStream)
            {
                var c = (char) Read();

                if (c == '\n')
                    return; //Ignore single line comments for now
                buffer.Append(c);
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