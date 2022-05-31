using Newtonsoft.Json;

namespace Syren.Syren.DataTypes;

public static class TrainerJson
{
    
    public class TrainerJsonRoot
    {
        [JsonProperty("trainers")]
        public List<Trainer> Trainers { get; set; } = null!;
    }

    public class Trainer
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; } = null!;

        [JsonProperty("info")]
        public Info Info { get; set; } = null!;
    }

    public class Info
    {
        [JsonProperty("name")]
        public string Name { get; set; } = null!;

        [JsonProperty("captured_pokemon")]
        public List<string> CapturedPokemon { get; set; } = null!;
    }
    
}