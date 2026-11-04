using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Ostathi;

public class ScheduleManagementViewModel : BindableObject
{
    public ICommand AddLectureCommand { get; }
    public ICommand EditLectureCommand { get; }

    public ScheduleManagementViewModel()
    {
        // أمر الانتقال لشاشة إضافة/تعديل المحاضرة عند الضغط على زر "+ إضافة محاضرة"
        AddLectureCommand = new Command(async () => await OnAddLectureExecuted());

        // أمر التعديل عند الضغط على زر "تعديل" في كروت المحاضرات
        EditLectureCommand = new Command(async () => await OnEditLectureExecuted());
    }

    private async Task OnAddLectureExecuted()
    {
        await Shell.Current.GoToAsync(nameof(AddUpdateLecturePage));
    }

    private async Task OnEditLectureExecuted()
    {
        // يمكنك لاحقاً تمرير بيانات المحاضرة المراد تعديلها عبر الـ Shell إذا لزم الأمر
        await Shell.Current.GoToAsync(nameof(AddUpdateLecturePage));
    }
}
