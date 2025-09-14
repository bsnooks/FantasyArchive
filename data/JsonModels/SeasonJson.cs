using System;
using System.Collections.Generic;

namespace FantasyArchive.Data.JsonModels
{
    public class SeasonJson
    {
        public int Year { get; set; }
        public string Name => $"{Year} Season";
        public bool IsActive => !(Finished ?? false);
        public bool? Finished { get; set; }
        public int CurrentWeek { get; set; }
        public List<TeamDetailJson> Teams { get; set; } = new List<TeamDetailJson>();
    }

    public class TeamDetailJson
    {
        public Guid Id { get; set; }
        public Guid FranchiseId { get; set; }
        public string FranchiseName { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Ties { get; set; }
        public double Points { get; set; }
        public int Standing { get; set; }
        public bool Champion { get; set; }
        public bool SecondPlace { get; set; }
        public string Color { get; set; } = string.Empty;
    }
}