using System;
using System.Collections.Generic;
using System.Text;

namespace FantasyArchive.Data.Models
{
    public class PlayerData
    {
        public Player Player { get; set; }
        public IEnumerable<Transaction> Transactions { get; set; }
        public IEnumerable<DraftPick> DraftPicks { get; set; }
    }
}
