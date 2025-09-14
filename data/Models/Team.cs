using System;

namespace FantasyArchive.Data.Models
{
    public class Team
    {
        public Guid TeamId { get; set; }
        public Guid LeagueId { get; set; }
        public Guid FranchiseId { get; set; }
        public Guid? OwnerId { get; set; }
        public int Year { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? YahooTeamId { get; set; }
        public int Wins { get; set; }
        public int Loses { get; set; } // Note: keeping original spelling "Loses" instead of "Losses"
        public int Ties { get; set; }
        public int Standing { get; set; }
        public double Points { get; set; }
        public bool Champion { get; set; }
        public bool SecondPlace { get; set; }
        
        // Navigation properties
        public virtual Season Season { get; set; } = null!;
        public virtual Franchise Franchise { get; set; } = null!;
        public virtual Owner? Owner { get; set; }
    }
}