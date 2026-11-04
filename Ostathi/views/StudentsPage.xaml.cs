namespace Ostathi;

public partial class StudentsPage : ContentView
{
    // استقبال الـ ViewModel عبر الـ Constructor وحقنه تلقائياً
    public StudentsPage(StudentsViewModel viewModel)
    {
        InitializeComponent();
        
        // تعيين الـ ViewModel كـ BindingContext للـ Page
        BindingContext = viewModel;
    }
}
