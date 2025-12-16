using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FantasyArchive.Data.Models;

namespace FantasyArchive.Data.Services
{
    public static class PlayerWeekExtensions
    {
        public static decimal CalculateFantasyPoints(this PlayerWeek pw)
        {
            return (pw.PassYards / 25.0m) + 
                   (pw.PassTDs * 4) + 
                   (pw.RushYards / 10.0m) + 
                   (pw.RushTDs * 6) + 
                   (pw.RecYards / 10.0m) + 
                   (pw.RecTDs * 6) +
                   (pw.TwoPointConvert * 2) -
                   (pw.Interceptions * 2) -
                   (pw.FumblesLost * 2);
        }
    }

    public class RecordsService
    {
        private readonly FantasyArchiveContext _context;
        private readonly WeeklyRecordsService _weeklyRecordsService;

        public RecordsService(FantasyArchiveContext context, WeeklyRecordsService weeklyRecordsService)
        {
            _context = context;
            _weeklyRecordsService = weeklyRecordsService;
        }

        public async Task<List<LeagueRecords>> GetRecordsAsync(
            Guid? franchiseId = null, 
            int? season = null, 
            int number = 10)
        {
            var records = new List<LeagueRecords>();

            // Determine context for records
            bool isFranchiseSpecific = franchiseId.HasValue;
            bool isSeasonSpecific = season.HasValue;

            // If both franchise and season are set, prioritize season records
            if (isSeasonSpecific)
            {
                records.AddRange(await GetSeasonRecords(season.Value, number));
            }
            else if (isFranchiseSpecific)
            {
                records.AddRange(await GetFranchiseRecords(franchiseId.Value, number));
            }
            else
            {
                records.AddRange(await GetAllTimeRecords(number));
            }

            return records;
        }

        private async Task<List<LeagueRecords>> GetAllTimeRecords(int number)
        {
            var records = new List<LeagueRecords>();

            // Franchise Records
            records.Add(await GetMostChampionshipsRecord(number));
            records.Add(await GetMostWinsRecord(number));
            records.Add(await GetMostPointsRecord(number));
            records.Add(await GetBestWinPercentageRecord(number));
            records.Add(await GetMostPlayoffAppearancesRecord(number));
            records.Add(await GetMostFinalsAppearancesRecord(number));
            records.Add(await GetMostTradesRecord(number));

            // Season Records
            records.Add(await GetBestSeasonRecordRecord(number));
            records.Add(await GetMostSeasonPointsRecord(number));
            records.Add(await GetMostPointsPerGameRecord(number));
            records.Add(await GetBestERASeasonRecord(number));
            records.Add(await GetWorstSeasonRecordRecord(number));

            // Weekly Records
            var weeklyRecords = await _weeklyRecordsService.GetAllTimeWeeklyRecordsAsync();
            records.AddRange(ConvertWeeklyRecordsToLeagueRecords(weeklyRecords, null));

            // Player Records
            records.Add(await GetMostTimesKeptRecord(number));
            records.Add(await GetMostTimesKeptByPositionRecord("QB", number));
            records.Add(await GetMostTimesKeptByPositionRecord("RB", number));
            records.Add(await GetMostTimesKeptByPositionRecord("WR", number));
            records.Add(await GetMostTimesKeptByPositionRecord("TE", number));
            
            records.Add(await GetMostTimesTradedRecord(number));
            records.Add(await GetMostTimesTradedByPositionRecord("QB", number));
            records.Add(await GetMostTimesTradedByPositionRecord("RB", number));
            records.Add(await GetMostTimesTradedByPositionRecord("WR", number));
            records.Add(await GetMostTimesTradedByPositionRecord("TE", number));
            
            records.Add(await GetMostPointsOnBenchRecord(number));
            records.Add(await GetMostPointsOnBenchByPositionRecord("QB", number));
            records.Add(await GetMostPointsOnBenchByPositionRecord("RB", number));
            records.Add(await GetMostPointsOnBenchByPositionRecord("WR", number));
            records.Add(await GetMostPointsOnBenchByPositionRecord("TE", number));
            
            records.Add(await GetHighestPointPercentWeekRecord(number));
            records.Add(await GetHighestPointPercentWeekByPositionRecord("QB", number));
            records.Add(await GetHighestPointPercentWeekByPositionRecord("RB", number));
            records.Add(await GetHighestPointPercentWeekByPositionRecord("WR", number));
            records.Add(await GetHighestPointPercentWeekByPositionRecord("TE", number));
            
            records.Add(await GetHighestPointPercentSeasonRecord(number));
            records.Add(await GetHighestPointPercentSeasonByPositionRecord("QB", number));
            records.Add(await GetHighestPointPercentSeasonByPositionRecord("RB", number));
            records.Add(await GetHighestPointPercentSeasonByPositionRecord("WR", number));
            records.Add(await GetHighestPointPercentSeasonByPositionRecord("TE", number));

            // Owner Earnings Records
            records.Add(await GetCareerEarningsRecord(number));
            records.Add(await GetCareerEarningsLast10YearsRecord(number));

            // Skip player records for now until we fix the column mapping
            // records.Add(await GetBestPlayerWeekRecord("QB", number));
            // records.Add(await GetBestPlayerWeekRecord("RB", number));
            // records.Add(await GetBestPlayerWeekRecord("WR", number));
            // records.Add(await GetBestPlayerWeekRecord("TE", number));

            return records;
        }

        private async Task<List<LeagueRecords>> GetSeasonRecords(int year, int number)
        {
            var records = new List<LeagueRecords>();

            // Season-specific records
            records.Add(await GetSeasonTeamStandingsRecord(year));
            records.Add(await GetSeasonMostPointsRecord(year, number));
            records.Add(await GetSeasonHighestAveragePointsPerWeekRecord(year, number));

            // Weekly Records for this season
            var weeklyRecords = await _weeklyRecordsService.GetSeasonWeeklyRecordsAsync(year);
            records.AddRange(ConvertWeeklyRecordsToLeagueRecords(weeklyRecords, year));

            // Skip player records for now
            // records.Add(await GetSeasonBestPlayerWeekRecord("QB", year, number));
            // records.Add(await GetSeasonBestPlayerWeekRecord("RB", year, number));
            // records.Add(await GetSeasonBestPlayerWeekRecord("WR", year, number));
            // records.Add(await GetSeasonBestPlayerWeekRecord("TE", year, number));

            return records;
        }

        private async Task<List<LeagueRecords>> GetFranchiseRecords(Guid franchiseId, int number)
        {
            var records = new List<LeagueRecords>();

            // Franchise-specific records
            records.Add(await GetFranchiseChampionshipYearsRecord(franchiseId));
            records.Add(await GetFranchiseBestSeasonsRecord(franchiseId, number));
            // Skip player records for now
            // records.Add(await GetFranchiseBestPlayerWeeksRecord(franchiseId, number));

            return records;
        }

