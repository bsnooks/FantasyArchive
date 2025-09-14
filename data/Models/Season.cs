using System;
using System.Collections.Generic;

namespace FantasyArchive.Data.Models
{
    public class Season
    {
        public Guid LeagueId { get; set; }
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
        
        // Navigation properties
        public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
    }
}