using Microsoft.Extensions.Logging;
using Victoria;
using Victoria.Node;
using Victoria.Node.EventArgs;
using Victoria.Player;

namespace Syren.Syren.Events;

public class AudioHandler
{
    private readonly LavaNode _lavaNode;
    public AudioHandler(LavaNode lavaNode)
    {
        _lavaNode = lavaNode;
    }

    public void RegisterEvents()
    {
        
        _lavaNode.OnTrackEnd += OnTrackEnded;
        _lavaNode.OnTrackException += OnTrackException;
        _lavaNode.OnTrackStuck += OnTrackStuck;
        _lavaNode.OnWebSocketClosed += OnWebSocketClosed;
    }

    private async Task OnTrackEnded(TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg) {
        if (arg.Reason != TrackEndReason.Finished) {
            return;
        }

        var player = arg.Player;
        if (!player.Vueue.TryDequeue(out var lavaTrack)) {
            return;
        }

        if (lavaTrack is null) {
            await player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
            return;
        }
  
        await player.PlayAsync(lavaTrack);
    }
    
    private async Task OnTrackException(TrackExceptionEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        Console.WriteLine($"Track {arg.Track.Title} threw an exception. Please check Lavalink console/logs.");
        await arg.Player.SkipAsync();
        await arg.Player.TextChannel.SendMessageAsync($"{arg.Track.Title} has been skipped after throwing an exception.");
    }
    private async Task OnTrackStuck(TrackStuckEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        Console.WriteLine($"Track {arg.Track.Title} got stuck for {arg.Threshold}ms. Please check Lavalink console/logs.");
        await arg.Player.SkipAsync();
        await arg.Player.TextChannel.SendMessageAsync($"{arg.Track.Title} has been skipped after getting stuck.");
    }

    private Task OnWebSocketClosed(WebSocketClosedEventArg arg)
    {
        Console.WriteLine($"Discord WebSocket connection closed with following reason: {arg.Reason}");
        return Task.CompletedTask;
    }
}