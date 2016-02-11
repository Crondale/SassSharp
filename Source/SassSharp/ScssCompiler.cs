using System;
using System.IO;
using System.Text;

namespace Crondale.SassSharp
{
    public class ScssCompiler
    {

        public string Compile(string source)
        {

            using (var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(source)))
            using (var destination = new MemoryStream())
            {
                using (var writer = new StreamWriter(destination))
                using (var reader = new StreamReader(sourceStream))
                {
                    Compile(reader, writer);
                }

                return Encoding.UTF8.GetString(destination.ToArray());
            }

        }

        public void Compile(StreamReader source, StreamWriter destination)
        {
            var reader = new ScssReader();

            var tree = reader.ReadTree(source);

            var writer = new CssWriter();

            writer.Write(tree, destination);
        }

    }
}
