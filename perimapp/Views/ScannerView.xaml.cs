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
        _viewModel = new ScannerViewModel(this);
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