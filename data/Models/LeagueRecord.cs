using System;

namespace FantasyArchive.Data.Models
{
    public class LeagueRecord
    {
        public int Rank { get; set; }
        public Guid? FranchiseId { get; set; }
        public Guid? OtherFranchiseId { get; set; }
        public int? PlayerID { get; set; }
        public string RecordValue { get; set; } = string.Empty;
        public double RecordNumericValue { get; set; }
        public int? Year { get; set; }
        public int? Week { get; set; }
        
        // Navigation properties
        public virtual Franchise? Franchise { get; set; }
        public virtual Franchise? OtherFranchise { get; set; }
        public virtual Player? Player { get; set; }
    }
}