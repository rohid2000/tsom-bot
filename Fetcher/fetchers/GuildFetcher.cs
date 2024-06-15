using System.Net.Http.Json;
using Newtonsoft.Json;

public static class GuildFetcher 
{
    private static readonly string url = "https://swgoh-comlink-latest-nfw1.onrender.com/guild";

    public static async Task<IGuild?> GetGuildById(string guildId, bool includeRecentGuildActivityInfo, HttpClient client)
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

        using (StreamReader sr = new StreamReader(contentStream))
        {
            string json = await sr.ReadToEndAsync();
            Guild? data = JsonConvert.DeserializeObject<Guild>(json);

            return data?.guild;
        }
    }

}

public class Guild
{
    public IGuild guild;
}