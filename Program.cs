using DSharpPlus;
using DSharpPlus.CommandsNext;
using tsom_bot.config;
using tsom_bot.Commands;
using tsom_bot.Fetcher.database;
using tsom_bot.Commands.Helpers;

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
            TokenType = DSharpPlus.TokenType.Bot,
            AutoReconnect = true,
        };

        client = new DiscordClient(discordConfig);

        client.Ready += Client_Ready;

        var commandsConfig = new CommandsNextConfiguration()
        {
            StringPrefixes = new string[] { configReader.prefix },
            EnableMentionPrefix = true,
            EnableDefaultHelp = false,
        };

        commands = client.UseCommandsNext(commandsConfig);

        //Initialize commands here
        commands.RegisterCommands<commandTemplate>();
        commands.RegisterCommands<TicketTrackerCommand>();

        //Make connection
        await client.ConnectAsync();

        Database.Init("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\dev\\tsom-bot\\Fetcher\\database\\Database1.mdf;Integrated Security=True");
        TimerHelper timer = new(client, 60);
        //Keep bot running
        await Task.Delay(-1);
    }

    private static Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args)
    {
        return Task.CompletedTask;
    }
}
