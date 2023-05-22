using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Syren.Syren.DataTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Syren.Syren.Commands
{
    public class Fortnite : ModuleBase<SocketCommandContext>
    {
        private ApiKeys _apiKeys;
        public Fortnite(ApiKeys apiKeys)
        {
            _apiKeys = apiKeys;
        }

        [Command("fortnite"), Alias("fn")]
        public async Task fortnite([Optional] string name, [Optional] string season)
        {
            if (name == null) { await ReplyAsync("Please enter a valid name."); return; }
            var validSeasons = new List<String> { "lifetime", "season", null };
            if (!validSeasons.Contains(season))
            {
                await ReplyAsync($"{season} is not a valid season (lifetime / season)");
                return;
            }
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", _apiKeys.fortniteApiKey);
            
            var response = client.GetAsync($"https://fortnite-api.com/v2/stats/br/v2?name={name}&image=all&timeWindow={season}").Result;
            if (response.IsSuccessStatusCode) { 
                var responseString = response.Content.ReadAsStringAsync().Result;
                if (responseString != null)
                {
                    var data = JsonConvert.DeserializeObject<FortniteJson.FortniteRoot>(responseString);
                    if (data != null)
                    {

                        await ReplyAsync(data.data.image);
                        return;
                    }
                }
            } else
            {
                await ReplyAsync("An account with that name could not be found.");
            }
        }  
    }
}
