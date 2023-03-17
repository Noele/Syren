using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syren.Syren.Events
{
    public abstract class Event
    {
        public abstract Task run(SocketMessage message, SocketCommandContext context);
        public abstract bool GuildOnly { get; }
    }
}
