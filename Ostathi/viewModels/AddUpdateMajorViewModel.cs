using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Ostathi;

[QueryProperty(nameof(MajorId), "MajorId")]
public class AddUpdateMajorViewModel : BaseViewModel
{
    private readonly MajorService _majorService;

    private int _majorId;
    public int MajorId
    {
        get => _majorId;
        set
        {
            if (SetProperty(ref _majorId, value))
            {
                _ = LoadMajorForEditAsync(_majorId);
            }
        }
    }

    private string _pageTitle = "إضافة تخصص جديد";
    public string PageTitle
    {
        get => _pageTitle;
        set => SetProperty(ref _pageTitle, value);
    }

    private string _majorName = string.Empty;
    public string MajorName
    {
        get => _majorName;
        set => SetProperty(ref _majorName, value);
    }

    private int _durationYears = 4;
    public int DurationYears
    {
        get => _durationYears;
        set => SetProperty(ref _durationYears, value);
    }

    public ICommand SaveMajorCommand { get; }
    public ICommand CancelCommand { get; }

    public AddUpdateMajorViewModel(MajorService majorService)
    {
        _majorService = majorService;

        SaveMajorCommand = new Command(async () => await OnSaveMajorAsync());
        CancelCommand = new Command(async () => await OnCancelAsync());
    }

    // Load existing major data when navigating in edit mode
    public async Task LoadMajorForEditAsync(int id)
    {
        if (id <= 0)
        {
            _majorId = 0;
            PageTitle = "إضافة تخصص جديد";
            MajorName = string.Empty;
            DurationYears = 4;
            return;
        }

        try
        {
            var existingMajor = await _majorService.GetMajorByIdAsync(id);
            if (existingMajor == null) return;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                _majorId = existingMajor.Id;
                PageTitle = "تعديل التخصص";
                MajorName = existingMajor.Name;
                DurationYears = existingMajor.DurationYears > 0 ? existingMajor.DurationYears : 4;
            });
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load major: {ex.Message}", "OK");
            }
        }
    }

    // Validate and save new or updated major via MajorService
    private async Task OnSaveMajorAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(MajorName))
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("تحذير", "الرجاء إدخال اسم التخصص", "موافق");
            }
            return;
        }

        if (DurationYears < 1 || DurationYears > 10)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("تحذير", "عدد سنوات الدراسة يجب أن يكون بين 1 و 10", "موافق");
            }
            return;
        }

        try
        {
            IsBusy = true;

            var major = new Major
            {
                Id = _majorId,
                Name = MajorName.Trim(),
                DurationYears = DurationYears
            };

            await _majorService.SaveMajorAsync(major);

            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            string details = ex.InnerException?.Message ?? ex.Message;
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to save major: {details}", "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OnCancelAsync()
    {
        if (IsBusy || Shell.Current == null) return;
        await Shell.Current.GoToAsync("..");
    }
}
