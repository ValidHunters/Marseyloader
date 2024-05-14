using System.IO.Pipes;


namespace Marsey.IPC;

public class Client
{
    public string ConnRecv(string name)
    {
        using NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", name, PipeDirection.In);

        pipeClient.Connect();

        using StreamReader reader = new StreamReader(pipeClient);
        return reader.ReadToEnd();
    }
}
