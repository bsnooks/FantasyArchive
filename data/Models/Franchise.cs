using System;
using System.Collections.Generic;

namespace FantasyArchive.Data.Models
{
    public class Franchise
    {
        public Guid FranchiseId { get; set; }
        public Guid LeagueId { get; set; }
        public string MainName { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
        public virtual ICollection<Owner> Owners { get; set; } = new List<Owner>();
    }
}