﻿
using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Metrino.Development.Studio.Library.Converters
{
    public class TreeLevelToMarginConverter : IValueConverter
    {
        public double Indent { get; set; } = 15;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var level = (int)value;
            var margin = new Thickness(level * Indent, 0, 0, 0);

            return margin;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
