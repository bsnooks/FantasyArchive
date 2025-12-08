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

            // Season Records
            records.Add(await GetBestSeasonRecordRecord(number));
            records.Add(await GetMostSeasonPointsRecord(number));
            records.Add(await GetWorstSeasonRecordRecord(number));

            // Weekly Records
            var weeklyRecords = await _weeklyRecordsService.GetAllTimeWeeklyRecordsAsync();
            records.AddRange(ConvertWeeklyRecordsToLeagueRecords(weeklyRecords, null));

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
                    PlayerId = pw.PlayerID,
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
                    PlayerId = pw.PlayerID,
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
                    PlayerId = pw.PlayerID,
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
                    RecordType = year.HasValue ? RecordType.Season : RecordType.AllTime,
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
                    RecordType = year.HasValue ? RecordType.Season : RecordType.AllTime,
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
                    RecordType = year.HasValue ? RecordType.Season : RecordType.AllTime,
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
                    RecordType = year.HasValue ? RecordType.Season : RecordType.AllTime,
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
                    RecordType = year.HasValue ? RecordType.Season : RecordType.AllTime,
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
                    RecordType = year.HasValue ? RecordType.Season : RecordType.AllTime,
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
                    RecordType = year.HasValue ? RecordType.Season : RecordType.AllTime,
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
    }
}