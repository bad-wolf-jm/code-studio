using System.Collections.ObjectModel;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

using ScottPlot;
using ScottPlot.Avalonia;

using Metrino.Development.Core;
using Metrino.Development.UI.Core;
using System;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using ScottPlot.Plottable;

namespace Metrino.Development.Studio.Library.Controls;

public class PlotControl : TemplatedControl
{
    public static readonly StyledProperty<string> TitleProperty
        = AvaloniaProperty.Register<PlotControl, string>(nameof(Title));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<string> XAxisTitleProperty
        = AvaloniaProperty.Register<PlotControl, string>(nameof(XAxisTitle));

    public string XAxisTitle
    {
        get => GetValue(XAxisTitleProperty);
        set => SetValue(XAxisTitleProperty, value);
    }

    public static readonly StyledProperty<string> YAxisTitleProperty
        = AvaloniaProperty.Register<PlotControl, string>(nameof(YAxisTitle));

    public string YAxisTitle
    {
        get => GetValue(YAxisTitleProperty);
        set => SetValue(YAxisTitleProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<PlotElementData>> PlotElementSourceProperty
        = AvaloniaProperty.Register<PlotControl, ObservableCollection<PlotElementData>>(nameof(PlotElementSource));

    public ObservableCollection<PlotElementData> PlotElementSource
    {
        get => GetValue(PlotElementSourceProperty);
        set => SetValue(PlotElementSourceProperty, value);
    }

    static PlotControl()
    {
        TitleProperty.Changed.AddClassHandler<PlotControl>(OnTitleChanged);
        XAxisTitleProperty.Changed.AddClassHandler<PlotControl>(OnXAxisTitleChanged);
        YAxisTitleProperty.Changed.AddClassHandler<PlotControl>(OnYAxisTitleChanged);
        PlotElementSourceProperty.Changed.AddClassHandler<PlotControl>(OnPlotElementsSourceChanged);
    }

    AvaPlot? _plot;
    Plot? _plotArea;
    Dictionary<PlotElementData, IPlottable> _plottableElements = new Dictionary<PlotElementData, IPlottable>();
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _plot = e.NameScope?.Find<AvaPlot>("PlotArea");
        _plotArea = _plot?.Plot;
        if (_plotArea == null || _plot == null)
            return;

        _plotArea.Style(Style.Gray1);

        _plotArea.Title(Title);
        _plotArea.XLabel(XAxisTitle);
        _plotArea.YLabel(YAxisTitle);

        if(PlotElementSource != null)
            AddGraphElements(PlotElementSource);

        _plotArea?.Render();
        _plot?.Render();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        _plotArea?.Clear();

        if (PlotElementSource != null)
            AddGraphElements(PlotElementSource);
    }

    private static void OnTitleChanged(PlotControl control, AvaloniaPropertyChangedEventArgs e)
    {
        control._plotArea?.Title(control.Title);

        control._plotArea?.Render();
        control._plot?.Render();
    }

    private static void OnXAxisTitleChanged(PlotControl control, AvaloniaPropertyChangedEventArgs args)
    {
        control._plotArea?.XLabel(control.XAxisTitle);

        control._plotArea?.Render();
        control._plot?.Render();
    }

    private static void OnYAxisTitleChanged(PlotControl control, AvaloniaPropertyChangedEventArgs args)
    {
        control._plotArea?.YLabel(control.YAxisTitle);

        control._plotArea?.Render();
        control._plot?.Render();
    }

    private static System.Drawing.Color ConvertColor(Color c, double alpha = 1.0)
    {
        int alphaValue = Convert.ToInt32(alpha);
        return System.Drawing.Color.FromArgb(
            Convert.ToInt32(alphaValue * 255), Convert.ToInt32(c.R), Convert.ToInt32(c.G), Convert.ToInt32(c.B)
        );
    }

    private static System.Drawing.Color ConvertColor(Color c)
    {
        return System.Drawing.Color.FromArgb(
            Convert.ToInt32(c.A), Convert.ToInt32(c.R), Convert.ToInt32(c.G), Convert.ToInt32(c.B)
        );
    }

    private static ScottPlot.LineStyle ConvertLineStyle(UI.Core.LineStyle c)
    {
        return (ScottPlot.LineStyle)c;
    }

    private MarkerShape ConvertMarkerShape(MarkerStyle style)
    {
        return (MarkerShape)style;
    }

    private void RemoveGraphElements(IEnumerable<PlotElementData> oldValue)
    {
        if (_plotArea == null)
            return;

        foreach (var graphElement in oldValue)
        {
            graphElement.PropertyChanged -= OnPlotElementsSourceChanged;
            if (_plottableElements.ContainsKey(graphElement))
            {
                _plotArea.Remove(_plottableElements[graphElement]);
                _plottableElements.Remove(graphElement);
            }
        }

        _plotArea.Clear();
    }

    void AddGraphElements(IEnumerable<PlotElementData> graphElements)
    {
        if (_plotArea == null)
            return;

        foreach (var graphElement in graphElements)
        {
            graphElement.PropertyChanged += OnPlotElementsSourceChanged;

            if (graphElement is LineGraphData lineData)
            {

                double[] x = null;
                double[] y = null;

                if (lineData.X == null || lineData.Y == null)
                {
                    x = new double[] { 0.0 };
                    y = new double[] { 0.0 };
                }
                else
                {
                    var validIndices = Enumerable.Range(0, lineData.X.Length)
                        .Where(i => !(double.IsNaN(lineData.X[i]) || double.IsNaN(lineData.Y[i])))
                        .Select(i => new { x = lineData.X[i], y = lineData.Y[i] }).ToList();

                    x = validIndices.Select(i => i.x).ToArray();
                    y = validIndices.Select(i => i.y).ToArray();

                    if(x.Length == 0 || y.Length == 0)
                    {
                        x = new double[] { 0.0 };
                        y = new double[] { 0.0 };
                    }
                }

                var signalXYPlot = _plotArea.AddSignalXY(x, y);
                signalXYPlot.LineWidth = lineData.LineWidth;
                signalXYPlot.LineStyle = (ScottPlot.LineStyle)lineData.LineStyle;
                if (lineData.LineColor != null)
                    signalXYPlot.Color = ConvertColor(lineData.LineColor, lineData.Opacity);

                _plottableElements[graphElement] = signalXYPlot;
            }
            else if (graphElement is RectangleData rectangleData)
            {
                var rectanglePlot = _plotArea.AddRectangle(
                    xMin: rectangleData.XMin, xMax: rectangleData.XMax,
                    yMin: rectangleData.YMin, yMax: rectangleData.YMax);

                rectanglePlot.BorderColor = ConvertColor(rectangleData.LineColor);
                rectanglePlot.BorderLineWidth = (float)rectangleData.LineWidth;
                rectanglePlot.BorderLineStyle = ConvertLineStyle(rectangleData.LineStyle);
                rectanglePlot.Color = ConvertColor(rectangleData.FillColor);

                _plottableElements[graphElement] = rectanglePlot;
            }
            else if (graphElement is VerticalLineData vLine)
            {
                var vLinePlot = _plotArea.AddVerticalLine(vLine.X);
                vLinePlot.LineWidth = (float)vLine.LineWidth;
                vLinePlot.LineStyle = ConvertLineStyle(vLine.LineStyle);
                vLinePlot.Color = ConvertColor(vLine.LineColor);

                _plottableElements[graphElement] = vLinePlot;
            }
            else if (graphElement is HorizontalLineData hLine)
            {
                var hLinePlot = _plotArea.AddHorizontalLine(hLine.Y);
                hLinePlot.LineWidth = (float)hLine.LineWidth;
                hLinePlot.LineStyle = ConvertLineStyle(hLine.LineStyle);
                hLinePlot.Color = ConvertColor(hLine.LineColor);

                _plottableElements[graphElement] = hLinePlot;
            }
            else if (graphElement is MarkerData hMarker)
            {
                var markerPlot = _plotArea.AddMarker( hMarker.X, hMarker.Y);
                markerPlot.Text = hMarker.Label;
                markerPlot.MarkerLineWidth = (float)hMarker.LineWidth;
                markerPlot.MarkerSize = (float)hMarker.Size;
                markerPlot.MarkerShape = ConvertMarkerShape(hMarker.Style);
                markerPlot.Color = ConvertColor(hMarker.FillColor);
                markerPlot.MarkerColor = ConvertColor(hMarker.FillColor);
                markerPlot.TextFont.Color = ConvertColor(hMarker.LabelColor);
                markerPlot.TextFont.Size = (float)hMarker.LabelFontSize;
                markerPlot.TextFont.Alignment = Alignment.UpperCenter;
                

                _plottableElements[graphElement] = markerPlot;
            }
        }
    }


    private static void OnPlotElementsSourceChanged(PlotControl control, AvaloniaPropertyChangedEventArgs args)
    {
        var oldValue = (ObservableCollection<PlotElementData>)args?.OldValue;
        var newValue = (ObservableCollection<PlotElementData>)args?.NewValue;

        if (oldValue != null)
        {
            oldValue.CollectionChanged -= control.OnPlotElementsSourceCollectionChanged;
            control.RemoveGraphElements(oldValue);
        }

        if (newValue != null)
        {
            control.AddGraphElements(newValue);
            newValue.CollectionChanged += control.OnPlotElementsSourceCollectionChanged;
        }

        if (control._plotArea == null)
            return;

        control._plotArea?.Render();
        control._plot?.Render();
    }


    private void OnPlotElementsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            _plotArea?.Clear();
            _plottableElements.Clear();
        }
        else
        {
            var oldItems = e.OldItems?.OfType<PlotElementData>();
            if (oldItems != null)
                RemoveGraphElements(oldItems);

            var newItems = e.NewItems?.OfType<PlotElementData>();
            if (newItems != null)
                AddGraphElements(newItems);
        }

