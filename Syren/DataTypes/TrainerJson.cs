using Newtonsoft.Json;

namespace Syren.Syren.DataTypes;

public class TrainerJson
{
    
    public partial class TrainerJsonRoot
    {
        [JsonProperty("trainers")]
        public List<Trainer> Trainers { get; set; }
    }

    public partial class Trainer
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("info")]
        public Info Info { get; set; }
    }

    public partial class Info
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("captured_pokemon")]
        public List<string> CapturedPokemon { get; set; }
    }
}