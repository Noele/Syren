using Discord;
using Discord.Interactions;
using Newtonsoft.Json;
using Syren.Syren.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Syren.Syren.DataTypes.LeagueOfLegendsPlayerData;

namespace Syren.Syren.SlashCommands
{
    public class LeagueOfLegends : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("leaderboard", "Displays the 'Who's that champion' leaderboard")]
        public async Task Leaderboard(string pageQuery = "0")
        {
            await DeferAsync();
            var dict = new Dictionary<string, int>();
            var text = File.ReadAllText("Data/LeagueOfLegends/players.json");
            var playerData = JsonConvert.DeserializeObject<LeagueOfLegendsPlayerData.Players>(text);
          
            foreach (var player in playerData.players)
            {
                dict.Add(player.name, player.points);
            }

            var sortedDict = from entry in dict orderby entry.Value ascending select entry;

            var stringBuilder = new StringBuilder();
            var index = 2;
            var stringList = new List<String>();

            stringList.Add($":crown: {sortedDict.ElementAt(0).Key}: {sortedDict.ElementAt(0).Value}");

            foreach (KeyValuePair<string, int> player in sortedDict.Skip(1))
            {
                stringList.Add($"  {Toolbox.GetRegionalIndicator(index)}  {player.Key}: {player.Value}");
                index++;
            }

            var (page, pageCount, pagenumber) = Toolbox.CreatePageFromList(stringList, pageQuery, false, 250, false);
        
            await FollowupAsync(text: "", embed: new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = $"Who's that champion leaderboard".ToUpper()
                },
                Description = page,
                Color = Discord.Color.Red,
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Page {pagenumber}/{pageCount}"
                }
            }.Build());
        }
    }
}