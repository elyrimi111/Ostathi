using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Ostathi;

public class GroupSubjectService
{
    private readonly AppDbContext _dbContext;

    public GroupSubjectService()
    {
        _dbContext = new AppDbContext();
    }

    // Retrieve a single GroupSubject record including related Group and Subject
    public async Task<GroupSubject?> GetByIdAsync(int groupSubjectId)
    {
        try
        {
            return await _dbContext.GroupSubjects
                .Include(gs => gs.Group)
                .Include(gs => gs.Subject)
                .FirstOrDefaultAsync(gs => gs.Id == groupSubjectId);
        }
        catch (Exception)
        {
            return null;
        }
    }

    // Get all subjects assigned to a specific group with related details
    public async Task<List<GroupSubject>> GetSubjectsForGroupAsync(int groupId)
    {
        try
        {
            return await _dbContext.GroupSubjects
                .Include(gs => gs.Group)
                .Include(gs => gs.Subject)
                .Where(gs => gs.GroupId == groupId)
                .ToListAsync();
        }
        catch (Exception)
        {
            return new List<GroupSubject>();
        }
    }

    // Assign a subject to a specific group with optional status and notes
    public async Task<bool> AssignSubjectToGroupAsync(
        int groupId,
        int subjectId,
        string status = "Active",
        string? notes = null)
    {
        try
        {
            var existing = await _dbContext.GroupSubjects
                .FirstOrDefaultAsync(gs => gs.GroupId == groupId && gs.SubjectId == subjectId);

            if (existing != null)
                return false;

            var groupSubject = new GroupSubject
            {
                GroupId = groupId,
                SubjectId = subjectId,
                Status = string.IsNullOrWhiteSpace(status) ? "Active" : status.Trim(),
                Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
            };

            _dbContext.GroupSubjects.Add(groupSubject);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    // Update an existing GroupSubject record including status and notes
    public async Task<bool> UpdateGroupSubjectAsync(
        int groupSubjectId,
        int newGroupId,
        int newSubjectId,
        string? status = null,
        string? notes = null)
    {
        try
        {
            var existingRecord = await _dbContext.GroupSubjects
                .FirstOrDefaultAsync(gs => gs.Id == groupSubjectId);

            if (existingRecord == null)
                return false;

            var duplicate = await _dbContext.GroupSubjects
                .FirstOrDefaultAsync(gs => gs.Id != groupSubjectId &&
                                           gs.GroupId == newGroupId &&
                                           gs.SubjectId == newSubjectId);

            if (duplicate != null)
                return false;

            existingRecord.GroupId = newGroupId;
            existingRecord.SubjectId = newSubjectId;

            if (!string.IsNullOrWhiteSpace(status))
            {
                existingRecord.Status = status.Trim();
            }

            if (notes != null)
            {
                existingRecord.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    // Remove a subject from a group
    public async Task<bool> RemoveSubjectFromGroupAsync(int groupSubjectId)
    {
        try
        {
            var item = await _dbContext.GroupSubjects.FindAsync(groupSubjectId);
            if (item != null)
            {
                _dbContext.GroupSubjects.Remove(item);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
