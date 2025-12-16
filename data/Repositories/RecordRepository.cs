using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FantasyArchive.Data.Models;

namespace FantasyArchive.Data.Repositories
{
    public class RecordRepository
    {
        private readonly Func<FantasyArchiveContext> dbFunc;

        public RecordRepository(Func<FantasyArchiveContext> dbFunc)
        {
            this.dbFunc = dbFunc;
        }

        public async Task<IEnumerable<LeagueRecords>> GetLeagueRecords(Guid leagueId, int number, bool? recordPositivity = null, RecordType? recordType = null)
        {
            using (var dbContext = dbFunc())
            {
                IList<LeagueRecords> recordCollection = new List<LeagueRecords>();

                if (!recordPositivity.HasValue || recordPositivity.Value)
                {
                    if (!recordType.HasValue || recordType == RecordType.Franchise)
                    {
                        var matches = await dbContext.Matches
                                    .Where(x => x.LeagueID == leagueId &&
                                        !x.Tied &&
                                        x.MatchTypeId == MatchType.Regular)
                                    .Include(x => x.WinningTeam).ThenInclude(x => x.Franchise)
                                    .OrderBy(x => x.Year)
                                    .ThenBy(x => x.Week)
                                    .ToListAsync();

                        IList<Tuple<Franchise, int, Tuple<int, int>, Tuple<int, int>>> tuples = new List<Tuple<Franchise, int, Tuple<int, int>, Tuple<int, int>>>();
                        var groups = matches.GroupBy(x => x.WinningTeam.Franchise);
                        foreach (var group in groups)
                        {
                            int lastYear = 0;
                            int lastWeek = 0;
                            int startWeek = 0;
                            int startYear = 0;
                            int wins = 0;
                            foreach (var match in group)
                            {
                                if ((match.Week == (lastWeek + 1) && match.Year == lastYear) ||
                                    (lastWeek == 14 && match.Week == 1 && match.Year == (lastYear + 1)))
                                {
                                    wins++;
                                }
                                else
                                {
                                    if (wins > 5)
                                    {
                                        tuples.Add(
                                            new Tuple<Franchise, int, Tuple<int, int>, Tuple<int, int>>(
                                                group.Key,
                                                wins,
                                                new Tuple<int, int>(startYear, startWeek),
                                                new Tuple<int, int>(match.Year, match.Week)));
                                    }
                                    wins = 1;
                                    startWeek = match.Week;
                                    startYear = match.Year;
                                }

                                lastWeek = match.Week;
                                lastYear = match.Year;
                            }

                            if (wins > 5)
                            {
                                tuples.Add(
                                    new Tuple<Franchise, int, Tuple<int, int>, Tuple<int, int>>(
                                        group.Key,
                                        wins,
                                        new Tuple<int, int>(startYear, startWeek),
                                        new Tuple<int, int>(lastYear, lastWeek)));
                            }
                        }

                        LeagueRecords record = new LeagueRecords()
                        {
                            RecordTitle = "Longest Win Streak",
                            PositiveRecord = true,
                            RecordType = RecordType.Franchise,
                            Records = new List<LeagueRecord>()
                        };
                        int rank = 1;
                        foreach (var tuple in tuples.OrderByDescending(x => x.Item2).Take(number))
                        {
                            record.Records.Add(new LeagueRecord()
                            {
                                Rank = rank++,
                                Franchise = tuple.Item1,
                                RecordValue = $"{tuple.Item2} ({tuple.Item3.Item1} Week {tuple.Item3.Item2} - {tuple.Item4.Item1} Week {tuple.Item4.Item2})",
                                RecordNumericValue = tuple.Item2
                            });
                        }

                        IList<FranchiseStats> franchiseStats = await dbContext.Franchises
                                    .Where(x => x.LeagueID == leagueId)
                                    .Include(x => x.Teams)
                                    .Select(x => new FranchiseStats()
                                    {
                                        Franchise = x,
                                        Wins = x.Teams.Sum(t => t.Wins),
                                        Loses = x.Teams.Sum(t => t.Loses),
                                        Ties = x.Teams.Sum(t => t.Ties),
                                        Points = x.Teams.Sum(t => t.Points),
                                        Championships = x.Teams.Count(t => t.Champion),
                                        WinningSeasons = x.Teams.Count(t => t.Wins > t.Loses)
                                    })
                                    .ToListAsync();

                        recordCollection.Add(
                            CreateFranchiseRecord("Most Championships",
                                franchiseStats.OrderByDescending(x => x.Championships).ThenByDescending(x => x.Wins).Take(number).ToList(),
                                recordValueType: FranchiseRecordValueType.Championships));

                        recordCollection.Add(
                            CreateFranchiseRecord("Most Winning Seasons",
                                franchiseStats.OrderByDescending(x => x.WinningSeasons).ThenByDescending(x => x.Wins).Take(number).ToList(),
                                recordValueType: FranchiseRecordValueType.WinningSeasons));

                        recordCollection.Add(
                            CreateFranchiseRecord("Most Wins",
                                franchiseStats.OrderByDescending(x => x.Wins).ThenByDescending(x => x.Championships).Take(number).ToList(),
                                recordValueType: FranchiseRecordValueType.Wins));

                        recordCollection.Add(
                            CreateFranchiseRecord("Most Franchise Points",
                                franchiseStats.OrderByDescending(x => x.Points).ThenByDescending(x => x.Championships).Take(number).ToList(),
                                recordValueType: FranchiseRecordValueType.Points));

                        recordCollection.Add(record);

                        recordCollection.Add(
                            CreateFranchiseRecord("Most Times Player Kept",
                                dbContext.Transactions
                                    .Where(x => x.TransactionType == TransactionType.Kept)
                                    .Include(x => x.Team).ThenInclude(x => x.Franchise)
                                    .Include(x => x.Player)
                                    // this makes me sad.
                                    .ToList()
                                    .GroupBy(x => new { x.PlayerID, x.Player.Name, x.Team.Franchise })
                                    .Select(x => new
                                    {
                                        Franchise = x.Key.Franchise,
                                        Name = x.Key.Name,
                                        Count = x.Count()
                                    })
                                    .OrderByDescending(x => x.Count)
                                    .Take(number)
                                    .Select(x => new FranchiseStats()
                                    {
                                        Franchise = x.Franchise,
                                        OtherRecord = $"{x.Name} ({x.Count})",
                                        OtherRecordValue = x.Count
                                    })
                                    .ToList(),
                                recordValueType: FranchiseRecordValueType.OtherRecord));
                    }

                    if (!recordType.HasValue || recordType == RecordType.Season)
                    {
                        recordCollection.Add(
                            CreateTeamRecord("Best Record",
                                await dbContext.Teams
                                    .Where(x => x.LeagueID == leagueId)
                                    .OrderByDescending(x => x.Wins)
                                    .ThenByDescending(x => x.Ties)
                                    .ThenByDescending(x => x.Points)
                                    .Include(x => x.Franchise)
                                    .Take(number)
                                    .ToListAsync(),
                                true,
                                RecordType.Season));

                        recordCollection.Add(
                            CreateTeamRecord("Season Points",
                                await dbContext.Teams
                                    .Where(x => x.LeagueID == leagueId)
                                    .OrderByDescending(x => x.Points)
                                    .Include(x => x.Franchise)
                                    .Take(number)
                                    .ToListAsync(),
                                true,
                                RecordType.Season,
                                TeamRecordValueType.Points));

                        recordCollection.Add(
                            CreateTeamRecord("Least Points (when making playoffs)",
                                await dbContext.Teams
                                    .Where(x => x.LeagueID == leagueId &&
                                    x.Standing <= 4 &&
                                    x.Season.Finished.HasValue &&
                                    x.Season.Finished.Value)
                                    .OrderBy(x => x.Points)
                                    .Include(x => x.Franchise)
                                    .Take(number)
                                    .ToListAsync(),
                                true,
                                RecordType.Season,
                                TeamRecordValueType.Points));

                        recordCollection.Add(
                            CreateTeamRecord("Best Record (without making playoffs)",
                                await dbContext.Teams
                                    .Where(x => x.LeagueID == leagueId && x.Standing > 4)
                                    .OrderByDescending(x => x.Wins)
                                    .ThenByDescending(x => x.Ties)
                                    .ThenByDescending(x => x.Points)
                                    .Include(x => x.Franchise)
                                    .Take(number)
                                    .ToListAsync(),
                                true,
                                RecordType.Season));

                        recordCollection.Add(
                            CreateTeamRecord("Most Season Points (without making playoffs)",
                                await dbContext.Teams
                                    .Where(x => x.LeagueID == leagueId && x.Standing > 4)
                                    .OrderByDescending(x => x.Points)
                                    .Include(x => x.Franchise)
                                    .Take(number)
                                    .ToListAsync(),
                                true,
                                RecordType.Season,
                                TeamRecordValueType.Points));
                    }

                    if (!recordType.HasValue || recordType == RecordType.Match)
                    {
                        recordCollection.Add(
                            CreateTeamScoreRecord("Most Points",
                                await dbContext.TeamScores
                                    .Where(x => x.LeagueID == leagueId)
                                    .OrderByDescending(x => x.Points)
                                    .Include(x => x.Team)
                                    .ThenInclude(x => x.Franchise)
                                    .Take(number)
                                    .ToListAsync(),
                                true,
                                RecordType.Match));

                        recordCollection.Add(
                            CreateMatchResultRecord("Biggest Blowouts",
                                await dbContext.Matches
                                    .Where(x => x.LeagueID == leagueId)
                                    .Select(x => new MatchResult
                                    {
                                        Year = x.Year,
                                        Week = x.Week,
                                        Winner = x.WinningTeam.Franchise,
                                        WinnerPoints = x.WinningTeam.TeamScores.FirstOrDefault(s => s.Week == x.Week && s.Year == x.Year).Points,
                                        Loser = x.LosingTeam.Franchise,
                                        LoserPoints = x.LosingTeam.TeamScores.FirstOrDefault(s => s.Week == x.Week && s.Year == x.Year).Points,
                                        Tied = x.Tied
                                    })
                                    .OrderByDescending(x => x.WinnerPoints - x.LoserPoints)
                                    .ThenBy(x => x.Year)
                                    .Take(number)
                                    .ToListAsync()));

                        recordCollection.Add(
                            CreateMatchResultRecord("Closest Wins",
                                await dbContext.Matches
                                    .Where(x => x.LeagueID == leagueId)
                                    .Select(x => new MatchResult
                                    {
                                        Year = x.Year,
                                        Week = x.Week,
                                        Winner = x.WinningTeam.Franchise,
                                        WinnerPoints = x.WinningTeam.TeamScores.FirstOrDefault(s => s.Week == x.Week && s.Year == x.Year).Points,
                                        Loser = x.LosingTeam.Franchise,
                                        LoserPoints = x.LosingTeam.TeamScores.FirstOrDefault(s => s.Week == x.Week && s.Year == x.Year).Points,
                                        Tied = x.Tied
                                    })
                                    .OrderBy(x => x.WinnerPoints - x.LoserPoints)
                                    .ThenBy(x => x.Year)
                                    .Take(number)
                                    .ToListAsync()));

                        recordCollection.Add(
                            CreateMatchResultRecord("Highest Combined Points",
                                await dbContext.Matches
                                    .Where(x => x.LeagueID == leagueId)
                                    .Select(x => new MatchResult
                                    {
                                        Year = x.Year,
                                        Week = x.Week,
                                        Winner = x.WinningTeam.Franchise,
                                        WinnerPoints = x.WinningTeam.TeamScores.FirstOrDefault(s => s.Week == x.Week && s.Year == x.Year).Points,
                                        Loser = x.LosingTeam.Franchise,
                                        LoserPoints = x.LosingTeam.TeamScores.FirstOrDefault(s => s.Week == x.Week && s.Year == x.Year).Points,
                                        Tied = x.Tied
                                    })
                                    .OrderByDescending(x => x.WinnerPoints + x.LoserPoints)
                                    .ThenBy(x => x.Year)
                                    .Take(number)
                                    .ToListAsync(),
                                recordValueType: MatchResultRecordType.Combined));

                        recordCollection.Add(
                            CreateMatchResultRecord("Lowest Combined Points",
                                await dbContext.Matches
                                    .Where(x => x.LeagueID == leagueId)
                                    .Select(x => new MatchResult
                                    {
                                        Year = x.Year,
                                        Week = x.Week,
                                        Winner = x.WinningTeam.Franchise,
                                        WinnerPoints = x.WinningTeam.TeamScores.FirstOrDefault(s => s.Week == x.Week && s.Year == x.Year).Points,
                                        Loser = x.LosingTeam.Franchise,
                                        LoserPoints = x.LosingTeam.TeamScores.FirstOrDefault(s => s.Week == x.Week && s.Year == x.Year).Points,
                                        Tied = x.Tied
                                    })
                                    .OrderBy(x => x.WinnerPoints + x.LoserPoints)
                                    .ThenBy(x => x.Year)
                                    .Take(number)
                                    .ToListAsync(),
                                recordValueType: MatchResultRecordType.Combined));

                        recordCollection.Add(
                            CreateMatchResultRecord("Lowest Point Win",
                                await dbContext.Matches
                                    .Where(x => x.LeagueID == leagueId)
                                    .Select(x => new MatchResult
                                    {
                                        Year = x.Year,
                                        Week = x.Week,
                                        Winner = x.WinningTeam.Franchise,
                                        WinnerPoints = x.WinningTeam.TeamScores.FirstOrDefault(s => s.Week == x.Week && s.Year == x.Year).Points,
                                        Loser = x.LosingTeam.Franchise,
                                        LoserPoints = x.LosingTeam.TeamScores.FirstOrDefault(s => s.Week == x.Week && s.Year == x.Year).Points,
                                        Tied = x.Tied
                                    })
                                    .OrderBy(x => x.WinnerPoints)
                                    .ThenBy(x => x.Year)
                                    .Take(number)
                                    .ToListAsync(),
                                recordValueType: MatchResultRecordType.Winner));

                        recordCollection.Add(
                            CreateMatchResultRecord("Highest Point Loss",
                                await dbContext.Matches
                                    .Where(x => x.LeagueID == leagueId)
                                    .Select(x => new MatchResult
                                    {
                                        Year = x.Year,
                                        Week = x.Week,
                                        Winner = x.WinningTeam.Franchise,
                                        WinnerPoints = x.WinningTeam.TeamScores.FirstOrDefault(s => s.Week == x.Week && s.Year == x.Year).Points,
                                        Loser = x.LosingTeam.Franchise,
                                        LoserPoints = x.LosingTeam.TeamScores.FirstOrDefault(s => s.Week == x.Week && s.Year == x.Year).Points,
                                        Tied = x.Tied
                                    })
                                    .OrderByDescending(x => x.LoserPoints)
                                    .ThenBy(x => x.Year)
                                    .Take(number)
                                    .ToListAsync(),
                                recordValueType: MatchResultRecordType.Loser));

                        recordCollection.Add(
                            CreateTeamScoreRecord("High Projection Exceeded",
                                await dbContext.TeamScores
                                    .Where(x => x.LeagueID == leagueId && x.ProjectedPoints.HasValue && x.ProjectedPoints.Value != 0)
                                    .OrderByDescending(x => x.Points - x.ProjectedPoints)
                                    .Include(x => x.Team)
                                    .ThenInclude(x => x.Franchise)
                                    .Take(number)
                                    .ToListAsync(),
                                true,
                                RecordType.Match,
                                TeamScoreRecordValueType.ProjectedPointsDifference));

                        recordCollection.Add(
                            CreateTeamScoreRecord("Highest Projection",
                                await dbContext.TeamScores
                                    .Where(x => x.LeagueID == leagueId)
                                    .OrderByDescending(x => x.ProjectedPoints)
                                    .Include(x => x.Team)
                                    .ThenInclude(x => x.Franchise)
                                    .Take(number)
                                    .ToListAsync(),
                                true,
                                RecordType.Match,
                                TeamScoreRecordValueType.ProjectedPoints));
                    }

                    if (!recordType.HasValue || recordType == RecordType.Player)
                    {
                        string[] keyPositions = new[] { "QB", "RB", "WR", "TE" };

                        foreach (var position in keyPositions)
                        {
                            recordCollection.Add(
                                CreatePlayerRecord($"Most Kept {position}",
                                await dbContext.Players
                                    .Where(x => x.PrimaryPosition == position)
                                    .Select(x => new PlayerData
                                    {
                                        Player = x,
                                        Transactions = x.Transactions.Where(t => t.TransactionType == TransactionType.Kept)
                                    })
                                    .OrderByDescending(x => x.Transactions.Count())
                                    .Take(number)
                                    .ToListAsync()));
                        }

                        recordCollection.Add(
                            CreatePlayerRecord("Most Traded",
                            await dbContext.Players
                                .Select(x => new PlayerData
                                {
                                    Player = x,
                                    Transactions = x.Transactions.Where(t => t.TransactionType == TransactionType.Traded)
                                })
                                .OrderByDescending(x => x.Transactions.Count())
                                .Take(number)
                                .ToListAsync()));

                        recordCollection.Add(
                            CreatePlayerRecord("Most Drafted",
                            await dbContext.Players
                                .Where(x => keyPositions.Contains(x.PrimaryPosition))
                                .Select(x => new PlayerData
                                {
                                    Player = x,
                                    Transactions = x.Transactions.Where(t => t.TransactionType == TransactionType.DraftPicked)
                                })
                                .OrderByDescending(x => x.Transactions.Count())
                                .Take(number)
                                .ToListAsync()));

                        recordCollection.Add(
                            CreatePlayerRecord("Most Drafted in the 1st Round",
                            await dbContext.Players
                                .Where(x => keyPositions.Contains(x.PrimaryPosition))
                                .Select(x => new PlayerData
                                {
                                    Player = x,
                                    DraftPicks = x.DraftPicks.Where(d => d.Round == 1),
                                })
                                .OrderByDescending(x => x.DraftPicks.Count())
                                .Take(number)
                                .ToListAsync(),
                                recordValueType: PlayerRecordType.DraftPicks));

                        recordCollection.Add(
                            CreatePlayerRecord("Most Added",
                            await dbContext.Players
                                .Where(x => keyPositions.Contains(x.PrimaryPosition))
                                .Select(x => new PlayerData
                                {
                                    Player = x,
                                    Transactions = x.Transactions.Where(t => t.TransactionType == TransactionType.Added)
                                })
                                .OrderByDescending(x => x.Transactions.Count())
                                .Take(number)
                                .ToListAsync()));

                        recordCollection.Add(
                            CreatePlayerRecord("Most Dropped",
                            await dbContext.Players
                                .Where(x => keyPositions.Contains(x.PrimaryPosition))
                                .Select(x => new PlayerData
                                {
                                    Player = x,
                                    Transactions = x.Transactions.Where(t => t.TransactionType == TransactionType.Dropped)
                                })
                                .OrderByDescending(x => x.Transactions.Count())
                                .Take(number)
                                .ToListAsync()));
                    }
                    if (!recordType.HasValue || recordType == RecordType.PlayerStats)
                    {
                        string[] keyPositions = new[] { "QB", "RB", "WR", "TE" };

                        foreach (var position in keyPositions)
                        {
                            try
                            {
                                recordCollection.Add(
                                    CreatePlayerWeekStatRecord($"Best Week {position}",
                                        await dbContext.PlayerWeeks
                                            .Where(x => x.Player.PrimaryPosition == position)
                                            .Include(x => x.Player)
                                            .OrderByDescending(x => x.PassYards / 25.0 + x.PassTDs * 4 + x.RushYards / 10.0 + x.RushTDs * 6 + x.RecYards / 10.0 + x.RecTDs * 6)
                                            .Take(number)
                                            .ToListAsync()));
                            }
                            catch (Exception ex)
                            {
                                var d = ex;
                            }
                        }
                    }
                }

                if (!recordPositivity.HasValue || !recordPositivity.Value)
                {
                    if (!recordType.HasValue || recordType == RecordType.Franchise)
                    {
                    }
                    if (!recordType.HasValue || recordType == RecordType.Season)
                    {
                        recordCollection.Add(
                            CreateTeamRecord("Worst Record",
                                await dbContext.Teams
                                    .Where(x => x.LeagueID == leagueId)
                                    .OrderByDescending(x => x.Loses)
                                    .ThenBy(x => x.Ties)
                                    .ThenBy(x => x.Points)
                                    .Include(x => x.Franchise)
                                    .Take(number)
                                    .ToListAsync(),
                                false,
                                RecordType.Season));
                    }
                    if (!recordType.HasValue || recordType == RecordType.Match)
                    {
                        recordCollection.Add(
                        CreateTeamScoreRecord("Least Points",
                            await dbContext.TeamScores
                                .Where(x => x.LeagueID == leagueId)
                                .OrderBy(x => x.Points)
                                .Include(x => x.Team)
                                .ThenInclude(x => x.Franchise)
                                .Take(number)
                                .ToListAsync(),
                            false,
                            RecordType.Match));

                        recordCollection.Add(
                            CreateTeamScoreRecord("Lowest Projection",
                                await dbContext.TeamScores
                                    .Where(x => x.LeagueID == leagueId && x.ProjectedPoints.HasValue && x.ProjectedPoints.Value != 0)
                                    .OrderBy(x => x.ProjectedPoints)
                                    .Include(x => x.Team)
                                    .ThenInclude(x => x.Franchise)
                                    .Take(number)
                                    .ToListAsync(),
                                false,
                                RecordType.Match,
                                TeamScoreRecordValueType.ProjectedPoints));
                    }

                    if (!recordType.HasValue || recordType == RecordType.Player)
                    {
                    }
                }

                return recordCollection;
            }
        }

