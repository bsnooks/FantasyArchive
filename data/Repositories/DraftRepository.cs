using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyArchive.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FantasyArchive.Data.Repositories
{
    public class DraftRepository
    {

        private readonly Func<FantasyArchiveContext> dbFunc;

        public DraftRepository(Func<FantasyArchiveContext> dbFunc)
        {
            this.dbFunc = dbFunc;
        }

        public async Task<IEnumerable<Draft>> GetAll()
        {
            using (var dbContext = dbFunc())
            {
                return await dbContext.Drafts
                .OrderBy(x => x.Year)
                .ToListAsync();
            }
        }

        public async Task<Draft> GetOne(Guid id)
        {
            using (var dbContext = dbFunc())
            {
                return await dbContext.Drafts
                .SingleOrDefaultAsync(x => x.DraftId == id);
            }
        }

        public async Task<Draft> GetOneByYear(int year)
        {
            using (var dbContext = dbFunc())
            {
                return await dbContext.Drafts
                .SingleOrDefaultAsync(x => x.Year == year);
            }
        }

        public async Task<Guid> CreateDraft(Draft newDraft)
        {
            using (var dbContext = dbFunc())
            {
                await dbContext.AddAsync(newDraft);
                await dbContext.SaveChangesAsync();
            }

            return newDraft.DraftId;
        }
    }
}
