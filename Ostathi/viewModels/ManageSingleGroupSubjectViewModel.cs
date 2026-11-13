using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace Ostathi
{
    [QueryProperty(nameof(CurrentGroupSubject), "GroupSubject")]
    public partial class ManageSingleGroupSubjectViewModel : ObservableObject
    {
        [ObservableProperty]
        private GroupSubject? _currentGroupSubject;

        [RelayCommand]
        private async Task SaveChangesAsync()
        {
            await Shell.Current.DisplayAlert("نجاح", "تم حفظ التعديلات بنجاح", "موافق");
        }
    }
}
