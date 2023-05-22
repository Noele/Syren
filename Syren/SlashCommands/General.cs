using Discord.Commands;
using Discord.Interactions;

namespace Syren.Syren.SlashCommands
{
    public class General : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("ping", "Checks if the bot is alive")]
        public async Task Ping()
        {
            Console.WriteLine(1);
            try
            {
                await RespondAsync("Pong!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.WriteLine(2);
        }
    }
}