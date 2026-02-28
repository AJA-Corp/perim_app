// Converters/DlcColorConverter.cs
using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace perimapp.Converters
{
    public class DlcColorConverter : IValueConverter
    {
        // Ajout des '?' aux paramètres pour correspondre à l'interface IValueConverter
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not int days)
                return Color.FromArgb("#808080");

            return days switch
            {
                < 0 => Color.FromArgb("#696969"),
                <= 1 => Color.FromArgb("#FF0000"),
                <= 2 => Color.FromArgb("#F94144"),
                <= 3 => Color.FromArgb("#F8961E"),
                <= 5 => Color.FromArgb("#F9C74F"),
                <= 7 => Color.FromArgb("#8CD6BF"),
                _ => Color.FromArgb("#30C2FF")
            };
        }

        // Ajout des '?' aux paramètres pour correspondre à l'interface IValueConverter
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}