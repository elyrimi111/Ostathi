using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Ostathi;

public class MajorService
{
    private readonly AppDbContext _dbContext;

    // Constructor injecting AppDbContext for database operations
    public MajorService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Retrieve all majors asynchronously from the SQLite database
    public async Task<List<Major>> GetMajorsAsync()
    {
        try
        {
            return await _dbContext.Majors
                .AsNoTracking()
                .OrderBy(m => m.Id)
                .ToListAsync();
        }
        catch (Exception)
        {
            return new List<Major>();
        }
    }

    // Retrieve a single major by Id for edit mode
    public async Task<Major?> GetMajorByIdAsync(int id)
    {
        try
        {
            return await _dbContext.Majors
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
        }
        catch (Exception)
        {
            return null;
        }
    }

    // Save a new major or update an existing major record safely
    public async Task SaveMajorAsync(Major major)
    {
        if (major == null) return;

        if (major.Id == 0)
        {
            // Insert a new major entity
            await _dbContext.Majors.AddAsync(major);
        }
        else
        {
            // Detach any already-tracked instance with the same Id to avoid tracking conflicts
            var trackedEntity = _dbContext.Majors.Local.FirstOrDefault(m => m.Id == major.Id);
            if (trackedEntity != null)
            {
                _dbContext.Entry(trackedEntity).State = EntityState.Detached;
            }

            _dbContext.Majors.Update(major);
        }

        await _dbContext.SaveChangesAsync();
    }

    // Delete a specific major from the database safely
    public async Task DeleteMajorAsync(Major major)
    {
        if (major == null) return;

        var trackedEntity = _dbContext.Majors.Local.FirstOrDefault(m => m.Id == major.Id);
        if (trackedEntity != null && !ReferenceEquals(trackedEntity, major))
        {
            _dbContext.Entry(trackedEntity).State = EntityState.Detached;
        }

        _dbContext.Majors.Remove(major);
        await _dbContext.SaveChangesAsync();
    }
}
