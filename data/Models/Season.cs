using System;
using System.Collections.Generic;

namespace FantasyArchive.Data.Models
{
    public class Season
    {
        public Season()
        {
            Drafts = new HashSet<Draft>();
            Matches = new HashSet<Match>();
            TeamScores = new HashSet<TeamScore>();
            Teams = new HashSet<Team>();
        }

        public Guid LeagueID { get; set; }
        public int Year { get; set; }
        public bool? Finished { get; set; }
        public int? YahooGameId { get; set; }
        public int? YahooLeagueId { get; set; }
        public bool? KeepersSet { get; set; }
        public bool? DraftImported { get; set; }
        public int? MatchupSyncWeek { get; set; }
        public DateTime? LastTransactionSyncDate { get; set; }
        public int? WeeklyRosterSyncWeek { get; set; }
        public int? WeekStatsSyncWeek { get; set; }
        public int CurrentWeek { get; set; }

        public virtual League League { get; set; }
        public virtual ICollection<Draft> Drafts { get; set; }
        public virtual ICollection<Match> Matches { get; set; }
        public virtual ICollection<TeamScore> TeamScores { get; set; }
        public virtual ICollection<Team> Teams { get; set; }

    }
}