        _plotArea?.Render();
        _plot?.Render();
    }

    private void OnPlotElementsSourceChanged(object? sender, PropertyChangedEventArgs e)
    {
        var graphElement = (PlotElementData)sender;
        if (graphElement == null) return;


        if (graphElement is LineGraphData lineData)
        {
            var signalXYPlot = _plottableElements[graphElement] as SignalPlotXY;
            if (signalXYPlot == null) return;

            signalXYPlot.IsVisible = lineData.IsVisible;  
            
            if(lineData.X == null || lineData.Y == null)
            {
                signalXYPlot.Xs = lineData.X;
                signalXYPlot.Ys = lineData.Y;
                signalXYPlot.MinRenderIndex = 0;
                signalXYPlot.MaxRenderIndex = signalXYPlot.Ys.Length - 1;
            }
            else
            {
                var validIndices = Enumerable.Range(0, lineData.X.Length)
                    .Where(i => !(double.IsNaN(lineData.X[i]) || double.IsNaN(lineData.Y[i])))
                    .Select(i => new { x = lineData.X[i], y = lineData.Y[i] }).ToList();

                signalXYPlot.Xs = validIndices.Select(i => i.x).ToArray();
                signalXYPlot.Ys = validIndices.Select(i => i.y).ToArray();
                signalXYPlot.MinRenderIndex = 0;
                signalXYPlot.MaxRenderIndex = signalXYPlot.Ys.Length - 1;
            }

            signalXYPlot.LineWidth = lineData.LineWidth;
            signalXYPlot.LineStyle = (ScottPlot.LineStyle)lineData.LineStyle;
            if (lineData.LineColor != null)
                signalXYPlot.Color = ConvertColor(lineData.LineColor, lineData.Opacity);

            //_plottableElements[graphElement] = signalXYPlot;
        }
        else if (graphElement is RectangleData rectangleData)
        {
            var rectanglePlot = _plottableElements[graphElement] as RectanglePlot;
            if (rectanglePlot == null) return;

            rectanglePlot.IsVisible = rectangleData.IsVisible;
            rectanglePlot.Rectangle = new CoordinateRect(rectangleData.XMin, rectangleData.XMax, rectangleData.YMin, rectangleData.YMax);
            rectanglePlot.BorderColor = ConvertColor(rectangleData.LineColor);
            rectanglePlot.BorderLineWidth = (float)rectangleData.LineWidth;
            rectanglePlot.BorderLineStyle = ConvertLineStyle(rectangleData.LineStyle);
            rectanglePlot.Color = ConvertColor(rectangleData.FillColor);

            //_plottableElements[graphElement] = rectanglePlot;
        }
        else if (graphElement is VerticalLineData vLine)
        {
            var vLinePlot = _plottableElements[graphElement] as VLine;
            if (vLinePlot == null) return;

            vLinePlot.IsVisible = vLine.IsVisible;
            vLinePlot.X = vLine.X;
            vLinePlot.LineWidth = (float)vLine.LineWidth;
            vLinePlot.LineStyle = ConvertLineStyle(vLine.LineStyle);
            vLinePlot.Color = ConvertColor(vLine.LineColor);

            //_plottableElements[graphElement] = vLinePlot;
        }
        else if (graphElement is HorizontalLineData hLine)
        {
            var hLinePlot = _plottableElements[graphElement] as HLine;
            if (hLinePlot == null) return;

            hLinePlot.IsVisible = hLine.IsVisible;
            hLinePlot.Y = hLine.Y;
            hLinePlot.LineWidth = (float)hLine.LineWidth;
            hLinePlot.LineStyle = ConvertLineStyle(hLine.LineStyle);
            hLinePlot.Color = ConvertColor(hLine.LineColor);

            //_plottableElements[graphElement] = hLinePlot;
        }

        _plotArea?.Render();
        _plot?.Render();
    }
}
