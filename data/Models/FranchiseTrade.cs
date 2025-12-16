using System;
using System.Collections.Generic;
using System.Text;

namespace FantasyArchive.Data.Models
{
    public class FranchiseTrade
    {
        public Guid TradeId { get; set; }
        public DateTime Date { get; set; }
        public Franchise Franchise { get; set; }
        public Franchise TradedWith { get; set; }
        public IEnumerable<Transaction> TradedAwayTransactions { get; set; }
        public IEnumerable<Transaction> TradedForTransactions { get; set; }
    }
}
