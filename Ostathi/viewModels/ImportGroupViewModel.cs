using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace Ostathi;

[QueryProperty(nameof(GroupId), "GroupId")]
public class ImportGroupViewModel : BaseViewModel
{
    private readonly AppDbContext _dbContext;
    private readonly StudentImportService _importService;

    private int _groupId;
    public int GroupId
    {
        get => _groupId;
        set
        {
            if (SetProperty(ref _groupId, value) && _groupId > 0)
            {
                _ = LoadTargetGroupAsync(_groupId);
            }
        }
    }

    private Group? _targetGroup;

    private string _groupHeaderInfo = "جاري تحميل بيانات المجموعة...";
    public string GroupHeaderInfo
    {
        get => _groupHeaderInfo;
        set => SetProperty(ref _groupHeaderInfo, value);
    }

    private string _rawStudentsText = string.Empty;
    public string RawStudentsText
    {
        get => _rawStudentsText;
        set => SetProperty(ref _rawStudentsText, value);
    }

    private string _selectedFileName = string.Empty;
    public string SelectedFileName
    {
        get => _selectedFileName;
        set
        {
            if (SetProperty(ref _selectedFileName, value))
            {
                OnPropertyChanged(nameof(HasSelectedFile));
            }
        }
    }

    public bool HasSelectedFile => !string.IsNullOrWhiteSpace(SelectedFileName);

    public ObservableCollection<ImportedStudentPreviewItem> ImportedStudents { get; } = new();

    public ICommand PickFileCommand { get; }
    public ICommand PasteFromClipboardCommand { get; }
    public ICommand ParseTextCommand { get; }
    public ICommand ClearListCommand { get; }
    public ICommand RemoveStudentItemCommand { get; }
    public ICommand SaveImportedStudentsCommand { get; }
    public ICommand CancelCommand { get; }

    public ImportGroupViewModel()
    {
        _dbContext = new AppDbContext();
        _importService = new StudentImportService(_dbContext);

        PickFileCommand = new Command(async () => await OnPickFileAsync());
        PasteFromClipboardCommand = new Command(async () => await OnPasteFromClipboardAsync());
        ParseTextCommand = new Command(ParseRawTextToStudents);
        ClearListCommand = new Command(() => ImportedStudents.Clear());
        RemoveStudentItemCommand = new Command<ImportedStudentPreviewItem>(OnRemoveStudentItem);
        SaveImportedStudentsCommand = new Command(async () => await OnSaveImportedStudentsAsync());
        CancelCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    private async Task LoadTargetGroupAsync(int groupId)
    {
        try
        {
            _targetGroup = await _dbContext.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
            if (_targetGroup != null)
            {
                var major = await _dbContext.Majors.FirstOrDefaultAsync(m => m.Id == _targetGroup.MajorId);
                string majorName = major?.Name ?? "عام";
                GroupHeaderInfo = $"المجموعة: {_targetGroup.Name} • {majorName}";
            }
            else
            {
                GroupHeaderInfo = $"مجموعة رقم #{groupId}";
            }
        }
        catch (Exception ex)
        {
            GroupHeaderInfo = $"خطأ في تحميل المجموعة: {ex.Message}";
        }
    }

    private async Task OnPickFileAsync()
    {
        try
        {
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "text/plain", "text/csv", "text/comma-separated-values", "application/csv", "*/*" } },
                { DevicePlatform.WinUI, new[] { ".csv", ".txt" } }
            });

            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "اختر ملف كشف الطلاب (CSV أو TXT)",
                FileTypes = customFileType
            });

            if (result == null) return;

            SelectedFileName = result.FileName;

            using var stream = await result.OpenReadAsync();
            RawStudentsText = await _importService.ReadStreamTextAsync(stream);

            ParseRawTextToStudents();
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطأ", $"تعذر قراءة الملف: {ex.Message}", "موافق");
            }
        }
    }

    private async Task OnPasteFromClipboardAsync()
    {
        try
        {
            if (Clipboard.Default.HasText)
            {
                var text = await Clipboard.Default.GetTextAsync();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    RawStudentsText = text;
                    ParseRawTextToStudents();
                }
            }
            else if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("تنبيه", "الحافظة فارغة حالياً.", "موافق");
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطأ", $"فشل اللصق: {ex.Message}", "موافق");
            }
        }
    }

    private void ParseRawTextToStudents()
    {
        ImportedStudents.Clear();
        if (string.IsNullOrWhiteSpace(RawStudentsText)) return;

        var parsedList = _importService.ParseStudentsFromText(RawStudentsText, GroupId);

        int displayIndex = 1;
        foreach (var dto in parsedList)
        {
            ImportedStudents.Add(new ImportedStudentPreviewItem
            {
                Index = displayIndex++,
                FullName = dto.FullName,
                FirstName = dto.FirstName,
                SecondName = dto.SecondName,
                ThirdName = dto.ThirdName,
                LastName = dto.LastName,
                AcademicId = dto.AcademicId
            });
        }
    }

    private void OnRemoveStudentItem(ImportedStudentPreviewItem? item)
    {
        if (item == null) return;
        ImportedStudents.Remove(item);

        for (int i = 0; i < ImportedStudents.Count; i++)
        {
            ImportedStudents[i].Index = i + 1;
        }
    }

    private async Task OnSaveImportedStudentsAsync()
    {
        if (GroupId <= 0)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("تنبيه", "لم يتم تحديد المجموعة المستهدفة.", "موافق");
            }
            return;
        }

        if (ImportedStudents.Count == 0)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("تنبيه", "لا يوجد طلاب في القائمة لحفظهم.", "موافق");
            }
            return;
        }

        try
        {
            IsBusy = true;

            var dtos = ImportedStudents.Select(item => new ParsedStudentDto
            {
                FullName = item.FullName,
                FirstName = item.FirstName,
                SecondName = item.SecondName,
                ThirdName = item.ThirdName,
                LastName = item.LastName,
                AcademicId = item.AcademicId
            }).ToList();

            var result = await _importService.SaveImportedStudentsAsync(GroupId, dtos);

            if (Shell.Current != null)
            {
                string message = result.SkippedCount > 0
                    ? $"تم استيراد {result.AddedCount} طالب بنجاح، وتم تخطي {result.SkippedCount} سجل (مكرر أو ناقص)."
                    : $"تم استيراد {result.AddedCount} طالب بنجاح إلى المجموعة.";

                await Shell.Current.DisplayAlert("نتيجة الاستيراد", message, "موافق");

                if (result.Success)
                {
                    await Shell.Current.GoToAsync("..");
                }
            }
        }
        catch (Exception ex)
        {
            var err = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطأ", $"فشل حفظ الطلاب المستوردين: {err}", "موافق");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public class ImportedStudentPreviewItem : BaseViewModel
{
    private int _index;
    public int Index
    {
        get => _index;
        set => SetProperty(ref _index, value);
    }

    public string FullName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string SecondName { get; set; } = string.Empty;
    public string ThirdName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string AcademicId { get; set; } = string.Empty;
}
