using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Ostathi;

public class GroupService
{
    private readonly AppDbContext _dbContext;

    // Constructor injecting AppDbContext for database operations
    public GroupService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Retrieve all groups asynchronously
    public async Task<List<Group>> GetGroupsAsync()
    {
        try
        {
            return await _dbContext.Groups
                .ToListAsync();
        }
        catch (Exception)
        {
            return new List<Group>();
        }
    }

    // Retrieve a single group by its ID
    public async Task<Group?> GetGroupByIdAsync(int id)
    {
        try
        {
            return await _dbContext.Groups
                .FirstOrDefaultAsync(g => g.Id == id);
        }
        catch (Exception)
        {
            return null;
        }
    }

    // Save a new group or update an existing group record safely with tracking fix
    public async Task SaveGroupAsync(Group group)
    {
        if (group == null) return;

        if (group.Id == 0)
        {
            // Insert a new group entity
            _dbContext.Groups.Add(group);
        }
        else
        {
            // Check if an entity with the same Id is already tracked, and detach it if necessary
            var trackedEntity = _dbContext.Groups.Local.FirstOrDefault(g => g.Id == group.Id);
            if (trackedEntity != null)
            {
                _dbContext.Entry(trackedEntity).State = EntityState.Detached;
            }

            // Update an existing group entity safely
            _dbContext.Groups.Update(group);
        }

        await _dbContext.SaveChangesAsync();
    }

    // Delete a specific group from the database
    public async Task DeleteGroupAsync(Group group)
    {
        if (group != null)
        {
            _dbContext.Groups.Remove(group);
            await _dbContext.SaveChangesAsync();
        }
    }
}
