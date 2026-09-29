using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using perimapp.Services;
using perimapp.ViewModels;

namespace perimapp.Views;

public partial class AddProductView : ContentPage
{
    private AddProductViewModel _viewModel;

    public AddProductView(LocalProductService localProductService, LocalUserService localUserService, ApiProductService apiProductService)
    {
        InitializeComponent();
        _viewModel = new AddProductViewModel(localProductService, localUserService, apiProductService);
        BindingContext = _viewModel;

        SizeChanged += (_, __) => AdjustDatePickerWidth();
        DlcPicker.DateSelected += (_, __) => AdjustDatePickerWidth();
        AdjustDatePickerWidth();
    }

    private void AdjustDatePickerWidth()
    {
        if (Width <= 0)
            return;

        string format = string.IsNullOrWhiteSpace(DlcPicker.Format)
            ? "dd/MM/yyyy"
            : DlcPicker.Format;
        string sample = ((DateTime)DlcPicker.Date).ToString(format, CultureInfo.CurrentCulture);

        double fontSize = DlcPicker.FontSize > 0 ? DlcPicker.FontSize : 18;
        var probe = new Label
        {
            Text = sample,
            FontSize = fontSize,
            FontFamily = DlcPicker.FontFamily,
        };

        double measured = probe.Measure(double.PositiveInfinity, double.PositiveInfinity).Width;

        double target = measured + 24;
        double max = Math.Min(Width * 0.6, 260);
        double min = 140;
        target = Math.Max(min, Math.Min(max, target));

        DlcPicker.WidthRequest = target;
        DlcBorder.WidthRequest = target + 16;
    }
}