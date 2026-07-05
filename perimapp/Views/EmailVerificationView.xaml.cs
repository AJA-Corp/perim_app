using perimapp.ViewModels;
using Microsoft.Maui.Controls;
using perimapp.Services;

namespace perimapp.Views
{
    public partial class EmailVerificationView : ContentPage
    {
        private EmailVerificationViewModel _viewModel;

        public EmailVerificationView(EmailVerificationViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel?.StopTimer();
        }
    }
}