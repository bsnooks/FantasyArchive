using System;

namespace FantasyArchive.Data.Models
{
    public class Transaction
    {
        public Guid TransactionId { get; set; }
        public Guid? TransactionGroupId { get; set; }
        public Guid TeamId { get; set; }
        public TransactionType TransactionType { get; set; }
        public int PlayerID { get; set; }
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public Guid? DraftPickId { get; set; }
        public int Year { get; set; }
        public int PlayerTransactionIndex { get; set; }


        // Navigation properties
        public virtual TransactionGroup? TransactionGroup { get; set; }
        public virtual Team? Team { get; set; }
        public virtual Player? Player { get; set; }
        public virtual DraftPick? DraftPick { get; set; }
    }
}