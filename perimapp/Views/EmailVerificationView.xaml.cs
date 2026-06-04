using perimapp.ViewModels;
using Microsoft.Maui.Controls;

namespace perimapp.Views
{
    public partial class EmailVerificationView : ContentPage
    {
        private EmailVerificationViewModel _viewModel;

        public EmailVerificationView()
        {
            InitializeComponent();

            _viewModel = new EmailVerificationViewModel(this);
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