using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json;
using Syren.Syren.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RunMode = Discord.Interactions.RunMode;

namespace Syren.Syren.SlashCommands
{
    public class Ai : InteractionModuleBase<SocketInteractionContext>
    {
        private DiscordSocketClient _client;
        private ApiKeys _apiKeys;

        public Ai(DiscordSocketClient client, ApiKeys apiKeys)
        {
            _client = client;
            _apiKeys = apiKeys;
        }


        [SlashCommand("image", "returns a dal-e image from a prompt", runMode: RunMode.Async)]
        public async Task image([Remainder] string prompt)
        {
            await DeferAsync();
            var url = "https://api.openai.com/v1/images/generations";
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKeys.aiApiKey}");

            var imageRequest = new ImageRequest()
            {
                prompt = prompt,
                n = 1,
                size = "1024x1024",
                response_format = "url",
                user = "Zieypie"
            };

            var content = JsonConvert.SerializeObject(imageRequest).ToString();

            var response = client.PostAsync(url, new StringContent(content, Encoding.UTF8, "application/json")).Result;
            var responseString = response.Content.ReadAsStringAsync().Result;
            if (responseString != null)
            {
                var imageJson = JsonConvert.DeserializeObject<ImageJson.ImageRoot>(responseString);
                await FollowupAsync(imageJson.data[0].url);
            }
            else { await FollowupAsync("An error occured generating a response."); }
        }
    }
}