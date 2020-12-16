using System.Collections.Immutable;
using Robust.LoaderApi;

namespace SS14.Loader
{
    internal sealed class MainArgs : IMainArgs
    {
        public MainArgs(string[] args, IFileApi fileApi)
        {
            Args = args;
            FileApi = fileApi;
        }

        public string[] Args { get; }
        public IFileApi FileApi { get; }
    }
}
