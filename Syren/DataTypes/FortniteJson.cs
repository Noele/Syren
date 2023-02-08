using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syren.Syren.DataTypes
{
    public class FortniteJson
    {
        public class Account
        {
            public string id { get; set; }
            public string name { get; set; }
        }

        public class All
        {
            public Overall overall { get; set; }
            public Solo solo { get; set; }
            public Duo duo { get; set; }
            public object trio { get; set; }
            public Squad squad { get; set; }
            public Ltm ltm { get; set; }
        }

        public class BattlePass
        {
            public string level { get; set; }
            public string progress { get; set; }
        }

        public class Data
        {
            public Account account { get; set; }
            public BattlePass battlePass { get; set; }
            public string image { get; set; }
            public Stats stats { get; set; }
        }

        public class Duo
        {
            public string score { get; set; }
            public string scorePerMin { get; set; }
            public string scorePerMatch { get; set; }
            public string wins { get; set; }
            public string top5 { get; set; }
            public string top12 { get; set; }
            public string kills { get; set; }
            public string killsPerMin { get; set; }
            public string killsPerMatch { get; set; }
            public string deaths { get; set; }
            public string kd { get; set; }
            public string matches { get; set; }
            public string winRate { get; set; }
            public string minutesPlayed { get; set; }
            public string playersOutlived { get; set; }
            public DateTime lastModified { get; set; }
        }

        public class KeyboardMouse
        {
            public Overall overall { get; set; }
            public Solo solo { get; set; }
            public Duo duo { get; set; }
            public object trio { get; set; }
            public Squad squad { get; set; }
            public Ltm ltm { get; set; }
        }

        public class Ltm
        {
            public string score { get; set; }
            public string scorePerMin { get; set; }
            public string scorePerMatch { get; set; }
            public string wins { get; set; }
            public string kills { get; set; }
            public string killsPerMin { get; set; }
            public string killsPerMatch { get; set; }
            public string deaths { get; set; }
            public string kd { get; set; }
            public string matches { get; set; }
            public string winRate { get; set; }
            public string minutesPlayed { get; set; }
            public string playersOutlived { get; set; }
            public DateTime lastModified { get; set; }
        }

        public class Overall
        {
            public string score { get; set; }
            public string scorePerMin { get; set; }
            public string scorePerMatch { get; set; }
            public string wins { get; set; }
            public string top3 { get; set; }
            public string top5 { get; set; }
            public string top6 { get; set; }
            public string top10 { get; set; }
            public string top12 { get; set; }
            public string top25 { get; set; }
            public string kills { get; set; }
            public string killsPerMin { get; set; }
            public string killsPerMatch { get; set; }
            public string deaths { get; set; }
            public string kd { get; set; }
            public string matches { get; set; }
            public string winRate { get; set; }
            public string minutesPlayed { get; set; }
            public string playersOutlived { get; set; }
            public DateTime lastModified { get; set; }
        }

        public class FortniteRoot
        {
            public string status { get; set; }
            public Data data { get; set; }
        }

        public class Solo
        {
            public string score { get; set; }
            public string scorePerMin { get; set; }
            public string scorePerMatch { get; set; }
            public string wins { get; set; }
            public string top10 { get; set; }
            public string top25 { get; set; }
            public string kills { get; set; }
            public string killsPerMin { get; set; }
            public string killsPerMatch { get; set; }
            public string deaths { get; set; }
            public string kd { get; set; }
            public string matches { get; set; }
            public string winRate { get; set; }
            public string minutesPlayed { get; set; }
            public string playersOutlived { get; set; }
            public DateTime lastModified { get; set; }
        }

        public class Squad
        {
            public string score { get; set; }
            public string scorePerMin { get; set; }
            public string scorePerMatch { get; set; }
            public string wins { get; set; }
            public string top3 { get; set; }
            public string top6 { get; set; }
            public string kills { get; set; }
            public string killsPerMin { get; set; }
            public string killsPerMatch { get; set; }
            public string deaths { get; set; }
            public string kd { get; set; }
            public string matches { get; set; }
            public string winRate { get; set; }
            public string minutesPlayed { get; set; }
            public string playersOutlived { get; set; }
            public DateTime lastModified { get; set; }
        }

        public class Stats
        {
            public All all { get; set; }
            public KeyboardMouse keyboardMouse { get; set; }
            public object gamepad { get; set; }
            public object touch { get; set; }
        }


    }
}
