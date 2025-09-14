using System.Collections.Generic;

namespace FantasyArchive.Data.Models
{
    public class LeagueRecords
    {
        public string RecordTitle { get; set; } = string.Empty;
        public bool PositiveRecord { get; set; }
        public RecordType RecordType { get; set; }
        public List<LeagueRecord> Records { get; set; } = new List<LeagueRecord>();
    }
}