using System.IO;
using SassSharp.Model.Css;

namespace SassSharp
{
    internal class CssWriter
    {
        private string _lineBreak = "\n";
        private string _indentation = "  ";


        internal void Write(CssSheet sheet, StreamWriter sw)
        {
            
            WriteSheet(sheet, sw);
            //sw.Write(_lineBreak);
        }

        private void WriteSheet(CssSheet sheet, StreamWriter sw)
        {
            var lastLevel = -1;
            foreach (var node in sheet.Nodes)
            {
                if (node is CssSelector)
                {
                    lastLevel = WriteSelector(sw, lastLevel, (CssSelector)node);
                }
                else if (node is CssComment)
                {
                    WriteComment((CssComment) node, sw);
                }
                sw.Write(_lineBreak);
            }
        }

        private int WriteSelector(StreamWriter sw, int lastLevel, CssSelector selector)
        {
            if (selector.Level == 0 && lastLevel != -1)
                sw.Write(_lineBreak);

            for (var i = 0; i < selector.Level; i++)
                sw.Write(_indentation);

            sw.Write(selector.Selector);
            sw.Write(" {");
            WriteSelectorContent(selector, sw);
            sw.Write(" }");
            
            lastLevel = selector.Level;
            return lastLevel;
        }

        private void WriteSelectorContent(CssSelector selector, StreamWriter sw)
        {
            foreach (var node in selector.Nodes)
            {
                sw.Write(_lineBreak);
                if (node is CssProperty)
                {
                    WriteProperty((CssProperty) node, sw);
                }
                if (node is CssComment)
                {
                    WriteComment((CssComment)node, sw);
                }
            }
        }

        private void WriteComment(CssComment comment, StreamWriter sw)
        {
            for (var i = 0; i < comment.Level; i++)
                sw.Write(_indentation);

            sw.Write(comment.Comment);
        }

        private void WriteProperty(CssProperty property, StreamWriter sw)
        {
            for (var i = 0; i < property.Level; i++)
                sw.Write(_indentation);

            sw.Write(property.Name);
            sw.Write(": ");
            sw.Write(property.Value);
            sw.Write(";");
        }
    }
}