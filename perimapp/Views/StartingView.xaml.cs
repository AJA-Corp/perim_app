using Microsoft.Maui.Controls;
using perimapp.ViewModels;

namespace perimapp.Views
{
    public partial class StartingView : ContentPage
    {
        public StartingView(StartingViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}