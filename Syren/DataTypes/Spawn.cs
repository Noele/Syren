namespace Syren.Syren.DataTypes;

public class Spawn
{
    public string pokemonName = "";
    public readonly string channelId;
    public Spawn(string channel_id)
    {
        channelId = channel_id;
    }
}