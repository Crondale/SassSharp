using System.Collections.Generic;
using System.IO;
using Xunit;

namespace SassSharp.Tests
{
    public class FileTests
    {
        public static IEnumerable<object[]> TestData
        {
            get
            {
                foreach (var path in Directory.GetFiles("testfiles", "*.scss"))
                {
                    if (path.StartsWith("testfiles\\_"))
                        continue;

                    yield return new object[] {path};
                }
            }
        }


        [Theory]
        [MemberData("TestData")]
        public void TestFile(string sourcePath)
        {
            //var sres = new LibSassNet.SassCompiler().CompileFile(sourcePath, OutputStyle.Nested, null, false);
            var sres = new LibSassHost.SassCompiler().CompileFile(sourcePath, null, new LibSassHost.CompilationOptions
            {
                OutputStyle = LibSassHost.OutputStyle.Nested,
                LineFeedType = LibSassHost.LineFeedType.Lf,
                IndentType = LibSassHost.IndentType.Space,
                IndentWidth = 2
            });

            var compiler = new ScssCompiler();

            var result = compiler.CompileFile(sourcePath);
            var expected = sres.CompiledContent;

            // LibSass does not clean up line endings in commments
            expected = expected.Replace("\r", "");

            Assert.Equal(expected, result);
        }
    }
}