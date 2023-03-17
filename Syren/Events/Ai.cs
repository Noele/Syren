using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Syren.Syren.DataTypes;
using System.Runtime.InteropServices;
using System.Text;
using static Syren.Syren.DataTypes.AiJson;

namespace Syren.Syren.Events
{
    public class Ai : Event
    {
        public override bool GuildOnly => false;
        private DiscordSocketClient _client;
        private ApiKeys _apiKeys;
        private List<AiRequestMessages> globalConversation;
        private List<AiPrivateConversation> privateConversations;
        public Ai(DiscordSocketClient client, ApiKeys apiKeys)
        {
            _client = client;
            _apiKeys = apiKeys;
            globalConversation = new List<AiRequestMessages>();
            privateConversations = new List<AiPrivateConversation>();
        }

        public override async Task run(SocketMessage message, SocketCommandContext context)
        {
            var isPrivateConversation = message.Channel.GetChannelType() == ChannelType.DM;
            var isReferencingBot = false;
            if (message.Reference != null)
            {
                isReferencingBot = message.Channel.GetMessageAsync(message.Reference.MessageId.Value).Result.Author.Id == _client.CurrentUser.Id;
            }
            if (message.Content.Contains($"<@{_client.CurrentUser.Id}>") || isReferencingBot || isPrivateConversation)
            {
                var input = message.Content.Replace($"<@{_client.CurrentUser.Id}>", "").Trim();
                if(input == "debug//clear")
                {
                    globalConversation.Clear();
                    return;
                }
                var model = "gpt-3.5-turbo";

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKeys.aiApiKey}");
                string user = "";
                if (isPrivateConversation)
                {
                    var userHasOpenConvo = false;
                    foreach(var convers in privateConversations)
                    {
                        if(convers.id == message.Author.Id)
                        {
                            userHasOpenConvo = true;
                            user = "user";
                            convers.messages.Add(new AiRequestMessages() { role = user, content = input });
                        }
                    }
                    if(!userHasOpenConvo)
                    {
                        user = "system";
                        privateConversations.Add(new AiPrivateConversation() { id = message.Author.Id, messages = new List<AiRequestMessages>() { new AiRequestMessages() { role = user, content = input } } });
                    }
                } else
                {
                    user = globalConversation.Count == 0 ? "system" : "user";
                    globalConversation.Add(new AiRequestMessages() { role = user, content = input });
                }


                var aiRequest = new AiRequest()
                {
                    model = model,
                    max_tokens = 100,
                    temperature = 0.5
                };
                if(isPrivateConversation)
                {
                    foreach (var convers in privateConversations)
                    {
                        if (convers.id == message.Author.Id)
                        {
                            aiRequest.messages = convers.messages;
                        }
                    }
                }
                else
                {
                    aiRequest.messages = globalConversation;
                }

                var content = JsonConvert.SerializeObject(aiRequest).ToString();

                var response = client.PostAsync("https://api.openai.com/v1/chat/completions", new StringContent(content, Encoding.UTF8, "application/json")).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                if (responseString != null)
                {
                    var aiJson = JsonConvert.DeserializeObject<AiJson.AIJsonRoot>(responseString);
                    await message.Channel.SendMessageAsync(aiJson.choices[0].message.content);

                    if(isPrivateConversation)
                    {
                        foreach (var convers in privateConversations)
                        {
                            if (convers.id == message.Author.Id)
                            {
                                convers.messages.Add(new AiRequestMessages() { role = "assistant", content = aiJson.choices[0].message.content });
                            }
                        }
                    } 
                    else
                    {
                        globalConversation.Add(new AiRequestMessages() { role = "assistant", content = aiJson.choices[0].message.content });
                    }

                   }
                else { await message.Channel.SendMessageAsync("An error occured generating a response."); }
            }
        }
    }
}
