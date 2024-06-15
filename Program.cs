using System.Net.Http.Json;

await ProcessRepositoriesAsync();

static async Task ProcessRepositoriesAsync()
{
    HttpClient client = new();

    IGuild? guildData = await GuildFetcher.GetGuildById("l943tTO8QQ-_IwWHfwyJuQ", true, client);

    DateTime nextChallengesRefresh = FetchTypeHelper.ConvertStringToDateTime(guildData.nextChallengesRefresh);

    Console.WriteLine(nextChallengesRefresh.ToString());
}