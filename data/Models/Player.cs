using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace FantasyArchive.Data.Models
{
    [Table("Player")]
    public class Player
    {
        [Key]
        [Column("PlayerID")]
        public int PlayerID { get; set; }
        
        [Column("Name")]
        [StringLength(100)]
        public string? Name { get; set; }
        
        [Column("Position")]
        [StringLength(10)]
        public string? Position { get; set; }
        
        [Column("YahooPlayerID")]
        public int? YahooPlayerID { get; set; }
        
        [Column("PrimaryPosition")]
        [StringLength(2)]
        public string? PrimaryPosition { get; set; }
        
        [Column("ShortName")]
        [StringLength(100)]
        public string? ShortName { get; set; }
        
        [Column("BirthYear")]
        public int? BirthYear { get; set; }
        
        // Navigation properties
        public virtual ICollection<DraftPick> DraftPicks { get; set; }
        public virtual ICollection<PlayerSeason> PlayerSeasons { get; set; }
        public virtual ICollection<PlayerWeek> PlayerWeeks { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}