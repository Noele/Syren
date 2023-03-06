using Discord.Commands;
using Discord.Interactions;

namespace Syren.Syren.SlashCommands;

public class General : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Checks if the bot is alive")]
    public async Task Ping()
    {
        await RespondAsync("Pong!");
    }
}