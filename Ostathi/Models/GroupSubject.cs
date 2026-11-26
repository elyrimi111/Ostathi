using System;

namespace Ostathi;

public class GroupSubject
{
    public int Id { get; set; }

    // Foreign key referencing the Group entity
    public int GroupId { get; set; }

    // Navigation property for the related Group
    public Group? Group { get; set; }

    // Foreign key referencing the Subject entity
    public int SubjectId { get; set; }

    // Navigation property for the related Subject
    public Subject? Subject { get; set; }

    // Current status of the subject assignment within the group (e.g., Active, Completed, Archived)
    public string Status { get; set; } = "Active";

    // Optional notes or remarks for this group-subject link
    public string? Notes { get; set; }
}
