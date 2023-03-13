using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syren.Syren.DataTypes
{
    public class LeagueOfLegendsSpawn
    {
        public string ChampionName = "";
        public readonly string ChannelId;
        public LeagueOfLegendsSpawn(string channelId)
        {
            ChannelId = channelId;
        }
    }
}
