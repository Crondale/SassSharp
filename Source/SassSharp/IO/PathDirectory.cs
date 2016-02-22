using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SassSharp.IO
{
    public class PathDirectory
    {
        private static List<string[]> searchPatterns = new List<string[]>()
        {
            new []{"^(.*)$", "$1"},
            new []{"^(.*)$", "$1.scss"},
            new []{ "^([^/]+/)*([^/]+)$", "$1_$2.scss" }

        }; 

        private readonly IFileManager _fileManager;

        public string Path { get; }

        internal PathDirectory(IFileManager fileManager, string path)
        {
            _fileManager = fileManager;
            Path = path;
        }

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
