using System;
using System.Collections.Generic;

namespace FantasyArchive.Data.Models
{
    public class Owner
    {
        public Guid FranchiceId { get; set; } // Note: keeping the typo from original model
        public Guid OwnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? Active { get; set; }

        public virtual Franchise Franchice { get; set; } = null!; // Note: keeping the typo from original model
        public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
    }
}