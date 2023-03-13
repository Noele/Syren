using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syren.Syren.DataTypes
{
    public class LeagueChampionNames
    {
        [JsonProperty("Names")]
        public string[]? Names { get; set; }
    }
}
