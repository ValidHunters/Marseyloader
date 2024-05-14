using System.IO.Pipes;
using System.Text;

namespace Marsey.IPC;

public class Server
{
    public async Task ReadySend(string name, string data)
    {
        await using NamedPipeServerStream pipeServer = new NamedPipeServerStream(name, PipeDirection.Out);
        await pipeServer.WaitForConnectionAsync();

        byte[] buffer = Encoding.UTF8.GetBytes(data);
        await pipeServer.WriteAsync(buffer);

        pipeServer.Close();
    }
}
