using Microsoft.Maui.Controls;

namespace Ostathi;

public partial class EditStudentGradesPage : ContentPage
{
    public EditStudentGradesPage()
    {
        InitializeComponent();
        
        BindingContext = new EditStudentGradesPageViewModel();
    }
}
