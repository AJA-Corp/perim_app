using perimapp.Data;
using perimapp.Services;
using perimapp.ViewModels;
using Microsoft.Maui.Controls;

namespace perimapp.Views;

public partial class LogInView : ContentPage
{
    public LogInView()
    {
        InitializeComponent();
        BindingContext = new LogInViewModel(this);
    }
}
