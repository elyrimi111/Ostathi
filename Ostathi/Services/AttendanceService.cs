using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Ostathi;

public class AttendanceService
{
    private readonly AppDbContext _context;

    public AttendanceService(AppDbContext context)
    {
        _context = context;
    }

    // Get all attendance records
    public async Task<List<Attendance>> GetAllAttendancesAsync()
    {
        return await _context.Attendances.ToListAsync();
    }

    // Find attendance record by ID (used internally for updating)
    public async Task<Attendance> GetAttendanceByIdAsync(int id)
    {
        return await _context.Attendances.FindAsync(id);
    }

    // Get attendance records by Lecture ID
    public async Task<List<Attendance>> GetAttendancesByLectureAsync(int lectureId)
    {
        return await _context.Attendances
            .Where(a => a.LectureId == lectureId)
            .ToListAsync();
    }

    // Get attendance records by Student ID
    public async Task<List<Attendance>> GetAttendancesByStudentAsync(int studentId)
    {
        return await _context.Attendances
            .Where(a => a.StudentId == studentId)
            .ToListAsync();
    }

    // Dedicated method for adding an attendance record only
    public async Task<int> AddAttendanceAsync(Attendance attendance)
    {
        _context.Attendances.Add(attendance);
        return await _context.SaveChangesAsync();
    }

    // Dedicated method for updating an attendance record only (uses ID lookup first)
    public async Task<int> UpdateAttendanceAsync(Attendance attendance)
    {
        var existingAttendance = await GetAttendanceByIdAsync(attendance.Id);
        
        if (existingAttendance != null)
        {
            // Update properties of the tracked entity to prevent tracking conflicts
            existingAttendance.LectureId = attendance.LectureId;
            existingAttendance.StudentId = attendance.StudentId;
            existingAttendance.Date = attendance.Date;
            existingAttendance.Time = attendance.Time;
        }
        else
        {
            _context.Attendances.Update(attendance);
        }

        return await _context.SaveChangesAsync();
    }

    // Main save method that directs to either add or update based on ID
    public async Task<int> SaveAttendanceAsync(Attendance attendance)
    {
        try
        {
            int result;

            if (attendance.Id == 0)
            {
                // Call add method
                result = await AddAttendanceAsync(attendance);
            }
            else
            {
                // Call update method
                result = await UpdateAttendanceAsync(attendance);
            }
            
            return result;
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            System.Diagnostics.Debug.WriteLine($"DB ERROR: {innerMessage}");
            throw;
        }
    }

    // Delete an attendance record
    public async Task<int> DeleteAttendanceAsync(Attendance attendance)
    {
        _context.Attendances.Remove(attendance);
        return await _context.SaveChangesAsync();
    }
}
