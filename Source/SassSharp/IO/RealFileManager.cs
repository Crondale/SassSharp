using System.IO;

namespace SassSharp.IO
{
    public class RealFileManager : IFileManager
    {
        public RealFileManager()
        {
            RootPath = Directory.GetCurrentDirectory();
        }

        public string RootPath { get; set; }


        public Stream GetStream(string path)
        {
            return File.Open(path, FileMode.Open, FileAccess.Read);
        }

        public PathFile GetFile(string path)
        {
            var fullPath = Path.Combine(RootPath, path);

            return new PathFile(this, fullPath);
        }

        public string Combine(params string[] paths)
        {
            return Path.Combine(paths);
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public string GetDirectoryPath(string path)
        {
            return Path.GetDirectoryName(path);
        }
    }
}