using System;
using System.Collections.Generic;
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
            return await _dbContext.Majors.ToListAsync();
        }
        catch (Exception)
        {
            return new List<Major>();
        }
    }

    // Save a new major or update an existing major record
    // Save a new major or update an existing major record safely
    public async Task SaveMajorAsync(Major major)
    {
    if (major == null) return;

    if (major.Id == 0)
    {
        // Insert a new major entity
        _dbContext.Majors.Add(major);
    }
    else
    {
        // Check if an entity with the same Id is already tracked, and detach it if necessary
        var trackedEntity = _dbContext.Majors.Local.FirstOrDefault(m => m.Id == major.Id);
        if (trackedEntity != null)
        {
            _dbContext.Entry(trackedEntity).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        }

        // Update an existing major entity safely
        _dbContext.Majors.Update(major);
    }

    await _dbContext.SaveChangesAsync();
}


    // Delete a specific major from the database
    public async Task DeleteMajorAsync(Major major)
    {
        if (major != null)
        {
            _dbContext.Majors.Remove(major);
            await _dbContext.SaveChangesAsync();
        }
    }
}
