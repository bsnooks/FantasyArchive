using Microsoft.EntityFrameworkCore;
using FantasyArchive.Data.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;

namespace FantasyArchive.Data.Services;

public class WeeklyRecordsService
{
    private readonly FantasyArchiveContext _context;

    public WeeklyRecordsService(FantasyArchiveContext context)
    {
        _context = context;
    }

    public async Task<WeeklyRecordsCollection> GetSeasonWeeklyRecordsAsync(int year, int recordCount = 10)
    {
        var records = new WeeklyRecordsCollection();

        // Get all team scores for the season with team and franchise information
        var seasonScores = await _context.TeamScores
            .Include(ts => ts.Team)
                .ThenInclude(t => t!.Franchise)
            .Where(ts => ts.Year == year)
            .ToListAsync();

        // Get all matches for the season with team and franchise information
        var seasonMatches = await _context.Matches
            .Include(m => m.WinningTeam)
                .ThenInclude(t => t!.Franchise)
            .Include(m => m.LosingTeam)
                .ThenInclude(t => t!.Franchise)
            .Where(m => m.Year == year)
            .ToListAsync();

        // Calculate highest scores
        records.HighestScores = seasonScores
            .OrderByDescending(ts => ts.Points)
            .Take(recordCount)
            .Select(ts => new WeeklyRecord
            {
                RecordType = "HighestScore",
                Description = "Highest Weekly Score",
                Value = ts.Points,
                FranchiseName = ts.Team?.Franchise?.MainName ?? "Unknown",
                FranchiseId = ts.Team?.FranchiseId ?? Guid.Empty,
                TeamName = ts.Team?.Name ?? "Unknown",
                Year = ts.Year,
                Week = ts.Week
            }).ToList();

        // Calculate lowest scores
        records.LowestScores = seasonScores
            .Where(ts => ts.Points > 0) // Exclude zero scores (byes, etc.)
            .OrderBy(ts => ts.Points)
            .Take(recordCount)
            .Select(ts => new WeeklyRecord
            {
                RecordType = "LowestScore",
                Description = "Lowest Weekly Score",
                Value = ts.Points,
                FranchiseName = ts.Team?.Franchise?.MainName ?? "Unknown",
                FranchiseId = ts.Team?.FranchiseId ?? Guid.Empty,
                TeamName = ts.Team?.Name ?? "Unknown",
                Year = ts.Year,
                Week = ts.Week
            }).ToList();

        // Calculate margins of victory
        var marginRecords = new List<WeeklyRecord>();
        
        foreach (var match in seasonMatches.Where(m => !m.Tied && m.MatchTypeId != MatchType.Consolation))
        {
            var winnerScore = seasonScores.FirstOrDefault(s => s.TeamID == match.WinningTeamID && s.Week == match.Week);
            var loserScore = seasonScores.FirstOrDefault(s => s.TeamID == match.LosingTeamID && s.Week == match.Week);
            
            if (winnerScore != null && loserScore != null)
            {
                var margin = winnerScore.Points - loserScore.Points;
                
                marginRecords.Add(new WeeklyRecord
                {
                    RecordType = "MarginOfVictory",
                    Description = "Margin of Victory",
                    Value = margin,
                    FranchiseName = match.WinningTeam?.Franchise?.MainName ?? "Unknown",
                    FranchiseId = match.WinningTeam?.FranchiseId ?? Guid.Empty,
                    TeamName = match.WinningTeam?.Name ?? "Unknown",
                    Year = match.Year,
                    Week = match.Week,
                    OpponentFranchiseName = match.LosingTeam?.Franchise?.MainName ?? "Unknown",
                    OpponentFranchiseId = match.LosingTeam?.FranchiseId,
                    OpponentTeamName = match.LosingTeam?.Name ?? "Unknown",
                    OpponentScore = loserScore.Points
                });
            }
        }

        // Largest margins of victory
        records.LargestMarginsOfVictory = marginRecords
            .OrderByDescending(r => r.Value)
            .Take(recordCount)
            .ToList();

        // Smallest margins of victory
        records.SmallestMarginsOfVictory = marginRecords
            .Where(r => r.Value > 0) // Exclude ties
            .OrderBy(r => r.Value)
            .Take(recordCount)
            .ToList();

        // Calculate highest and lowest scoring matchups
        var matchupRecords = new List<WeeklyRecord>();
        
        foreach (var match in seasonMatches)
        {
            var team1Score = seasonScores.FirstOrDefault(s => s.TeamID == match.WinningTeamID && s.Week == match.Week);
            var team2Score = seasonScores.FirstOrDefault(s => s.TeamID == match.LosingTeamID && s.Week == match.Week);
            
            if (team1Score != null && team2Score != null)
            {
                var totalPoints = team1Score.Points + team2Score.Points;
                
                matchupRecords.Add(new WeeklyRecord
                {
                    RecordType = "MatchupTotal",
                    Description = "Combined Matchup Score",
                    Value = totalPoints,
                    FranchiseName = match.WinningTeam?.Franchise?.MainName ?? "Unknown",
                    FranchiseId = match.WinningTeam?.FranchiseId ?? Guid.Empty,
                    TeamName = match.WinningTeam?.Name ?? "Unknown",
                    Year = match.Year,
                    Week = match.Week,
                    OpponentFranchiseName = match.LosingTeam?.Franchise?.MainName ?? "Unknown",
                    OpponentFranchiseId = match.LosingTeam?.FranchiseId,
                    OpponentTeamName = match.LosingTeam?.Name ?? "Unknown",
                    OpponentScore = team2Score.Points
                });
            }
        }

        // Highest scoring matchups
        records.HighestScoringMatchups = matchupRecords
            .OrderByDescending(r => r.Value)
            .Take(recordCount)
            .ToList();

        // Lowest scoring matchups
        records.LowestScoringMatchups = matchupRecords
            .Where(r => r.Value > 0)
            .OrderBy(r => r.Value)
            .Take(recordCount)
            .ToList();

        // Calculate lowest winning scores and highest losing scores
        var winnerRecords = new List<WeeklyRecord>();
        var loserRecords = new List<WeeklyRecord>();

        foreach (var match in seasonMatches.Where(m => !m.Tied && m.MatchTypeId != MatchType.Consolation))
        {
            var winnerScore = seasonScores.FirstOrDefault(s => s.TeamID == match.WinningTeamID && s.Week == match.Week);
            var loserScore = seasonScores.FirstOrDefault(s => s.TeamID == match.LosingTeamID && s.Week == match.Week);
            
            if (winnerScore != null && loserScore != null)
            {
                // Lowest winning scores
                winnerRecords.Add(new WeeklyRecord
                {
                    RecordType = "LowestWinningScore",
                    Description = "Lowest Winning Score",
                    Value = winnerScore.Points,
                    FranchiseName = match.WinningTeam?.Franchise?.MainName ?? "Unknown",
                    FranchiseId = match.WinningTeam?.FranchiseId ?? Guid.Empty,
                    TeamName = match.WinningTeam?.Name ?? "Unknown",
                    Year = match.Year,
                    Week = match.Week,
                    OpponentFranchiseName = match.LosingTeam?.Franchise?.MainName ?? "Unknown",
                    OpponentFranchiseId = match.LosingTeam?.FranchiseId,
                    OpponentTeamName = match.LosingTeam?.Name ?? "Unknown",
                    OpponentScore = loserScore.Points
                });

                // Highest losing scores
                loserRecords.Add(new WeeklyRecord
                {
                    RecordType = "HighestLosingScore",
                    Description = "Highest Losing Score",
                    Value = loserScore.Points,
                    FranchiseName = match.LosingTeam?.Franchise?.MainName ?? "Unknown",
                    FranchiseId = match.LosingTeam?.FranchiseId ?? Guid.Empty,
                    TeamName = match.LosingTeam?.Name ?? "Unknown",
                    Year = match.Year,
                    Week = match.Week,
                    OpponentFranchiseName = match.WinningTeam?.Franchise?.MainName ?? "Unknown",
                    OpponentFranchiseId = match.WinningTeam?.FranchiseId,
                    OpponentTeamName = match.WinningTeam?.Name ?? "Unknown",
                    OpponentScore = winnerScore.Points
                });
            }
        }

        // Lowest winning scores
        records.LowestWinningScores = winnerRecords
            .OrderBy(r => r.Value)
            .Take(recordCount)
            .ToList();

        // Highest losing scores
        records.HighestLosingScores = loserRecords
            .OrderByDescending(r => r.Value)
            .Take(recordCount)
            .ToList();

        return records;
    }

