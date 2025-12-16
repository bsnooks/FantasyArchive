using System;
using System.Collections.Generic;



namespace FantasyArchive.Data.Models
{
    public partial class League
    {
        public League()
        {
            Drafts = new HashSet<Draft>();
            Franchises = new HashSet<Franchise>();
            Matches = new HashSet<Match>();
            Seasons = new HashSet<Season>();
            TeamScores = new HashSet<TeamScore>();
            Teams = new HashSet<Team>();
        }

        public Guid LeagueID { get; set; }
        public string Name { get; set; }
        public int? HostId { get; set; }
        public int? StartYear { get; set; }

        public virtual ICollection<Draft> Drafts { get; set; }
        public virtual ICollection<Franchise> Franchises { get; set; }
        public virtual ICollection<Match> Matches { get; set; }
        public virtual ICollection<Season> Seasons { get; set; }
        public virtual ICollection<TeamScore> TeamScores { get; set; }
        public virtual ICollection<Team> Teams { get; set; }
    }
}
