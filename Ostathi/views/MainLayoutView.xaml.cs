using Ostathi.ViewModels;

namespace Ostathi;

public partial class MainLayoutView : ContentPage
{
    public MainLayoutView()
    {
        InitializeComponent();
        
        // جلب الـ ViewModel من حاوية الخدمات تلقائياً بدون تمريره في الـ Constructor
        BindingContext = Application.Current?.Handler?.MauiContext?.Services.GetService<MainLayoutViewModel>();
    }
}
