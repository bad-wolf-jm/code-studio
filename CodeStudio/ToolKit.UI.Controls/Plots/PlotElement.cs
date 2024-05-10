using Metrino.Development.Core;

using CommunityToolkit.Mvvm.ComponentModel;

namespace ToolKit.UI.Controls;

public partial class PlotElementData : ObservableObject
{
    [ObservableProperty] bool _isVisible = true;
    [ObservableProperty] string _name = string.Empty;
    [ObservableProperty] Color _lineColor = ColorNames.white_smoke;
    [ObservableProperty] Color _fillColor = ColorNames.white_smoke;
    [ObservableProperty] double _lineWidth = 1.0;
    [ObservableProperty] LineStyle _lineStyle = LineStyle.Solid;
}

public partial class LineGraphData : PlotElementData
{
    [ObservableProperty] double[] _x = new double[] {};
    [ObservableProperty] double[] _y = new double[] {};
    [ObservableProperty] double _opacity = 1.0;
}

public partial class RectangleData : PlotElementData
{
    [ObservableProperty] double _xMin = 0.0;
    [ObservableProperty] double _xMax = 0.0;
    [ObservableProperty] double _yMin = 0.0;
    [ObservableProperty] double _yMax = 0.0;
}

public partial class VerticalLineData : PlotElementData
{
    [ObservableProperty] double _x = 0.0;
}

public partial class HorizontalLineData : PlotElementData
{
    [ObservableProperty] double _y = 0.0;
}

public partial class MarkerData : PlotElementData
{
    [ObservableProperty] double _x = 0.0;
    [ObservableProperty] double _y = 0.0;
    [ObservableProperty] MarkerStyle _style = MarkerStyle.FilledSquare;
    [ObservableProperty] double _size = 0.0;
    [ObservableProperty] string _label = "";
    [ObservableProperty] Color _labelColor = ColorNames.white_smoke;
    [ObservableProperty] double _labelFontSize = 12.0;
}