        private async Task<LeagueRecords> GetMostChampionshipsRecord(int number)
        {
            var franchiseStats = await _context.Franchises
                .Select(f => new
                {
                    Franchise = f,
                    Championships = f.Teams.Count(t => t.Champion)
                })
                .Where(f => f.Championships > 0)
                .OrderByDescending(f => f.Championships)
                .Take(number)
                .ToListAsync();

            return new LeagueRecords
            {
                RecordTitle = "Most Championships",
                PositiveRecord = true,
                RecordType = RecordType.AllTime,
                Records = franchiseStats.Select((stat, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    FranchiseId = stat.Franchise.FranchiseId,
                    Franchise = stat.Franchise,
                    RecordValue = stat.Championships.ToString(),
                    RecordNumericValue = stat.Championships
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetMostWinsRecord(int number)
        {
            var franchiseStats = await _context.Franchises
                .Select(f => new
                {
                    Franchise = f,
                    TotalWins = f.Teams.Sum(t => t.Wins)
                })
                .OrderByDescending(f => f.TotalWins)
                .Take(number)
                .ToListAsync();

            return new LeagueRecords
            {
                RecordTitle = "Most Wins (All Time)",
                PositiveRecord = true,
                RecordType = RecordType.AllTime,
                Records = franchiseStats.Select((stat, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    FranchiseId = stat.Franchise.FranchiseId,
                    Franchise = stat.Franchise,
                    RecordValue = stat.TotalWins.ToString(),
                    RecordNumericValue = stat.TotalWins
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetMostPointsRecord(int number)
        {
            var franchiseStats = await _context.Franchises
                .Select(f => new
                {
                    Franchise = f,
                    TotalPoints = f.Teams.Sum(t => t.Points)
                })
                .OrderByDescending(f => f.TotalPoints)
                .Take(number)
                .ToListAsync();

            return new LeagueRecords
            {
                RecordTitle = "Most Points (All Time)",
                PositiveRecord = true,
                RecordType = RecordType.AllTime,
                Records = franchiseStats.Select((stat, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    FranchiseId = stat.Franchise.FranchiseId,
                    Franchise = stat.Franchise,
                    RecordValue = stat.TotalPoints.ToString("F1"),
                    RecordNumericValue = stat.TotalPoints
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetBestWinPercentageRecord(int number)
        {
            var franchiseStats = await _context.Franchises
                .Select(f => new
                {
                    Franchise = f,
                    TotalWins = f.Teams.Sum(t => t.Wins),
                    TotalGames = f.Teams.Sum(t => t.Wins + t.Loses + t.Ties)
                })
                .Where(f => f.TotalGames >= 10) // Minimum games threshold
                .ToListAsync();

            var winPercentages = franchiseStats
                .Select(f => new
                {
                    f.Franchise,
                    WinPercentage = f.TotalGames > 0 ? (double)f.TotalWins / f.TotalGames : 0
                })
                .OrderByDescending(f => f.WinPercentage)
                .Take(number)
                .ToList();

            return new LeagueRecords
            {
                RecordTitle = "Best Win Percentage",
                PositiveRecord = true,
                RecordType = RecordType.AllTime,
                Records = winPercentages.Select((stat, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    FranchiseId = stat.Franchise.FranchiseId,
                    Franchise = stat.Franchise,
                    RecordValue = $"{(stat.WinPercentage * 100):F1}%",
                    RecordNumericValue = stat.WinPercentage
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetBestSeasonRecordRecord(int number)
        {
            var teams = await _context.Teams
                .Include(t => t.Franchise)
                .OrderByDescending(t => t.Wins)
                .ThenByDescending(t => t.Points)
                .Take(number)
                .ToListAsync();

            return new LeagueRecords
            {
                RecordTitle = "Best Season Records",
                PositiveRecord = true,
                RecordType = RecordType.Season,
                Records = teams.Select((team, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    FranchiseId = team.FranchiseId,
                    Franchise = team.Franchise,
                    RecordValue = $"{team.Wins}-{team.Loses}" + (team.Ties > 0 ? $"-{team.Ties}" : ""),
                    RecordNumericValue = team.Wins,
                    Year = team.Year
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetMostSeasonPointsRecord(int number)
        {
            var teams = await _context.Teams
                .Include(t => t.Franchise)
                .OrderByDescending(t => t.Points)
                .Take(number)
                .ToListAsync();

            return new LeagueRecords
            {
                RecordTitle = "Most Points in a Season",
                PositiveRecord = true,
                RecordType = RecordType.Season,
                Records = teams.Select((team, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    FranchiseId = team.FranchiseId,
                    Franchise = team.Franchise,
                    RecordValue = team.Points.ToString("F1"),
                    RecordNumericValue = team.Points,
                    Year = team.Year
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetMostPointsPerGameRecord(int number)
        {
            // Get regular season week mapping
            var regularSeasonWeeks = await GetRegularSeasonWeekMapping();
            
            // Calculate regular season points and games for each team
            var teams = await _context.Teams
                .Include(t => t.Franchise)
                .Where(t => t.Wins + t.Loses + t.Ties > 0) // Ensure they played games
                .ToListAsync();

            // Get team scores for regular season weeks only
            var teamScores = await _context.TeamScores
                .ToListAsync();

            var teamsWithPPG = new List<dynamic>();
            
            foreach (var team in teams)
            {
                if (!regularSeasonWeeks.ContainsKey(team.Year)) continue;
                
                var maxRegularWeek = regularSeasonWeeks[team.Year];
                var regularSeasonScores = teamScores
                    .Where(ts => ts.TeamID == team.TeamId && ts.Year == team.Year && ts.Week <= maxRegularWeek)
                    .ToList();
                    
                if (regularSeasonScores.Any())
                {
                    var totalRegularSeasonPoints = regularSeasonScores.Sum(ts => ts.Points);
                    var regularSeasonGames = regularSeasonScores.Count();
                    var ppg = totalRegularSeasonPoints / regularSeasonGames;
                    
                    teamsWithPPG.Add(new {
                        Team = team,
                        TotalGames = regularSeasonGames,
                        PPG = ppg,
                        TotalPoints = totalRegularSeasonPoints
                    });
                }
            }
            
            var topTeams = teamsWithPPG
                .OrderByDescending(x => ((dynamic)x).PPG)
                .Take(number)
                .ToList();

            return new LeagueRecords
            {
                RecordTitle = "Most Points Per Game (Regular Season)",
                PositiveRecord = true,
                RecordType = RecordType.Season,
                Records = topTeams.Select((teamData, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    FranchiseId = ((dynamic)teamData).Team.FranchiseId,
                    Franchise = ((dynamic)teamData).Team.Franchise,
                    RecordValue = $"{((dynamic)teamData).PPG:F2} ({((dynamic)teamData).TotalGames} games)",
                    RecordNumericValue = ((dynamic)teamData).PPG,
                    Year = ((dynamic)teamData).Team.Year
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetBestERASeasonRecord(int number)
        {
            // Get regular season week mapping
            var regularSeasonWeeks = await GetRegularSeasonWeekMapping();
            
            // Calculate ERA (points per game relative to league average) for regular season only
            var teams = await _context.Teams
                .Include(t => t.Franchise)
                .Where(t => t.Wins + t.Loses + t.Ties > 0) // Ensure they played games
                .ToListAsync();

            // Get team scores for all teams
            var teamScores = await _context.TeamScores
                .ToListAsync();

            // Calculate regular season stats for each team and year
            var teamStats = new List<dynamic>();
            var yearlyTotals = new Dictionary<int, (double TotalPoints, int TotalGames)>();
            
            foreach (var team in teams)
            {
                if (!regularSeasonWeeks.ContainsKey(team.Year)) continue;
                
                var maxRegularWeek = regularSeasonWeeks[team.Year];
                var regularSeasonScores = teamScores
                    .Where(ts => ts.TeamID == team.TeamId && ts.Year == team.Year && ts.Week <= maxRegularWeek)
                    .ToList();
                    
                if (regularSeasonScores.Any())
                {
                    var totalRegularSeasonPoints = regularSeasonScores.Sum(ts => ts.Points);
                    var regularSeasonGames = regularSeasonScores.Count();
                    var ppg = totalRegularSeasonPoints / regularSeasonGames;
                    
                    teamStats.Add(new {
                        Team = team,
                        TotalGames = regularSeasonGames,
                        PPG = ppg,
                        TotalPoints = totalRegularSeasonPoints,
                        Year = team.Year
                    });
                    
                    // Track yearly totals for league average calculation
                    if (!yearlyTotals.ContainsKey(team.Year))
                    {
                        yearlyTotals[team.Year] = (0, 0);
                    }
                    var current = yearlyTotals[team.Year];
                    yearlyTotals[team.Year] = (current.TotalPoints + totalRegularSeasonPoints, current.TotalGames + regularSeasonGames);
                }
            }
            
            // Calculate league averages for each year
            var yearlyAverages = yearlyTotals.ToDictionary(
                kvp => kvp.Key, 
                kvp => kvp.Value.TotalGames > 0 ? kvp.Value.TotalPoints / kvp.Value.TotalGames : 0
            );

            // Calculate ERA for each team (team PPG - league average PPG for that year)
            var teamsWithERA = teamStats
                .Select(ts => new {
                    Team = ((dynamic)ts).Team,
                    TotalGames = ((dynamic)ts).TotalGames,
                    PPG = ((dynamic)ts).PPG,
                    LeagueAvg = yearlyAverages.ContainsKey(((dynamic)ts).Year) ? yearlyAverages[((dynamic)ts).Year] : 0,
                    ERA = ((dynamic)ts).PPG - (yearlyAverages.ContainsKey(((dynamic)ts).Year) ? yearlyAverages[((dynamic)ts).Year] : 0)
                })
                .OrderByDescending(x => x.ERA)
                .Take(number)
                .ToList();

            return new LeagueRecords
            {
                RecordTitle = "Best ERA Season (Regular Season)",
                PositiveRecord = true,
                RecordType = RecordType.Season,
                Records = teamsWithERA.Select((teamData, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    FranchiseId = teamData.Team.FranchiseId,
                    Franchise = teamData.Team.Franchise,
                    RecordValue = $"{(teamData.ERA >= 0 ? "+" : "")}{teamData.ERA:F1} ({teamData.PPG:F1} vs {teamData.LeagueAvg:F1})",
                    RecordNumericValue = teamData.ERA,
                    Year = teamData.Team.Year
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetWorstSeasonRecordRecord(int number)
        {
            var teams = await _context.Teams
                .Include(t => t.Franchise)
                .OrderByDescending(t => t.Loses)
                .ThenBy(t => t.Wins)
                .Take(number)
                .ToListAsync();

            return new LeagueRecords
            {
                RecordTitle = "Worst Season Records",
                PositiveRecord = false,
                RecordType = RecordType.Season,
                Records = teams.Select((team, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    FranchiseId = team.FranchiseId,
                    Franchise = team.Franchise,
                    RecordValue = $"{team.Wins}-{team.Loses}" + (team.Ties > 0 ? $"-{team.Ties}" : ""),
                    RecordNumericValue = team.Loses,
                    Year = team.Year
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetBestPlayerWeekRecord(string position, int number)
        {
            // Get all player weeks for this position, then calculate points in memory
            var allPlayerWeeks = await _context.PlayerWeeks
                .Include(pw => pw.Player)
                .Where(pw => pw.Player.Position == position && pw.Started)
                .ToListAsync();

            var playerWeeks = allPlayerWeeks
                .OrderByDescending(pw => pw.CalculateFantasyPoints())
                .Take(number)
                .ToList();

            return new LeagueRecords
            {
                RecordTitle = $"Best {position} Performances",
                PositiveRecord = true,
                RecordType = RecordType.PlayerStats,
                Records = playerWeeks.Select((pw, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    PlayerID = pw.PlayerID,
                    Player = pw.Player,
                    RecordValue = $"{pw.CalculateFantasyPoints():F1} ({pw.Year} Week {pw.Week})",
                    RecordNumericValue = (double)pw.CalculateFantasyPoints(),
                    Year = pw.Year,
                    Week = pw.Week
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetSeasonTeamStandingsRecord(int year)
        {
            var teams = await _context.Teams
                .Include(t => t.Franchise)
                .Where(t => t.Year == year)
                .OrderBy(t => t.Standing)
                .ToListAsync();

            return new LeagueRecords
            {
                RecordTitle = $"{year} Final Standings",
                PositiveRecord = true,
                RecordType = RecordType.Season,
                Records = teams.Select(team => new LeagueRecord
                {
                    Rank = team.Standing,
                    FranchiseId = team.FranchiseId,
                    Franchise = team.Franchise,
                    RecordValue = $"{team.Wins}-{team.Loses}" + (team.Ties > 0 ? $"-{team.Ties}" : "") + $" ({team.Points:F1} pts)",
                    RecordNumericValue = team.Points,
                    Year = team.Year
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetSeasonMostPointsRecord(int year, int number)
        {
            var teams = await _context.Teams
                .Include(t => t.Franchise)
                .Where(t => t.Year == year)
                .OrderByDescending(t => t.Points)
                .Take(number)
                .ToListAsync();

            return new LeagueRecords
            {
                RecordTitle = $"{year} Most Points",
                PositiveRecord = true,
                RecordType = RecordType.Season,
                Records = teams.Select((team, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    FranchiseId = team.FranchiseId,
                    Franchise = team.Franchise,
                    RecordValue = team.Points.ToString("F1"),
                    RecordNumericValue = team.Points,
                    Year = team.Year
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetSeasonBestPlayerWeekRecord(string position, int year, int number)
        {
            // Get all player weeks for this position and year, then calculate points in memory
            var allPlayerWeeks = await _context.PlayerWeeks
                .Include(pw => pw.Player)
                .Where(pw => pw.Player.Position == position && pw.Year == year && pw.Started)
                .ToListAsync();

            var playerWeeks = allPlayerWeeks
                .OrderByDescending(pw => pw.CalculateFantasyPoints())
                .Take(number)
                .ToList();

            return new LeagueRecords
            {
                RecordTitle = $"{year} Best {position} Performances",
                PositiveRecord = true,
                RecordType = RecordType.PlayerStats,
                Records = playerWeeks.Select((pw, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    PlayerID = pw.PlayerID,
                    Player = pw.Player,
                    RecordValue = $"{pw.CalculateFantasyPoints():F1} (Week {pw.Week})",
                    RecordNumericValue = (double)pw.CalculateFantasyPoints(),
                    Year = pw.Year,
                    Week = pw.Week
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetFranchiseChampionshipYearsRecord(Guid franchiseId)
        {
            var championships = await _context.Teams
                .Include(t => t.Franchise)
                .Where(t => t.FranchiseId == franchiseId && t.Champion)
                .OrderByDescending(t => t.Year)
                .ToListAsync();

            return new LeagueRecords
            {
                RecordTitle = "Championship Years",
                PositiveRecord = true,
                RecordType = RecordType.Franchise,
                Records = championships.Select((team, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    FranchiseId = team.FranchiseId,
                    Franchise = team.Franchise,
                    RecordValue = $"{team.Year} ({team.Wins}-{team.Loses}, {team.Points:F1} pts)",
                    RecordNumericValue = team.Year,
                    Year = team.Year
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetFranchiseBestSeasonsRecord(Guid franchiseId, int number)
        {
            var teams = await _context.Teams
                .Include(t => t.Franchise)
                .Where(t => t.FranchiseId == franchiseId)
                .OrderByDescending(t => t.Wins)
                .ThenByDescending(t => t.Points)
                .Take(number)
                .ToListAsync();

            return new LeagueRecords
            {
                RecordTitle = "Best Seasons",
                PositiveRecord = true,
                RecordType = RecordType.Franchise,
                Records = teams.Select((team, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    FranchiseId = team.FranchiseId,
                    Franchise = team.Franchise,
                    RecordValue = $"{team.Year}: {team.Wins}-{team.Loses} ({team.Points:F1} pts)",
                    RecordNumericValue = team.Wins,
                    Year = team.Year
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetFranchiseBestPlayerWeeksRecord(Guid franchiseId, int number)
        {
            // Get all player weeks for this franchise, then calculate points in memory
            var allPlayerWeeks = await _context.PlayerWeeks
                .Include(pw => pw.Player)
                .Include(pw => pw.Team)
                .Where(pw => pw.Team.FranchiseId == franchiseId && pw.Started)
                .ToListAsync();

            var playerWeeks = allPlayerWeeks
                .OrderByDescending(pw => pw.CalculateFantasyPoints())
                .Take(number)
                .ToList();

            return new LeagueRecords
            {
                RecordTitle = "Best Player Performances",
                PositiveRecord = true,
                RecordType = RecordType.PlayerStats,
                Records = playerWeeks.Select((pw, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    PlayerID = pw.PlayerID,
                    Player = pw.Player,
                    RecordValue = $"{pw.Player.Name} ({pw.Player.Position}): {pw.CalculateFantasyPoints():F1} ({pw.Year} Week {pw.Week})",
                    RecordNumericValue = (double)pw.CalculateFantasyPoints(),
                    Year = pw.Year,
                    Week = pw.Week
                }).ToList()
            };
        }

        private List<LeagueRecords> ConvertWeeklyRecordsToLeagueRecords(WeeklyRecordsCollection weeklyRecords, int? year)
        {
            var records = new List<LeagueRecords>();

            // Convert high scores
            if (weeklyRecords.HighestScores.Any())
            {
                records.Add(new LeagueRecords
                {
                    RecordTitle = year.HasValue ? $"{year} Weekly High Scores" : "Weekly High Scores",
                    PositiveRecord = true,
                    RecordType = RecordType.Match,
                    Records = weeklyRecords.HighestScores.Take(10).Select((wr, index) => new LeagueRecord
                    {
                        Rank = index + 1,
                        FranchiseId = wr.FranchiseId,
                        Franchise = wr.FranchiseId != Guid.Empty ? new Franchise { FranchiseId = wr.FranchiseId, MainName = wr.FranchiseName ?? "Unknown" } : null,
                        RecordValue = $"{wr.Value:F1} pts",
                        RecordNumericValue = wr.Value,
                        Year = wr.Year,
                        Week = wr.Week
                    }).ToList()
                });
            }

            // Convert low scores
            if (weeklyRecords.LowestScores.Any())
            {
                records.Add(new LeagueRecords
                {
                    RecordTitle = year.HasValue ? $"{year} Weekly Low Scores" : "Weekly Low Scores",
                    PositiveRecord = false,
                    RecordType = RecordType.Match,
                    Records = weeklyRecords.LowestScores.Take(10).Select((wr, index) => new LeagueRecord
                    {
                        Rank = index + 1,
                        FranchiseId = wr.FranchiseId,
                        Franchise = wr.FranchiseId != Guid.Empty ? new Franchise { FranchiseId = wr.FranchiseId, MainName = wr.FranchiseName ?? "Unknown" } : null,
                        RecordValue = $"{wr.Value:F1} pts",
                        RecordNumericValue = wr.Value,
                        Year = wr.Year,
                        Week = wr.Week
                    }).ToList()
                });
            }

            // Convert largest victory margins
            if (weeklyRecords.LargestMarginsOfVictory.Any())
            {
                records.Add(new LeagueRecords
                {
                    RecordTitle = year.HasValue ? $"{year} Largest Victory Margins" : "Largest Victory Margins",
                    PositiveRecord = true,
                    RecordType = RecordType.Match,
                    Records = weeklyRecords.LargestMarginsOfVictory.Take(10).Select((wr, index) => new LeagueRecord
                    {
                        Rank = index + 1,
                        FranchiseId = wr.FranchiseId,
                        Franchise = wr.FranchiseId != Guid.Empty ? new Franchise { FranchiseId = wr.FranchiseId, MainName = wr.FranchiseName ?? "Unknown" } : null,
                        RecordValue = $"{wr.Value:F1} pts",
                        RecordNumericValue = wr.Value,
                        Year = wr.Year,
                        Week = wr.Week
                    }).ToList()
                });
            }

            // Convert highest matchup totals
            if (weeklyRecords.HighestScoringMatchups.Any())
            {
                records.Add(new LeagueRecords
                {
                    RecordTitle = year.HasValue ? $"{year} Highest Matchup Totals" : "Highest Matchup Totals",
                    PositiveRecord = true,
                    RecordType = RecordType.Match,
                    Records = weeklyRecords.HighestScoringMatchups.Take(10).Select((wr, index) => new LeagueRecord
                    {
                        Rank = index + 1,
                        FranchiseId = wr.FranchiseId,
                        Franchise = wr.FranchiseId != Guid.Empty ? new Franchise { FranchiseId = wr.FranchiseId, MainName = wr.FranchiseName ?? "Unknown" } : null,
                        RecordValue = $"{wr.Value:F1} pts",
                        RecordNumericValue = wr.Value,
                        Year = wr.Year,
                        Week = wr.Week
                    }).ToList()
                });
            }

            // Convert smallest victory margins
            if (weeklyRecords.SmallestMarginsOfVictory.Any())
            {
                records.Add(new LeagueRecords
                {
                    RecordTitle = year.HasValue ? $"{year} Smallest Victory Margins" : "Smallest Victory Margins",
                    PositiveRecord = false,
                    RecordType = RecordType.Match,
                    Records = weeklyRecords.SmallestMarginsOfVictory.Take(10).Select((wr, index) => new LeagueRecord
                    {
                        Rank = index + 1,
                        FranchiseId = wr.FranchiseId,
                        Franchise = wr.FranchiseId != Guid.Empty ? new Franchise { FranchiseId = wr.FranchiseId, MainName = wr.FranchiseName ?? "Unknown" } : null,
                        RecordValue = $"{wr.Value:F1} pts",
                        RecordNumericValue = wr.Value,
                        Year = wr.Year,
                        Week = wr.Week
                    }).ToList()
                });
            }

            // Convert lowest winning scores
            if (weeklyRecords.LowestWinningScores.Any())
            {
                records.Add(new LeagueRecords
                {
                    RecordTitle = year.HasValue ? $"{year} Lowest Winning Scores" : "Lowest Winning Scores",
                    PositiveRecord = false,
                    RecordType = RecordType.Match,
                    Records = weeklyRecords.LowestWinningScores.Take(10).Select((wr, index) => new LeagueRecord
                    {
                        Rank = index + 1,
                        FranchiseId = wr.FranchiseId,
                        Franchise = wr.FranchiseId != Guid.Empty ? new Franchise { FranchiseId = wr.FranchiseId, MainName = wr.FranchiseName ?? "Unknown" } : null,
                        RecordValue = $"{wr.Value:F1} pts",
                        RecordNumericValue = wr.Value,
                        Year = wr.Year,
                        Week = wr.Week
                    }).ToList()
                });
            }

            // Convert highest losing scores
            if (weeklyRecords.HighestLosingScores.Any())
            {
                records.Add(new LeagueRecords
                {
                    RecordTitle = year.HasValue ? $"{year} Highest Losing Scores" : "Highest Losing Scores",
                    PositiveRecord = true,
                    RecordType = RecordType.Match,
                    Records = weeklyRecords.HighestLosingScores.Take(10).Select((wr, index) => new LeagueRecord
                    {
                        Rank = index + 1,
                        FranchiseId = wr.FranchiseId,
                        Franchise = wr.FranchiseId != Guid.Empty ? new Franchise { FranchiseId = wr.FranchiseId, MainName = wr.FranchiseName ?? "Unknown" } : null,
                        RecordValue = $"{wr.Value:F1} pts",
                        RecordNumericValue = wr.Value,
                        Year = wr.Year,
                        Week = wr.Week
                    }).ToList()
                });
            }

            return records;
        }

        // All Time Team/Franchise Records

        private async Task<LeagueRecords> GetSeasonHighestAveragePointsPerWeekRecord(int year, int number)
        {
            var franchiseStats = await _context.Franchises
                .Select(f => new
                {
                    Franchise = f,
                    TotalPoints = f.Teams.Where(t => t.Year == year).Sum(t => t.Points),
                    TotalWeeks = f.Teams.Where(t => t.Year == year).Sum(t => t.Wins + t.Loses + t.Ties)
                })
                .Where(f => f.TotalWeeks > 0)
                .ToListAsync();

            var averageStats = franchiseStats
                .Select(f => new
                {
                    f.Franchise,
                    AvgPointsPerWeek = f.TotalWeeks > 0 ? f.TotalPoints / f.TotalWeeks : 0
                })
                .OrderByDescending(f => f.AvgPointsPerWeek)
                .Take(number)
                .ToList();

            return new LeagueRecords
            {
                RecordTitle = $"{year} Highest Average Points per Week",
                PositiveRecord = true,
                RecordType = RecordType.Season,
                Records = averageStats.Select((stat, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    FranchiseId = stat.Franchise.FranchiseId,
                    Franchise = stat.Franchise,
                    RecordValue = $"{stat.AvgPointsPerWeek:F1}",
                    RecordNumericValue = stat.AvgPointsPerWeek,
                    Year = year
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetMostPlayoffAppearancesRecord(int number)
        {
            var franchiseStats = await _context.Franchises
                .Select(f => new
                {
                    Franchise = f,
                    PlayoffAppearances = f.Teams.Count(t => 
                        t.MatchWinningTeams.Any(m => m.MatchTypeId == MatchType.Playoff) ||
                        t.MatchLosingTeams.Any(m => m.MatchTypeId == MatchType.Playoff)
                    )
                })
                .Where(f => f.PlayoffAppearances > 0)
                .OrderByDescending(f => f.PlayoffAppearances)
                .Take(number)
                .ToListAsync();

            return new LeagueRecords
            {
                RecordTitle = "Most Playoff Appearances",
                PositiveRecord = true,
                RecordType = RecordType.AllTime,
                Records = franchiseStats.Select((stat, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    FranchiseId = stat.Franchise.FranchiseId,
                    Franchise = stat.Franchise,
                    RecordValue = stat.PlayoffAppearances.ToString(),
                    RecordNumericValue = stat.PlayoffAppearances
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetMostFinalsAppearancesRecord(int number)
        {
            var franchiseStats = await _context.Franchises
                .Select(f => new
                {
                    Franchise = f,
                    FinalsAppearances = f.Teams.Count(t => t.Champion || t.SecondPlace)
                })
                .Where(f => f.FinalsAppearances > 0)
                .OrderByDescending(f => f.FinalsAppearances)
                .Take(number)
                .ToListAsync();

            return new LeagueRecords
            {
                RecordTitle = "Most Finals Appearances",
                PositiveRecord = true,
                RecordType = RecordType.AllTime,
                Records = franchiseStats.Select((stat, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    FranchiseId = stat.Franchise.FranchiseId,
                    Franchise = stat.Franchise,
                    RecordValue = stat.FinalsAppearances.ToString(),
                    RecordNumericValue = stat.FinalsAppearances
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetMostTradesRecord(int number)
        {
            var franchiseStats = await _context.Franchises
                .Select(f => new
                {
                    Franchise = f,
                    TradeCount = f.Teams.SelectMany(t => t.Transactions)
                        .Where(trans => trans.TransactionType == TransactionType.Traded)
                        .Select(trans => trans.TransactionGroupId)
                        .Distinct()
                        .Count()
                })
                .Where(f => f.TradeCount > 0)
                .OrderByDescending(f => f.TradeCount)
                .Take(number)
                .ToListAsync();

            return new LeagueRecords
            {
                RecordTitle = "Most Trades",
                PositiveRecord = true,
                RecordType = RecordType.AllTime,
                Records = franchiseStats.Select((stat, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    FranchiseId = stat.Franchise.FranchiseId,
                    Franchise = stat.Franchise,
                    RecordValue = stat.TradeCount.ToString(),
                    RecordNumericValue = stat.TradeCount
                }).ToList()
            };
        }

        // Player Records

        private async Task<LeagueRecords> GetMostTimesKeptRecord(int number)
        {
            var playerStats = await _context.Players
                .Select(p => new
                {
                    Player = p,
                    TimesKept = p.Transactions.Count(t => t.TransactionType == TransactionType.Kept)
                })
                .Where(p => p.TimesKept > 0)
                .OrderByDescending(p => p.TimesKept)
                .Take(number)
                .ToListAsync();

            return new LeagueRecords
            {
                RecordTitle = "Most Times Kept",
                PositiveRecord = true,
                RecordType = RecordType.Player,
                Records = playerStats.Select((stat, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    PlayerID = stat.Player.PlayerID,
                    Player = stat.Player,
                    RecordValue = stat.TimesKept.ToString(),
                    RecordNumericValue = stat.TimesKept
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetMostTimesTradedRecord(int number)
        {
            var playerStats = await _context.Players
                .Select(p => new
                {
                    Player = p,
                    TimesTraded = p.Transactions.Count(t => t.TransactionType == TransactionType.Traded)
                })
                .Where(p => p.TimesTraded > 0)
                .OrderByDescending(p => p.TimesTraded)
                .Take(number)
                .ToListAsync();

            return new LeagueRecords
            {
                RecordTitle = "Most Times Traded",
                PositiveRecord = true,
                RecordType = RecordType.Player,
                Records = playerStats.Select((stat, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    PlayerID = stat.Player.PlayerID,
                    Player = stat.Player,
                    RecordValue = stat.TimesTraded.ToString(),
                    RecordNumericValue = stat.TimesTraded
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetMostPointsOnBenchRecord(int number)
        {
            try
            {
                Console.WriteLine("Starting GetMostPointsOnBenchRecord...");
                
                // Count total PlayerWeeks first
                var totalPlayerWeeks = await _context.PlayerWeeks.CountAsync();
                var benchPlayerWeeks = await _context.PlayerWeeks.CountAsync(pw => !pw.Started);
                Console.WriteLine($"Total PlayerWeeks: {totalPlayerWeeks}, Bench PlayerWeeks: {benchPlayerWeeks}");

                // Get bench player weeks first without Include to avoid entity framework issues
                var benchPlayerWeekIds = await _context.PlayerWeeks
                    .Where(pw => !pw.Started && pw.TeamId.HasValue)
                    .Select(pw => new { pw.PlayerID, pw.Year, pw.Week })
                    .ToListAsync();

                Console.WriteLine($"Retrieved {benchPlayerWeekIds.Count} bench player week identifiers");

                if (!benchPlayerWeekIds.Any())
                {
                    Console.WriteLine("No bench player weeks found, returning empty record");
                    return new LeagueRecords
                    {
                        RecordTitle = "Most Points on Bench",
                        PositiveRecord = true,
                        RecordType = RecordType.Player,
                        Records = new List<LeagueRecord>()
                    };
                }

                // Get actual player week data and player data separately
                var benchPlayerWeekData = new List<dynamic>();
                
                foreach (var identifier in benchPlayerWeekIds)
                {
                    try
                    {
                        var playerWeek = await _context.PlayerWeeks
                            .FirstOrDefaultAsync(pw => pw.PlayerID == identifier.PlayerID && 
                                                      pw.Year == identifier.Year && 
                                                      pw.Week == identifier.Week);
                                                      
                        var player = await _context.Players
                            .FirstOrDefaultAsync(p => p.PlayerID == identifier.PlayerID);

                        if (playerWeek != null && player != null && !string.IsNullOrEmpty(player.Name))
                        {
                            // Add null checking for all properties before calculating points
                            try 
                            {
                                var points = CalculatePlayerPoints(playerWeek);
                                if (points > 0)
                                {
                                    benchPlayerWeekData.Add(new
                                    {
                                        PlayerID = player.PlayerID,
                                        Player = player,
                                        Points = points
                                    });
                                }
                            }
                            catch (Exception pointsEx)
                            {
                                Console.WriteLine($"Error calculating points for player week {identifier.PlayerID}.{identifier.Year}.{identifier.Week}: {pointsEx.Message}");
                            }
                        }
                        else if (player == null)
                        {
                            Console.WriteLine($"Player not found for PlayerID: {identifier.PlayerID}");
                        }
                        else if (player != null && string.IsNullOrEmpty(player.Name))
                        {
                            Console.WriteLine($"Player {identifier.PlayerID} has empty Name");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing player week {identifier.PlayerID}.{identifier.Year}.{identifier.Week}: {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                        }
                    }
                }

                Console.WriteLine($"Successfully processed {benchPlayerWeekData.Count} bench player weeks with points > 0");

                if (!benchPlayerWeekData.Any())
                {
                    Console.WriteLine("No bench player weeks with points > 0 found, returning empty record");
                    return new LeagueRecords
                    {
                        RecordTitle = "Most Points on Bench",
                        PositiveRecord = true,
                        RecordType = RecordType.Player,
                        Records = new List<LeagueRecord>()
                    };
                }

                var playerStats = new List<dynamic>();
                
                try
                {
                    Console.WriteLine("Starting grouping operation...");
                    
                    // Group by PlayerID and calculate average points per game
                    var groupedStats = benchPlayerWeekData
                        .GroupBy(x => ((dynamic)x).PlayerID)
                        .Select(g => new
                        {
                            Player = g.First().Player, // Get the first Player object from the group
                            TotalBenchPoints = g.Sum(x => (double)((dynamic)x).Points),
                            BenchGames = g.Count(),
                            AvgBenchPoints = g.Sum(x => (double)((dynamic)x).Points) / g.Count()
                        })
                        .OrderByDescending(p => p.AvgBenchPoints)
                        .Take(number)
                        .ToList();
                        
                    playerStats = groupedStats.Cast<dynamic>().ToList();
                    Console.WriteLine($"Successfully grouped into {playerStats.Count} top bench players");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during grouping operation: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    return new LeagueRecords
                    {
                        RecordTitle = "Most Points on Bench (Avg Per Game)",
                        PositiveRecord = true,
                        RecordType = RecordType.Player,
                        Records = new List<LeagueRecord>()
                    };
                }

                Console.WriteLine($"Found {playerStats.Count} top bench players");

                return new LeagueRecords
                {
                    RecordTitle = "Most Points on Bench (Avg Per Game)",
                    PositiveRecord = true,
                    RecordType = RecordType.Player,
                    Records = playerStats.Select((stat, index) => new LeagueRecord
                    {
                        Rank = index + 1,
                        PlayerID = stat.Player.PlayerID,
                        Player = stat.Player,
                        RecordValue = $"{stat.AvgBenchPoints:F2} ({stat.BenchGames} games)",
                        RecordNumericValue = stat.AvgBenchPoints
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMostPointsOnBenchRecord: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                // Return empty record on error to prevent export failure
                return new LeagueRecords
                {
                    RecordTitle = "Most Points on Bench (Avg Per Game)",
                    PositiveRecord = true,
                    RecordType = RecordType.Player,
                    Records = new List<LeagueRecord>()
                };
            }
        }

        private async Task<Dictionary<int, int>> GetRegularSeasonWeekMapping()
        {
            // Get the maximum week number for regular season matches by year
            var regularSeasonWeeks = await _context.Matches
                .Where(m => (int)m.MatchTypeId == (int)MatchType.Regular)
                .GroupBy(m => m.Year)
                .Select(g => new { Year = g.Key, MaxWeek = g.Max(m => m.Week) })
                .ToListAsync();

            return regularSeasonWeeks.ToDictionary(x => x.Year, x => x.MaxWeek);
        }

        private double CalculatePlayerPoints(PlayerWeek playerWeek)
        {
            if (playerWeek == null) return 0;
            
            try 
            {
                return (playerWeek.PassYards / 25.0) + (playerWeek.PassTDs * 4) + 
                       (playerWeek.RushYards / 10.0) + (playerWeek.RushTDs * 6) + 
                       (playerWeek.RecYards / 10.0) + (playerWeek.RecTDs * 6) + 
                       (playerWeek.TwoPointConvert * 2) - (playerWeek.Interceptions * 2) - 
                       (playerWeek.FumblesLost * 2);
            }
            catch (Exception)
            {
                // If any property access fails (Data is Null), return 0
                return 0;
            }
        }

        private async Task<LeagueRecords> GetHighestPointPercentWeekRecord(int number)
        {
            // Get all started player weeks and calculate on client side
            var playerWeekData = await _context.PlayerWeeks
                .Where(pw => pw.Started)
                .Include(pw => pw.Player)
                .ToListAsync();

            var teamScoreData = await _context.TeamScores.ToListAsync();

            var percentageStats = new List<dynamic>();
            
            foreach (var pw in playerWeekData)
            {
                var playerPoints = pw.CalculatePoints();
                var teamScore = teamScoreData.FirstOrDefault(ts => ts.TeamID == pw.TeamId && ts.Year == pw.Year && ts.Week == pw.Week);
                
                if (playerPoints > 0 && teamScore != null && teamScore.Points > 0)
                {
                    percentageStats.Add(new
                    {
                        Player = pw.Player,
                        Year = pw.Year,
                        Week = pw.Week,
                        PointPercentage = (playerPoints / teamScore.Points) * 100
                    });
                }
            }
            
            var topStats = percentageStats
                .OrderByDescending(ps => ((dynamic)ps).PointPercentage)
                .Take(number)
                .ToList();

            return new LeagueRecords
            {
                RecordTitle = "Highest Point Percent Week",
                PositiveRecord = true,
                RecordType = RecordType.Player,
                Records = topStats.Select((stat, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    PlayerID = ((dynamic)stat).Player.PlayerID,
                    Player = ((dynamic)stat).Player,
                    RecordValue = $"{((dynamic)stat).PointPercentage:F1}%",
                    RecordNumericValue = ((dynamic)stat).PointPercentage,
                    Year = ((dynamic)stat).Year,
                    Week = ((dynamic)stat).Week
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetHighestPointPercentSeasonRecord(int number)
        {
            // Get all started player weeks first, then calculate on client side
            var playerWeekData = await _context.PlayerWeeks
                .Where(pw => pw.Started)
                .Include(pw => pw.Player)
                .ToListAsync();

            var teamData = await _context.Teams.ToListAsync();

            // Group by player, year, and team on client side
            var playerSeasons = playerWeekData
                .GroupBy(pw => new { pw.Player.PlayerID, pw.Year, pw.TeamId })
                .Select(g => new
                {
                    Player = g.First().Player,
                    Year = g.Key.Year,
                    TeamId = g.Key.TeamId,
                    PlayerSeasonPoints = g.Sum(pw => pw.CalculatePoints())
                })
                .ToList();

            var percentageStats = new List<dynamic>();
            
            foreach (var ps in playerSeasons)
            {
                var team = teamData.FirstOrDefault(t => t.TeamId == ps.TeamId);
                if (ps.PlayerSeasonPoints > 0 && team != null && team.Points > 0)
                {
                    percentageStats.Add(new
                    {
                        ps.Player,
                        ps.Year,
                        PointPercentage = (ps.PlayerSeasonPoints / team.Points) * 100
                    });
                }
            }
            
            var topStats = percentageStats
                .OrderByDescending(ps => ((dynamic)ps).PointPercentage)
                .Take(number)
                .ToList();

            return new LeagueRecords
            {
                RecordTitle = "Highest Point Percent Season",
                PositiveRecord = true,
                RecordType = RecordType.Player,
                Records = topStats.Select((stat, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    PlayerID = ((dynamic)stat).Player.PlayerID,
                    Player = ((dynamic)stat).Player,
                    RecordValue = $"{((dynamic)stat).PointPercentage:F1}%",
                    RecordNumericValue = ((dynamic)stat).PointPercentage,
                    Year = ((dynamic)stat).Year
                }).ToList()
            };
        }

        // Position-specific player record methods
        private async Task<LeagueRecords> GetMostTimesKeptByPositionRecord(string position, int number)
        {
            var playerStats = await _context.Players
                .Where(p => p.Position != null && p.Position == position)
                .Select(p => new
                {
                    Player = p,
                    TimesKept = p.Transactions.Count(t => t.TransactionType == TransactionType.Kept)
                })
                .Where(p => p.TimesKept > 0)
                .OrderByDescending(p => p.TimesKept)
                .Take(number)
                .ToListAsync();

            return new LeagueRecords
            {
                RecordTitle = $"Most Times Kept - {position}",
                PositiveRecord = true,
                RecordType = RecordType.Player,
                Records = playerStats.Select((stat, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    PlayerID = stat.Player.PlayerID,
                    Player = stat.Player,
                    RecordValue = stat.TimesKept.ToString(),
                    RecordNumericValue = stat.TimesKept
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetMostTimesTradedByPositionRecord(string position, int number)
        {
            var playerStats = await _context.Players
                .Where(p => p.Position != null && p.Position == position)
                .Select(p => new
                {
                    Player = p,
                    TimesTraded = p.Transactions.Count(t => t.TransactionType == TransactionType.Traded)
                })
                .Where(p => p.TimesTraded > 0)
                .OrderByDescending(p => p.TimesTraded)
                .Take(number)
                .ToListAsync();

            return new LeagueRecords
            {
                RecordTitle = $"Most Times Traded - {position}",
                PositiveRecord = true,
                RecordType = RecordType.Player,
                Records = playerStats.Select((stat, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    PlayerID = stat.Player.PlayerID,
                    Player = stat.Player,
                    RecordValue = stat.TimesTraded.ToString(),
                    RecordNumericValue = stat.TimesTraded
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetCareerEarningsRecord(int number)
        {
            // Calculate earnings for each owner using simplified zero-sum logic
            // Based on: Championships*300 + RunnerUps*100 + RegularSeasonWins*100 - Seasons*50
            // Only include teams from finished seasons (Season.Finished = 1)
            // First, get base earnings for each owner
            var ownerBaseEarnings = await _context.Owners
                .Where(o => o.Teams.Any(t => t.Season != null && t.Season.Finished == true)) // Only owners who have teams in finished seasons
                .Select(o => new
                {
                    OwnerName = o.Name,
                    OwnerId = o.OwnerId,
                    // Championships: $300 each (only finished seasons)
                    Championships = o.Teams.Count(t => t.Champion && t.Season != null && t.Season.Finished == true) * 300,
                    // Runner-ups: $100 each (only finished seasons)
                    RunnerUps = o.Teams.Count(t => t.SecondPlace && t.Season != null && t.Season.Finished == true) * 100,
                    // Regular season winners: $100 each (Standing = 1, only finished seasons)
                    RegularSeasonWins = o.Teams.Count(t => t.Standing == 1 && t.Season != null && t.Season.Finished == true) * 100,
                    // Entry fees: $50 per finished season
                    EntryFees = o.Teams.Count(t => t.Season != null && t.Season.Finished == true) * 50,
                    // Base earnings calculation (only finished seasons)
                    BaseEarnings = (o.Teams.Count(t => t.Champion && t.Season != null && t.Season.Finished == true) * 300) + 
                                 (o.Teams.Count(t => t.SecondPlace && t.Season != null && t.Season.Finished == true) * 100) + 
                                 (o.Teams.Count(t => t.Standing == 1 && t.Season != null && t.Season.Finished == true) * 100) - 
                                 (o.Teams.Count(t => t.Season != null && t.Season.Finished == true) * 50)
                })
                .ToListAsync();

            // Calculate weekly bonuses for each owner
            var weeklyBonuses = new Dictionary<Guid, int>();
            
            // Get all finished seasons for weekly calculations
            var finishedSeasons = await _context.Seasons
                .Where(s => s.Finished == true)
                .ToListAsync();

            // Get the regular season week mapping for accurate week counts
            var regularSeasonWeekMapping = await GetRegularSeasonWeekMapping();

            foreach (var season in finishedSeasons)
            {
                // Get regular season weeks for this specific year (exclude playoffs)
                if (!regularSeasonWeekMapping.ContainsKey(season.Year))
                    continue;
                    
                var maxRegularSeasonWeek = regularSeasonWeekMapping[season.Year];
                var regularSeasonWeeks = Enumerable.Range(1, maxRegularSeasonWeek);
                
                foreach (var week in regularSeasonWeeks)
                {
                    // Get all scores for this week in finished seasons only
                    var weekScores = await _context.TeamScores
                        .Include(ts => ts.Team)
                            .ThenInclude(t => t!.Owner)
                        .Where(ts => ts.Year == season.Year && ts.Week == week && ts.Points > 0)
                        .ToListAsync();

                    if (weekScores.Any())
                    {
                        // Weekly high score gets $10
                        var highScore = weekScores.OrderByDescending(ts => ts.Points).First();
                        if (highScore.Team?.Owner?.OwnerId != null)
                        {
                            var ownerId = highScore.Team.Owner.OwnerId;
                            weeklyBonuses[ownerId] = weeklyBonuses.GetValueOrDefault(ownerId, 0) + 10;
                        }

                        // Weekly low score loses $10
                        var lowScore = weekScores.OrderBy(ts => ts.Points).First();
                        if (lowScore.Team?.Owner?.OwnerId != null)
                        {
                            var ownerId = lowScore.Team.Owner.OwnerId;
                            weeklyBonuses[ownerId] = weeklyBonuses.GetValueOrDefault(ownerId, 0) - 10;
                        }
                    }
                }
            }

            // Combine base earnings with weekly bonuses
            var ownerEarnings = ownerBaseEarnings.Select(o => new
            {
                o.OwnerName,
                o.Championships,
                o.RunnerUps,
                o.RegularSeasonWins,
                o.EntryFees,
                WeeklyBonuses = weeklyBonuses.GetValueOrDefault(o.OwnerId, 0),
                TotalEarnings = o.BaseEarnings + weeklyBonuses.GetValueOrDefault(o.OwnerId, 0)
            }).ToList();

            var topEarnings = ownerEarnings
                .OrderByDescending(e => e.TotalEarnings)
                .Take(number)
                .ToList();

            return new LeagueRecords
            {
                RecordTitle = "Career Earnings (All-Time)",
                PositiveRecord = true,
                RecordType = RecordType.AllTime,
                Records = topEarnings.Select((earning, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    RecordValue = $"{earning.OwnerName}: {earning.TotalEarnings:C0}", // Owner name with formatted currency
                    RecordNumericValue = (double)earning.TotalEarnings
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetCareerEarningsLast10YearsRecord(int number)
        {
            var currentYear = DateTime.Now.Year;
            var startYear = currentYear - 10;

            // Calculate earnings for each owner in the last 10 years using simplified zero-sum logic
            // First, get base earnings for each owner in last 10 years
            var ownerBaseEarnings = await _context.Owners
                .Where(o => o.Teams.Any(t => t.Year >= startYear && t.Season != null && t.Season.Finished == true)) // Only owners with teams in last 10 years of finished seasons
                .Select(o => new
                {
                    OwnerName = o.Name,
                    OwnerId = o.OwnerId,
                    // Filter teams to last 10 years and finished seasons
                    TeamsLast10Years = o.Teams.Where(t => t.Year >= startYear && t.Season != null && t.Season.Finished == true),
                    // Championships: $300 each (only finished seasons)
                    Championships = o.Teams.Where(t => t.Year >= startYear && t.Season != null && t.Season.Finished == true).Count(t => t.Champion) * 300,
                    // Runner-ups: $100 each (only finished seasons)
                    RunnerUps = o.Teams.Where(t => t.Year >= startYear && t.Season != null && t.Season.Finished == true).Count(t => t.SecondPlace) * 100,
                    // Regular season winners: $100 each (Standing = 1, only finished seasons)
                    RegularSeasonWins = o.Teams.Where(t => t.Year >= startYear && t.Season != null && t.Season.Finished == true).Count(t => t.Standing == 1) * 100,
                    // Entry fees: $50 per finished season
                    EntryFees = o.Teams.Where(t => t.Year >= startYear && t.Season != null && t.Season.Finished == true).Count() * 50,
                    // Base earnings calculation (only finished seasons)
                    BaseEarnings = (o.Teams.Where(t => t.Year >= startYear && t.Season != null && t.Season.Finished == true).Count(t => t.Champion) * 300) + 
                                 (o.Teams.Where(t => t.Year >= startYear && t.Season != null && t.Season.Finished == true).Count(t => t.SecondPlace) * 100) + 
                                 (o.Teams.Where(t => t.Year >= startYear && t.Season != null && t.Season.Finished == true).Count(t => t.Standing == 1) * 100) - 
                                 (o.Teams.Where(t => t.Year >= startYear && t.Season != null && t.Season.Finished == true).Count() * 50)
                })
                .ToListAsync();

            // Calculate weekly bonuses for each owner in last 10 years
            var weeklyBonuses = new Dictionary<Guid, int>();
            
            // Get finished seasons in the last 10 years for weekly calculations
            var finishedSeasonsLast10Years = await _context.Seasons
                .Where(s => s.Year >= startYear && s.Finished == true)
                .ToListAsync();

            // Get the regular season week mapping for accurate week counts
            var regularSeasonWeekMapping = await GetRegularSeasonWeekMapping();

            foreach (var season in finishedSeasonsLast10Years)
            {
                // Get regular season weeks for this specific year (exclude playoffs)
                if (!regularSeasonWeekMapping.ContainsKey(season.Year))
                    continue;
                    
                var maxRegularSeasonWeek = regularSeasonWeekMapping[season.Year];
                var regularSeasonWeeks = Enumerable.Range(1, maxRegularSeasonWeek);
                
                foreach (var week in regularSeasonWeeks)
                {
                    // Get all scores for this week in finished seasons only
                    var weekScores = await _context.TeamScores
                        .Include(ts => ts.Team)
                            .ThenInclude(t => t!.Owner)
                        .Where(ts => ts.Year == season.Year && ts.Week == week && ts.Points > 0)
                        .ToListAsync();

                    if (weekScores.Any())
                    {
                        // Weekly high score gets $10
                        var highScore = weekScores.OrderByDescending(ts => ts.Points).First();
                        if (highScore.Team?.Owner?.OwnerId != null)
                        {
                            var ownerId = highScore.Team.Owner.OwnerId;
                            weeklyBonuses[ownerId] = weeklyBonuses.GetValueOrDefault(ownerId, 0) + 10;
                        }

                        // Weekly low score loses $10
                        var lowScore = weekScores.OrderBy(ts => ts.Points).First();
                        if (lowScore.Team?.Owner?.OwnerId != null)
                        {
                            var ownerId = lowScore.Team.Owner.OwnerId;
                            weeklyBonuses[ownerId] = weeklyBonuses.GetValueOrDefault(ownerId, 0) - 10;
                        }
                    }
                }
            }

            // Combine base earnings with weekly bonuses
            var ownerEarnings = ownerBaseEarnings.Select(o => new
            {
                o.OwnerName,
                o.Championships,
                o.RunnerUps,
                o.RegularSeasonWins,
                o.EntryFees,
                WeeklyBonuses = weeklyBonuses.GetValueOrDefault(o.OwnerId, 0),
                TotalEarnings = o.BaseEarnings + weeklyBonuses.GetValueOrDefault(o.OwnerId, 0)
            }).ToList();

            var topEarnings = ownerEarnings
                .OrderByDescending(e => e.TotalEarnings)
                .Take(number)
                .ToList();

            return new LeagueRecords
            {
                RecordTitle = "Career Earnings (Last 10 Years)",
                PositiveRecord = true,
                RecordType = RecordType.AllTime,
                Records = topEarnings.Select((earning, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    RecordValue = $"{earning.OwnerName}: {earning.TotalEarnings:C0}", // Owner name with formatted currency
                    RecordNumericValue = (double)earning.TotalEarnings
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetMostPointsOnBenchByPositionRecord(string position, int number)
        {
            try
            {
                Console.WriteLine($"Starting GetMostPointsOnBenchByPositionRecord for {position}...");
                
                // Get bench player weeks for position using individual queries like the main method
                var benchPlayerWeekIds = await _context.PlayerWeeks
                    .Where(pw => !pw.Started && pw.TeamId.HasValue)
                    .Select(pw => new { pw.PlayerID, pw.Year, pw.Week })
                    .ToListAsync();

                Console.WriteLine($"Retrieved {benchPlayerWeekIds.Count} bench player week identifiers for {position}");

                if (!benchPlayerWeekIds.Any())
                {
                    Console.WriteLine($"No bench player weeks found for {position}, returning empty record");
                    return new LeagueRecords
                    {
                        RecordTitle = $"Most Points on Bench ({position}) - Avg Per Game",
                        PositiveRecord = true,
                        RecordType = RecordType.Player,
                        Records = new List<LeagueRecord>()
                    };
                }

                // Get actual player week data and player data separately, filtering by position
                var benchPlayerWeekData = new List<dynamic>();
                
                foreach (var identifier in benchPlayerWeekIds)
                {
                    try
                    {
                        var playerWeek = await _context.PlayerWeeks
                            .FirstOrDefaultAsync(pw => pw.PlayerID == identifier.PlayerID && 
                                                      pw.Year == identifier.Year && 
                                                      pw.Week == identifier.Week);
                                                      
                        var player = await _context.Players
                            .FirstOrDefaultAsync(p => p.PlayerID == identifier.PlayerID && p.Position == position);

                        if (playerWeek != null && player != null && !string.IsNullOrEmpty(player.Name))
                        {
                            // Add null checking for all properties before calculating points
                            try 
                            {
                                var points = CalculatePlayerPoints(playerWeek);
                                if (points > 0)
                                {
                                    benchPlayerWeekData.Add(new
                                    {
                                        PlayerID = player.PlayerID,
                                        Player = player,
                                        Points = points
                                    });
                                }
                            }
                            catch (Exception pointsEx)
                            {
                                Console.WriteLine($"Error calculating points for {position} player week {identifier.PlayerID}.{identifier.Year}.{identifier.Week}: {pointsEx.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing {position} player week {identifier.PlayerID}.{identifier.Year}.{identifier.Week}: {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                        }
                    }
                }

                Console.WriteLine($"Successfully processed {benchPlayerWeekData.Count} bench player weeks with points > 0 for {position}");

                if (!benchPlayerWeekData.Any())
                {
                    Console.WriteLine($"No bench player weeks with points > 0 found for {position}, returning empty record");
                    return new LeagueRecords
                    {
                        RecordTitle = $"Most Points on Bench ({position}) - Avg Per Game",
                        PositiveRecord = true,
                        RecordType = RecordType.Player,
                        Records = new List<LeagueRecord>()
                    };
                }

                var playerStats = new List<dynamic>();
                
                try
                {
                    Console.WriteLine($"Starting grouping operation for {position}...");
                    
                    // Group by PlayerID and calculate average points per game
                    var groupedStats = benchPlayerWeekData
                        .GroupBy(x => ((dynamic)x).PlayerID)
                        .Select(g => new
                        {
                            Player = g.First().Player, // Get the first Player object from the group
                            TotalBenchPoints = g.Sum(x => (double)((dynamic)x).Points),
                            BenchGames = g.Count(),
                            AvgBenchPoints = g.Sum(x => (double)((dynamic)x).Points) / g.Count()
                        })
                        .OrderByDescending(p => p.AvgBenchPoints)
                        .Take(number)
                        .ToList();
                        
                    playerStats = groupedStats.Cast<dynamic>().ToList();
                    Console.WriteLine($"Successfully grouped into {playerStats.Count} top bench players for {position}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during grouping operation for {position}: {ex.Message}");
                    return new LeagueRecords
                    {
                        RecordTitle = $"Most Points on Bench ({position}) - Avg Per Game",
                        PositiveRecord = true,
                        RecordType = RecordType.Player,
                        Records = new List<LeagueRecord>()
                    };
                }

                return new LeagueRecords
                {
                    RecordTitle = $"Most Points on Bench ({position}) - Avg Per Game",
                    PositiveRecord = true,
                    RecordType = RecordType.Player,
                    Records = playerStats.Select((stat, index) => new LeagueRecord
                    {
                        Rank = index + 1,
                        PlayerID = stat.Player.PlayerID,
                        Player = stat.Player,
                        RecordValue = $"{stat.AvgBenchPoints:F2} ({stat.BenchGames} games)",
                        RecordNumericValue = stat.AvgBenchPoints
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMostPointsOnBenchByPositionRecord for {position}: {ex.Message}");
                // Return empty record on error to prevent export failure
                return new LeagueRecords
                {
                    RecordTitle = $"Most Points on Bench ({position}) - Avg Per Game",
                    PositiveRecord = true,
                    RecordType = RecordType.Player,
                    Records = new List<LeagueRecord>()
                };
            }
        }

        private async Task<LeagueRecords> GetHighestPointPercentWeekByPositionRecord(string position, int number)
        {
            var playerWeeks = await _context.PlayerWeeks
                .Where(pw => pw.Started == true && pw.TeamId.HasValue && pw.Player.Position != null && pw.Player.Position == position)
                .Join(_context.TeamScores,
                      pw => new { TeamId = pw.TeamId.Value, pw.Year, pw.Week },
                      ts => new { TeamId = ts.TeamID, ts.Year, ts.Week },
                      (pw, ts) => new
                      {
                          pw.Player,
                          pw.Year,
                          pw.Week,
                          PlayerPoints = (pw.PassYards / 25.0) + (pw.PassTDs * 4) + (pw.RushYards / 10.0) + (pw.RushTDs * 6) + (pw.RecYards / 10.0) + (pw.RecTDs * 6) + (pw.TwoPointConvert * 2) - (pw.Interceptions * 2) - (pw.FumblesLost * 2),
                          TeamPoints = ts.Points
                      })
                .Where(x => x.PlayerPoints > 0 && x.TeamPoints > 0)
                .Select(x => new
                {
                    x.Player,
                    x.Year,
                    x.Week,
                    PointPercentage = (x.PlayerPoints / x.TeamPoints) * 100
                })
                .OrderByDescending(x => x.PointPercentage)
                .Take(number)
                .ToListAsync();

            return new LeagueRecords
            {
                RecordTitle = $"Highest Point Percent (Week) - {position}",
                PositiveRecord = true,
                RecordType = RecordType.Player,
                Records = playerWeeks.Select((stat, index) => new LeagueRecord
                {
                    Rank = index + 1,
                    PlayerID = stat.Player.PlayerID,
                    Player = stat.Player,
                    RecordValue = $"{stat.PointPercentage:F1}%",
                    RecordNumericValue = stat.PointPercentage,
                    Year = stat.Year,
                    Week = stat.Week
                }).ToList()
            };
        }

        private async Task<LeagueRecords> GetHighestPointPercentSeasonByPositionRecord(string position, int number)
        {
            try
            {
                // Get all started player weeks for this position first, then calculate on client side
                var playerWeekData = await _context.PlayerWeeks
                    .Where(pw => pw.Started)
                    .Include(pw => pw.Player)
                    .ToListAsync();

                // Filter by position on client side
                var positionPlayerWeeks = playerWeekData
                    .Where(pw => pw.Player != null && pw.Player.Position == position)
                    .ToList();

                var teamData = await _context.Teams.ToListAsync();

                // Group by player, year, and team on client side
                var playerSeasons = positionPlayerWeeks
                    .GroupBy(pw => new { pw.Player.PlayerID, pw.Year, pw.TeamId })
                    .Select(g => new
                    {
                        Player = g.First().Player,
                        Year = g.Key.Year,
                        TeamId = g.Key.TeamId,
                        PlayerSeasonPoints = g.Sum(pw => pw.CalculatePoints())
                    })
                    .ToList();

                var percentageStats = new List<dynamic>();
                
                foreach (var ps in playerSeasons)
                {
                    var team = teamData.FirstOrDefault(t => t.TeamId == ps.TeamId);
                    if (ps.PlayerSeasonPoints > 0 && team != null && team.Points > 0)
                    {
                        percentageStats.Add(new
                        {
                            ps.Player,
                            ps.Year,
                            PointPercentage = (ps.PlayerSeasonPoints / team.Points) * 100
                        });
                    }
                }
                
                var topStats = percentageStats
                    .OrderByDescending(ps => ((dynamic)ps).PointPercentage)
                    .Take(number)
                    .ToList();

                return new LeagueRecords
                {
                    RecordTitle = $"Highest Point Percent (Season) - {position}",
                    PositiveRecord = true,
                    RecordType = RecordType.Player,
                    Records = topStats.Select((stat, index) => new LeagueRecord
                    {
                        Rank = index + 1,
                        PlayerID = ((dynamic)stat).Player.PlayerID,
                        Player = ((dynamic)stat).Player,
                        RecordValue = $"{((dynamic)stat).PointPercentage:F1}%",
                        RecordNumericValue = ((dynamic)stat).PointPercentage,
                        Year = ((dynamic)stat).Year
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                // Return empty record on error to prevent export failure
                return new LeagueRecords
                {
                    RecordTitle = $"Highest Point Percent (Season) - {position}",
                    PositiveRecord = true,
                    RecordType = RecordType.Player,
                    Records = new List<LeagueRecord>()
                };
            }
        }
    }
}