using System;
using System.Collections.Generic;
using System.Text;

namespace FantasyArchive.Data.Models
{
    public class MatchResult
    {
        public int Year { get; set; }
        public int Week { get; set; }
        public Franchise Winner { get; set; }
        public double WinnerPoints { get; set; }
        public Franchise Loser { get; set; }
        public double LoserPoints { get; set; }
        public bool Tied { get; set; }
    }
}
