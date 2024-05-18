using System.IO.Pipes;
using System.Text;
using Marsey.Misc;

namespace Marsey.IPC;

public class Server
{
    public async Task ReadySend(string name, string data)
    {
        MarseyLogger.Log(MarseyLogger.LogType.INFO, "IPC-SERVER", $"Opening {name}");
        await using NamedPipeServerStream pipeServer = new NamedPipeServerStream(name, PipeDirection.Out);
        await pipeServer.WaitForConnectionAsync();

        byte[] buffer = Encoding.UTF8.GetBytes(data);
        await pipeServer.WriteAsync(buffer);

        MarseyLogger.Log(MarseyLogger.LogType.INFO, "IPC-SERVER", $"Closing {name}");
        pipeServer.Close();
    }
}
