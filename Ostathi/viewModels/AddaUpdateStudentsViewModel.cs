using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Ostathi.Models;
using Ostathi.Services;

namespace Ostathi;

public class AddaUpdateStudentsViewModel : INotifyPropertyChanged, IQueryAttributable
{
    private readonly StudentService _studentService;
    private int _studentId = 0; // Holds the ID for update operations (0 for new student)

    private string _firstName = string.Empty;
    private string _secondName = string.Empty;
    private string _thirdName = string.Empty;
    private string _lastName = string.Empty;
    private string _academicId = string.Empty;
    private string _phoneNumber = string.Empty;
    private string? _selectedMajor;
    private string? _selectedYear;
    private string? _selectedGroupText;
    private int _groupId;
    private int _majorId;

    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }

    public string SecondName
    {
        get => _secondName;
        set => SetProperty(ref _secondName, value);
    }

    public string ThirdName
    {
        get => _thirdName;
        set => SetProperty(ref _thirdName, value);
    }

    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }

    public string AcademicId
    {
        get => _academicId;
        set => SetProperty(ref _academicId, value);
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set => SetProperty(ref _phoneNumber, value);
    }

    // ربط اختيار التخصص في الواجهة وتحويله إلى MajorId برمجياً
    public string? SelectedMajor
    {
        get => _selectedMajor;
        set
        {
            if (SetProperty(ref _selectedMajor, value))
            {
                _majorId = value switch
                {
                    "علوم الحاسب (CIS)" => 1,
                    "تقنية المعلومات (IT)" => 2,
                    "هندسة البرمجيات (SE)" => 3,
                    _ => 1
                };
            }
        }
    }

    public int MajorId
    {
        get => _majorId;
        set => SetProperty(ref _majorId, value);
    }

    public string? SelectedYear
    {
        get => _selectedYear;
        set => SetProperty(ref _selectedYear, value);
    }

    public string? SelectedGroupText
    {
        get => _selectedGroupText;
        set
        {
            if (SetProperty(ref _selectedGroupText, value))
            {
                // Map group text selection to GroupId integer
                _groupId = value switch
                {
                    "مجموعة C#" => 1,
                    "مجموعة قواعد البيانات" => 2,
                    "مجموعة MAUI" => 3,
                    _ => 1
                };
            }
        }
    }

    public int GroupId
    {
        get => _groupId;
        set => SetProperty(ref _groupId, value);
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    // Inject StudentService automatically via Dependency Injection
    public AddaUpdateStudentsViewModel(StudentService studentService)
    {
        _studentService = studentService;
        SaveCommand = new Command(OnSave);
        CancelCommand = new Command(OnCancel);
    }

    // Receives query navigation parameters to populate data when editing an existing student
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Student", out var studentObj) && studentObj is Student student)
        {
            _studentId = student.Id;
            FirstName = student.FirstName;
            SecondName = student.SecondName;
            ThirdName = student.ThirdName;
            LastName = student.LastName;
            AcademicId = student.AcademicId;
            GroupId = student.GroupId;
            MajorId = student.MajorId;
            SelectedYear = student.AcademicYear;
            
            // Map GroupId back to Picker text
            SelectedGroupText = student.GroupId switch
            {
                1 => "مجموعة C#",
                2 => "مجموعة قواعد البيانات",
                3 => "مجموعة MAUI",
                _ => null
            };

            // Map MajorId back to Picker text
            SelectedMajor = student.MajorId switch
            {
                1 => "علوم الحاسب (CIS)",
                2 => "تقنية المعلومات (IT)",
                3 => "هندسة البرمجيات (SE)",
                _ => null
            };
        }
    }

    private async void OnSave()
    {
        if (string.IsNullOrWhiteSpace(FirstName))
        {
            await Shell.Current.DisplayAlert("Warning", "Please enter the student's first name", "OK");
            return;
        }

        try
        {
            // 1. Prepare student data including MajorId & GroupId foreign keys
            var student = new Student
            {
                Id = _studentId,
                FirstName = FirstName,
                SecondName = SecondName,
                ThirdName = ThirdName,
                LastName = LastName,
                AcademicId = AcademicId,
                MajorId = _majorId > 0 ? _majorId : 1, // ربط المعرف الأجنبي للتخصص
                GroupId = _groupId > 0 ? _groupId : 1, // ربط المعرف الأجنبي للمجموعة
                AcademicYear = SelectedYear ?? string.Empty
            };

            // 2. Perform actual save or update operation in SQLite database via service
            await _studentService.SaveStudentAsync(student);

            // 3. Send instant notification via MessengerService to update student list UI
            MessengerService.NotifyStudentAdded();

            await Shell.Current.DisplayAlert("Success", "Student data saved successfully", "OK");
            await Shell.Current.GoToAsync(".."); // Return to the previous page
        }
        catch (Exception ex)
        {
            // Catch and display detailed error message if database save fails
            string errorDetails = ex.InnerException?.Message ?? ex.Message;
            await Shell.Current.DisplayAlert("Database Error", $"Failed to save student: {errorDetails}", "OK");
        }
    }

    private async void OnCancel()
    {
        await Shell.Current.GoToAsync("..");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
