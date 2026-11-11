using System.ComponentModel.DataAnnotations;

namespace Ostathi;

public class Subject
{
    [Key]
    public int Id { get; set; }

    // Subject Name (e.g., Mathematics, Physics)
    public string Name { get; set; } = string.Empty;

    // Optional description or notes about the subject
    public string Description { get; set; } = string.Empty;
}
