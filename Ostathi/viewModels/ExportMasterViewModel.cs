using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;

namespace Ostathi;

[QueryProperty(nameof(GroupId), "GroupId")]
[QueryProperty(nameof(GroupSubjectId), "GroupSubjectId")]
public partial class ExportMasterViewModel : ObservableObject
{
    private readonly AppDbContext _context;
    private readonly ExportService _exportService;

    [ObservableProperty]
    private int _groupId;

    [ObservableProperty]
    private int _groupSubjectId;

    [ObservableProperty]
    private Group? _currentGroup;

    public ICommand ClosePageCommand { get; }
    public ICommand ExportMasterExcelCommand { get; }
    public ICommand ExportPdfCommand { get; }

    public ExportMasterViewModel()
    {
        _context = new AppDbContext();
        _exportService = new ExportService(_context);

        ClosePageCommand = new AsyncRelayCommand(ClosePageAsync);
        ExportMasterExcelCommand = new AsyncRelayCommand(ExportMasterExcelAsync);
        ExportPdfCommand = new AsyncRelayCommand(ExportPdfAsync);
    }

    partial void OnGroupIdChanged(int value)
    {
        if (value > 0)
        {
            _ = LoadGroupDetailsAsync(value);
        }
    }

    private async Task LoadGroupDetailsAsync(int id)
    {
        CurrentGroup = await _context.Groups.FirstOrDefaultAsync(g => g.Id == id);
    }

    private async Task ClosePageAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    // ربط زر إكسل بالدالة الصحيحة في الخدمة
    private async Task ExportMasterExcelAsync()
    {
        if (GroupId <= 0)
        {
            await Shell.Current.DisplayAlert("تنبيه", "لم يتم تحديد المجموعة المستهدفة", "موافق");
            return;
        }

        await _exportService.ExportToExcelAsync(GroupId, GroupSubjectId);
    }

    // ربط زر PDF بالدالة الصحيحة في الخدمة
    private async Task ExportPdfAsync()
    {
        if (GroupId <= 0)
        {
            await Shell.Current.DisplayAlert("تنبيه", "لم يتم تحديد المجموعة المستهدفة", "موافق");
            return;
        }

        await _exportService.ExportToPdfAsync(GroupId, GroupSubjectId);
    }
}
