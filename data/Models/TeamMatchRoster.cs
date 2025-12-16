using System;
using System.Collections.Generic;



namespace FantasyArchive.Data.Models
{
    public partial class TeamMatchRoster
    {
        public Guid TeamID { get; set; }
        public int Week { get; set; }
        public int PlayerID { get; set; }
        public string Position { get; set; }
    }
}
