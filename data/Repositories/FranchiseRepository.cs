using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyArchive.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FantasyArchive.Data.Repositories
{
    public class FranchiseRepository
    {

        private readonly FantasyArchiveContext dbContext;

        public FranchiseRepository(FantasyArchiveContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<Franchise>> GetAll()
        {
            return await dbContext.Franchises
                .Include(x => x.Teams)
                .ToListAsync();
        }


        public async Task<Franchise> GetOne(Guid id)
        {
            return await dbContext.Franchises
                .Include(x => x.Teams)
                .ThenInclude(x => x.Transactions)
                .Include(x => x.Teams)
                .ThenInclude(x => x.PlayerSeasons)
                .ThenInclude(x => x.Player)
                .SingleOrDefaultAsync(x => x.FranchiseId == id);
        }

        public async Task<Franchise> GetOneByName(string name)
        {
            return await dbContext.Franchises
                .Include(x => x.Teams)
                .ThenInclude(x => x.Transactions)
                .Include(x => x.Teams)
                .ThenInclude(x => x.PlayerSeasons)
                .ThenInclude(x => x.Player)
                .SingleOrDefaultAsync(x => x.MainName == name);
        }

        public async Task<IEnumerable<Legend>> GetTeamLegends(Franchise franchise, string? position = null, bool started = true, int limit = 15)
        {
            try
            {
                var allWeeks = await dbContext.PlayerWeeks
                    .Where(pw => pw.Started == started && pw.Team.FranchiseId == franchise.FranchiseId && (position == null || pw.Player.PrimaryPosition == position))
                    .Include(x => x.Player)
                    .ToListAsync();

                return allWeeks
                    .GroupBy(pw => pw.PlayerID)
                    .OrderByDescending(g => g.Sum(x => x.PassYards / 25.0 + x.PassTDs * 4 + x.RushYards / 10.0 + x.RushTDs * 6 + x.RecYards / 10.0 + x.RecTDs * 6))
                    .Take(limit)
                    .Select(g => new Legend()
                    {
                        Years = g.Select(x => x.Year).Distinct().ToList(),
                        GamesPlayed = g.Count(),
                        Player = g.FirstOrDefault().Player,
                        Points = g.Sum(x => x.Points)
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return Enumerable.Empty<Legend>();
        }

        public async Task<IEnumerable<Legend>> GetAllFranchiseLegends(Franchise franchise)
        {
            var QBs = await GetTeamLegends(franchise, "QB", limit: 2);
            var RBs = await GetTeamLegends(franchise, "RB", limit: 2);
            var WRs = await GetTeamLegends(franchise, "WR", limit: 3);
            var TEs = await GetTeamLegends(franchise, "TE", limit: 1);

            return new[]
            {
                QBs.ElementAt(0),
                QBs.ElementAt(1),
                RBs.ElementAt(0),
                RBs.ElementAt(1),
                WRs.ElementAt(0),
                WRs.ElementAt(1),
                WRs.ElementAt(2),
                TEs.ElementAt(0)
            };
        }
    }
}