        private LeagueRecords CreateFranchiseRecord(string title, ICollection<FranchiseStats> franchisesStats,
            FranchiseRecordValueType recordValueType = FranchiseRecordValueType.Points,
            bool positiveRecord = true)
        {
            LeagueRecords record = new LeagueRecords()
            {
                RecordTitle = title,
                PositiveRecord = positiveRecord,
                RecordType = RecordType.Franchise,
                Records = new List<LeagueRecord>()
            };

            var count = 1;
            foreach (var franchiseStats in franchisesStats)
            {
                string recordValue = "";
                double recordNumericValue;
                switch (recordValueType)
                {
                    case FranchiseRecordValueType.Championships:
                        recordValue = franchiseStats.Championships.ToString();
                        recordNumericValue = franchiseStats.Championships;
                        break;
                    case FranchiseRecordValueType.WinningSeasons:
                        recordValue = franchiseStats.WinningSeasons.ToString();
                        recordNumericValue = franchiseStats.WinningSeasons;
                        break;
                    case FranchiseRecordValueType.Loses:
                        recordValue = franchiseStats.Loses.ToString();
                        recordNumericValue = franchiseStats.Loses;
                        break;
                    case FranchiseRecordValueType.Wins:
                        recordValue = franchiseStats.Wins.ToString();
                        recordNumericValue = franchiseStats.Wins;
                        break;
                    case FranchiseRecordValueType.Points:
                        recordValue = franchiseStats.Points.ToString("0.##");
                        recordNumericValue = franchiseStats.Points;
                        break;
                    case FranchiseRecordValueType.OtherRecord:
                    default:
                        recordValue = franchiseStats.OtherRecord;
                        recordNumericValue = franchiseStats.OtherRecordValue;
                        break;
                }

                record.Records.Add(new LeagueRecord()
                {
                    Rank = count++,
                    Franchise = franchiseStats.Franchise,
                    RecordValue = recordValue,
                    RecordNumericValue = recordNumericValue
                });
            }
            return record;
        }

