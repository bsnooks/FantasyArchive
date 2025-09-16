using System;
using System.Collections.Generic;

namespace FantasyArchive.Data.Models
{
    public class TransactionGroup
    {
        public Guid TransactionGroupId { get; set; }
        public DateTime Date { get; set; }
        
        // Navigation properties
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}