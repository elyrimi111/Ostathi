using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ostathi;

public class GroupSubject
{
    [Key]
    public int Id { get; set; }

    // Foreign key for the Study Group
    public int GroupId { get; set; }
    [ForeignKey("GroupId")]
    public Group Group { get; set; } = null!;

    // Foreign key for the Subject
    public int SubjectId { get; set; }
    [ForeignKey("SubjectId")]
    public Subject Subject { get; set; } = null!;
}
