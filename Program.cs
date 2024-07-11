using DSharpPlus;
using DSharpPlus.CommandsNext;
using tsom_bot.config;
using tsom_bot.Commands;
using tsom_bot.Fetcher.database;
using tsom_bot.Commands.Helpers;
using tsom_bot;
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
        commands.RegisterCommands<DiscordNameSync>();
        commands.RegisterCommands<PromotionCommand>();

        //Make connection
        await client.ConnectAsync();

        await Database.Init(configReader.connectionString);
        TimerHelper timer = new(client, 60);
        ClientManager.timerStartTime = DateTime.Now;
        //Keep bot running
        await Task.Delay(-1);
    }

    private static Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args)
    {
        return Task.CompletedTask;
    }
}
