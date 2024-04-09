using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.IO;

namespace Metrino.Development.Studio.Library.Converters;

public class FilenameToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        //if (value == null)
        //    return Color.FromArgb(0, 0, 0, 0);

        var p = value as string;
        if (p == null) return null;

        var extension = Path.GetExtension(p);
        switch (extension)
        {
            case ".cs": return "\ue648";
            case ".lua": return "\ue620";
            case ".json": return "\ueb0f";
            case ".csv": return "\ue64a";
            case ".iolm": return "\uea98";
            case ".olx": return "\uea98";
            case ".trcx": return "\uea98";
            case ".trc": return "\uea98";
            default: return "\uea7b";
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}
