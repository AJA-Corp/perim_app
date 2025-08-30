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
            if (value is int daysRemaining)
            {
                if (daysRemaining < 0)
                    return Colors.DimGray;
                if (daysRemaining <= 1)
                    return Colors.DarkRed;
                if (daysRemaining <= 2)
                    return Color.FromArgb("#F94144"); // Red
                if (daysRemaining <= 3)
                    return Color.FromArgb("#F8961E"); // Orange
                if (daysRemaining <= 5)
                    return Color.FromArgb("#F9C74F"); // Yellow
                if (daysRemaining <= 7)
                    return Color.FromArgb("#8CD6BF"); // Green
                
                return Color.FromArgb("#30C2FF");     // Blue
            }
            return Colors.Gray;
        }

        // Ajout des '?' aux paramètres pour correspondre à l'interface IValueConverter
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}