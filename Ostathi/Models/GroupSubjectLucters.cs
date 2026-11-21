using System;

namespace Ostathi;

public class GroupSubjectLucters
{
    public int Id { get; set; }
    public int GroupSubjectId { get; set; }
    public int LectureId { get; set; }
    public int LectureNumber { get; set; }
    public bool IsTaken { get; set; }
}
