using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class CreateHubViewModel : ObservableObject
{
    public ICommand ClosePageCommand { get; }
    public ICommand AddStudentCommand { get; }
    public ICommand AddGroupCommand { get; }
    public ICommand AddLectureCommand { get; }
    public ICommand AddAssignmentCommand { get; }
    public ICommand AddGroupSubjectCommand { get; }

    public CreateHubViewModel()
    {
        ClosePageCommand = new AsyncRelayCommand(ClosePageAsync);
        AddStudentCommand = new AsyncRelayCommand(AddStudentAsync);
        AddGroupCommand = new AsyncRelayCommand(AddGroupAsync);
        AddLectureCommand = new AsyncRelayCommand(AddLectureAsync);
        AddAssignmentCommand = new AsyncRelayCommand(AddAssignmentAsync);
        AddGroupSubjectCommand = new AsyncRelayCommand(AddGroupSubjectAsync);
    }

    private async Task ClosePageAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("..");
        }
    }

    private async Task AddStudentAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync(nameof(AddUpdateStudent));
        }
    }

    private async Task AddGroupAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync(nameof(AddUpdateGroup));
        }
    }

    private async Task AddLectureAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync(nameof(AddUpdateLecturePage));
        }
    }

    private async Task AddAssignmentAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync(nameof(AddUpdateAssignmentPage));
        }
    }

    private async Task AddGroupSubjectAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync(nameof(AddGroupSubjectPage));
        }
    }
}
