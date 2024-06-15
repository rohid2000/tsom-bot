using System.Net.Http.Json;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using tsom_bot.config;
using tsom_bot.Commands;

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

        Task apiTask = Task.Run(ProcessRepositoriesAsync);

        var commandsConfig = new CommandsNextConfiguration()
        {
            StringPrefixes = new string[] { configReader.prefix },
            EnableMentionPrefix = true,
            EnableDefaultHelp = false,
        };

        commands = client.UseCommandsNext(commandsConfig);

        //Initialize commands here
        commands.RegisterCommands<commandTemplate>();

        //Make connection
        await client.ConnectAsync();

        //Keep bot running
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

    DateTime nextChallengesRefresh = FetchTypeHelper.ConvertStringToDateTime(guildData.nextChallengesRefresh);

    Console.WriteLine(nextChallengesRefresh.ToString());

    HttpClient Hclient = new();

    IGuild? guildData = await GuildFetcher.GetGuildById("l943tTO8QQ-_IwWHfwyJuQ", true, Hclient);
}

    private static Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args)
    {
        return Task.CompletedTask;
    }
}
