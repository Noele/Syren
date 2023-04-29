using System.Runtime.InteropServices;
using System.Text;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using SpotifyAPI.Web.Http;
using Syren.Syren.Events;
using Victoria;
using Victoria.Node;
using Victoria.Player;
using Victoria.Player.Args;
using Victoria.Responses.Search;
using static Syren.Syren.DataTypes.Spotify;
using RunMode = Discord.Interactions.RunMode;

namespace Syren.Syren.Commands;

public class Music : InteractionModuleBase<SocketInteractionContext> { 
    private readonly LavaNode _lavaNode;
    private readonly SyrenSpotifyClient _syrenSpotifyClient;

    public Music(LavaNode lavaNode, SyrenSpotifyClient syrenSpotifyClient)
    {
        _lavaNode = lavaNode;
        _syrenSpotifyClient = syrenSpotifyClient;
    }

    [SlashCommand("join", "Makes the bot join the channel you are currently in")]
    public async Task JoinAsync() {
        await DeferAsync();
        var voiceState = Context.User as IVoiceState;
        if (_lavaNode.HasPlayer(Context.Guild)) {
            if(_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                if (player.Vueue.Count == 0)
                {
                    var voiceChannel = (Context.User as IVoiceState)?.VoiceChannel ?? player.VoiceChannel;
                    if (voiceState?.VoiceChannel != null)
                    {
                        await _lavaNode.LeaveAsync(voiceChannel);
                        await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                        await FollowupAsync("Refreshed player instance.");
                        return;
                    }
                }
            }
            await FollowupAsync("I'm already connected to a voice channel!");
            return;
        }

        if (voiceState?.VoiceChannel == null) {
            await FollowupAsync("You must be connected to a voice channel!");
            return;
        }

        try {
            await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
            await FollowupAsync($"Joined {voiceState.VoiceChannel.Name}!");
        }
        catch (Exception exception) {
            await FollowupAsync(exception.Message);
        }
    }

    [SlashCommand("shuffle", "Shuffles the playlist")]
    public async Task ShuffleAsync()
    {
        await DeferAsync();
        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
            await FollowupAsync("I'm not connected to a voice channel.");
            return;
        }
        if (player.Vueue.Count == 0) {
            await FollowupAsync("There are no tracks in the queue");
            return;
        }
        
