using System;
using perimapp.Data;
using perimapp.Models;
using perimapp.Services;
using perimapp.ViewModels;
using Microsoft.Maui.Controls;

namespace perimapp.Pages
{
    public partial class SignUpPage : ContentPage
    {
        public SignUpPage()
        {
            InitializeComponent();
            BindingContext = new SignUpViewModel(this);
        }
    }
}
