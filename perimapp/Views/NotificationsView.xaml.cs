using Microsoft.Maui.Controls;
using perimapp.Services;
using perimapp.ViewModels;

namespace perimapp.Views
{
    public partial class NotificationsView : ContentPage
    {
        private readonly NotificationsViewModel _viewModel;

        public NotificationsView(LocalProductService localProductService, LocalUserService localUserService)
        {
            InitializeComponent();
            _viewModel = new NotificationsViewModel(localProductService, localUserService);
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadNotificationsAsync();
        }
    }
}
