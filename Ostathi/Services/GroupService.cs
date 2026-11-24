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

    // Retrieve all groups ordered by name without change tracking
    public async Task<List<Group>> GetGroupsAsync()
    {
        try
        {
            return await _dbContext.Groups
                .AsNoTracking()
                .OrderBy(g => g.Name)
                .ToListAsync();
        }
        catch (Exception)
        {
            return new List<Group>();
        }
    }

    // Retrieve a single group by ID without change tracking
    public async Task<Group?> GetGroupByIdAsync(int id)
    {
        try
        {
            return await _dbContext.Groups
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == id);
        }
        catch (Exception)
        {
            return null;
        }
    }

    // Insert a new group when Id is 0, or update an existing group safely
    public async Task SaveGroupAsync(Group group)
    {
        if (group == null) return;

        if (group.Id == 0)
        {
            await _dbContext.Groups.AddAsync(group);
        }
        else
        {
            var trackedEntity = _dbContext.Groups.Local.FirstOrDefault(g => g.Id == group.Id);
            if (trackedEntity != null)
            {
                _dbContext.Entry(trackedEntity).State = EntityState.Detached;
            }

            _dbContext.Groups.Update(group);
        }

        await _dbContext.SaveChangesAsync();
    }

    // Delete a group record while safely handling detached/tracked instances
    public async Task DeleteGroupAsync(Group group)
    {
        if (group == null) return;

        var trackedEntity = _dbContext.Groups.Local.FirstOrDefault(g => g.Id == group.Id);
        if (trackedEntity != null && !ReferenceEquals(trackedEntity, group))
        {
            _dbContext.Entry(trackedEntity).State = EntityState.Detached;
        }

        _dbContext.Groups.Remove(group);
        await _dbContext.SaveChangesAsync();
    }
}
