using System.Threading.Tasks;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json;
using Syren.Syren.DataTypes;

namespace Syren.Syren.Events;

public class EventHandler
{
    private Spawn _spawn;
    private DiscordSocketClient _client;

    public EventHandler(Spawn spawn, DiscordSocketClient client)
    {
        _spawn = spawn;
        _client = client;
    }

    public async Task OnMessage(SocketMessage message)
    {
        if (message.Author.IsBot) return;
        var Context = new SocketCommandContext(_client, message as SocketUserMessage);
        
        var channel = Context.Guild.GetChannel(Convert.ToUInt64(_spawn.channelId)) as ISocketMessageChannel;
        if (string.IsNullOrEmpty(_spawn.pokemonName))
        {
            var randomNumber = new Random().Next(0, 10);
            if (randomNumber == 5)
            {
                var text = await File.ReadAllTextAsync("Data/Pokemon/pokemon.json");
                var pokemonJson = JsonConvert.DeserializeObject<PokemonJson>(text);
                if(pokemonJson == null) { throw new Exception("Failed to load pokemon.json!!!"); }
                
                var randomPokemon = pokemonJson.Pokemon[new Random().Next(pokemonJson.Pokemon.Length)];
                _spawn.pokemonName = randomPokemon;
                using(var fs = File.OpenRead($"Data/Pokemon/Images/{randomPokemon}.png"))
                {
                    await channel.SendFileAsync(filename:"pokemon.png", stream:fs, text: "Who's That Pokémon?");
                }
                

            }
        }
        else
        {
            if (message.Content.ToLower() == _spawn.pokemonName.ToLower())
            {
                await Context.Channel.SendMessageAsync($"Correct! {_spawn.pokemonName} was added to your Pokédex.");
                var text = await File.ReadAllTextAsync("Data/Pokemon/trainers.json");
                var pokemonJson = JsonConvert.DeserializeObject<TrainerJson.TrainerJsonRoot>(text);
                var added = false;
                var i = 0;
                foreach (var trainer in pokemonJson.Trainers)
                {
                    if (trainer.UserId == message.Author.Id.ToString())
                    {
                        pokemonJson.Trainers[i].Info.CapturedPokemon.Add(_spawn.pokemonName);
                        added = true;
                    }

                    i++;
                }
                
                if (!added)
                {
                    var newInfo = new TrainerJson.Info()
                    {
                        Name = message.Author.Username,
                        CapturedPokemon = new List<string> {_spawn.pokemonName}
                    };
                    var newTrainer = new TrainerJson.Trainer()
                    {
                        Info = newInfo,
                        UserId = message.Author.Id.ToString()
                    };
                    pokemonJson.Trainers.Add(newTrainer);
                }
           
                var stringOfNewData = JsonConvert.SerializeObject(pokemonJson);
                await File.WriteAllTextAsync("Data/Pokemon/trainers.json", stringOfNewData);
                _spawn.pokemonName = "";
            }
        }
    }
}