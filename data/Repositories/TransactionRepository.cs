using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyArchive.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FantasyArchive.Data.Repositories
{
    public class TransactionRepository
    {

        private readonly Func<FantasyArchiveContext> dbFunc;

        public TransactionRepository(Func<FantasyArchiveContext> dbFunc)
        {
            this.dbFunc = dbFunc;
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(int limit, int offset, Guid? franchiseId = null, int? playerId = null, TransactionType? type = null, int? year = null)
        {
            using (var dbContext = dbFunc())
            {
                return await dbContext.Transactions
                    .Where(x => (!franchiseId.HasValue || x.Team.FranchiseId == franchiseId)
                        && (!playerId.HasValue || x.PlayerID == playerId)
                        && (!type.HasValue || x.TransactionType == type)
                        && (!year.HasValue || x.Team.Year == year))
                    .Include(x => x.Team.Franchise)
                    .Include(x => x.Player)
                    .ThenInclude(x => x.PlayerSeasons)
                    .OrderByDescending(x => x.Date)
                    .Skip(offset)
                    .Take(limit)
                    .ToListAsync();
            }
        }

        public async Task<Transaction> GetTransaction(Guid id)
        {
            using (var dbContext = dbFunc())
            {
                return await dbContext.Transactions
                    .Where(x => x.TransactionId == id)
                    .Include(x => x.Team.Franchise)
                    .Include(x => x.Player)
                    .FirstOrDefaultAsync();
            }
        }

        public async Task<TransactionGroup> GetTransactionGroup(Guid groupId)
        {
            using (var dbContext = dbFunc())
            {
                return await dbContext.TransactionGroups
                    .Where(x => x.TransactionGroupId == groupId)
                    .Include(x => x.Transactions)
                    .ThenInclude(x => x.Team.Franchise)
                    .Include(x => x.Transactions)
                    .ThenInclude(x => x.Player)
                    .FirstOrDefaultAsync();
            }
        }

        public async Task<IEnumerable<Transaction>> GetRelatedTransactions(Guid id, Guid? groupId)
        {
            if (groupId == null)
            {
                return Enumerable.Empty<Transaction>();
            }

            using (var dbContext = dbFunc())
            {
                return await dbContext.Transactions
                    .Where(x => x.TransactionId != id && x.TransactionGroupId == groupId)
                    .Include(x => x.Team.Franchise)
                    .Include(x => x.Player)
                    .ThenInclude(x => x.PlayerSeasons)
                    .OrderByDescending(x => x.Date)
                    .ThenBy(x => x.TeamId)
                    .ToListAsync();
            }
        }

        public async Task<Transaction?> GetSubsequentTransaction(Transaction transaction)
        {
            using (var dbContext = dbFunc())
            {
                IList<Transaction> related = new List<Transaction>();
                var potential = await dbContext.Transactions
                    .Where(x => x.TransactionId != transaction.TransactionId &&
                        x.PlayerID == transaction.PlayerID &&
                        x.Date >= transaction.Date)
                    .Include(x => x.Team.Franchise)
                    .Include(x => x.Player)
                    .ThenInclude(x => x.PlayerSeasons)
                    .OrderBy(x => x.Date)
                    .ToListAsync();


                foreach (var p in potential)
                {
                    bool lastTransaction = false;
                    switch (p.TransactionType)
                    {
                        case TransactionType.Kept:
                        case TransactionType.Dropped:
                        case TransactionType.Traded:
                            return p;
                        case TransactionType.Added:
                        case TransactionType.DraftPicked:
                            lastTransaction = true;
                            break;
                        case TransactionType.VetoedTrade:
                            break;
                    }

                    if (lastTransaction)
                    {
                        break;
                    }

                    if (p.Team.FranchiseId != transaction.Team.FranchiseId)
                    {
                        break;
                    }
                }

                return null;
            }
        }

        public async Task<IEnumerable<Transaction>> GetSubsequentTransactions(Transaction transaction)
        {
            using (var dbContext = dbFunc())
            {
                IList<Transaction> related = new List<Transaction>();
                var potential = await dbContext.Transactions
                    .Where(x => x.TransactionId != transaction.TransactionId &&
                        x.PlayerID == transaction.PlayerID &&
                        x.Date >= transaction.Date)
                    .Include(x => x.Team.Franchise)
                    .Include(x => x.Player)
                    .ThenInclude(x => x.PlayerSeasons)
                    .OrderBy(x => x.Date)
                    .ToListAsync();


                foreach (var p in potential)
                {
                    bool lastTransaction = false;
                    switch(p.TransactionType)
                    {
                        case TransactionType.Kept:
                            related.Add(p);
                            break;
                        case TransactionType.Dropped:
                        case TransactionType.Traded:
                            related.Add(p);
                            lastTransaction = true;
                            break;
                        case TransactionType.Added:
                        case TransactionType.DraftPicked:
                            lastTransaction = true;
                            break;
                        case TransactionType.VetoedTrade:
                            break;
                    }

                    if (lastTransaction)
                    {
                        break;
                    }

                    if (p.Team.FranchiseId != transaction.Team.FranchiseId)
                    {
                        break;
                    }
                }

                return related;
            }
        }

        public async Task<Guid> CreateTransaction(Transaction newTransaction)
        {
            using (var dbContext = dbFunc())
            {
                await dbContext.AddAsync(newTransaction);
                await dbContext.SaveChangesAsync();
            }

            return newTransaction.TransactionId;
        }

        public async Task<int> CreateTransactions(IEnumerable<Transaction> newTransactions)
        {
            using (var dbContext = dbFunc())
            {
                await dbContext.AddRangeAsync(newTransactions);
                await dbContext.SaveChangesAsync();
            }

            return newTransactions.Count();
        }

        public async Task<Guid> CreateTransactionGroup(TransactionGroup newTransactionGroup)
        {
            using (var dbContext = dbFunc())
            {
                await dbContext.AddAsync(newTransactionGroup);
                await dbContext.SaveChangesAsync();
            }

            return newTransactionGroup.TransactionGroupId;
        }

        public async Task<int> CreateTransactionGroups(IEnumerable<TransactionGroup> newTransactionGroups)
        {
            using (var dbContext = dbFunc())
            {
                await dbContext.AddRangeAsync(newTransactionGroups);
                await dbContext.SaveChangesAsync();
            }

            return newTransactionGroups.Count();
        }
    }
}
