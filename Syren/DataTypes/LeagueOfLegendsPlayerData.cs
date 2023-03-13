using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syren.Syren.DataTypes
{
    public static class LeagueOfLegendsPlayerData
    {
        public class Player
        {
            [JsonProperty("name")]
            public string name { get; set; }
            [JsonProperty("id")]
            public string id { get; set; }
            [JsonProperty("points")]
            public int points { get; set; }
        }

        public class Players
        {
            [JsonProperty("players")]
            public List<Player> players { get; set; }
        }

    }
}