    public async Task<WeeklyRecordsCollection> GetAllTimeWeeklyRecordsAsync(int recordCount = 10)
    {
        var records = new WeeklyRecordsCollection();

        // Get all team scores with team and franchise information
        var allScores = await _context.TeamScores
            .Include(ts => ts.Team)
                .ThenInclude(t => t!.Franchise)
            .ToListAsync();

        // Get all matches with team and franchise information
        var allMatches = await _context.Matches
            .Include(m => m.WinningTeam)
                .ThenInclude(t => t!.Franchise)
            .Include(m => m.LosingTeam)
                .ThenInclude(t => t!.Franchise)
            .ToListAsync();

        // Calculate highest scores
        records.HighestScores = allScores
            .OrderByDescending(ts => ts.Points)
            .Take(recordCount)
            .Select(ts => new WeeklyRecord
            {
                RecordType = "HighestScore",
                Description = "Highest Weekly Score",
                Value = ts.Points,
                FranchiseName = ts.Team?.Franchise?.MainName ?? "Unknown",
                FranchiseId = ts.Team?.FranchiseId ?? Guid.Empty,
                TeamName = ts.Team?.Name ?? "Unknown",
                Year = ts.Year,
                Week = ts.Week
            }).ToList();

        // Calculate lowest scores
        records.LowestScores = allScores
            .Where(ts => ts.Points > 0) // Exclude zero scores
            .OrderBy(ts => ts.Points)
            .Take(recordCount)
            .Select(ts => new WeeklyRecord
            {
                RecordType = "LowestScore",
                Description = "Lowest Weekly Score",
                Value = ts.Points,
                FranchiseName = ts.Team?.Franchise?.MainName ?? "Unknown",
                FranchiseId = ts.Team?.FranchiseId ?? Guid.Empty,
                TeamName = ts.Team?.Name ?? "Unknown",
                Year = ts.Year,
                Week = ts.Week
            }).ToList();

        // Calculate margins of victory for all time
        var marginRecords = new List<WeeklyRecord>();
        
        foreach (var match in allMatches.Where(m => !m.Tied && m.MatchTypeId != MatchType.Consolation))
        {
            var winnerScore = allScores.FirstOrDefault(s => s.TeamID == match.WinningTeamID && s.Week == match.Week && s.Year == match.Year);
            var loserScore = allScores.FirstOrDefault(s => s.TeamID == match.LosingTeamID && s.Week == match.Week && s.Year == match.Year);
            
            if (winnerScore != null && loserScore != null)
            {
                var margin = winnerScore.Points - loserScore.Points;
                
                marginRecords.Add(new WeeklyRecord
                {
                    RecordType = "MarginOfVictory",
                    Description = "Margin of Victory",
                    Value = margin,
                    FranchiseName = match.WinningTeam?.Franchise?.MainName ?? "Unknown",
                    FranchiseId = match.WinningTeam?.FranchiseId ?? Guid.Empty,
                    TeamName = match.WinningTeam?.Name ?? "Unknown",
                    Year = match.Year,
                    Week = match.Week,
                    OpponentFranchiseName = match.LosingTeam?.Franchise?.MainName ?? "Unknown",
                    OpponentFranchiseId = match.LosingTeam?.FranchiseId,
                    OpponentTeamName = match.LosingTeam?.Name ?? "Unknown",
                    OpponentScore = loserScore.Points
                });
            }
        }

        // Largest margins of victory
        records.LargestMarginsOfVictory = marginRecords
            .OrderByDescending(r => r.Value)
            .Take(recordCount)
            .ToList();

        // Smallest margins of victory
        records.SmallestMarginsOfVictory = marginRecords
            .Where(r => r.Value > 0) // Exclude ties
            .OrderBy(r => r.Value)
            .Take(recordCount)
            .ToList();

        // Calculate highest and lowest scoring matchups for all time
        var matchupRecords = new List<WeeklyRecord>();
        
        foreach (var match in allMatches)
        {
            var team1Score = allScores.FirstOrDefault(s => s.TeamID == match.WinningTeamID && s.Week == match.Week && s.Year == match.Year);
            var team2Score = allScores.FirstOrDefault(s => s.TeamID == match.LosingTeamID && s.Week == match.Week && s.Year == match.Year);
            
            if (team1Score != null && team2Score != null)
            {
                var totalPoints = team1Score.Points + team2Score.Points;
                
                matchupRecords.Add(new WeeklyRecord
                {
                    RecordType = "MatchupTotal",
                    Description = "Combined Matchup Score",
                    Value = totalPoints,
                    FranchiseName = match.WinningTeam?.Franchise?.MainName ?? "Unknown",
                    FranchiseId = match.WinningTeam?.FranchiseId ?? Guid.Empty,
                    TeamName = match.WinningTeam?.Name ?? "Unknown",
                    Year = match.Year,
                    Week = match.Week,
                    OpponentFranchiseName = match.LosingTeam?.Franchise?.MainName ?? "Unknown",
                    OpponentFranchiseId = match.LosingTeam?.FranchiseId,
                    OpponentTeamName = match.LosingTeam?.Name ?? "Unknown",
                    OpponentScore = team2Score.Points
                });
            }
        }

        // Highest scoring matchups
        records.HighestScoringMatchups = matchupRecords
            .OrderByDescending(r => r.Value)
            .Take(recordCount)
            .ToList();

        // Lowest scoring matchups
        records.LowestScoringMatchups = matchupRecords
            .Where(r => r.Value > 0)
            .OrderBy(r => r.Value)
            .Take(recordCount)
            .ToList();

        // Calculate lowest winning scores and highest losing scores
        var winnerRecords = new List<WeeklyRecord>();
        var loserRecords = new List<WeeklyRecord>();

        foreach (var match in allMatches.Where(m => !m.Tied && m.MatchTypeId != MatchType.Consolation))
        {
            var winnerScore = allScores.FirstOrDefault(s => s.TeamID == match.WinningTeamID && s.Week == match.Week && s.Year == match.Year);
            var loserScore = allScores.FirstOrDefault(s => s.TeamID == match.LosingTeamID && s.Week == match.Week && s.Year == match.Year);
            
            if (winnerScore != null && loserScore != null)
            {
                // Lowest winning scores
                winnerRecords.Add(new WeeklyRecord
                {
                    RecordType = "LowestWinningScore",
                    Description = "Lowest Winning Score",
                    Value = winnerScore.Points,
                    FranchiseName = match.WinningTeam?.Franchise?.MainName ?? "Unknown",
                    FranchiseId = match.WinningTeam?.FranchiseId ?? Guid.Empty,
                    TeamName = match.WinningTeam?.Name ?? "Unknown",
                    Year = match.Year,
                    Week = match.Week,
                    OpponentFranchiseName = match.LosingTeam?.Franchise?.MainName ?? "Unknown",
                    OpponentFranchiseId = match.LosingTeam?.FranchiseId,
                    OpponentTeamName = match.LosingTeam?.Name ?? "Unknown",
                    OpponentScore = loserScore.Points
                });

                // Highest losing scores
                loserRecords.Add(new WeeklyRecord
                {
                    RecordType = "HighestLosingScore",
                    Description = "Highest Losing Score",
                    Value = loserScore.Points,
                    FranchiseName = match.LosingTeam?.Franchise?.MainName ?? "Unknown",
                    FranchiseId = match.LosingTeam?.FranchiseId ?? Guid.Empty,
                    TeamName = match.LosingTeam?.Name ?? "Unknown",
                    Year = match.Year,
                    Week = match.Week,
                    OpponentFranchiseName = match.WinningTeam?.Franchise?.MainName ?? "Unknown",
                    OpponentFranchiseId = match.WinningTeam?.FranchiseId,
                    OpponentTeamName = match.WinningTeam?.Name ?? "Unknown",
                    OpponentScore = winnerScore.Points
                });
            }
        }

        // Lowest winning scores
        records.LowestWinningScores = winnerRecords
            .OrderBy(r => r.Value)
            .Take(recordCount)
            .ToList();

        // Highest losing scores
        records.HighestLosingScores = loserRecords
            .OrderByDescending(r => r.Value)
            .Take(recordCount)
            .ToList();

        return records;
    }
}