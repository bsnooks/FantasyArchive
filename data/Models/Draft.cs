using System;
using System.Collections.Generic;



namespace FantasyArchive.Data.Models
{
    public partial class Draft
    {
        public Draft()
        {
            DraftPicks = new HashSet<DraftPick>();
        }

        public Guid DraftId { get; set; }
        public Guid LeagueID { get; set; }
        public int Year { get; set; }
        public DateTime? Date { get; set; }
        public bool Snake { get; set; }
        public int Rounds { get; set; }

        public virtual League League { get; set; }
        public virtual Season Season { get; set; }
        public virtual ICollection<DraftPick> DraftPicks { get; set; }
    }
}
