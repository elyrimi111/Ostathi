using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Ostathi;

public class SubjectService
{
    private readonly AppDbContext _dbContext;

    public SubjectService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Retrieve all subjects ordered by name.
    public async Task<List<Subject>> GetSubjectsAsync()
    {
        try
        {
            return await _dbContext.Subjects
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
        catch (Exception)
        {
            return new List<Subject>();
        }
    }

    // Retrieve a single subject by ID.
    public async Task<Subject?> GetSubjectByIdAsync(int id)
    {
        try
        {
            return await _dbContext.Subjects
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        catch (Exception)
        {
            return null;
        }
    }

    // Insert a new subject or update an existing subject.
    public async Task SaveSubjectAsync(Subject subject)
    {
        if (subject == null)
            return;

        if (subject.Id == 0)
        {
            await _dbContext.Subjects.AddAsync(subject);
        }
        else
        {
            var existingSubject = await _dbContext.Subjects
                .FirstOrDefaultAsync(s => s.Id == subject.Id);

            if (existingSubject == null)
                throw new InvalidOperationException("Subject not found.");

            existingSubject.Name = subject.Name;
            existingSubject.Description = subject.Description;
        }

        await _dbContext.SaveChangesAsync();
    }

    // Delete a subject by ID.
    public async Task DeleteSubjectAsync(Subject subject)
    {
        if (subject == null)
            return;

        var existingSubject = await _dbContext.Subjects
            .FirstOrDefaultAsync(s => s.Id == subject.Id);

        if (existingSubject == null)
            return;

        _dbContext.Subjects.Remove(existingSubject);
        await _dbContext.SaveChangesAsync();
    }
}
