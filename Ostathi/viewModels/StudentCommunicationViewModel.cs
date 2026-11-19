using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace Ostathi;

[QueryProperty(nameof(GroupId), "GroupId")]
public partial class StudentCommunicationViewModel : ObservableObject
{
    private readonly AppDbContext _context;

    [ObservableProperty]
    private int _groupId;

    [ObservableProperty]
    private string? _representativeName;

    [ObservableProperty]
    private string? _representativePhone;

    [ObservableProperty]
    private string _messageTitle = string.Empty;

    [ObservableProperty]
    private string _messageBody = string.Empty;

    public ICommand CallRepresentativeCommand { get; }
    public ICommand OpenWhatsAppRepresentativeCommand { get; }
    public ICommand BroadcastMessageCommand { get; }

    public StudentCommunicationViewModel(AppDbContext context)
    {
        _context = context;

        CallRepresentativeCommand = new AsyncRelayCommand(CallRepresentativeAsync);
        OpenWhatsAppRepresentativeCommand = new AsyncRelayCommand(OpenWhatsAppRepresentativeAsync);
        BroadcastMessageCommand = new AsyncRelayCommand(BroadcastMessageAsync);
    }

    partial void OnGroupIdChanged(int value)
    {
        if (value > 0)
        {
            _ = LoadGroupRepresentativeAsync();
        }
    }

    // جلب بيانات المندوب باستخدام معرّفه (RepresentativeStudentId) فقط
    private async Task LoadGroupRepresentativeAsync()
    {
        var group = await _context.Groups
            .FirstOrDefaultAsync(g => g.Id == GroupId);

        if (group != null && group.RepresentativeStudentId.HasValue)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == group.RepresentativeStudentId.Value);

            if (student != null)
            {
                RepresentativeName = $"{student.FirstName} {student.SecondName} {student.ThirdName} {student.LastName}";
                RepresentativePhone = student.AcademicId;
                return;
            }
        }

        RepresentativeName = "لم يتم تعيين مندوب";
        RepresentativePhone = string.Empty;
    }

    private async Task CallRepresentativeAsync()
    {
        if (string.IsNullOrEmpty(RepresentativePhone))
        {
            await Shell.Current.DisplayAlert("تنبيه", "لا يوجد رقم هاتف مسجل لهذا المندوب", "موافق");
            return;
        }

        try
        {
            if (PhoneDialer.Default.IsSupported)
            {
                PhoneDialer.Default.Open(RepresentativePhone);
            }
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert("خطأ", "تعذر إجراء الاتصال من هذا الجهاز", "موافق");
        }
    }

    private async Task OpenWhatsAppRepresentativeAsync()
    {
        if (string.IsNullOrEmpty(RepresentativePhone))
        {
            await Shell.Current.DisplayAlert("تنبيه", "لا يوجد رقم هاتف مسجل لهذا المندوب", "موافق");
            return;
        }

        try
        {
            var url = $"https://wa.me/{RepresentativePhone}";
            await Launcher.Default.OpenAsync(url);
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert("خطأ", "تعذر فتح تطبيق واتساب", "موافق");
        }
    }

    private async Task BroadcastMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(MessageTitle) || string.IsNullOrWhiteSpace(MessageBody))
        {
            await Shell.Current.DisplayAlert("تنبيه", "الرجاء كتابة عنوان ونش الرسالة أولاً قبل الإرسال", "موافق");
            return;
        }

        try
        {
            var encodedMessage = Uri.EscapeDataString($"*{MessageTitle}*\n\n{MessageBody}");
            var url = $"https://wa.me/?text={encodedMessage}";
            await Launcher.Default.OpenAsync(url);
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert("خطأ", "تعذر إرسال الرسالة", "موافق");
        }
    }
}
