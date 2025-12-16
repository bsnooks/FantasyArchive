using System;
using System.ComponentModel.DataAnnotations;

namespace FantasyArchive.Data.Models;

public class Match
{
    [Key]
    public Guid MatchID { get; set; }
    
    [Required]
    public Guid LeagueID { get; set; }
    
    [Required]
    public int Year { get; set; }
    
    [Required]
    public int Week { get; set; }
    
    [Required]
    public Guid WinningTeamID { get; set; }
    
    [Required]
    public Guid LosingTeamID { get; set; }
    
    [Required]
    public MatchType MatchTypeId { get; set; }

    [Required]
    public bool Tied { get; set; } = false;

    // Navigation properties
    public virtual League? League { get; set; }
    public virtual Team? WinningTeam { get; set; }
    public virtual Team? LosingTeam { get; set; }
    public virtual Season? Season { get; set; }
}