        private LeagueRecords CreateTeamRecord(string title, ICollection<Team> teams,
            bool positiveRecord = true,
            RecordType recordType = RecordType.Match,
            TeamRecordValueType recordValueType = TeamRecordValueType.Record)
        {
            LeagueRecords record = new LeagueRecords()
            {
                RecordTitle = title,
                PositiveRecord = positiveRecord,
                RecordType = recordType,
                Records = new List<LeagueRecord>()
            };

            var count = 1;
            foreach (var team in teams)
            {
                string recordValue = "";
                double recordNumericValue;
                switch (recordValueType)
                {
                    case TeamRecordValueType.Loses:
                        recordValue = team.Loses.ToString();
                        recordNumericValue = team.Loses;
                        break;
                    case TeamRecordValueType.Wins:
                        recordValue = team.Wins.ToString();
                        recordNumericValue = team.Wins;
                        break;
                    case TeamRecordValueType.Points:
                        recordValue = team.Points.ToString("0.##");
                        recordNumericValue = team.Points;
                        break;
                    case TeamRecordValueType.Record:
                    default:
                        recordValue = $"{team.Wins}-{team.Loses}-{team.Ties}";
                        recordNumericValue = team.Wins;
                        break;
                }

                record.Records.Add(new LeagueRecord()
                {
                    Rank = count++,
                    Franchise = team.Franchise,
                    RecordValue = recordValue,
                    RecordNumericValue = recordNumericValue,
                    Year = team.Year
                });
            }
            return record;
        }

