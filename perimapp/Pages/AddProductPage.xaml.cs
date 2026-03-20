using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using perimapp.Data;
using perimapp.Services;
using perimapp.Models;
using Microsoft.Maui.Controls;
using perimapp.ViewModels;

namespace perimapp.Pages;

public partial class AddProductPage : ContentPage 
{
    private AddProductViewModel _viewModel;

    public AddProductPage()
    {
        InitializeComponent();
        _viewModel = new AddProductViewModel(this);
        BindingContext = _viewModel;
        
        // Ajuste la largeur du sélecteur de date au démarrage et quand ça change
        SizeChanged += (_, __) => AdjustDatePickerWidth();
        DlcPicker.DateSelected += (_, __) => AdjustDatePickerWidth();
        // Appel initial
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

        double target = measured + 24; // padding interne
        double max = Math.Min(Width * 0.6, 260);
        double min = 140;
        target = Math.Max(min, Math.Min(max, target));

        DlcPicker.WidthRequest = target;
        DlcBorder.WidthRequest = target + 16;
    }
}
