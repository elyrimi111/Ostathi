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

    // Get all subjects assigned to a specific group
    public async Task<List<GroupSubject>> GetSubjectsForGroupAsync(int groupId)
    {
        try
        {
            return await _dbContext.GroupSubjects
                .Include(gs => gs.Subject)
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
            // Check if the relation already exists
            var existing = await _dbContext.GroupSubjects
                .FirstOrDefaultAsync(gs => gs.GroupId == groupId && gs.SubjectId == subjectId);

            if (existing != null)
                return false; // Already assigned

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
