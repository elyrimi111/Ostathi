using System;
using System.Collections.Generic;
using System.Linq;
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
    
    // Automatically generate default lectures up to a total of 12 for a specific group and subject
    public async Task GenerateDefaultLecturesAsync(int groupId, string subjectName, string defaultHall = "قاعة 1", string defaultDay = "السبت")
    {
        // Fetch existing lectures count for this group and subject
        var existingLectures = await _context.Lectures
            .Where(l => l.GroupId == groupId && l.SubjectName == subjectName)
            .ToListAsync();

        int totalNeeded = 12;
        int currentCount = existingLectures.Count;

        // If we already have 12 or more, do nothing
        if (currentCount >= totalNeeded) return;

        var startDate = DateTime.Now;
        // Generate the remaining lectures to reach 12
        for (int i = currentCount + 1; i <= totalNeeded; i++)
        {
            var lecture = new Lecture
            {
                GroupId = groupId,
                SubjectName = subjectName,
                HallName = defaultHall,
                Day = defaultDay,
                LectureDate = startDate.AddDays((i - 1) * 7), // Weekly interval
            };
            _context.Lectures.Add(lecture);
        }

        await _context.SaveChangesAsync();
        MessengerService.NotifyLectureAdded();
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
