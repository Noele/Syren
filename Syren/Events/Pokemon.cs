﻿using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Syren.Syren.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syren.Syren.Events
{
    public class Pokemon : Event
    {
        private PokemonSpawn _spawn;
        private DiscordSocketClient _client;
        public override bool GuildOnly => true;
        public Pokemon(PokemonSpawn spawn, DiscordSocketClient client)
        {
            _spawn = spawn;
            _client = client;
        }

        public override async Task run(SocketMessage message, SocketCommandContext context)
        {
            var channel = context.Guild.GetChannel(Convert.ToUInt64(this._spawn.ChannelId)) as ISocketMessageChannel;
            if (string.IsNullOrEmpty(_spawn.PokemonName))
            {
                var randomNumber = new Random().Next(0, 10);
                if (randomNumber == 5)
                {
                    var text = await File.ReadAllTextAsync("Data/Pokemon/pokemon.json");
                    var pokemonJson = JsonConvert.DeserializeObject<PokemonJson>(text);
                    if (pokemonJson == null) { throw new Exception("Failed to load pokemon.json!!!"); }

                    var randomPokemon = pokemonJson.Pokemon[new Random().Next(pokemonJson.Pokemon.Length)];
                    while (randomPokemon == "Ditto")
                    {
                        randomPokemon = pokemonJson.Pokemon[new Random().Next(pokemonJson.Pokemon.Length)]; ;
                    }
                    _spawn.PokemonName = randomPokemon;

                    await using var fs = File.OpenRead($"Data/Pokemon/Images/{randomPokemon}.png");
                    await channel.SendFileAsync(filename: "pokemon.png", stream: fs, text: "Who's That Pokémon?");
                }
            }
            else
            {
                if (string.Equals(message.Content, _spawn.PokemonName, StringComparison.CurrentCultureIgnoreCase))
                {
                    await context.Channel.SendMessageAsync($"Correct! {_spawn.PokemonName} was added to your Pokédex.");
                    var text = await File.ReadAllTextAsync("Data/Pokemon/trainers.json");
                    var pokemonJson = JsonConvert.DeserializeObject<TrainerJson.TrainerJsonRoot>(text);
                    var added = false;
                    var i = 0;
                    foreach (var trainer in pokemonJson.Trainers)
                    {
                        if (trainer.UserId == message.Author.Id.ToString())
                        {
                            if (pokemonJson.Trainers[i].Info.CapturedPokemon.Contains(_spawn.PokemonName))
                            {
                                added = true;
                                var randomNumber = new Random().Next(0, 50);
                                if (randomNumber == 40)
                                {
                                    await context.Channel.SendMessageAsync($"Oh ? ... {_spawn.PokemonName} was actually a Ditto ! Ditto has been added to your pokédex !");
                                    pokemonJson.Trainers[i].Info.CapturedPokemon.Add("Ditto");
                                }
                            }
                            else
                            {
                                added = true;
                                pokemonJson.Trainers[i].Info.CapturedPokemon.Add(_spawn.PokemonName);
                            }

                            if (new Random().Next(0, 8192) == 6969)
                            {
                                pokemonJson.Trainers[i].Info.CapturedPokemon.Add($"Shiny-{_spawn.PokemonName}");
                                await context.Channel.SendMessageAsync($"Oh ? ... {_spawn.PokemonName} was actually a Shiny ! Shiny-{_spawn.PokemonName} has been added to your pokédex !");
                            }

                        }
                        i++;
                    }

                    if (!added)
                    {
                        var newInfo = new TrainerJson.Info()
                        {
                            Name = message.Author.Username,
                            CapturedPokemon = new List<string> { _spawn.PokemonName }
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
                    _spawn.PokemonName = "";
                }
            }
        }
    }
}
