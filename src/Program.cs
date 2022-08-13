using Discord;
using Discord.WebSocket;
using System.Text;

class Program
{
    public static char COMMAND_PREFIX = '.';

    public static Task Main(string[] args) => new Program().MainAsync();

    private DiscordSocketClient client;

    public async Task MainAsync()
    {
        client = new DiscordSocketClient();

        client.Log += Log;
        client.MessageReceived += OnMessage;

        string token = "ODgzNjI0NDg3NjU5MTM1MDE2.G4UNG0.2l8V6Xq9OlS1qJZIKdJYyGI8e1kC5pmHaKswjo";

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();
        
        await Task.Delay(-1);
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private async Task OnMessage(SocketMessage msg)
    {
        if (msg.Author.IsBot) return;

        string messageContent = msg.Content;

        if (messageContent.StartsWith(COMMAND_PREFIX)) await HandleCommand(messageContent);

        if (messageContent.StartsWith("```\n&\n") && messageContent.EndsWith("```")) // 
        {
            string programCode = messageContent.Substring(6, messageContent.Length - 9);
            
            Environment env = new Environment(msg.Channel);
            env.programCode = programCode;
            env.dumpMemory = true;

            await RunEnvironment(env);
        }
    }

    private async Task HandleCommand(string message)
    {
        
    }

    private async Task RunEnvironment(Environment env)
    {
        string fileName = ".\\memoryDump.txt";

        if (env.Compile())
        {
            if (env.dumpMemory)
            {
                if (File.Exists(fileName)) File.Delete(fileName);
                Console.WriteLine("Hello");
                using (FileStream fs = File.Create(fileName))
                {
                    byte[] toWrite = new UTF8Encoding(true).GetBytes(env.preMemoryDump);
                    fs.Write(toWrite, 0, toWrite.Length);
                }

                await env.channelSent.SendFileAsync(fileName);
            }

            if (env.Run())
            {
                if (env.dumpMemory)
                {
                    if (File.Exists(fileName)) File.Delete(fileName);

                    using (FileStream fs = File.Create(fileName))
                    {
                        byte[] toWrite = new UTF8Encoding(true).GetBytes(env.postMemoryDump);
                        fs.Write(toWrite, 0, toWrite.Length);
                    }

                    await env.channelSent.SendFileAsync(fileName);
                }
            }
        }
        Console.WriteLine(env.clientTasks.consoleBuffer);
        if (env.clientTasks.consoleBuffer.Length > 0) await env.channelSent.SendMessageAsync(env.clientTasks.consoleBuffer);
    }
}
