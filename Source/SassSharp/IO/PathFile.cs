using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SassSharp.IO
{
    public class PathFile
    {
        private readonly IFileManager _fileManager;

        public string Path { get; }

        internal PathFile(IFileManager fileManager, string path)
        {
            _fileManager = fileManager;
            Path = path;
        }

        public Stream GetStream()
        {
            return _fileManager.GetStream(Path);
        }

        public PathDirectory Directory
        {
            get { return new PathDirectory(_fileManager, _fileManager.GetDirectoryPath(Path)); }
        }

        public PathFile SolveReference(string path)
        {
            return Directory.SolveReference(path);
        }

        public PathDirectory Dictionary { get; set; }
    }
}
