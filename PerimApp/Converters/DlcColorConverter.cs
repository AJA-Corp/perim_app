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
                if (daysRemaining <= 0)
                    return Colors.DarkRed;
                if (daysRemaining <= 2)
                    return Colors.Red;
                if (daysRemaining <= 7)
                    return Colors.Orange;
                
                return Colors.Green;
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