using perimapp.Services;
using perimapp.ViewModels;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace perimapp.Pages
{
    [QueryProperty(nameof(SessionId), "sessionId")]
    public partial class EmailVerificationPage : ContentPage
    {
        private EmailVerificationViewModel _viewModel;

        public string SessionId
        {
            get => _viewModel?.SessionId;
            set
            {
                if (_viewModel != null)
                {
                    _viewModel.SessionId = value;
                }
            }
        }

        public EmailVerificationPage()
        {
            InitializeComponent();
            _viewModel = new EmailVerificationViewModel(this);
            BindingContext = _viewModel;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.StopTimer();
        }
    }
}