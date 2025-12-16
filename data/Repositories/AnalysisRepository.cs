using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FantasyArchive.Data.Repositories
{
    public class AnalysisRepository
    {
        private readonly Func<FantasyArchiveContext> dbFunc;

        public AnalysisRepository(Func<FantasyArchiveContext> dbFunc)
        {
            this.dbFunc = dbFunc;
        }

        public async Task<IDictionary<string, string[]>> GetTeamStrength(
            int? startYear = null, int? endYear = null, bool champion = false, int? standing = 1)
        {
            using (var dbContext = dbFunc())
            {
                var applicablePositions = new[] { "QB", "WR", "RB", "TE" };
                IDictionary<string, string[]> teamStrength = new Dictionary<string, string[]>();

                try
                {
                    var queryResults = await dbContext.PlayerSeasons
                        .Where(ps => ((!champion && ps.EndTeam.Standing <= standing) ||
                            (champion && ps.EndTeam.Champion)) &&
                            (startYear == null || ps.Year >= startYear) &&
                            (endYear == null || ps.Year <= endYear) &&
                            applicablePositions.Contains(ps.Player.PrimaryPosition))
                        .Include(ps => ps.Player)
                        .Include(ps => ps.EndTeam)
                        .ThenInclude(t => t.Franchise)
                        .ToListAsync();

                    var winningPlayers = queryResults.OrderBy(ps => ps.Player.PrimaryPosition)
                        .GroupBy(ps => ps.Player.PrimaryPosition)
                        .ToDictionary(g => g.Key,
                            g => g.OrderBy(ps => ps.Year)
                                .GroupBy(ps => ps.Year).ToDictionary(
                                    sg => sg.Key,
                                    sg => sg.OrderBy(ps => ps.PositionRank).ToList()));

                    foreach (var positionGroup in winningPlayers)
                    {
                        var position = positionGroup.Key;
                        int passes = position == "WR" ? 4 : position == "TE" ? 2 : 3;
                        var thing = new[] { 0, 0, 0, 0 };
                        var applicableCount = new[] { 0, 0, 0, 0 };
                        var raw = new IList<int>[] { new List<int>(), new List<int>(), new List<int>(), new List<int>() };
                        foreach (var yearGroup in positionGroup.Value)
                        {
                            var year = yearGroup.Key;
                            for (int i = 0; i < Math.Min(passes, yearGroup.Value.Count); i++)
                            {
                                if (yearGroup.Value[i].PositionRank > 0)
                                {
                                    thing[i] += yearGroup.Value[i].PositionRank;
                                    raw[i].Add(yearGroup.Value[i].PositionRank);
                                    applicableCount[i]++;
                                }
                            }
                        }

                        var data = new string?[4];
                        for (int i = 0; i < passes; i++)
                        {
                            data[i] = $"{(applicableCount[i] > 0 ? (thing[i] / applicableCount[i]) : (int?)null)} ({string.Join(", ", raw[i])})";
                        }

                        teamStrength.Add(position, data);
                    }
                }
                catch (Exception e)
                {
                    var d = e;
                }

                return teamStrength;

            }
        }
    }
}
