using System.Net.Http.Json;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using tsombot.config;

internal class Program
{
    private static DiscordClient client { get; set; }
    private static CommandsNextExtension commands { get; set; }

    private static async Task Main(string[] args)
    {
        //Initialize Discord Bot
        var configReader = new ConfigReader();
        await configReader.readConfig();

        var discordConfig = new DiscordConfiguration()
        {
            Intents = DiscordIntents.All,
            Token = configReader.token,
            TokenType = TokenType.Bot,
            AutoReconnect = true,
        };

        client = new DiscordClient(discordConfig);

        client.Ready += Client_Ready;

        await client.ConnectAsync();

        Task apiTask = Task.Run(ProcessRepositoriesAsync);

        await Task.Delay(-1);
    }

    static async Task ProcessRepositoriesAsync()
    {   
        HttpClient client = new HttpClient();

        var jsonContent = JsonContent.Create(new
        {
            payload = new
            {
                guildId = "l943tTO8QQ-_IwWHfwyJuQ",
                includeRecentGuildActivityInfo = true
            },
            enums = false
        });


        var response = await client.PostAsync("https://swgoh-comlink-latest-nfw1.onrender.com/guild", jsonContent);
        var responseString = await response.Content.ReadAsStringAsync();

        Console.WriteLine(responseString);
    }

    private static Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args)
    {
        return Task.CompletedTask;
    }
}
