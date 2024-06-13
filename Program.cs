using System.Net.Http.Json;

await ProcessRepositoriesAsync();

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
}

interface IGuild
{
    IMember[] members { get; }
}

interface IMember
{
    int playerId { get; }
}
