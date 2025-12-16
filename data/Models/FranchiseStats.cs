using System;
using System.Collections.Generic;
using System.Text;

namespace FantasyArchive.Data.Models
{
    public class FranchiseStats
    {
        public Franchise Franchise { get; set; }
        public int Wins { get; set; }
        public int Loses { get; set; }
        public int Ties { get; set; }
        public double Points { get; set; }
        public int Trades { get; set; }
        public int Championships { get; set; }
        public int PlayoffGames { get; set; }
        public int WinningSeasons { get; set; }
        public int HighestWeekPointsCount { get; set; }
        public string OtherRecord { get; set; }
        public double OtherRecordValue { get; set; }
    }
}
