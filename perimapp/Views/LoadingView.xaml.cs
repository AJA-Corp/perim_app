using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using perimapp.ViewModels;

namespace perimapp.Views;

public partial class LoadingView : ContentPage
{
    private LoadingViewModel _viewModel;

    public LoadingView(LoadingViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAppCommand.ExecuteAsync(null);
    }
}
