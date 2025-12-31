using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FantasyArchive.Data.Models
{
    [Table("PlayerWeek")]
    public class PlayerWeek
    {
        // Composite primary key - no single ID column
        [Key]
        [Column("PlayerID")]
        public int PlayerID { get; set; }
        
        [Key]
        [Required]
        [Column("Year")]
        public int Year { get; set; }
        
        [Key]
        [Required]
        [Column("Week")]
        public int Week { get; set; }
        
        [Column("TeamId")]
        public Guid? TeamId { get; set; }
        
        [Column("Started")]
        public bool Started { get; set; }
        
        // Passing stats - match actual DB column names
        [Column("PassYards")]
        public int PassYards { get; set; }
        
        [Column("PassTDs")]
        public int PassTDs { get; set; }
        
        [Column("Interceptions")]
        public int Interceptions { get; set; }
        
        // Rushing stats - match actual DB column names
        [Column("RushYards")]
        public int RushYards { get; set; }
        
        [Column("RushTDs")]
        public int RushTDs { get; set; }
        
        // Receiving stats - match actual DB column names
        [Column("RecYards")]
        public int RecYards { get; set; }
        
        [Column("RecTDs")]
        public int RecTDs { get; set; }
        
        // Other stats - match actual DB column names
        [Column("FumblesLost")]
        public int FumblesLost { get; set; }
        
        [Column("TwoPointConvert")]
        public int TwoPointConvert { get; set; }
        
        // Navigation properties
        [ForeignKey("PlayerID")]
        public virtual Player Player { get; set; } = null!;
        
        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; } = null!;
        [NotMapped]
        public double Points => CalculatePoints();

        public override string ToString()
        {
            return $"{PlayerID}.{Year}.{Week}";
        }
        public double CalculatePoints()
        {
            return PassYards / 25.0 + PassTDs * 4 + RushYards / 10.0 + RushTDs * 6 + RecYards / 10.0 + RecTDs * 6;
        }
    }
}