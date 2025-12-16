using System;
using System.Collections.Generic;


namespace FantasyArchive.Data.Models
{
    public partial class TeamPlayer
    {
        public TeamPlayer()
        {
        }

        public int PlayerID { get; set; }
        public int Year { get; set; }
        public int Week { get; set; }
        public int PassYards { get; set; }
        public int PassTDs { get; set; }
        public int RushYards { get; set; }
        public int RushTDs { get; set; }
        public int RecYards { get; set; }
        public int RecTDs { get; set; }
        public int Interceptions { get; set; }
        public int FumblesLost { get; set; }
        public int TwoPointConvert { get; set; }
        public int GamesStarted { get; set; }
        public int GamesBenched { get; set; }
        public double TeamPoints => CalculatePoints();
        public double SeasonPoints { get; set; }
        public virtual Player Player { get; set; }
        public double CalculatePoints()
        {
            return PassYards / 25.0 + PassTDs * 4 + RushYards / 10.0 + RushTDs * 6 + RecYards / 10.0 + RecTDs * 6;
        }

    }
}
