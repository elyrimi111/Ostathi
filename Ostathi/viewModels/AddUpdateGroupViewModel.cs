using Microsoft.Maui.Controls;

namespace Ostathi.viewModels
{
    // استقبال المعلمة (GroupId) الممررة عبر مسار الـ Shell
    [QueryProperty(nameof(GroupId), "GroupId")]
    public class AddUpdateGroupViewModel : BindableObject
    {
        private string _groupId = string.Empty;
        public string GroupId
        {
            get => _groupId;
            set
            {
                if (_groupId != value)
                {
                    _groupId = value;
                    OnPropertyChanged(nameof(GroupId));
                    
                    // هنا يمكنك لاحقاً استخدام الـ GroupId لجلب بيانات المجموعة من قاعدة البيانات
                }
            }
        }

        public AddUpdateGroupViewModel()
        {
        }
    }
}