        private LeagueRecords CreateTeamScoreRecord(string title, ICollection<TeamScore> scores,
            bool positiveRecord = true,
            RecordType recordType = RecordType.Match,
            TeamScoreRecordValueType recordValueType = TeamScoreRecordValueType.Points)
        {
            LeagueRecords record = new LeagueRecords()
            {
                RecordTitle = title,
                PositiveRecord = positiveRecord,
                RecordType = recordType,
                Records = new List<LeagueRecord>()
            };

            var count = 1;
            foreach (var score in scores)
            {
                string recordValue;
                double recordNumericValue;
                switch (recordValueType)
                {
                    case TeamScoreRecordValueType.ProjectedPoints:
                        recordValue = score.ProjectedPoints.Value.ToString("0.##");
                        recordNumericValue = score.ProjectedPoints.Value;
                        break;
                    case TeamScoreRecordValueType.ProjectedPointsDifference:
                        recordValue = $"{(score.Points - score.ProjectedPoints.Value).ToString("0.##")} ({score.Points.ToString("0.##") } vs {score.ProjectedPoints.Value.ToString("0.##")})";
                        recordNumericValue = score.Points - score.ProjectedPoints.Value;
                        break;
                    case TeamScoreRecordValueType.Points:
                    default:
                        recordValue = score.Points.ToString("0.##");
                        recordNumericValue = score.Points;
                        break;
                }

                record.Records.Add(new LeagueRecord()
                {
                    Rank = count++,
                    Franchise = score.Team.Franchise,
                    RecordValue = recordValue,
                    RecordNumericValue = recordNumericValue,
                    Year = score.Year,
                    Week = score.Week
                });
            }
            return record;
        }

