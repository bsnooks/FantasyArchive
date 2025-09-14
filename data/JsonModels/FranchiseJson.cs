using System;
using System.Collections.Generic;

namespace FantasyArchive.Data.JsonModels
{
    public class FranchiseJson
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty; // Current owner (from latest season)
        public List<string> Owners { get; set; } = new List<string>(); // All historical owners
        public DateTime? EstablishedDate { get; set; }
        public bool IsActive { get; set; }
        public string Color { get; set; } = string.Empty;
        public List<TeamSummaryJson> Teams { get; set; } = new List<TeamSummaryJson>();
        public AllTimeRosterJson? AllTimeRoster { get; set; }
    }

    public class TeamSummaryJson
    {
        public Guid Id { get; set; }
        public int Year { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Ties { get; set; }
        public double Points { get; set; }
        public int Standing { get; set; }
        public bool Champion { get; set; }
        public bool SecondPlace { get; set; }
    }
}