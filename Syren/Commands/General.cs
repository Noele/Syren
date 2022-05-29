using System.Threading.Tasks;
using Discord.Commands;

namespace Syren.Syren.Commands;

public class General : ModuleBase<SocketCommandContext>
{
    [Command("ping")]
    public async Task Ping()
    {
        await ReplyAsync("Pong!");
    }
}