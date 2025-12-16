
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace FantasyArchive.Api.Models.Yahoo
{
    [XmlRoot(ElementName = "team_logo")]
    public class TeamLogo
    {
        [XmlElement(ElementName = "size")]
        public string Size { get; set; }
        [XmlElement(ElementName = "url")]
        public string Url { get; set; }
    }

    [XmlRoot(ElementName = "team_logos")]
    public class TeamLogos
    {
        [XmlElement(ElementName = "team_logo")]
        public TeamLogo TeamLogo { get; set; }
        [XmlElement(ElementName = "size", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public string Size { get; set; }
        [XmlElement(ElementName = "url", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public string Url { get; set; }
    }

    [XmlRoot(ElementName = "roster_adds")]
    public class RosterAdds
    {
        [XmlElement(ElementName = "coverage_type")]
        public string CoverageType { get; set; }
        [XmlElement(ElementName = "coverage_value")]
        public string CoverageValue { get; set; }
        [XmlElement(ElementName = "value")]
        public string Value { get; set; }
    }

    public abstract class TeamBase
    {
        [XmlElement(ElementName = "team_key")]
        public string TeamKey { get; set; }
        [XmlElement(ElementName = "team_id")]
        public int TeamId { get; set; }
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "is_owned_by_current_login")]
        public bool IsOwnedByCurrentLogin { get; set; }
        [XmlElement(ElementName = "url")]
        public string Url { get; set; }
        [XmlElement(ElementName = "team_logos")]
        public TeamLogos TeamLogos { get; set; }
        [XmlElement(ElementName = "waiver_priority")]
        public string WaiverPriority { get; set; }
        [XmlElement(ElementName = "faab_balance")]
        public string FaabBalance { get; set; }
        [XmlElement(ElementName = "number_of_moves")]
        public string NumberOfMoves { get; set; }
        [XmlElement(ElementName = "number_of_trades")]
        public string NumberOfTrades { get; set; }
        [XmlElement(ElementName = "roster_adds")]
        public RosterAdds RosterAdds { get; set; }
        [XmlElement(ElementName = "league_scoring_type")]
        public string LeagueScoringType { get; set; }
        [XmlElement(ElementName = "has_draft_grade")]
        public bool HasDraftGrade { get; set; }
        [XmlElement(ElementName = "draft_grade")]
        public string DraftGrade { get; set; }
        [XmlElement(ElementName = "draft_recap_url")]
        public string DraftRecapUrl { get; set; }
        [XmlElement(ElementName = "clinched_playoffs")]
        public bool ClinchedPlayoffs { get; set; }
    }

    [YahooLookup(Name="team")]
    [XmlRoot(ElementName = "team", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
    public class YahooTeam : TeamBase
    {

        [XmlElement(ElementName = "roster")]
        public Roster Roster { get; set; }


        [XmlElement(ElementName = "team_points")]
        public TeamPoints TeamPoints { get; set; }
        [XmlElement(ElementName = "team_standings")]
        public TeamStandings TeamStandings { get; set; }
    }

    [XmlRoot(ElementName = "teams")]
    public class TeamList
    {
        [XmlElement(ElementName = "team")]
        public List<YahooTeam> Teams { get; set; }
        [XmlAttribute(AttributeName = "count")]
        public string Count { get; set; }
    }


    [XmlRoot(ElementName = "roster", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
    public class Roster
    {
        [XmlElement(ElementName = "coverage_type", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public string CoverageType { get; set; }
        [XmlElement(ElementName = "week", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public string Week { get; set; }
        [XmlElement(ElementName = "is_editable", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public string IsEditable { get; set; }
        [XmlElement(ElementName = "players", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public YahooPlayerList PlayerList { get; set; }
    }

    [XmlRoot(ElementName = "team_points")]
    public class TeamPoints
    {
        [XmlElement(ElementName = "coverage_type")]
        public string CoverageType { get; set; }
        [XmlElement(ElementName = "season")]
        public string Season { get; set; }
        [XmlElement(ElementName = "total")]
        public string Total { get; set; }
    }

    [XmlRoot(ElementName = "outcome_totals")]
    public class OutcomeTotals
    {
        [XmlElement(ElementName = "wins")]
        public int Wins { get; set; }
        [XmlElement(ElementName = "losses")]
        public int Losses { get; set; }
        [XmlElement(ElementName = "ties")]
        public int Ties { get; set; }
        [XmlElement(ElementName = "percentage")]
        public string Percentage { get; set; }
    }

    [XmlRoot(ElementName = "streak")]
    public class Streak
    {
        [XmlElement(ElementName = "type")]
        public string Type { get; set; }
        [XmlElement(ElementName = "value")]
        public string Value { get; set; }
    }

    [XmlRoot(ElementName = "team_standings")]
    public class TeamStandings
    {
        [XmlElement(ElementName = "rank")]
        public int Rank { get; set; }
        [XmlElement(ElementName = "playoff_seed")]
        public int PlayoffSeed { get; set; }
        [XmlElement(ElementName = "outcome_totals")]
        public OutcomeTotals OutcomeTotals { get; set; }
        [XmlElement(ElementName = "streak")]
        public Streak Streak { get; set; }
        [XmlElement(ElementName = "points_for")]
        public double PointsFor { get; set; }
        [XmlElement(ElementName = "points_against")]
        public double PointsAgainst { get; set; }
    }


    [XmlRoot(ElementName = "standings")]
    public class Standings
    {
        [XmlElement(ElementName = "teams")]
        public TeamList TeamList { get; set; }
    }

}
