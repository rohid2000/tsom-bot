using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using tsom_bot.Models;

public static class GuildFetcher 
{
    private static readonly string url = "https://swgoh-comlink-latest-nfw1.onrender.com/guild";
    private static readonly HttpClient client = new();

    public static async Task<IGuild?> GetGuildById(string guildId, bool includeRecentGuildActivityInfo)
    {
        var bodyContent = new {
            payload = new
            {
                guildId = guildId,
                includeRecentGuildActivityInfo = includeRecentGuildActivityInfo
            },
            enums = false
        };


        var response = await client.PostAsync(url, JsonContent.Create(bodyContent));

        var contentStream = await response.Content.ReadAsStreamAsync();

        Console.WriteLine(await response.Content.ReadAsStringAsync());

        using var streamReader = new StreamReader(contentStream);
        var json = await streamReader.ReadToEndAsync();

        IGuild data = JsonConvert.DeserializeObject<IGuild>(json);

        Console.WriteLine(data);

        return null;
    }

}