using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Syren.Syren.DataTypes;
using System;
using System.Net;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Syren.Syren.Commands
{
    public class Ai
    {
        private DiscordSocketClient _client;
        public Ai(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task chat(SocketMessage message)
        {
            if (message.Content.Contains(_client.CurrentUser.Id.ToString().Remove(5)))
            {
                var input = message.Content.Replace($"<@{_client.CurrentUser.Id.ToString()}>", "").Trim();
                if(input.StartsWith("<@&"))
                {
                    input = input.Substring(22).Trim();
                }
                if (input == null) return;
                string apiKey = "";
                var model = "text-davinci-003";

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var content = new StringBuilder();
                content.Append("{");
                content.Append("\"model\": \"" + model + "\",");
                content.Append("\"prompt\": \"" + input + "\",");
                content.Append("\"max_tokens\": 100");
                content.Append("\"temperature\": 0.5");
                content.Append("}");

                var response = client.PostAsync("https://api.openai.com/v1/completions", new StringContent(content.ToString(), Encoding.UTF8, "application/json")).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                if (responseString != null)
                {
                    var data = JsonConvert.DeserializeObject<AiJson.AIJsonRoot>(responseString);
                    await message.Channel.SendMessageAsync(data.Choices[0].Text);
                }
                else { await message.Channel.SendMessageAsync("An error occured generating a response."); }
            }
        }
    }
}
