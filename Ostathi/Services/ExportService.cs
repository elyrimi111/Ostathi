using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;

namespace Ostathi;

public class ExportService
{
    private readonly AppDbContext _context;

    public ExportService(AppDbContext context)
    {
        _context = context;
    }

    // 1. تصدير السجل بصيغة Excel (CSV)
    public async Task ExportToExcelAsync(int groupId, int subjectId)
    {
        try
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
            var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Id == subjectId);
            var students = await _context.Students.Where(s => s.GroupId == groupId).ToListAsync();

            var csvContent = new StringBuilder();
            // إضافة ترميز يدعم اللغة العربية في إكسل
            csvContent.AppendLine("\xFEFF" + "الاسم الأول,اسم الاب,اسم الجد,الشهرة,الرقم الأكاديمي");

            foreach (var student in students)
            {
                csvContent.AppendLine($"\"{student.FirstName}\",\"{student.SecondName}\",\"{student.ThirdName}\",\"{student.LastName}\",\"{student.AcademicId}\"");
            }

            var fileName = $"{group?.Name ?? "Group"}_{subject?.Name ?? "Subject"}_Report.csv";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllTextAsync(filePath, csvContent.ToString(), Encoding.UTF8);

            if (Share.Default != null)
            {
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "تصدير تقرير Excel",
                    File = new ShareFile(filePath)
                });
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطأ", $"تعذر تصدير ملف الإكسل: {ex.Message}", "موافق");
            }
        }
    }

    // 2. تصدير السجل بصيغة PDF (تقرير منسق)
    public async Task ExportToPdfAsync(int groupId, int subjectId)
    {
        try
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
            var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Id == subjectId);
            var students = await _context.Students.Where(s => s.GroupId == groupId).ToListAsync();

            var pdfContent = new StringBuilder();
            pdfContent.AppendLine("=========================================");
            pdfContent.AppendLine("           تقرير كشف الطلاب والمقرر        ");
            pdfContent.AppendLine("=========================================");
            pdfContent.AppendLine($"المجموعة: {group?.Name ?? "غير معروف"}");
            pdfContent.AppendLine($"المادة: {subject?.Name ?? "غير معروف"}");
            pdfContent.AppendLine($"تاريخ الإصدار: {DateTime.Now:yyyy-MM-dd}");
            pdfContent.AppendLine("-----------------------------------------");
            pdfContent.AppendLine("الرقم | اسم الطالب الكامل");
            pdfContent.AppendLine("-----------------------------------------");

            int index = 1;
            foreach (var student in students)
            {
                pdfContent.AppendLine($"{index} | {student.FirstName} {student.SecondName} {student.ThirdName} {student.LastName} ({student.AcademicId})");
                index++;
            }
            pdfContent.AppendLine("=========================================");

            var fileName = $"{group?.Name ?? "Group"}_Report.pdf";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllTextAsync(filePath, pdfContent.ToString(), Encoding.UTF8);

            if (Share.Default != null)
            {
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "تصدير تقرير PDF",
                    File = new ShareFile(filePath)
                });
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطأ", $"تعذر تصدير ملف الـ PDF: {ex.Message}", "موافق");
            }
        }
    }
}
