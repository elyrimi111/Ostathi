using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Ostathi;

[QueryProperty(nameof(StudentId), "StudentId")]
[QueryProperty(nameof(FinalScore), "FinalScore")]
[QueryProperty(nameof(MidtermScore), "MidtermScore")]
[QueryProperty(nameof(AttendanceScore), "AttendanceScore")]
[QueryProperty(nameof(ActivitiesScore), "ActivitiesScore")]
public class EditStudentGradesPageViewModel : BaseViewModel
{
    private int _studentId;
    public int StudentId
    {
        get => _studentId;
        set => SetProperty(ref _studentId, value);
    }

    private string _finalScore = string.Empty;
    public string FinalScore
    {
        get => _finalScore;
        set => SetProperty(ref _finalScore, value);
    }

    private string _midtermScore = string.Empty;
    public string MidtermScore
    {
        get => _midtermScore;
        set => SetProperty(ref _midtermScore, value);
    }

    private string _attendanceScore = string.Empty;
    public string AttendanceScore
    {
        get => _attendanceScore;
        set => SetProperty(ref _attendanceScore, value);
    }

    private string _activitiesScore = string.Empty;
    public string ActivitiesScore
    {
        get => _activitiesScore;
        set => SetProperty(ref _activitiesScore, value);
    }

    public ICommand SaveAllGradesCommand { get; }
    public ICommand CancelCommand { get; }

    public EditStudentGradesPageViewModel()
    {
        SaveAllGradesCommand = new Command(async () => await OnSaveAllExecuted());
        CancelCommand = new Command(async () => await OnCancelExecuted());
    }

    private async Task OnSaveAllExecuted()
    {
        // التحقق من صحة المدخلات والالتزام بالحدود القصوى
        if (!int.TryParse(FinalScore, out int final) || final < 0 || final > 60)
        {
            await Shell.Current.DisplayAlert("تنبيه", "درجة الاختبار النهائي يجب ألا تزيد عن 60 درجة.", "OK");
            return;
        }

        if (!int.TryParse(MidtermScore, out int midterm) || midterm < 0 || midterm > 20)
        {
            await Shell.Current.DisplayAlert("تنبيه", "درجة الاختبار النصي يجب ألا تزيد عن 20 درجة.", "OK");
            return;
        }

        if (!int.TryParse(AttendanceScore, out int attendance) || attendance < 0 || attendance > 10)
        {
            await Shell.Current.DisplayAlert("تنبيه", "درجة الحضور والغياب يجب ألا تزيد عن 10 درجات.", "OK");
            return;
        }

        if (!int.TryParse(ActivitiesScore, out int activities) || activities < 0 || activities > 10)
        {
            await Shell.Current.DisplayAlert("تنبيه", "درجة السعي والأنشطة يجب ألا تزيد عن 10 درجات.", "OK");
            return;
        }

        int totalScore = final + midterm + attendance + activities;

        // يمكنك هنا حفظ القيم المحدثة في قاعدة البيانات عبر AppDbContext باستخدام StudentId

        await Shell.Current.DisplayAlert("نجاح", $"تم حفظ جميع درجات الطالب بنجاح.\nالمجموع الكلي: {totalScore} / 100", "OK");
        await Shell.Current.GoToAsync("..");
    }

    private async Task OnCancelExecuted()
    {
        await Shell.Current.GoToAsync("..");
    }
}
