namespace FantasyArchive.Data.Models;

public class WeeklyRecord
{
    public string RecordType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Value { get; set; }
    public string FranchiseName { get; set; } = string.Empty;
    public Guid FranchiseId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Week { get; set; }
    public string OpponentFranchiseName { get; set; } = string.Empty;
    public Guid? OpponentFranchiseId { get; set; }
    public string OpponentTeamName { get; set; } = string.Empty;
    public double? OpponentScore { get; set; }
    public DateTime? Date { get; set; }
}

public class WeeklyRecordsCollection
{
    public List<WeeklyRecord> HighestScores { get; set; } = new();
    public List<WeeklyRecord> LowestScores { get; set; } = new();
    public List<WeeklyRecord> LargestMarginsOfVictory { get; set; } = new();
    public List<WeeklyRecord> SmallestMarginsOfVictory { get; set; } = new();
    public List<WeeklyRecord> HighestScoringMatchups { get; set; } = new();
    public List<WeeklyRecord> LowestScoringMatchups { get; set; } = new();
}