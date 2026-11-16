using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ostathi.Models;

namespace Ostathi;

public class AssignmentService
{
    private readonly AppDbContext _context;

    public AssignmentService(AppDbContext context)
    {
        _context = context;
    }

    // جلب قائمة التكاليف الخاصة بالمجموعة والمادة معاً مرتبة حسب تاريخ التسليم (تم التعديل لتفادي خطأ الترجمة)
    public async Task<List<Assignment>> GetAssignmentsAsync(int groupId, string subjectName)
    {
        // 1. جلب التكاليف الخاصة بالمجموعة وترتيبها من قاعدة البيانات مباشرة
        var groupAssignments = await _context.Assignments
            .Where(a => a.GroupId == groupId)
            .OrderBy(a => a.DueDate)
            .ToListAsync();

        // 2. تصفية المادة في الذاكرة باستخدام StringComparison لتجنب مشكلة الترجمة في EF Core
        return groupAssignments
            .Where(a => string.Equals(a.SubjectName, subjectName, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    // حفظ أو تحديث تكليف جديد في قاعدة البيانات
    public async Task<int> SaveAssignmentAsync(Assignment assignment)
    {
        if (assignment.Id == 0)
        {
            _context.Assignments.Add(assignment);
        }
        else
        {
            _context.Assignments.Update(assignment);
        }
        return await _context.SaveChangesAsync();
    }

    // حذف تكليف من قاعدة البيانات
    public async Task<int> DeleteAssignmentAsync(Assignment assignment)
    {
        _context.Assignments.Remove(assignment);
        return await _context.SaveChangesAsync();
    }
}
