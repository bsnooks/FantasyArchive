using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace FantasyArchive.Api.Models.Yahoo
{

    public enum SortOrder
    {
        [XmlEnum("0")] Asc = 0,
        [XmlEnum("1")] Desc = 1
    }


    [XmlRoot(ElementName = "stat", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
    public class YahooStat
    {
        [XmlElement(ElementName = "stat_id", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public string StatIdStr { get; set; }
        [XmlIgnore]
        public int StatId
        {
            get
            {
                if (int.TryParse(StatIdStr, out var value))
                {
                    return value;
                }

                return 0;
            }
        }
        [XmlElement(ElementName = "name", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public string Name { get; set; }
        [XmlElement(ElementName = "display_name", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public string DisplayName { get; set; }
        [XmlElement(ElementName = "sort_order", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public SortOrder SortOrder { get; set; }
        [XmlElement(ElementName = "value", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public string ValueStr { get; set; }
        [XmlIgnore]
        public int Value
        {
            get
            {
                if (int.TryParse(ValueStr, out var value))
                {
                    return value;
                }

                return 0;
            }
        }
    }

    [XmlRoot(ElementName = "stats", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
    public class YahooStats
    {
        [XmlElement(ElementName = "stat", Namespace = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng")]
        public List<YahooStat> Stat { get; set; }
    }
}
