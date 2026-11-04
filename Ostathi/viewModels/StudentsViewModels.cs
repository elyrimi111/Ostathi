using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Ostathi
{
    public class StudentsViewModel : BindableObject
    {
        #region Fields & Properties

        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    OnPropertyChanged();
                    FilterStudents(); // استدعاء دالة الفلترة عند تغيير النص
                }
            }
        }

        private string _selectedSpecialization;
        public string SelectedSpecialization
        {
            get => _selectedSpecialization;
            set
            {
                if (_selectedSpecialization != value)
                {
                    _selectedSpecialization = value;
                    OnPropertyChanged();
                    FilterStudents();
                }
            }
        }

        private string _selectedYear;
        public string SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (_selectedYear != value)
                {
                    _selectedYear = value;
                    OnPropertyChanged();
                    FilterStudents();
                }
            }
        }

        private string _selectedGroup;
        public string SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (_selectedGroup != value)
                {
                    _selectedGroup = value;
                    OnPropertyChanged();
                    FilterStudents();
                }
            }
        }

        // قائمة الطلاب (يمكنك استبدالها بنموذج الطالب الخاص بك StudentModel)
        private ObservableCollection<object> _filteredStudents;
        public ObservableCollection<object> FilteredStudents
        {
            get => _filteredStudents;
            set
            {
                _filteredStudents = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand ImportSheetCommand { get; }
        public ICommand AddStudentCommand { get; }

        #endregion

        #region Constructor

        public StudentsViewModel()
        {
            // تهيئة الأوامر
            ImportSheetCommand = new Command(OnImportSheetExecute);
            AddStudentCommand = new Command(OnAddStudentExecute);

            // تهيئة القوائم
            FilteredStudents = new ObservableCollection<object>();
        }

        #endregion

        #region Methods

        private void OnImportSheetExecute()
        {
            // TODO: أضف منطق اختيار واستيراد كشف الطلاب (Excel / CSV) هنا
            App.Current.MainPage.DisplayAlert("استيراد كشف", "تم النقر على زر استيراد كشف الطلاب", "حسناً");
        }

        private async void OnAddStudentExecute()
        {
            // فتح صفحة إضافة/تعديل طالب باستخدام مسار الـ Shell الحديث
            await Shell.Current.GoToAsync(nameof(AddUpdateStudent));
        }

        private void FilterStudents()
        {
            // TODO: ضع هنا منطق فلترة القائمة بناءً على SearchQuery والفلاتر الثلاثة
        }

        #endregion
    }
}
