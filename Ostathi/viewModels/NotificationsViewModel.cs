using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace Ostathi;

public partial class NotificationsViewModel : ObservableObject
{
    [ObservableProperty]
    private object currentGroup;

    public NotificationsViewModel()
    {
        // يمكنك تهيئة البيانات الافتراضية هنا عند الحاجة
    }

    [RelayCommand]
    private async Task ClosePage()
    {
        // إغلاق الصفحة الحالية أو العودة للوراء
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task SendNotification()
    {
        // منطق إرسال تعميم جديد (فتح صفحة جديدة أو نافذة منبثقة)
        await Shell.Current.DisplayAlert("إرسال تعميم", "سيتم فتح شاشة إنشاء تعميم جديد.", "حسناً");
    }
}
