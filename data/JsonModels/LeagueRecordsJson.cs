using System.Collections.Generic;

namespace FantasyArchive.Data.JsonModels
{
    public class LeagueRecordsJson
    {
        public string RecordTitle { get; set; } = string.Empty;
        public bool PositiveRecord { get; set; }
        public string RecordType { get; set; } = string.Empty;
        public List<LeagueRecordJson> Records { get; set; } = new List<LeagueRecordJson>();
    }
}