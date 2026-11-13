using System.ComponentModel.DataAnnotations;

namespace Ostathi
{
    public class MajorSubject
    {
        [Key]
        public int Id { get; set; }

        // Foreign Key pointing to the Major
        public int MajorId { get; set; }

        // Foreign Key pointing to the Subject
        public int SubjectId { get; set; }

        // Additional plan details
        public int? Level { get; set; }          // Academic level (e.g., 1, 2, 3, 4)
        public int? Semester { get; set; }       // Semester (1 or 2)
        public bool IsMandatory { get; set; } = true; // Indicates if the subject is mandatory
    }
}
