using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Ostathi;

public class AcademicSettingsPageViewModel : BaseViewModel
{
    private string _lecturerName = string.Empty;
    public string LecturerName
    {
        get => _lecturerName;
        set { _lecturerName = value; OnPropertyChanged(); }
    }

    private string _selectedAcademicYear = string.Empty;
    public string SelectedAcademicYear
    {
        get => _selectedAcademicYear;
        set { _selectedAcademicYear = value; OnPropertyChanged(); }
    }

    private string _selectedSemester = string.Empty;
    public string SelectedSemester
    {
        get => _selectedSemester;
        set { _selectedSemester = value; OnPropertyChanged(); }
    }

    private string _selectedMajor = string.Empty;
    public string SelectedMajor
    {
        get => _selectedMajor;
        set { _selectedMajor = value; OnPropertyChanged(); }
    }

    // Collections for Pickers
    public ObservableCollection<string> AcademicYears { get; }
    public ObservableCollection<string> Semesters { get; }
    public ObservableCollection<string> Majors { get; }

    public ICommand SaveSettingsCommand { get; }
    public ICommand BackCommand { get; }

    public AcademicSettingsPageViewModel()
    {
        // Populate Picker options
        AcademicYears = new ObservableCollection<string>
        {
            "2025 - 2026",
            "2026 - 2027",
            "2027 - 2028"
        };

        Semesters = new ObservableCollection<string>
        {
            "الترم الأول",
            "الترم الثاني",
            "الترم الصيفي"
        };

        Majors = new ObservableCollection<string>
        {
            "علوم الحاسب وتطوير الويب",
            "هندسة البرمجيات (.NET)",
            "قواعد البيانات وتحليل البيانات",
            "الشبكات وأمن المعلومات"
        };

        // Load initial values (e.g., from Preferences or local storage if saved previously)
        LoadSettings();

        SaveSettingsCommand = new Command(async () => await OnSaveSettingsExecuted());
        BackCommand = new Command(async () => await OnBackExecuted());
    }

    private void LoadSettings()
    {
        // Example loading from Microsoft.Maui.Storage.Preferences
        LecturerName = Preferences.Get("LecturerName", "د. محمد فؤاد أحمد");
        SelectedAcademicYear = Preferences.Get("AcademicYear", "2025 - 2026");
        SelectedSemester = Preferences.Get("Semester", "الترم الأول");
        SelectedMajor = Preferences.Get("Major", "هندسة البرمجيات (.NET)");
    }

    private async Task OnSaveSettingsExecuted()
    {
        try
        {
            // Save values locally using Preferences
            Preferences.Set("LecturerName", LecturerName);
            Preferences.Set("AcademicYear", SelectedAcademicYear ?? string.Empty);
            Preferences.Set("Semester", SelectedSemester ?? string.Empty);
            Preferences.Set("Major", SelectedMajor ?? string.Empty);

            await Shell.Current.DisplayAlert("نجاح", "تم حفظ البيانات الأكاديمية بنجاح", "حسناً");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("خطأ", $"فشل الحفظ: {ex.Message}", "حسناً");
        }
    }

    private async Task OnBackExecuted()
    {
        await Shell.Current.GoToAsync("..");
    }
}
