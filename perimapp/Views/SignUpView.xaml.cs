using System;
using perimapp.Data;
using perimapp.Models;
using perimapp.Services;
using perimapp.ViewModels;
using Microsoft.Maui.Controls;

namespace perimapp.Views
{
    public partial class SignUpView : ContentPage
    {
        public SignUpView()
        {
            InitializeComponent();
            BindingContext = new SignUpViewModel(this);
        }
    }
}
