using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crondale.SassSharp;
using Xunit;
using Xunit.Extensions;

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
                    if(path.StartsWith("testfiles\\_"))
                        continue;

                    yield return new object[] { path, path.Replace(".scss", ".css") };
                }
            }
        }


        [Theory]
        [MemberData("TestData")]
        public void TestFile(string sourcePath, string resultPath)
        {
            var compiler = new ScssCompiler();

            string result = compiler.Compile(File.ReadAllText(sourcePath));
            string expected = File.ReadAllText(resultPath);

            Assert.Equal(expected, result);
        }

    }
}
