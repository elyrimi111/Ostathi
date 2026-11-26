using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Ostathi;

public class GroupService
{
    private readonly AppDbContext _dbContext;

    public GroupService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Retrieve all groups with their related Major and Representative Student.
    public async Task<List<Group>> GetGroupsAsync()
    {
        try
        {
            return await _dbContext.Groups
                .AsNoTracking()
                .Include(g => g.Major)
                .Include(g => g.RepresentativeStudent)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }
        catch (Exception)
        {
            return new List<Group>();
        }
    }

    // Retrieve a single group by ID with its related entities.
    public async Task<Group?> GetGroupByIdAsync(int id)
    {
        try
        {
            return await _dbContext.Groups
                .AsNoTracking()
                .Include(g => g.Major)
                .Include(g => g.RepresentativeStudent)
                .FirstOrDefaultAsync(g => g.Id == id);
        }
        catch (Exception)
        {
            return null;
        }
    }

    // Set or clear the representative student for a group.
    public async Task SetGroupRepresentativeAsync(int groupId, int? studentId)
    {
        if (groupId <= 0)
            throw new ArgumentException("Invalid group ID.", nameof(groupId));

        var existingGroup = await _dbContext.Groups
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (existingGroup == null)
            throw new InvalidOperationException("Group not found.");

        existingGroup.RepresentativeStudentId = studentId is > 0 ? studentId : null;
        await _dbContext.SaveChangesAsync();
    }

    // Insert a new group or update an existing group.
    public async Task SaveGroupAsync(Group group)
    {
        if (group == null)
            return;

        if (group.Id == 0)
        {
            await _dbContext.Groups.AddAsync(group);
        }
        else
        {
            var existingGroup = await _dbContext.Groups
                .FirstOrDefaultAsync(g => g.Id == group.Id);

            if (existingGroup == null)
                throw new InvalidOperationException("Group not found.");

            existingGroup.Name = group.Name;
            existingGroup.Description = group.Description;
            existingGroup.AcademicYear = group.AcademicYear;
            existingGroup.MajorId = group.MajorId;
            existingGroup.RepresentativeStudentId = group.RepresentativeStudentId;
        }

        await _dbContext.SaveChangesAsync();
    }

    // Delete a group by ID.
    public async Task DeleteGroupAsync(Group group)
    {
        if (group == null)
            return;

        var existingGroup = await _dbContext.Groups
            .FirstOrDefaultAsync(g => g.Id == group.Id);

        if (existingGroup == null)
            return;

        _dbContext.Groups.Remove(existingGroup);

        await _dbContext.SaveChangesAsync();
    }
}
