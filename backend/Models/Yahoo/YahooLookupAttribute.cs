using System;
using System.Collections.Generic;
using System.Text;

namespace FantasyArchive.Api.Models.Yahoo
{
    public class YahooLookupAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
