using Discord.WebSocket;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using Syren.Syren.DataTypes;

namespace Syren.Syren.Events;

public class StatusReport
{
    public bool Ok { get; set; }
    public string Instance { get; set; }
}

public class SyrenSpotifyClient
{
    private HttpClient _client;
    private SpotifyClient _spotify;
    private ClientCredentialsRequest _credentials;
    public SyrenSpotifyClient(ApiKeys apiKeys)
    {
        this._client = new HttpClient();
        this._credentials = new ClientCredentialsRequest(apiKeys.spotifyClientId, apiKeys.spotifyClientSecret);
        this._spotify = new SpotifyClient(new OAuthClient().RequestToken(this._credentials).Result);

    }
    public string? GetTrack(string Query)
    {
        var parsedUrl = ParseUrl(Query);
        if (!parsedUrl.Result.Ok)
        {
            return null;
        }
        _spotify = new SpotifyClient(new OAuthClient().RequestToken(_credentials).Result);

        var song = _spotify.Tracks.Get(parsedUrl.Result.Instance).Result;

        if (song != null)
        {
            var artist = song.Artists.Count == 0 ? "" : song.Artists[0].Name;
            return $"{artist} {song.Name}";
        }
        return null;
    }

    public List<string> GetPlaylist(string Query)
    {
        var parsedUrl = ParseUrl(Query);
        if (!parsedUrl.Result.Ok)
        {
            return new List<string>();
        }
        _spotify = new SpotifyClient(new OAuthClient().RequestToken(_credentials).Result);
        
        var list = _spotify.Playlists.GetItems(parsedUrl.Result.Instance, new PlaylistGetItemsRequest { Offset = 0 });
        var result = list.Result;
        if (result.Items == null) return new List<string>();
        var tracklist = new List<PlaylistTrack<IPlayableItem>>();
        while (!(result.Items.Count == 0))
        {

           foreach(var track in result.Items)
            {
                tracklist.Add(track);
            }
            result.Offset += 100;
            list = _spotify.Playlists.GetItems(parsedUrl.Result.Instance, new PlaylistGetItemsRequest { Offset = result.Offset });
            result = list.Result;
        }
        var queueableList = new List<string>();
        foreach (var item in tracklist)
        {
            if (item.Track is FullTrack track)
            {
                var artist = track.Artists.Count == 0 ? "" : track.Artists[0].Name;
                queueableList.Add($"{artist} {track.Name}");
            }
        }
        return queueableList;
    }

    public Task<StatusReport> ParseUrl(string url)
    { //Example https://open.spotify.com/playlist/0mvHxwGeI3EBFvK7aJZqfe?si=2e34339948c94542
        if (url.StartsWith("https://open.spotify.com"))
        {
            url = url.Replace("https://open.spotify.com/playlist/", "");
            url = url.Replace("https://open.spotify.com/track/", "");
            if (url.Contains("?si="))
            {
                var index = url.IndexOf("?si", StringComparison.Ordinal);
                url = url[..index];
            }

            return Task.FromResult(new StatusReport {Ok = true, Instance = url});
        }

        return Task.FromResult(new StatusReport() { Ok = false });
    } 
}