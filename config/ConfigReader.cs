using Newtonsoft.Json;

namespace tsom_bot.config
{
    public class ConfigReader
    {
        public string token { get; set; }
        public string prefix { get; set; }

        public async Task readConfig()
        {
            using (StreamReader sr = new StreamReader("config.json"))
            {
                string json = await sr.ReadToEndAsync();
                ConfigStructure data = JsonConvert.DeserializeObject<ConfigStructure>(json);

                if (data != null)
                {
                    this.token = data.token;
                    this.prefix = data.prefix;
                }
            }
        }
    }

    internal sealed class ConfigStructure
    {
        public string token { get; set; }
        public string prefix { get; set; }
    }
}
