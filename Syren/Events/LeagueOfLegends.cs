using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using Syren.Syren.DataTypes;
using System;
using System.Text.RegularExpressions;
using Image = SixLabors.ImageSharp.Image;

namespace Syren.Syren.Events
{
    public class LeagueOfLegends : Event
    {
        private readonly Rectangle Source = new Rectangle(0, 0, 1215, 717);
        private readonly Rectangle ViewPort = new Rectangle(0, 0, 250, 250);

        private string _champion = "";
        private LeagueOfLegendsSpawn _leagueOfLegendsSpawn;
        private DiscordSocketClient _client;

        private int guesses = 0;
        private List<ulong> uniquePlayers = new List<ulong>();

        public LeagueOfLegends(LeagueOfLegendsSpawn leagueOfLegendsSpawn, DiscordSocketClient client)
        {
            this._leagueOfLegendsSpawn = leagueOfLegendsSpawn;
            this._client = client;
        }
        public override async Task run(SocketMessage message)
        {
            if (message.Author.IsBot) return;
            var Context = new SocketCommandContext(_client, message as SocketUserMessage);
            var channel = Context.Guild.GetChannel(Convert.ToUInt64(_leagueOfLegendsSpawn.ChannelId)) as ISocketMessageChannel;
            if (message.Channel.Id.ToString() != _leagueOfLegendsSpawn.ChannelId) return;

            var text = File.ReadAllText("Data/LeagueOfLegends/players.json");
            var playerData = JsonConvert.DeserializeObject<LeagueOfLegendsPlayerData.Players>(text);

            if(!uniquePlayers.Contains(message.Author.Id))
            {
                uniquePlayers.Add(message.Author.Id);
            }

            if (!DoesPlayerExist(message, playerData))
            {
                await CreateNewPlayer(message, playerData);
            }
            var guess = Regex.Replace(message.Content, @"\s+", "");
            guess = Regex.Replace(message.Content, "'", "");
            if (string.Equals(guess, _champion, StringComparison.CurrentCultureIgnoreCase)) {
                var pointsAdded = AddPointsToPlayer(message, playerData);
                await channel.SendMessageAsync($"<@{message.Author.Id}> Correct ! {pointsAdded} points have been added.");
                await channel.SendFileAsync(filename: "champion.jpg", stream: File.OpenRead($"Data/LeagueOfLegends/ChampionSplash/{_champion}-0.jpg"));
                _champion = "";
                guesses = 0;
                uniquePlayers.Clear();
            } else
            {
                if (_champion == "")
                {
                    await SpawnNewChampion(message.Channel);
                } else
                {
                    guesses += 1;
                }
            }
        }

        private LeagueChampionNames GetLeagueChampions() => JsonConvert.DeserializeObject<LeagueChampionNames>(File.ReadAllText("Data/LeagueOfLegends/Champions.json"));

        private async Task SpawnNewChampion(IMessageChannel channel)
        {
            var champions = GetLeagueChampions();
            var champion = champions.Names[new Random().Next(champions.Names.Length)];
            var championImage = Image.Load(File.OpenRead($"Data/LeagueOfLegends/ChampionSplash/{champion}-0.jpg"));
            var random = new Random();

            var x = (int)Math.Floor((decimal)(random.Next(0, Source.Width - ViewPort.Width)));
            var y = (int)Math.Floor((decimal)(random.Next(0, Source.Height - ViewPort.Height)));

            championImage.Mutate(i => i.Crop(new Rectangle(x < 0 ? 0 : x, y < 0 ? 0 : y, ViewPort.Height, ViewPort.Width)));
            this._champion = champion;

            championImage.SaveAsJpeg("image.jpg");
            await channel.SendFileAsync(filename: "champion.jpg", stream: File.OpenRead("image.jpg"), text: "Who's That Champion?");
        }

        private async Task CreateNewPlayer(SocketMessage message, LeagueOfLegendsPlayerData.Players playerData)
        {
            var newPlayer = new LeagueOfLegendsPlayerData.Player
            {
                name = message.Author.Username,
                id = message.Author.Id.ToString(),
                points = 0
            };
            playerData.players.Add(newPlayer);

            var stringOfNewData = JsonConvert.SerializeObject(playerData);
            await File.WriteAllTextAsync("Data/LeagueOfLegends/players.json", stringOfNewData);
        }

        private bool DoesPlayerExist(SocketMessage message, LeagueOfLegendsPlayerData.Players playerData)
        {

            foreach(var player in playerData.players)
            {
                if(ulong.Parse(player.id) == message.Author.Id)
                {
                    return true;
                }
            }
            return false;
        }

        private int AddPointsToPlayer(SocketMessage message, LeagueOfLegendsPlayerData.Players playerData)
        {
            var pointsAdded = 0;
            foreach(var player in playerData.players)
            {
                if(ulong.Parse(player.id) == message.Author.Id)
                {
                    if(guesses == 0)
                    {
                        player.points += pointsAdded = 5;
                    } 
                    else
                    {
                        if(uniquePlayers.Count == 1)
                        {
                            if (guesses > 10)
                            {
                                player.points += pointsAdded = 10;
                            } else
                            {
                                player.points += pointsAdded = guesses * uniquePlayers.Count;
                            }
                        } 
                        else
                        {
                            player.points += pointsAdded = guesses * uniquePlayers.Count;
                        }
                    }
                    player.name = message.Author.Username;
                }
            }

            var stringOfNewData = JsonConvert.SerializeObject(playerData);
            File.WriteAllText("Data/LeagueOfLegends/players.json", stringOfNewData);

            return pointsAdded;
        }
    }
}
