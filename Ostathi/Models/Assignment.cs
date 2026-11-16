using System;

namespace Ostathi;

public class Assignment
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; } = DateTime.Now.AddDays(7);
    public bool IsCompleted { get; set; }
}
