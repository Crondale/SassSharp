﻿using System.Collections.Generic;
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

                    yield return new object[] {path, path.Replace(".scss", ".css")};
                }
            }
        }


        [Theory]
        [MemberData("TestData")]
        public void TestFile(string sourcePath, string resultPath)
        {
            var compiler = new ScssCompiler();

            var result = compiler.Compile(File.ReadAllText(sourcePath));
            var expected = File.ReadAllText(resultPath);

            Assert.Equal(expected, result);
        }
    }
}