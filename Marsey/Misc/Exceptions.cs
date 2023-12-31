using System;

namespace Marsey.Misc;

public class PatchAssemblyException : Exception
{
    public PatchAssemblyException(string message) : base(message) {}

    public override string ToString()
    {
        MarseyLogger.Log(MarseyLogger.LogType.FATL, Message);
        return base.ToString();
    }
}

public class HideseyException : Exception
{
    public HideseyException(string message) : base(message) {}

    public override string ToString()
    {
        MarseyLogger.Log(MarseyLogger.LogType.FATL, Message);
        return base.ToString();
    }
}
