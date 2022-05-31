using System.Runtime.InteropServices;
using System.Text;
using Discord;
using Discord.Commands;
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

    [Command("Shuffle")]
    public async Task ShuffleAsync()
    {
        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
            await ReplyAsync("I'm not connected to a voice channel.");
            return;
        }
        if (player.Queue.Count == 0) {
            await ReplyAsync("There are no tracks in the queue");
            return;
        }
        
        player.Queue.Shuffle();
        await ReplyAsync("Shuffled.");
    }

    [Command("Playlist"), Alias("list", "q", "queue")]
    public async Task PlaylistAsync([Remainder, Optional] string pageQuery)
    {
        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
            await ReplyAsync("I'm not connected to a voice channel.");
            return;
        }
        if (player.Queue.Count == 0) {
            await ReplyAsync("There are no tracks in the queue");
            return;
        }

        var trackNames = player.Queue.Select(track => track.Title).ToList();

        var (page, pageCount, pagenumber) = Toolbox.CreatePageFromList(trackNames, pageQuery, false, 1000, true);
                
        await Context.Channel.SendMessageAsync(embed: new EmbedBuilder()
        {
            Author = new EmbedAuthorBuilder()
            {
              Name = $"Now playing: {player.Track.Title}"
            },
            Description = page,
            Footer = new EmbedFooterBuilder()
            {
                Text = $"Page {pagenumber}/{pageCount}"
            }
        }.Build());
    }

    [Command("Play")]
    public async Task PlayAsync([Remainder, Optional] string searchQuery) {
        if (string.IsNullOrWhiteSpace(searchQuery)) {
            await ReplyAsync("Please provide search terms.");
            return;
        }

        var shuffle = false;
        
        if (searchQuery.EndsWith(" -s", StringComparison.OrdinalIgnoreCase))
        {
            shuffle = true;
            searchQuery = searchQuery[..^3];
        }

        if (!_lavaNode.HasPlayer(Context.Guild)) {
            try {
                var voiceState = Context.User as IVoiceState;
                if (voiceState?.VoiceChannel == null) {
                    await ReplyAsync("You must be connected to a voice channel!");
                    return;
                }
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
            }
            catch (Exception exception) {
                await ReplyAsync(exception.Message);
            }
        }
        
        var searchResponse = await _lavaNode.SearchAsync(searchQuery.StartsWith("https") ? SearchType.Direct : SearchType.YouTube, searchQuery);
        
        if (searchResponse.Status is SearchStatus.LoadFailed or SearchStatus.NoMatches) {
            await ReplyAsync($"I wasn't able to find anything for `{searchQuery}`.");
            return;
        }

        var player = _lavaNode.GetPlayer(Context.Guild);
        if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name)) {
            if (shuffle)
            {
                var tracks = searchResponse.Tracks.ToList();
                tracks = Toolbox.Shuffle(tracks);
                player.Queue.Enqueue(tracks);
            }
            else
            {
                player.Queue.Enqueue(searchResponse.Tracks);

            }
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
    
    [Command("Remove"), Alias("dq", "dequeue")]
    public async Task RemoveAsync([Remainder] string removeQuery)
    {
        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
            await ReplyAsync("I'm not connected to a voice channel.");
            return;
        }

        if (player.Queue.Count == 0) {
            await ReplyAsync("Nothing in the queue to remove.");
            return;
        }
        
        var isNumeric = int.TryParse(removeQuery, out var index);
        if (!isNumeric)
        {
            await ReplyAsync($"{removeQuery} is not a number.");
            return;
        }
        
        if (index - 1 < 0 || index - 1 > player.Queue.Count)
        {
            await ReplyAsync($"Can't remove the track at index {removeQuery}");
            return;
        }
        
        var track = player.Queue.RemoveAt(index - 1);
        await ReplyAsync($"Removed {track.Title}");
    }

    [Command("Stop")]
    public async Task StopAsync()
    {
        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
            await ReplyAsync("I'm not connected to a voice channel.");
            return;
        }
        
        await ReplyAsync("Stopping.");
        await player.StopAsync();
        player.Queue.Clear();
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

        try {
            var (oldTrack, _) = await player.SkipAsync();
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
            await ReplyAsync("I'm not playing any tracks.");
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
            await ReplyAsync("I'm not playing any tracks.");
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