namespace Syren.Syren.DataTypes;

public class Spawn
{
    public string PokemonName = "";
    public readonly string ChannelId;
    public Spawn(string channelId)
    {
        ChannelId = channelId;
    }
}