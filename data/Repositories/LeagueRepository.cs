using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FantasyArchive.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FantasyArchive.Data.Repositories
{
    public class LeagueRepository
    {

        private readonly FantasyArchiveContext dbContext;

        public LeagueRepository(FantasyArchiveContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<League>> GetAll()
        {
            return await dbContext.Leagues
                .Include(l => l.Franchises)
                .ToListAsync();
        }

        public async Task<League> GetOne(Guid id)
        {
            return await dbContext.Leagues
                .Include(l => l.Franchises)
                .ThenInclude(f => f.Teams)
                .SingleOrDefaultAsync(p => p.LeagueID == id);
        }

        public async Task<League> GetOneByName(string name)
        {
            return await dbContext.Leagues
                .Include(l => l.Franchises)
                .ThenInclude(f => f.Teams)
                .SingleOrDefaultAsync(p => p.Name == name);
        }
    }
}
