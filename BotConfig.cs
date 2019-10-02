using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DiscordBot
{
    public class BotConfig
    {
        private readonly string _fileName;

        public string Token { get; private set; }

        public BotConfig() : this("config.json")
        {
        }

        public BotConfig(string fileName)
        {
            _fileName = fileName;
        }

        public async Task LoadAsync()
        {
            var text = await File.ReadAllTextAsync(_fileName);
            var tmp = JsonConvert.DeserializeObject<ConfigModel>(text);
            Token = tmp.Token;
        }
    }

    public class ConfigModel
    {
        public string Token { get; set; }
    }
}