        private LeagueRecords CreateMatchResultRecord(string title, ICollection<MatchResult> results,
            bool positiveRecord = true,
            MatchResultRecordType recordValueType = MatchResultRecordType.Difference,
            RecordType recordType = RecordType.Match)
        {
            LeagueRecords record = new LeagueRecords()
            {
                RecordTitle = title,
                PositiveRecord = positiveRecord,
                RecordType = recordType,
                Records = new List<LeagueRecord>()
            };

            var count = 1;
            foreach (var result in results)
            {
                string recordValue;
                double recordNumericValue;
                switch (recordValueType)
                {
                    case MatchResultRecordType.Winner:
                        recordValue = result.WinnerPoints.ToString("0.##");
                        recordNumericValue = result.WinnerPoints;
                        break;

                    case MatchResultRecordType.Loser:
                        recordValue = result.LoserPoints.ToString("0.##");
                        recordNumericValue = result.LoserPoints;
                        break;

                    case MatchResultRecordType.Combined:
                        recordValue = $"{(result.WinnerPoints + result.LoserPoints).ToString("0.##")} ({result.WinnerPoints.ToString("0.##")} - {result.LoserPoints.ToString("0.##")})";
                        recordNumericValue = result.WinnerPoints + result.LoserPoints;
                        break;

                    case MatchResultRecordType.Difference:
                    default:
                        recordValue = $"{(result.WinnerPoints - result.LoserPoints).ToString("0.##")} ({result.WinnerPoints.ToString("0.##")} - {result.LoserPoints.ToString("0.##")})";
                        recordNumericValue = result.WinnerPoints - result.LoserPoints;
                        break;
                }

                record.Records.Add(new LeagueRecord()
                {
                    Rank = count++,
                    Franchise = recordValueType != MatchResultRecordType.Loser ? result.Winner : result.Loser,
                    OtherFranchise = recordValueType != MatchResultRecordType.Loser ? result.Loser : result.Winner,
                    RecordValue = recordValue,
                    RecordNumericValue = recordNumericValue,
                    Year = result.Year,
                    Week = result.Week
                });
            }
            return record;
        }

