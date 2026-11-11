using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Ostathi.Services;

public class StudentService
{
    private readonly AppDbContext _context;

    // Injecting the DbContext to handle real database operations
    public StudentService(AppDbContext context)
    {
        _context = context;
    }

    // Fetch all real students from the database
    public async Task<List<Student>> GetStudentsAsync()
    {
        return await _context.Students.ToListAsync();
    }

    // Fetch students filtered by their real group ID
    public async Task<List<Student>> GetStudentsByGroupAsync(int groupId)
    {
        return await _context.Students
            .Where(s => s.GroupId == groupId)
            .ToListAsync();
    }

    // Real save or update operation for students
    public async Task<int> SaveStudentAsync(Student student)
    {
        if (student == null) return 0;

        if (student.Id == 0)
        {
            // Add a new real student entity to the database
            await _context.Students.AddAsync(student);
        }
        else
        {
            // Update an existing real student entity
            _context.Students.Update(student);
        }

        // Save changes permanently to SQLite database
        int result = await _context.SaveChangesAsync();

        // Notify UI components about the addition/update in real-time
        MessengerService.NotifyStudentAdded();

        return result;
    }

    // Permanently delete a real student from the database
    public async Task<int> DeleteStudentAsync(Student student)
    {
        if (student == null) return 0;

        _context.Students.Remove(student);
        return await _context.SaveChangesAsync();
    }
}
