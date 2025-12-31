using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FantasyArchive.Data.JsonModels
{
    public class FranchiseWrappedJson
    {
        public string FranchiseId { get; set; } = string.Empty;
        public string FranchiseName { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public int Year { get; set; }
        
        // Season Summary
        public SeasonSummaryJson SeasonSummary { get; set; } = new();
        
        // Performance Highlights
        public PerformanceHighlightsJson Performance { get; set; } = new();
        
        // Player Highlights
        public PlayerHighlightsJson Players { get; set; } = new();
        
        // League Comparisons
        public LeagueComparisonsJson LeagueComparisons { get; set; } = new();
        
        // Fun Facts & Achievements
        public List<string> FunFacts { get; set; } = new();
        public List<AchievementJson> Achievements { get; set; } = new();
        
        // Head-to-Head Records
        public List<HeadToHeadJson> HeadToHeadRecords { get; set; } = new();
    }

    public class SeasonSummaryJson
    {
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Ties { get; set; }
        public decimal Points { get; set; }
        public int Standing { get; set; }
        public int TotalTeams { get; set; }
        public bool Champion { get; set; }
        public bool SecondPlace { get; set; }
        public bool MadePlayoffs { get; set; }
        public string SeasonOutcome { get; set; } = string.Empty; // "Champion", "Runner-up", "Made Playoffs", "Missed Playoffs"
    }

    public class PerformanceHighlightsJson
    {
        public WeekPerformanceJson BestWeek { get; set; } = new();
        public WeekPerformanceJson WorstWeek { get; set; } = new();
        public decimal AverageWeeklyScore { get; set; }
        public decimal HighestWeeklyScore { get; set; }
        public decimal LowestWeeklyScore { get; set; }
        public int WeeksAsHighScorer { get; set; }
        public int WeeksAsLowScorer { get; set; }
        public decimal PointsAgainst { get; set; }
        public decimal AverageMarginOfVictory { get; set; }
        public decimal AverageMarginOfDefeat { get; set; }
        public int BlowoutWins { get; set; } // Wins by 30+ points
        public int HeartbreakLosses { get; set; } // Losses by 5 points or less
    }

    public class WeekPerformanceJson
    {
        public int Week { get; set; }
        public decimal Points { get; set; }
        public string Opponent { get; set; } = string.Empty;
        public decimal OpponentPoints { get; set; }
        public bool Won { get; set; }
        public decimal Margin { get; set; }
    }

    public class PlayerHighlightsJson
    {
        public PlayerSeasonStatsJson MVP { get; set; } = new(); // Highest scoring player
        public PlayerSeasonStatsJson MostConsistent { get; set; } = new(); // Lowest standard deviation
        public PlayerSeasonStatsJson Breakout { get; set; } = new(); // Biggest improvement from previous season
        public PlayerSeasonStatsJson Disappointment { get; set; } = new(); // Biggest letdown from expectations
        public PlayerSeasonStatsJson BestDraftPick { get; set; } = new(); // Best value based on draft position
        public PlayerSeasonStatsJson WorstDraftPick { get; set; } = new(); // Worst value based on draft position
        public List<PlayerSeasonStatsJson> TopStarters { get; set; } = new(); // Top 5 most used starters
    }

    public class PlayerSeasonStatsJson
    {
        public int PlayerID { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public decimal TotalPoints { get; set; }
        public decimal AveragePoints { get; set; }
        public int GamesStarted { get; set; }
        public int GamesOwned { get; set; }
        public string? DraftRound { get; set; }
        public int? DraftPosition { get; set; }
        public decimal? ExpectedPoints { get; set; } // Based on draft position
        public decimal? PointsAboveExpected { get; set; }
        public string? NflTeam { get; set; }
        public string HighlightReason { get; set; } = string.Empty; // Why this player is highlighted
    }

    public class LeagueComparisonsJson
    {
        public int PointsRank { get; set; }
        public string PointsRankSuffix { get; set; } = string.Empty; // "st", "nd", "rd", "th"
        public decimal PointsAboveAverage { get; set; }
        public int WinsRank { get; set; }
        public string WinsRankSuffix { get; set; } = string.Empty;
        public decimal WinPercentage { get; set; }
        public decimal LeagueAverageWinPercentage { get; set; }
        public int HighScoreWeeksRank { get; set; }
        public int ConsistencyRank { get; set; } // Based on standard deviation (lower is better)
        public string StrongestPosition { get; set; } = string.Empty; // Position where you ranked best vs league
        public string WeakestPosition { get; set; } = string.Empty; // Position where you ranked worst vs league
        
        // Bench Analysis
        public decimal BenchPoints { get; set; } // Total points from bench players
        public int BenchPointsRank { get; set; } // Rank compared to other teams' bench points
        public string BenchPointsRankSuffix { get; set; } = string.Empty;
        public decimal BenchPointsAboveAverage { get; set; } // Compared to league average bench points
        public PlayerSeasonStatsJson? TopBenchPlayer { get; set; } // Best bench performer
        
        // Roster Diversity
        public int TotalPlayersUsed { get; set; } // Total different players started throughout season
        public int TotalPlayersUsedRank { get; set; } // Rank compared to league
        public string TotalPlayersUsedRankSuffix { get; set; } = string.Empty;
        public decimal AvgPlayersUsedInLeague { get; set; } // League average
        
        // Position Diversity  
        public int QuarterbacksUsed { get; set; }
        public int RunningBacksUsed { get; set; }
        public int WideReceiversUsed { get; set; }
        public int TightEndsUsed { get; set; }
        public int KickersUsed { get; set; }
        public int DefensesUsed { get; set; }
        
        // Position Usage Rankings
        public int QBUsageRank { get; set; }
        public int RBUsageRank { get; set; }
        public int WRUsageRank { get; set; }
        public int TEUsageRank { get; set; }
        public int KUsageRank { get; set; }
        public int DSTUsageRank { get; set; }
        
        // Position Strength (total starter points by position)
        public decimal QuarterbackPoints { get; set; }
        public decimal RunningBackPoints { get; set; }
        public decimal WideReceiverPoints { get; set; }
        public decimal TightEndPoints { get; set; }
        public decimal KickerPoints { get; set; }
        public decimal DefensePoints { get; set; }
        public int QBPointsRank { get; set; }
        public int RBPointsRank { get; set; }
        public int WRPointsRank { get; set; }
        public int TEPointsRank { get; set; }
        public int KPointsRank { get; set; }
        public int DSTPointsRank { get; set; }
        public decimal AvgQBPointsInLeague { get; set; }
        public decimal AvgRBPointsInLeague { get; set; }
        public decimal AvgWRPointsInLeague { get; set; }
        public decimal AvgTEPointsInLeague { get; set; }
        public decimal AvgKPointsInLeague { get; set; }
        public decimal AvgDSTPointsInLeague { get; set; }
    }

    public class AchievementJson
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty; // Emoji or icon identifier
        public bool IsRare { get; set; } // True if achieved by few franchises
    }

    public class HeadToHeadJson
    {
        public string OpponentFranchiseId { get; set; } = string.Empty;
        public string OpponentName { get; set; } = string.Empty;
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Ties { get; set; }
        public decimal AveragePointsFor { get; set; }
        public decimal AveragePointsAgainst { get; set; }
        public decimal BiggestWin { get; set; }
        public decimal WorstLoss { get; set; }
        public string Rivalry { get; set; } = string.Empty; // "Dominated", "Close", "Rivalry", "Nemesis"
    }
}