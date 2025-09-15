using System.ComponentModel.DataAnnotations;

namespace FantasyArchive.Data.Models;

public class TeamScore
{
    [Required]
    public Guid LeagueID { get; set; }
    
    [Key]
    public Guid TeamID { get; set; }
    
    [Key]
    public int Year { get; set; }
    
    [Key]
    public int Week { get; set; }
    
    [Required]
    public double Points { get; set; }
    
    public double? ProjectedPoints { get; set; }
    
    // Navigation properties
    public virtual Team? Team { get; set; }
    public virtual Season? Season { get; set; }
}