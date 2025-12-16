using System.Collections.Generic;

namespace FantasyArchive.Data.JsonModels
{
    public class AllTimePlayerJson
    {
        public int PlayerID { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public decimal TotalPoints { get; set; }
        public int WeeksStarted { get; set; }
        public decimal AveragePoints { get; set; }
        public List<int> SeasonsWithFranchise { get; set; } = new();
        public bool IsBench { get; set; } = false;
    }

    public class AllTimeRosterJson
    {
        public List<AllTimePlayerJson> Quarterbacks { get; set; } = new();
        public List<AllTimePlayerJson> RunningBacks { get; set; } = new();
        public List<AllTimePlayerJson> WideReceivers { get; set; } = new();
        public List<AllTimePlayerJson> TightEnds { get; set; } = new();
    }
}