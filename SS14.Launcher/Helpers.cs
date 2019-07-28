using System.IO;
using System.IO.Compression;

namespace SS14.Launcher
{
    public static class Helpers
    {
        public static void ExtractZipToDirectory(string directory, Stream zipStream)
        {
            using (var zipArchive = new ZipArchive(zipStream))
            {
                zipArchive.ExtractToDirectory(directory);
            }
        }

        public static void ClearDirectory(string directory)
        {
            var dirInfo = new DirectoryInfo(directory);
            foreach (var fileInfo in dirInfo.EnumerateFiles())
            {
                fileInfo.Delete();
            }

            foreach (var childDirInfo in dirInfo.EnumerateDirectories())
            {
                childDirInfo.Delete(true);
            }
        }
    }
}