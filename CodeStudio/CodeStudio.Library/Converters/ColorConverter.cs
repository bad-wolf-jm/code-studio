using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Metrino.Development.Studio.Library.Converters;

public class ColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return Color.FromArgb(0, 0, 0, 0);

        var p = (Development.Core.Color)value;

        var physicalEventColor = Color.FromArgb(
            System.Convert.ToByte(p.A), 
            System.Convert.ToByte(p.R), 
            System.Convert.ToByte(p.G), 
            System.Convert.ToByte(p.B)
           );

        return new SolidColorBrush(physicalEventColor);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}
