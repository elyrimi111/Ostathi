using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ostathi.Models;

namespace Ostathi.Services;

public class StudentService
{
    private readonly AppDbContext _context;

    // Injecting the DbContext to handle database operations
    public StudentService(AppDbContext context)
    {
        _context = context;
    }

    // Fetch all students from the database
    public async Task<List<Student>> GetStudentsAsync()
    {
        return await _context.Students.ToListAsync();
    }

    // Fetch students filtered by their group ID
    public async Task<List<Student>> GetStudentsByGroupAsync(int groupId)
    {
        return await _context.Students
            .Where(s => s.GroupId == groupId)
            .ToListAsync();
    }

    // Save a new student or update an existing one
    public async Task<int> SaveStudentAsync(Student student)
    {
        if (student.Id == 0)
        {
            // Add a new student entity
            await _context.Students.AddAsync(student);
        }
        else
        {
            // Update an existing student entity
            _context.Students.Update(student);
        }

        return await _context.SaveChangesAsync();
    }

    // Delete a student from the database
    public async Task<int> DeleteStudentAsync(Student student)
    {
        _context.Students.Remove(student);
        return await _context.SaveChangesAsync();
    }
}
