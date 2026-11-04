using System;
using Microsoft.Maui.Controls;
using Ostathi.viewModels; // استخدم حرف v صغير ليتطابق مع اسم مجلدك

namespace Ostathi;

public partial class MainLayoutView : ContentPage
{
    public MainLayoutView()
    {
        InitializeComponent();
        
        // ربط الصفحة بالـ ViewModel الخاص بها
        BindingContext = new MainLayoutViewModel();
    }
}
