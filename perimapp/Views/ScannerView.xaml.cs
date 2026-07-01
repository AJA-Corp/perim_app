using BarcodeScanning;
using perimapp.ViewModels;

namespace perimapp.Views;

public partial class ScannerView : ContentPage
{
    private ScannerViewModel _viewModel;

    public Action<string> OnBarcodeScanned
    {
        get => _viewModel?.OnBarcodeScanned;
        set
        {
            if (_viewModel != null)
                _viewModel.OnBarcodeScanned = value;
        }
    }

    public ScannerView()
    {
        InitializeComponent();
        var services = App.Current?.Handler?.MauiContext?.Services;
        var navService = (perimapp.Services.INavigationService?)services?.GetService(typeof(perimapp.Services.INavigationService));
        var dispService = (perimapp.Services.IDispatcherService?)services?.GetService(typeof(perimapp.Services.IDispatcherService));

        _viewModel = new ScannerViewModel(
            navService ?? new perimapp.Services.NavigationService(),
            dispService ?? new perimapp.Services.DispatcherService()
        );
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.StartCamera();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.StopCamera();
    }

    private void Camera_OnDetectionFinished(object sender, OnDetectionFinishedEventArg e)
    {
        _viewModel.HandleBarcodeScanned(e);
    }
}