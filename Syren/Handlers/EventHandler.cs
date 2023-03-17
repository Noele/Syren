using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Syren.Syren.DataTypes;

namespace Syren.Syren.Events;

public class EventHandler
{
    private PokemonSpawn _spawn;
    private DiscordSocketClient _client;
    private LeagueOfLegendsSpawn _leagueOfLegendsSpawn;

    private List<Event> _events;

    public EventHandler(PokemonSpawn spawn, DiscordSocketClient client, LeagueOfLegendsSpawn leagueOfLegendsSpawn, ApiKeys apiKeys)
    {
        _spawn = spawn;
        _client = client;
        _leagueOfLegendsSpawn = leagueOfLegendsSpawn;
        _events = new List<Event>() { new Ai(client, apiKeys), new Pokemon(spawn, client), new LeagueOfLegends(leagueOfLegendsSpawn, client) };
    }

    public async Task OnMessage(SocketMessage message)
    {
        if (message.Author.IsBot) return;
        var context = new SocketCommandContext(_client, message as SocketUserMessage);
        var isChannelDM = message.Channel.GetChannelType() == ChannelType.DM;

        foreach (var even in _events)
        {
            if (even.GuildOnly && isChannelDM) continue;
            even.run(message, context);
        }
    }
}