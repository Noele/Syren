using Discord;
using Discord.Commands;
using Discord.Interactions;
using Lavalink4NET;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using Syren.Syren.DataTypes;
using Syren.Syren.Events;
using RunMode = Discord.Interactions.RunMode;

namespace Syren.Syren.SlashCommands
{
    public class Music : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IAudioService _audioService;
        private readonly SyrenSpotifyClient _syrenSpotifyClient;

        public Music(IAudioService audioService, ApiKeys apiKeys)
        {
            _audioService = audioService;
            _syrenSpotifyClient = new SyrenSpotifyClient(apiKeys);
        }

        [SlashCommand("queue", "Sends the current queue")]
        public async Task Queue([Remainder] string pageQuery = "1")
        {
            await DeferAsync(true);
            var player = await GetPlayerAsync(connectToVoiceChannel: false);

            if (player == null)
            {
                await FollowupAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.Queue.IsEmpty)
            {
                await FollowupAsync("There are no tracks in the queue");
                return;
            }

            var trackNames = player.Queue.Select(track => track.Title).ToList();

            var (page, pageCount, pagenumber) = Toolbox.CreatePageFromList(trackNames, pageQuery, false, 700, true);

            await FollowupAsync(text: "", embed: new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = $"Now playing: {player.CurrentTrack.Title}"
                },
                Description = page,
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Page {pagenumber}/{pageCount}"
                }
            }.Build());
        }

        [SlashCommand("skip", description: "Skips the currently playing song", runMode: RunMode.Async)]
        public async Task Skip()
        {
            var player = await GetPlayerAsync(connectToVoiceChannel: false);
            if (player == null)
            {
                return;
            }

            if (player.CurrentTrack == null)
            {
                await RespondAsync("Nothing playing!");
                return;
            }

            await player.SkipAsync();
            await RespondAsync("Skipping");
        }

        [SlashCommand("stop", description: "Stops the current track", runMode: RunMode.Async)]
        public async Task Stop()
        {
            var player = await GetPlayerAsync(connectToVoiceChannel: false);

            if (player == null)
            {
                return;
            }

            if (player.CurrentTrack == null)
            {
                await RespondAsync("Nothing playing!");
                return;
            }

            await player.StopAsync();
            await RespondAsync("Stopped playing.");
        }


        [SlashCommand("play", description: "Plays music")]
        public async Task Play(string searchQuery)
        {
            await DeferAsync();
            var player = await GetPlayerAsync();

            if (player == null)
            {
                return;
            }

            var shuffle = false;

            if (searchQuery.EndsWith(" -s", StringComparison.OrdinalIgnoreCase))
            {
                shuffle = true;
                searchQuery = searchQuery[..^3];
            }

            switch (GetQueryType(searchQuery))
            {
                case SongQueryType.YOUTUBESONG:
                    await PlayYoutubeSong(searchQuery, player);
                    break;
                case SongQueryType.YOUTUBEPLAYLIST:
                    await PlayYoutubePlaylist(searchQuery, shuffle, player);
                    break;
                case SongQueryType.SPOTIFYSONG:
                    await PlaySpotifySong(searchQuery, player);
                    break;
                case SongQueryType.SPOTIFYPLAYLIST:
                    await PlaySpotifyPlaylist(searchQuery, shuffle, player);
                    break;
                default:
                    await PlayYoutubeSong(searchQuery, player);
                    break;
            }
        }

        private async Task PlayYoutubeSong(string searchQuery, VoteLavalinkPlayer player)
        {
            var lavaTrack = _audioService.GetTrackAsync(searchQuery, SearchMode.YouTube).Result;
            if (lavaTrack != null)
            {
                await player.PlayAsync(lavaTrack);
                await RespondAsync($"🔈 Added {lavaTrack.Title} to the queue");
            }
            else
            {
                await FollowupAsync($"I wasn't able to find anything for `{searchQuery}`.");
            }
        }

        private async Task PlayYoutubePlaylist(string searchQuery, bool shuffle, VoteLavalinkPlayer player)
        {
            var lavaTracks = _audioService.GetTracksAsync(searchQuery, SearchMode.YouTube).Result;
            if (shuffle) lavaTracks = Toolbox.Shuffle(lavaTracks.ToList());
            foreach (var track in lavaTracks)
            {
                await player.PlayAsync(track, enqueue: true);
            }

            await RespondAsync($"🔈 Added {lavaTracks.Count()} tracks to the queue: ");
        }

        private async Task PlaySpotifySong(string searchQuery, VoteLavalinkPlayer player)
        {
            var response = _syrenSpotifyClient.GetTrack(searchQuery);
            if (response == null)
            {
                await FollowupAsync($"Could not find the track {searchQuery}");
            }

            var lavaTrack = _audioService.GetTrackAsync(response, SearchMode.YouTube).Result;
            if (lavaTrack != null)
            {
                await player.PlayAsync(lavaTrack);
                await RespondAsync($"🔈 Added {lavaTrack.Title} to the queue");
            }
            else
            {
                await FollowupAsync($"I wasn't able to find anything for `{searchQuery}`.");
            }
        }

        private async Task PlaySpotifyPlaylist(string searchQuery, bool shuffle, VoteLavalinkPlayer player)
        {
            var response = _syrenSpotifyClient.GetPlaylist(searchQuery);
            if (response.Count == 0)
            {
                await FollowupAsync($"No tracks found for {searchQuery}");
                return;
            }

            if (shuffle)
            {
                response = Toolbox.Shuffle(response) as List<string>;
            }

            foreach (var track in response)
            {
                var lavaTrack = _audioService.GetTrackAsync(track, SearchMode.YouTube).Result;
                if (lavaTrack != null)
                    await player.PlayAsync(lavaTrack);
            }

            await FollowupAsync($"Enqueued {response.Count} songs.");
        }

        private static SongQueryType GetQueryType(string searchQuery)
        {
            if (searchQuery.Contains("open.spotify.com/track/")) return SongQueryType.SPOTIFYSONG;
            if (searchQuery.Contains("youtube") & searchQuery.Contains("playlist?")) return SongQueryType.YOUTUBEPLAYLIST;
            if (searchQuery.Contains("youtube")) return SongQueryType.YOUTUBESONG;
            if (searchQuery.Contains("open.spotify.com/playlist/")) return SongQueryType.SPOTIFYPLAYLIST;
            return default;
        }

        private async ValueTask<VoteLavalinkPlayer> GetPlayerAsync(bool connectToVoiceChannel = true)
        {
            var player = _audioService.GetPlayer<VoteLavalinkPlayer>(Context.Guild.Id);

            if (player != null
                && player.State != PlayerState.NotConnected
                && player.State != PlayerState.Destroyed)
            {
                return player;
            }

            var user = Context.Guild.GetUser(Context.User.Id);

            if (!user.VoiceState.HasValue)
            {
                await RespondAsync("You must be in a voice channel!");
                return null;
            }

            if (!connectToVoiceChannel)
            {
                await RespondAsync("The bot is not in a voice channel!");
                return null;
            }

            return await _audioService.JoinAsync<VoteLavalinkPlayer>(user.Guild.Id, user.VoiceChannel.Id);
        }
    }
}