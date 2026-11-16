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
        public int? Level { get; set; }       
        
        public int? Semester { get; set; }       
        
        public bool IsMandatory { get; set; } = true; 
    }
}
