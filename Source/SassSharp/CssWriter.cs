using System.IO;
using SassSharp.Model.Css;

namespace SassSharp
{
    internal class CssWriter : StreamWriter
    {
        private readonly string _indentation = "  ";
        private readonly string _lineBreak = "\n";

        public CssWriter(Stream stream)
            : base(stream)
        {
        }

        internal void Write(CssSheet sheet)
        {
            WriteSheet(sheet);
            //this.Write(_lineBreak);
        }

        private void WriteSheet(CssSheet sheet)
        {
            var lastLevel = -1;
            foreach (var node in sheet.Nodes)
            {
                if (node is CssSelector)
                {
                    lastLevel = WriteSelector(lastLevel, (CssSelector) node);
                }
                else if (node is CssComment)
                {
                    WriteComment((CssComment) node);
                }
                else if (node is CssMedia)
                {
                    WriteMedia(lastLevel, (CssMedia) node);
                }
                else if (node is CssImport)
                {
                    WriteImport((CssImport) node);
                }
                Write(_lineBreak);
            }
        }

        private void WriteMedia(int lastLevel, CssMedia node)
        {
            for (var i = 0; i < node.Level; i++)
                Write(_indentation);

            Write("@media {0}", node.Definition);
            Write(" {");

            foreach (var child in node.Nodes)
            {
                Write(_lineBreak);
                if (child is CssSelector)
                {
                    WriteSelector(node.Level, (CssSelector) child);
                }
            }

            Write(" }");
        }

        private void WriteImport(CssImport node)
        {
            Write("@import {0};", node.Path);
        }

        private int WriteSelector(int lastLevel, CssSelector selector)
        {
            if (selector.Level == 0 && lastLevel != -1)
                Write(_lineBreak);

            for (var i = 0; i < selector.Level; i++)
                Write(_indentation);

            Write(selector.Selector);
            Write(" {");
            WriteSelectorContent(selector);
            Write(" }");

            lastLevel = selector.Level;
            return lastLevel;
        }

        private void WriteSelectorContent(CssSelector selector)
        {
            foreach (var node in selector.Nodes)
            {
                Write(_lineBreak);
                if (node is CssProperty)
                {
                    WriteProperty((CssProperty) node);
                }
                if (node is CssComment)
                {
                    WriteComment((CssComment) node);
                }
            }
        }

        private void WriteComment(CssComment comment)
        {
            for (var i = 0; i < comment.Level; i++)
                Write(_indentation);

            Write(comment.Comment);
        }

        private void WriteProperty(CssProperty property)
        {
            for (var i = 0; i < property.Level; i++)
                Write(_indentation);

            Write(property.Name);
            Write(": ");
            Write(property.Value);
            Write(";");
        }
    }
}