using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ostathi.Services;

namespace Ostathi;

public class LucterService
{
    private readonly AppDbContext _context;

    public LucterService(AppDbContext context)
    {
        _context = context;
    }

    // Get all lectures
    public async Task<List<Lecture>> GetAllLecturesAsync()
    {
        return await _context.Lectures.ToListAsync();
    }

    // Find lecture by ID (used internally for updating)
    public async Task<Lecture?> GetLectureByIdAsync(int id)
    {
        return await _context.Lectures.FindAsync(id);
    }

    // Dedicated method for adding a lecture only
    public async Task<int> AddLectureAsync(Lecture lecture)
    {
        _context.Lectures.Add(lecture);
        return await _context.SaveChangesAsync();
    }

    // Dedicated method for updating a lecture only (uses ID lookup first)
    public async Task<int> UpdateLectureAsync(Lecture lecture)
    {
        var existingLecture = await GetLectureByIdAsync(lecture.Id);
        
        if (existingLecture != null)
        {
            // Update properties of the tracked entity to prevent tracking conflicts
            existingLecture.GroupId = lecture.GroupId;
            existingLecture.Day = lecture.Day;
            existingLecture.HallName = lecture.HallName;
        }
        else
        {
            _context.Lectures.Update(lecture);
        }

        return await _context.SaveChangesAsync();
    }

    // Main save method that directs to either add or update based on ID
    public async Task<int> SaveLectureAsync(Lecture lecture)
    {
        try
        {
            int result;

            if (lecture.Id == 0)
            {
                // Call add method
                result = await AddLectureAsync(lecture);
            }
            else
            {
                // Call update method
                result = await UpdateLectureAsync(lecture);
            }
            
            // Notify UI about the update
            MessengerService.NotifyLectureAdded();
            return result;
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            System.Diagnostics.Debug.WriteLine($"DB ERROR: {innerMessage}");
            throw;
        }
    }

    // Delete a lecture
    public async Task<int> DeleteLectureAsync(Lecture lecture)
    {
        _context.Lectures.Remove(lecture);
        return await _context.SaveChangesAsync();
    }
}
