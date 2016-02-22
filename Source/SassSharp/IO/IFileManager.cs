using System.IO;

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