using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Syren.Syren.DataTypes;
using System.Runtime.InteropServices;
using System.Text;

namespace Syren.Syren.Commands
{
    public class Ai : ModuleBase<SocketCommandContext>
    {
        private DiscordSocketClient _client;
        private ApiKeys _apiKeys;

        public Ai(DiscordSocketClient client, ApiKeys apiKeys)
        {
            _client = client;
            _apiKeys = apiKeys;
        }

        public async Task chat(SocketMessage message)
        {
            if (message.Content.Contains($"<@{_client.CurrentUser.Id}>"))
            {
                var input = message.Content.Replace($"<@{_client.CurrentUser.Id}>", "").Trim();

                var model = "text-davinci-003";

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKeys.aiApiKey}");

                var aiRequest = new AiRequest()
                {
                    model = model, 
                    prompt = input, 
                    max_tokens = 100,
                    temperature = 0.5
                };
                var content = JsonConvert.SerializeObject(aiRequest).ToString();

                var response = client.PostAsync("https://api.openai.com/v1/completions", new StringContent(content, Encoding.UTF8, "application/json")).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                if (responseString != null)
                {
                    var aiJson = JsonConvert.DeserializeObject<AiJson.AIJsonRoot>(responseString);
                    await message.Channel.SendMessageAsync(aiJson.Choices[0].Text);
                }
                else { await message.Channel.SendMessageAsync("An error occured generating a response."); }
            }
        }
    }
}
