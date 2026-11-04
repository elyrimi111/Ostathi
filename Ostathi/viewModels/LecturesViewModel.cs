using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Ostathi;

public class LectureItemModel
{
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string LectureNumber { get; set; } = string.Empty;
}

public class LecturesViewModel : BindableObject
{
    private int _totalLectures = 12;
    public int TotalLectures
    {
        get => _totalLectures;
        set { _totalLectures = value; OnPropertyChanged(nameof(TotalLectures)); }
    }

    private int _completedLectures = 5;
    public int CompletedLectures
    {
        get => _completedLectures;
        set { _completedLectures = value; OnPropertyChanged(nameof(CompletedLectures)); }
    }

    public ObservableCollection<LectureItemModel> LecturesList { get; }

    public ICommand AddLectureCommand { get; }

    public LecturesViewModel()
    {
        LecturesList = new ObservableCollection<LectureItemModel>
        {
            new LectureItemModel
            {
                Title = "المحاضرة الأولى: مقدمة في .NET MAUI",
                Status = "تمت بنجاح • تم رفع المادة العلمية",
                LectureNumber = "محاضرة 1"
            },
            new LectureItemModel
            {
                Title = "المحاضرة الثانية: بناء واجهات المستخدم XAML",
                Status = "تمت بنجاح • تم رفع المادة العلمية",
                LectureNumber = "محاضرة 2"
            },
            new LectureItemModel
            {
                Title = "المحاضرة الثالثة: ربط البيانات MVVM",
                Status = "مقررة الأسبوع القادم",
                LectureNumber = "محاضرة 3"
            }
        };

        AddLectureCommand = new Command(async () => await OnAddLectureExecuted());
    }

    private async Task OnAddLectureExecuted()
    {
        await Shell.Current.GoToAsync(nameof(AddUpdateLecturePage));
    }
}
