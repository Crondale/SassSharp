using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SassSharp.IO
{
    public class RealFileManager:IFileManager
    {
        public string RootPath { get; set; }

        public RealFileManager()
        {
            RootPath = Directory.GetCurrentDirectory();
        }


        public Stream GetStream(string path)
        {
            return File.Open(path, FileMode.Open, FileAccess.Read);
        }

        public PathFile GetFile(string path)
        {
            string fullPath = Path.Combine(RootPath, path);

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
