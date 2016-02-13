using System.IO;
using SassSharp.Model.Css;

namespace SassSharp
{
    internal class CssWriter: StreamWriter
    {
        private string _lineBreak = "\n";
        private string _indentation = "  ";

        public CssWriter(Stream stream)
            :base(stream)
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
                    lastLevel = WriteSelector(lastLevel, (CssSelector)node);
                }
                else if (node is CssComment)
                {
                    WriteComment((CssComment) node);
                }
                this.Write(_lineBreak);
            }
        }

        private int WriteSelector(int lastLevel, CssSelector selector)
        {
            if (selector.Level == 0 && lastLevel != -1)
                this.Write(_lineBreak);

            for (var i = 0; i < selector.Level; i++)
                this.Write(_indentation);

            this.Write(selector.Selector);
            this.Write(" {");
            WriteSelectorContent(selector);
            this.Write(" }");
            
            lastLevel = selector.Level;
            return lastLevel;
        }

        private void WriteSelectorContent(CssSelector selector)
        {
            foreach (var node in selector.Nodes)
            {
                this.Write(_lineBreak);
                if (node is CssProperty)
                {
                    WriteProperty((CssProperty) node);
                }
                if (node is CssComment)
                {
                    WriteComment((CssComment)node);
                }
            }
        }

        private void WriteComment(CssComment comment)
        {
            for (var i = 0; i < comment.Level; i++)
                this.Write(_indentation);

            this.Write(comment.Comment);
        }

        private void WriteProperty(CssProperty property)
        {
            for (var i = 0; i < property.Level; i++)
                this.Write(_indentation);

            this.Write(property.Name);
            this.Write(": ");
            this.Write(property.Value);
            this.Write(";");
        }
    }
}