using System.ComponentModel.DataAnnotations;

namespace Ostathi;

public class Major
{
    // Primary key for the major entity
    [Key]
    public int Id { get; set; }
    
    // Major name (e.g., Computer Science, Information Technology)
    [Required]
    public string Name { get; set; } = string.Empty;

    // Duration of the major in years (e.g., 4 years)
    public int DurationYears { get; set; } = 4;
}
