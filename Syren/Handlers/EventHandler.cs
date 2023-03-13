using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Syren.Syren.DataTypes;

namespace Syren.Syren.Events;

public class EventHandler
{
    private PokemonSpawn _spawn;
    private DiscordSocketClient _client;
    private Ai _ai;
    private Pokemon _pokemon;
    private LeagueOfLegends _leagueOfLegends;
    private LeagueOfLegendsSpawn _leagueOfLegendsSpawn;

    public EventHandler(PokemonSpawn spawn, DiscordSocketClient client, LeagueOfLegendsSpawn leagueOfLegendsSpawn, ApiKeys apiKeys)
    {
        _spawn = spawn;
        _client = client;
        _leagueOfLegendsSpawn = leagueOfLegendsSpawn;

        _ai = new Ai(client, apiKeys);
        _pokemon = new Pokemon(spawn, client);
        _leagueOfLegends = new LeagueOfLegends(leagueOfLegendsSpawn, client);

    }

    public async Task OnMessage(SocketMessage message)
    {
        await _ai.run(message);
        await _pokemon.run(message);
        await _leagueOfLegends.run(message);
    }
}