        player.Vueue.Shuffle();
        await FollowupAsync("Shuffled.");
    }

    [SlashCommand("leave", "Makes the bot leave the channel its currently in")]
    public async Task LeaveAsync()
    {
        await DeferAsync();
        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
            await FollowupAsync("I'm not connected to any voice channels!");
            return;
        }

        var voiceChannel = (Context.User as IVoiceState)?.VoiceChannel ?? player.VoiceChannel;
        if (voiceChannel == null) {
            await FollowupAsync("Not sure which voice channel to disconnect from.");
            return;
        }

        try {
            await _lavaNode.LeaveAsync(voiceChannel);
            await FollowupAsync($"I've left {voiceChannel.Name}!");
        }
        catch (Exception exception) {
            await FollowupAsync(exception.Message);
        }
    }
    
    [SlashCommand("playlist", "Sends the current playlist"), Alias("list", "q", "queue")]
    public async Task PlaylistAsync([Remainder] string pageQuery = "1")
    {
        await DeferAsync(true);
        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
            await FollowupAsync("I'm not connected to a voice channel.");
            return;
        }
        if (player.Vueue.Count == 0) {
            await FollowupAsync("There are no tracks in the queue");
            return;
        }

        var trackNames = player.Vueue.Select(track => track.Title).ToList();

        var (page, pageCount, pagenumber) = Toolbox.CreatePageFromList(trackNames, pageQuery, false, 700, true);
                
        await FollowupAsync(text: "", embed: new EmbedBuilder()
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

    [SlashCommand("play", "Plays a song", runMode: RunMode.Async)]
    public async Task PlayAsync([Remainder, Optional] string searchQuery)
    {
        await DeferAsync();
        if (string.IsNullOrWhiteSpace(searchQuery)) {
            await FollowupAsync("Please provide search terms.");
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
                    await FollowupAsync("You must be connected to a voice channel!");
                    return;
                }
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
            }
            catch (Exception exception) {
                await FollowupAsync(exception.Message);
            }
        }

        if (searchQuery.Contains("open.spotify.com/playlist/"))
        {
            var response = _syrenSpotifyClient.GetPlaylist(searchQuery);
            if (response.Count == 0)
            {
                await FollowupAsync($"No tracks found for {searchQuery}");
            }
            
            if (shuffle)
            {
                response = Toolbox.Shuffle(response) as List<string>;
            }
            
            foreach (var track in response)
            {
                await PlayTrackAsync(track, false, false);
            } 
            await FollowupAsync($"Enqueued {response.Count} songs.");
        }

        else if (searchQuery.Contains("open.spotify.com/track/"))
        {
            var response = _syrenSpotifyClient.GetTrack(searchQuery);
            if(response == null)
            {
                await FollowupAsync($"Could not find the track {searchQuery}");
            }
            await PlayTrackAsync(response, true, false);
        }
        else
        {
            await PlayTrackAsync(searchQuery, true, shuffle);
        }
    }

    private async Task PlayTrackAsync(string searchQuery, bool shouldOutput, bool shuffle)
    {
        var searchResponse = await _lavaNode.SearchAsync(searchQuery.StartsWith("https") ? SearchType.Direct : SearchType.YouTube, searchQuery);
        
        if (searchResponse.Status is SearchStatus.LoadFailed or SearchStatus.NoMatches) {
            await FollowupAsync($"I wasn't able to find anything for `{searchQuery}`.");
            return;
        }
        LavaPlayer<LavaTrack>? player = null;
        var getPlayer = _lavaNode.TryGetPlayer(Context.Guild, out player);
        if(!getPlayer)
        {
            await FollowupAsync("No LavaPlayer found.");
            return;
        }
        if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name)) {
            if (shuffle)
            {
                var tracks = searchResponse.Tracks.ToList();
                tracks = Toolbox.Shuffle(tracks);
                player.Vueue.Enqueue(tracks);
            }
            else
            {
                player.Vueue.Enqueue(searchResponse.Tracks);


            }
            if(shouldOutput)
                await FollowupAsync($"Enqueued {searchResponse.Tracks.Count} songs.");
        }
        else {
            var track = searchResponse.Tracks.FirstOrDefault();
            player.Vueue.Enqueue(track);
            if(shouldOutput)
                await FollowupAsync($"Enqueued {track?.Title}");
        }

        if (player.PlayerState is PlayerState.Playing or PlayerState.Paused) {
            return;
        }

        player.Vueue.TryDequeue(out var lavaTrack);
        await player.PlayAsync(lavaTrack);
    }
    
    [SlashCommand("remove", "Removes a song at a queue position, Use /playlist for the queue id"), Alias("dq", "dequeue")]
    public async Task RemoveAsync([Remainder] string removeQuery)
    {
        await DeferAsync();
        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
            await FollowupAsync("I'm not connected to a voice channel.");
            return;
        }

        if (player.Vueue.Count == 0) {
            await FollowupAsync("Nothing in the queue to remove.");
            return;
        }
        
        var isNumeric = int.TryParse(removeQuery, out var index);
        if (!isNumeric)
        {
            await FollowupAsync($"{removeQuery} is not a number.");
            return;
        }
        
        if (index - 1 < 0 || index - 1 > player.Vueue.Count)
        {
            await FollowupAsync($"Can't remove the track at index {removeQuery}");
            return;
        }
        
        var track = player.Vueue.RemoveAt(index - 1);
        await FollowupAsync($"Removed {track.Title}");
    }

    [SlashCommand("stop", "Stop the track that is currently playing")]
    public async Task StopAsync()
    {
        await DeferAsync();
        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
            await FollowupAsync("I'm not connected to a voice channel.");
            return;
        }
        
        await FollowupAsync("Stopping.");
        await player.StopAsync();
        player.Vueue.Clear();
    }

    [SlashCommand("skip", "Skips the currently playing song")]
    public async Task SkipAsync()
    {
        await DeferAsync();
        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
            await FollowupAsync("I'm not connected to a voice channel.");
            return;
        }

        if (player.PlayerState != PlayerState.Playing) {
            await FollowupAsync("I can't skip when nothing is playing.");
            return;
        }

        try {
            var (oldTrack, _) = await player.SkipAsync();
            await FollowupAsync($"Skipped: {oldTrack.Title}\nNow Playing: {player.Track.Title}");
        }
        catch (Exception exception) {
            await FollowupAsync(exception.Message);
        }
    }
    
    [SlashCommand("nowplaying", "Displayes the currently running song"), Alias("np")]
    public async Task NowPlayingAsync()
    {
        await DeferAsync();
        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
            await FollowupAsync("I'm not connected to a voice channel.");
            return;
        }

        if (player.PlayerState != PlayerState.Playing) {
            await FollowupAsync("I'm not playing any tracks.");
            return;
        }

        var track = player.Track;
        var artwork = await track.FetchArtworkAsync();

        var embed = new EmbedBuilder()
            .WithAuthor(track.Author, Context.Client.CurrentUser.GetAvatarUrl(), track.Url)
            .WithTitle($"Now Playing: {track.Title}")
            .WithImageUrl(artwork)
            .WithFooter($"{track.Position}/{track.Duration}");
        
        await FollowupAsync(embed: embed.Build());
    }
    
    [SlashCommand("genius", "Displays the lyrics for the currently playing song"), Alias("lyrics")]
    public async Task ShowGeniusLyrics()
    {
        await DeferAsync();
        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
            await FollowupAsync("I'm not connected to a voice channel.");
            return;
        }

        if (player.PlayerState != PlayerState.Playing) {
            await FollowupAsync("I'm not playing any tracks.");
            return;
        }

        var lyrics = await player.Track.FetchLyricsFromGeniusAsync();
        if (string.IsNullOrWhiteSpace(lyrics)) {
            await FollowupAsync($"No lyrics found for {player.Track.Title}");
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
                await FollowupAsync($"```{stringBuilder}```");
                stringBuilder.Clear();
            }
            else {
                stringBuilder.AppendLine(line);
            }
        }

        await FollowupAsync($"```{stringBuilder}```");
    }
}