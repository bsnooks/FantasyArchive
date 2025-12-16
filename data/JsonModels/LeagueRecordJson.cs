namespace FantasyArchive.Data.JsonModels
{
    public class LeagueRecordJson
    {
        public int Rank { get; set; }
        public string? FranchiseId { get; set; }
        public string? FranchiseName { get; set; }
        public string? OtherFranchiseId { get; set; }
        public string? OtherFranchiseName { get; set; }
        public int? PlayerID { get; set; }
        public string? PlayerName { get; set; }
        public string? PlayerPosition { get; set; }
        public string RecordValue { get; set; } = string.Empty;
        public double RecordNumericValue { get; set; }
        public int? Year { get; set; }
        public int? Week { get; set; }
    }
}