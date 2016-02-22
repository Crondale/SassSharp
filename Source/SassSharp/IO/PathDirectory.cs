using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SassSharp.IO
{
    public class PathDirectory
    {
        private static readonly List<string[]> searchPatterns = new List<string[]>
        {
            new[] {"^(.*)$", "$1"},
            new[] {"^(.*)$", "$1.scss"},
            new[] {"^([^/]+/)*([^/]+)$", "$1_$2.scss"}
        };

        private readonly IFileManager _fileManager;

        internal PathDirectory(IFileManager fileManager, string path)
        {
            _fileManager = fileManager;
            Path = path;
        }

        public string Path { get; }

        public PathFile SolveReference(string path)
        {
            foreach (var searchPattern in searchPatterns)
            {
                var resultPath = _fileManager.Combine(Path, Regex.Replace(path, searchPattern[0], searchPattern[1]));

                if (_fileManager.Exists(resultPath))
                {
                    return _fileManager.GetFile(resultPath);
                }
            }

            throw new Exception($"Could not resolve reference: {path}");
        }
    }
}