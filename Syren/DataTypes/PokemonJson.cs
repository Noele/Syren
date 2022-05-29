using Newtonsoft.Json;

namespace Syren.Syren.DataTypes;

public class PokemonJson
{
    [JsonProperty("pokemon")]
    public string[] Pokemon { get; set; }
}