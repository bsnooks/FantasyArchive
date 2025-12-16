using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyArchive.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FantasyArchive.Data.Repositories
{
    public class SeasonRepository
    {

        private readonly Func<FantasyArchiveContext> dbFunc;

        public SeasonRepository(Func<FantasyArchiveContext> dbFunc)
        {
            this.dbFunc = dbFunc;
        }

        public async Task<Season> GetSeason(int year)
        {
            using (var dbContext = dbFunc())
            {
                return await dbContext.Seasons
                    .Include(x => x.Teams)
                    .ThenInclude(x => x.Franchise)
                    .SingleOrDefaultAsync(x => x.Year == year);
            }
        }

        public async Task<IEnumerable<Season>> GetSeasons(int? year = null, bool? finished = null)
        {
            using (var dbContext = dbFunc())
            {
                return await dbContext.Seasons
                    .Where(s => (!year.HasValue || s.Year == year.Value) &&
                    (!finished.HasValue || s.Finished == finished.Value)).ToListAsync();
            }
        }

        public async Task UpdateSeason(Season season)
        {
            using (var dbContext = dbFunc())
            {
                dbContext.Seasons.Update(season);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
