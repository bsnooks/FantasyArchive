using System;
using System.Collections.Generic;

namespace FantasyArchive.Data.Models
{
    public partial class DraftPick
    {
        public DraftPick()
        {
            Transactions = new HashSet<Transaction>();
        }

        public Guid DraftPickId { get; set; }
        public Guid DraftId { get; set; }
        public Guid TeamId { get; set; }
        public int Round { get; set; }
        public int Pick { get; set; }
        public int PlayerID { get; set; }
        public int PositionPick { get; set; }

        public virtual Draft Draft { get; set; }
        public virtual Player Player { get; set; }
        public virtual Team Team { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
