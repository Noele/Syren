using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;

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
        _lavaNode.OnTrackEnded += OnTrackEnded;
        _lavaNode.OnTrackException += OnTrackException;
        _lavaNode.OnTrackStuck += OnTrackStuck;
        _lavaNode.OnWebSocketClosed += OnWebSocketClosed;
    }
    
    private async Task OnTrackEnded(TrackEndedEventArgs args) {
        if (args.Reason != TrackEndReason.Finished) {
            return;
        }

        var player = args.Player;
        if (!player.Queue.TryDequeue(out var lavaTrack)) {
            return;
        }

        if (lavaTrack is null) {
            await player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
            return;
        }

        await args.Player.PlayAsync(lavaTrack);
        await args.Player.TextChannel.SendMessageAsync(
            $"{args.Reason}: {args.Track.Title}\nNow playing: {lavaTrack.Title}");
    }
    
    private async Task OnTrackException(TrackExceptionEventArgs arg) {
        Console.WriteLine($"Track {arg.Track.Title} threw an exception. Please check Lavalink console/logs.");
        arg.Player.Queue.Enqueue(arg.Track);
        await arg.Player.TextChannel.SendMessageAsync(
            $"{arg.Track.Title} has been re-added to queue after throwing an exception.");
    }
    private async Task OnTrackStuck(TrackStuckEventArgs arg) {
        Console.WriteLine(
            $"Track {arg.Track.Title} got stuck for {arg.Threshold}ms. Please check Lavalink console/logs.");
        arg.Player.Queue.Enqueue(arg.Track);
        await arg.Player.TextChannel.SendMessageAsync(
            $"{arg.Track.Title} has been re-added to queue after getting stuck.");
    }

    private Task OnWebSocketClosed(WebSocketClosedEventArgs arg) {
        Console.WriteLine($"Discord WebSocket connection closed with following reason: {arg.Reason}");
        return Task.CompletedTask;
    }
}