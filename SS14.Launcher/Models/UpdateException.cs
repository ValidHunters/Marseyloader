using System;

namespace SS14.Launcher.Models;

public sealed class UpdateException : Exception
{
    public UpdateException(string message) : base(message)
    {
    }
}