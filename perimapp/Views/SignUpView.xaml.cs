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
        public SignUpView(SignUpViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private void OnTogglePasswordVisibilityClicked(object sender, EventArgs e)
        {
            PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
            PasswordIcon.Source = PasswordEntry.IsPassword ? "visibility_off_green.png" : "visibility_green.png";
        }

        private void OnToggleConfirmPasswordVisibilityClicked(object sender, EventArgs e)
        {
            ConfirmPasswordEntry.IsPassword = !ConfirmPasswordEntry.IsPassword;
            ConfirmPasswordIcon.Source = ConfirmPasswordEntry.IsPassword ? "visibility_off_green.png" : "visibility_green.png";
        }
    }
}
