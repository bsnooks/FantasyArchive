using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyArchive.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FantasyArchive.Data.Repositories
{
    public class FranchiseTradeRepository
    {
        private readonly Func<FantasyArchiveContext> dbFunc;

        public FranchiseTradeRepository(Func<FantasyArchiveContext> dbFunc)
        {
            this.dbFunc = dbFunc;
        }

        public async Task<IEnumerable<FranchiseTrade>> GetFranchiseTrades(int limit, int offset, Guid? franchiseId = null, int? year = null)
        {
            using (var dbContext = dbFunc())
            {
                var tradeGroups = await dbContext.TransactionGroups
                    .Where(x => x.Transactions.Any(t => t.TransactionType == TransactionType.Traded
                        && (!franchiseId.HasValue || t.Team.FranchiseId == franchiseId)
                        && (!year.HasValue || t.Date.Year == year)))
                    .Include(x => x.Transactions).ThenInclude(x => x.Team).ThenInclude(x => x.Franchise)
                    .Include(x => x.Transactions).ThenInclude(x => x.Player)
                    .OrderByDescending(x => x.Date)
                    .Skip(offset)
                    .Take(limit)
                    .ToListAsync();

                return tradeGroups.Select(x =>
                {
                    var firstFranchise = x.Transactions.Where(t => !franchiseId.HasValue || t.Team.FranchiseId == franchiseId).First().Team.Franchise;
                    return new FranchiseTrade()
                    {
                        TradeId = x.TransactionGroupId,
                        Date = x.Date,
                        Franchise = firstFranchise,
                        TradedWith = x.Transactions.Where(t => t.Team.FranchiseId != firstFranchise.FranchiseId).Select(t => t.Team.Franchise).FirstOrDefault(),
                        TradedAwayTransactions = x.Transactions.Where(t => t.Team.FranchiseId != firstFranchise.FranchiseId),
                        TradedForTransactions = x.Transactions.Where(t => t.Team.FranchiseId == firstFranchise.FranchiseId),
                    };
                });
            }
        }

        public async Task<FranchiseTrade> GetFranchiseTrade(Guid tradeId)
        {
            using (var dbContext = dbFunc())
            {
                var tradeGroup = await dbContext.TransactionGroups
                    .Where(x => x.TransactionGroupId == tradeId)
                    .Include(x => x.Transactions).ThenInclude(x => x.Team).ThenInclude(x => x.Franchise)
                    .Include(x => x.Transactions).ThenInclude(x => x.Player).ThenInclude(x => x.PlayerSeasons)
                    .Include(x => x.Transactions).ThenInclude(x => x.Player).ThenInclude(x => x.Transactions)
                    .OrderByDescending(x => x.Date)
                    .SingleOrDefaultAsync();

                if (tradeGroup != null)
                {
                    var franchise = tradeGroup.Transactions.First().Team.Franchise;
                    return new FranchiseTrade()
                    {
                        TradeId = tradeGroup.TransactionGroupId,
                        Date = tradeGroup.Date,
                        Franchise = franchise,
                        TradedWith = tradeGroup.Transactions.Where(t => t.Team.FranchiseId != franchise.FranchiseId).Select(t => t.Team.Franchise).FirstOrDefault(),
                        TradedAwayTransactions = tradeGroup.Transactions.Where(t => t.Team.FranchiseId != franchise.FranchiseId),
                        TradedForTransactions = tradeGroup.Transactions.Where(t => t.Team.FranchiseId == franchise.FranchiseId),
                    };
                }
                return null;
            }
        }
    }
}