        private LeagueRecords CreatePlayerRecord(string title, ICollection<PlayerData> players,
            bool positiveRecord = true,
            PlayerRecordType recordValueType = PlayerRecordType.Transactions,
            RecordType recordType = RecordType.Player)
        {
            LeagueRecords record = new LeagueRecords()
            {
                RecordTitle = title,
                PositiveRecord = positiveRecord,
                RecordType = recordType,
                Records = new List<LeagueRecord>()
            };

            var count = 1;
            foreach (var player in players)
            {
                string recordValue;
                double recordNumericValue;
                switch (recordValueType)
                {
                    case PlayerRecordType.DraftPicks:
                        recordValue = player.DraftPicks.Count().ToString();
                        recordNumericValue = player.DraftPicks.Count();
                        break;

                    case PlayerRecordType.Transactions:
                    default:
                        recordValue = player.Transactions.Count().ToString();
                        recordNumericValue = player.Transactions.Count();
                        break;
                }

                record.Records.Add(new LeagueRecord()
                {
                    Rank = count++,
                    Player = player.Player,
                    RecordValue = recordValue,
                    RecordNumericValue = recordNumericValue
                });
            }
            return record;
        }
        private LeagueRecords CreatePlayerWeekStatRecord(string title, ICollection<PlayerWeek> weeks,
            bool positiveRecord = true,
            RecordType recordType = RecordType.PlayerStats)
        {
            LeagueRecords record = new LeagueRecords()
            {
                RecordTitle = title,
                PositiveRecord = positiveRecord,
                RecordType = recordType,
                Records = new List<LeagueRecord>()
            };
            var count = 1;
            foreach (var week in weeks)
            {
                record.Records.Add(new LeagueRecord()
                {
                    Rank = count++,
                    Player = week.Player,
                    RecordValue = $"{week.Points.ToString("#.##")} ({week.Year} - Week {week.Week})",
                    RecordNumericValue = week.Points
                });
            }

            return record;
        }

        private enum FranchiseRecordValueType
        {
            Championships,
            WinningSeasons,
            Wins,
            Loses,
            Points,
            Trades,
            OtherRecord
        }

        private enum TeamRecordValueType
        {
            Record,
            Wins,
            Loses,
            Points,
            Trades
        }

        private enum TeamScoreRecordValueType
        {
            Points,
            ProjectedPoints,
            ProjectedPointsDifference
        }

        private enum MatchResultRecordType
        {
            Difference,
            Combined,
            Winner,
            Loser
        }

        private enum PlayerRecordType
        {
            Transactions,
            DraftPicks,
            Stats
        }
    }
}
