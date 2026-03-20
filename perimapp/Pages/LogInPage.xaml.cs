using perimapp.Data;
using perimapp.Services;
using perimapp.ViewModels;
using Microsoft.Maui.Controls;

namespace perimapp.Pages;

public partial class LogInPage : ContentPage
{
    public LogInPage()
    {
        InitializeComponent();
        BindingContext = new LogInViewModel(this);
    }
}
