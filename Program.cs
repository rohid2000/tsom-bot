using DSharpPlus;
using DSharpPlus.CommandsNext;
using tsom_bot.config;
using tsom_bot.Commands;
using tsom_bot.Fetcher.database;
using tsom_bot.Commands.Helpers;
using tsom_bot;
using DSharpPlus.SlashCommands;
using tsom_bot.i18n;
using MySqlX.XDevAPI;
using tsom_bot.Commands.Helpers.EventQueue;
using static System.Runtime.InteropServices.JavaScript.JSType;
using DSharpPlus.Entities;
using System.Data;
using DSharpPlus.EventArgs;
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

        var slash = client.UseSlashCommands();

        slash.RegisterCommands<TicketTrackerCommand>();
        slash.RegisterCommands<PromotionCommand>();
        slash.RegisterCommands<GuildSwitchCommand>();
        slash.RegisterCommands<DiscordNameSync>();
        slash.RegisterCommands<TimeCommand>();

        commands = client.UseCommandsNext(commandsConfig);

        //Initialize commands here
        commands.RegisterCommands<commandTemplate>();

        //Make connection
        await client.ConnectAsync();
        ClientManager.client = client;
        await Database.Init(configReader.connectionString);

        TimerHelper timer = new(client, 60);
        ClientManager.timerStartTime = DateTime.Now;
        i18n.load();
        guildEvents.load();
        //Keep bot running
        await Task.Delay(-1);
    }

    private static Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args)
    {
        return Task.CompletedTask;
    }
}
