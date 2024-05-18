using System.IO.Pipes;
using Marsey.Misc;

namespace Marsey.IPC;

public class Client
{
    public string ConnRecv(string name)
    {
        MarseyLogger.Log(MarseyLogger.LogType.DEBG, "IPC-Client", $"Trying to open client with pipe name {name}");
        using NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", name, PipeDirection.In);

        try
        {
            pipeClient.Connect(150); // Pipe should connect immediately
        }
        catch(TimeoutException)
        {
            MarseyLogger.Log(MarseyLogger.LogType.DEBG, "IPC-Client", $"Connection timed out on pipe {name}, pipe not being created?");
            return "";
        }

        using StreamReader reader = new StreamReader(pipeClient);
        return reader.ReadToEnd();
    }
}

