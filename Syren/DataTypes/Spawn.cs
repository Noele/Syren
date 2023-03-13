namespace Syren.Syren.DataTypes;

public class PokemonSpawn
{
    public string PokemonName = "";
    public readonly string ChannelId;
    public PokemonSpawn(string channelId)
    {
        ChannelId = channelId;
    }
}