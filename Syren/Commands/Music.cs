using System.Text;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Victoria;
using Victoria.Enums;
using Victoria.Responses.Search;

namespace Syren.Syren.Commands;

public class Music : ModuleBase<SocketCommandContext> {
    private readonly LavaNode _lavaNode;
	
    public Music(LavaNode lavaNode)
        => _lavaNode = lavaNode;
		
    [Command("Join")]
    public async Task JoinAsync() {	
        if (_lavaNode.HasPlayer(Context.Guild)) {
            await ReplyAsync("I'm already connected to a voice channel!");
            return;
        }

        var voiceState = Context.User as IVoiceState;
        if (voiceState?.VoiceChannel == null) {
            await ReplyAsync("You must be connected to a voice channel!");
            return;
        }

        try {
            await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
            await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
        }
        catch (Exception exception) {
            await ReplyAsync(exception.Message);
        }
    }
    
    [Command("Play")]
    public async Task PlayAsync([Remainder] string searchQuery) {
        if (string.IsNullOrWhiteSpace(searchQuery)) {
            await ReplyAsync("Please provide search terms.");
            return;
        }

        if (!_lavaNode.HasPlayer(Context.Guild)) {
            await ReplyAsync("I'm not connected to a voice channel.");
            return;
        }

        var searchResponse = await _lavaNode.SearchAsync(SearchType.YouTube, searchQuery);
        if (searchResponse.Status is SearchStatus.LoadFailed or SearchStatus.NoMatches) {
            await ReplyAsync($"I wasn't able to find anything for `{searchQuery}`.");
            return;
        }

        var player = _lavaNode.GetPlayer(Context.Guild);
        if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name)) {
            player.Queue.Enqueue(searchResponse.Tracks);
            await ReplyAsync($"Enqueued {searchResponse.Tracks.Count} songs.");
        }
        else {
            var track = searchResponse.Tracks.FirstOrDefault();
            player.Queue.Enqueue(track);

            await ReplyAsync($"Enqueued {track?.Title}");
        }

        if (player.PlayerState is PlayerState.Playing or PlayerState.Paused) {
            return;
        }

        player.Queue.TryDequeue(out var lavaTrack);
        await player.PlayAsync(x => {
            x.Track = lavaTrack;
            x.ShouldPause = false;
        });
    }
    
    
    [Command("Skip")]
    public async Task SkipAsync() {
        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
            await ReplyAsync("I'm not connected to a voice channel.");
            return;
        }

        if (player.PlayerState != PlayerState.Playing) {
            await ReplyAsync("I can't skip when nothing is playing.");
            return;
        }

        var voiceChannelUsers = (player.VoiceChannel as SocketVoiceChannel)?.Users
            .Where(x => !x.IsBot)
            .ToArray();


        try {
            var (oldTrack, currenTrack) = await player.SkipAsync();
            await ReplyAsync($"Skipped: {oldTrack.Title}\nNow Playing: {player.Track.Title}");
        }
        catch (Exception exception) {
            await ReplyAsync(exception.Message);
        }
    }
    
    [Command("NowPlaying"), Alias("Np")]
    public async Task NowPlayingAsync() {
        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
            await ReplyAsync("I'm not connected to a voice channel.");
            return;
        }

        if (player.PlayerState != PlayerState.Playing) {
            await ReplyAsync("Woaaah there, I'm not playing any tracks.");
            return;
        }

        var track = player.Track;
        var artwork = await track.FetchArtworkAsync();

        var embed = new EmbedBuilder()
            .WithAuthor(track.Author, Context.Client.CurrentUser.GetAvatarUrl(), track.Url)
            .WithTitle($"Now Playing: {track.Title}")
            .WithImageUrl(artwork)
            .WithFooter($"{track.Position}/{track.Duration}");
        
        await ReplyAsync(embed: embed.Build());
    }
    
    [Command("Genius"), Alias("lyrics")]
    public async Task ShowGeniusLyrics() {
        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
            await ReplyAsync("I'm not connected to a voice channel.");
            return;
        }

        if (player.PlayerState != PlayerState.Playing) {
            await ReplyAsync("Woaaah there, I'm not playing any tracks.");
            return;
        }

        var lyrics = await player.Track.FetchLyricsFromGeniusAsync();
        if (string.IsNullOrWhiteSpace(lyrics)) {
            await ReplyAsync($"No lyrics found for {player.Track.Title}");
            return;
        }

        await SendLyricsAsync(lyrics);
    }
    private async Task SendLyricsAsync(string lyrics) {
        var splitLyrics = lyrics.Split(Environment.NewLine);
        var stringBuilder = new StringBuilder();
        foreach (var line in splitLyrics) {
            if (line.Contains('[')) {
                stringBuilder.Append(Environment.NewLine);
            }

            if (stringBuilder.Length > 1000) {
                await ReplyAsync($"```{stringBuilder}```");
                stringBuilder.Clear();
            }
            else {
                stringBuilder.AppendLine(line);
            }
        }

        await ReplyAsync($"```{stringBuilder}```");
    }
}