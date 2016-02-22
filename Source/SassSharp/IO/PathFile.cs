using System.IO;

namespace SassSharp.IO
{
    public class PathFile
    {
        private readonly IFileManager _fileManager;

        internal PathFile(IFileManager fileManager, string path)
        {
            _fileManager = fileManager;
            Path = path;
        }

        public string Path { get; }

        public PathDirectory Directory
        {
            get { return new PathDirectory(_fileManager, _fileManager.GetDirectoryPath(Path)); }
        }

        public PathDirectory Dictionary { get; set; }

        public Stream GetStream()
        {
            return _fileManager.GetStream(Path);
        }

        public PathFile SolveReference(string path)
        {
            return Directory.SolveReference(path);
        }
    }
}