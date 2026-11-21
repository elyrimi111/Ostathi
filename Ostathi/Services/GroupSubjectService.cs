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

    public async Task<GroupSubject?> GetByIdAsync(int groupSubjectId)
    {
        try
        {
            return await _dbContext.GroupSubjects
                .FirstOrDefaultAsync(gs => gs.Id == groupSubjectId);
        }
        catch (Exception)
        {
            return null;
        }
    }

    // Get all subjects assigned to a specific group
    public async Task<List<GroupSubject>> GetSubjectsForGroupAsync(int groupId)
    {
        try
        {
            return await _dbContext.GroupSubjects
                .Where(gs => gs.GroupId == groupId)
                .ToListAsync();
        }
        catch (Exception)
        {
            return new List<GroupSubject>();
        }
    }

    // Assign a subject to a specific group
    public async Task<bool> AssignSubjectToGroupAsync(int groupId, int subjectId)
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
                SubjectId = subjectId
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

    // Update an existing GroupSubject record
    public async Task<bool> UpdateGroupSubjectAsync(int groupSubjectId, int newGroupId, int newSubjectId)
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
