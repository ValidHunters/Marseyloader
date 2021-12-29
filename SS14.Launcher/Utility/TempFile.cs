using System.IO;

namespace SS14.Launcher.Utility;

public static class TempFile
{
    public static FileStream CreateTempFile()
    {
        return new TempFileStream(Path.GetTempFileName());
    }

    private sealed class TempFileStream : FileStream
    {
        public TempFileStream(string path) : base(path, FileMode.Open, FileAccess.ReadWrite)
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            File.Delete(Name);
        }
    }
}
