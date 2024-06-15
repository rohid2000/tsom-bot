using System.Net.Http.Json;

await ProcessRepositoriesAsync();

static async Task ProcessRepositoriesAsync()
{
    var a = await GuildFetcher.GetGuildById("l943tTO8QQ-_IwWHfwyJuQ", true);

    Console.WriteLine(a);
}