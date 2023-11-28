using System;
namespace Marsey;

public class PatchAssemblyException : Exception
{
    public PatchAssemblyException(string message) : base(message) {}

    public override string ToString()
    {
        MarseyLogger.Log(MarseyLogger.LogType.FATL, Message);
        return base.ToString();
    }
}
