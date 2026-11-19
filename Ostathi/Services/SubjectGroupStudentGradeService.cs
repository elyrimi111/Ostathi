using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Ostathi;

public class SubjectGroupStudentGradeService
{
    private readonly AppDbContext _context;

    public SubjectGroupStudentGradeService(AppDbContext context)
    {
        _context = context;
    }

    // Get all student grades
    public async Task<List<SubjectGroupStudentGrade>> GetAllAsync()
    {
        return await _context.SubjectGroupStudentGrades.ToListAsync();
    }

    // Get a specific student grade by its ID
    public async Task<SubjectGroupStudentGrade?> GetByIdAsync(int id)
    {
        return await _context.SubjectGroupStudentGrades.FindAsync(id);
    }

    // Get grades for a specific student
    public async Task<List<SubjectGroupStudentGrade>> GetByStudentIdAsync(int studentId)
    {
        return await _context.SubjectGroupStudentGrades
            .Where(g => g.StudentId == studentId)
            .ToListAsync();
    }

    // Get grades for a specific group subject
    public async Task<List<SubjectGroupStudentGrade>> GetByGroupSubjectIdAsync(int groupSubjectId)
    {
        return await _context.SubjectGroupStudentGrades
            .Where(g => g.GroupSubjectId == groupSubjectId)
            .ToListAsync();
    }

    // Add or update a student grade
    public async Task<bool> SaveAsync(SubjectGroupStudentGrade grade)
    {
        if (grade.Id == 0)
        {
            _context.SubjectGroupStudentGrades.Add(grade);
        }
        else
        {
            _context.SubjectGroupStudentGrades.Update(grade);
        }

        return await _context.SaveChangesAsync() > 0;
    }

    // Delete a student grade by ID
    public async Task<bool> DeleteAsync(int id)
    {
        var grade = await _context.SubjectGroupStudentGrades.FindAsync(id);
        if (grade == null) return false;

        _context.SubjectGroupStudentGrades.Remove(grade);
        return await _context.SaveChangesAsync() > 0;
    }
}
