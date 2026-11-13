using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Ostathi;

public class MajorSubjectService
{
    private readonly AppDbContext _context;

    public MajorSubjectService(AppDbContext context)
    {
        _context = context;
    }

    // Get all major-subject relations
    public async Task<List<MajorSubject>> GetAllAsync()
    {
        return await _context.MajorSubjects
            .AsNoTracking()
            .ToListAsync();
    }

    // Get all subjects associated with a specific major by performing a manual join
    public async Task<List<Subject>> GetSubjectsByMajorIdAsync(int majorId)
    {
        return await (from ms in _context.MajorSubjects
                      join s in _context.Subjects on ms.SubjectId equals s.Id
                      where ms.MajorId == majorId
                      select s)
                      .AsNoTracking()
                      .ToListAsync();
    }

    // Get all study plan entries for a specific major ordered by level and semester
    public async Task<List<MajorSubject>> GetMajorPlanAsync(int majorId)
    {
        return await _context.MajorSubjects
            .Where(ms => ms.MajorId == majorId)
            .OrderBy(ms => ms.Level)
            .ThenBy(ms => ms.Semester)
            .AsNoTracking()
            .ToListAsync();
    }

    // Assign a subject to a major
    public async Task<bool> AddAsync(MajorSubject majorSubject)
    {
        // Check if the association already exists to prevent duplicate entries
        bool exists = await _context.MajorSubjects
            .AnyAsync(ms => ms.MajorId == majorSubject.MajorId && ms.SubjectId == majorSubject.SubjectId);

        if (exists)
            return false;

        await _context.MajorSubjects.AddAsync(majorSubject);
        return await _context.SaveChangesAsync() > 0;
    }

    // Update major-subject record details (Level, Semester, IsMandatory)
    public async Task<bool> UpdateAsync(MajorSubject majorSubject)
    {
        var existing = await _context.MajorSubjects.FindAsync(majorSubject.Id);
        if (existing == null)
            return false;

        existing.Level = majorSubject.Level;
        existing.Semester = majorSubject.Semester;
        existing.IsMandatory = majorSubject.IsMandatory;

        _context.MajorSubjects.Update(existing);
        return await _context.SaveChangesAsync() > 0;
    }

    // Remove a major-subject relation by its unique record ID
    public async Task<bool> DeleteAsync(int id)
    {
        var record = await _context.MajorSubjects.FindAsync(id);
        if (record == null)
            return false;

        _context.MajorSubjects.Remove(record);
        return await _context.SaveChangesAsync() > 0;
    }

    // Remove a subject from a major using MajorId and SubjectId
    public async Task<bool> RemoveSubjectFromMajorAsync(int majorId, int subjectId)
    {
        var record = await _context.MajorSubjects
            .FirstOrDefaultAsync(ms => ms.MajorId == majorId && ms.SubjectId == subjectId);

        if (record == null)
            return false;

        _context.MajorSubjects.Remove(record);
        return await _context.SaveChangesAsync() > 0;
    }
}
