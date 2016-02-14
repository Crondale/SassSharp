using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SassSharp.IO
{
    public interface IFileManager
    {
        Stream GetStream(string path);

        PathFile GetFile(string path);

        string Combine(params string[] paths);

        bool Exists(string path);

        string GetDirectoryPath(string path);
    }
}
