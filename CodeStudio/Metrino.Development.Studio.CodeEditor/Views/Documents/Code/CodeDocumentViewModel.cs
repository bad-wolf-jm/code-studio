using Avalonia.Threading;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Metrino.Development.Builder;
using Metrino.Development.Core;
using Metrino.Development.Studio.Library.Controls;
using Metrino.Development.Studio.Library.Messages;
using Metrino.Development.UI.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Metrino.Development.Studio.Library.ViewModels;

public partial class ValueWatch : ObservableObject
{
    public string Name { get; set; }

    [ObservableProperty]
    string _value;
}

public partial class CodeDocumentViewModel : ObservableObject, UI.ViewModels.IDocument
{
    private string _path;

    [ObservableProperty]
    bool _scriptIsRunning = false;

    [ObservableProperty]
    bool _showProgressReport = false;

    [ObservableProperty]
    bool _isModified = false;

    [ObservableProperty]
    string _consoleText = "";

    [ObservableProperty]
    TextDocument _document = new TextDocument();

    [ObservableProperty]
    TextDocument _callbackDocument = new TextDocument();

    [ObservableProperty]
    bool _commandMode = true;

    TextDocument _consoleOut = new TextDocument();
    public TextDocument ConsoleOut => _consoleOut;

    ObservableCollection<ICompletionData> _codeCompletions = new ObservableCollection<ICompletionData>();
    public ObservableCollection<ICompletionData> CodeCompletions => _codeCompletions;

    ObservableCollection<string> _insights = new ObservableCollection<string>();
    public ObservableCollection<string> Insights => _insights;


    Dictionary<string, ValueWatch> _values = new Dictionary<string, ValueWatch>();
    ObservableCollection<ValueWatch> _watches = new ObservableCollection<ValueWatch>();
    public ObservableCollection<ValueWatch> Values => _watches;

    ObservableCollection<PlotElementData> _plots = new ObservableCollection<PlotElementData>();
    public ObservableCollection<PlotElementData> Plots => _plots;

    ApplicationModel _applicationModel;

    public CodeDocumentViewModel(ApplicationModel applicationModel, string path)
    {
        _applicationModel = applicationModel;
    }

    public async Task LoadPathAsync(string path, string branch)
    {
        _path = path;

        if (File.Exists(_path))
        {
            Document.Text = File.ReadAllText(_path);
            Document.FileName = _path;
            Document.Changing += Document_Changing;
            Document.Changed += Document_Changed;
            ScriptIsRunning = false;
        }
    }

    private void Document_Changing(object? sender, DocumentChangeEventArgs e)
    {
        IsModified = true;
    }

    private async void Document_Changed(object? sender, DocumentChangeEventArgs e)
    {

    }

    [RelayCommand]
    void Save(object parameter)
    {
        File.WriteAllText(_path, Document.Text);
        IsModified = false;
    }

#if false
    private Color Convert(Service.Color c)
    {
        return new Color { A = c.A, B = c.B, R = c.R, G = c.G };
    }

    Dictionary<string, PlotElementData> _plotElements = new Dictionary<string, PlotElementData>();

    private void UpdatePlot(LineGraphData plotElement, Service.LineGraphData lineData)
    {
        if (lineData.Configuration?.LineColor != null)
            plotElement.LineColor = Convert(lineData.Configuration.LineColor);

        if (lineData.Configuration?.LineStyle != null)
            plotElement.LineStyle = (LineStyle)lineData.Configuration.LineStyle;

        plotElement.LineWidth = lineData.Configuration?.LineWidth ?? plotElement.LineWidth;
        plotElement.X = lineData.X ?? plotElement.X;
        plotElement.Y = lineData.Y ?? plotElement.Y;
        plotElement.Opacity = lineData.Opacity ?? plotElement.Opacity;
    }

    private PlotElementData CreatePlot(string name, Service.LineGraphData lineData)
    {
        var plotElement = new LineGraphData() { Name = name };

        UpdatePlot(plotElement, lineData);

        return plotElement;
    }

    private void UpdatePlot(RectangleData plotElement, Service.RectangleData rectangleData)
    {
        if (rectangleData.Configuration?.LineColor != null)
            plotElement.LineColor = Convert(rectangleData.Configuration.LineColor);

        if (rectangleData.Configuration?.LineStyle != null)
            plotElement.LineStyle = (LineStyle)rectangleData.Configuration.LineStyle;

        if (rectangleData.Configuration?.FillColor != null)
            plotElement.FillColor = Convert(rectangleData.Configuration.FillColor);

        plotElement.LineWidth = rectangleData.Configuration?.LineWidth ?? plotElement.LineWidth;
        plotElement.XMin = rectangleData.XMin ?? plotElement.XMin;
        plotElement.YMin = rectangleData.YMin ?? plotElement.YMin;
        plotElement.XMax = rectangleData.XMax ?? plotElement.XMax;
        plotElement.YMax = rectangleData.YMax ?? plotElement.YMax;
    }

    private PlotElementData CreatePlot(string name, Service.RectangleData rectangleData)
    {
        var plotElement = new RectangleData() { Name = name };

        UpdatePlot(plotElement, rectangleData);

        return plotElement;
    }

    private void UpdatePlot(VerticalLineData plotElement, Service.VerticalLineData vLineData)
    {
        if (vLineData.Configuration?.LineColor != null)
            plotElement.LineColor = Convert(vLineData.Configuration.LineColor);

        if (vLineData.Configuration?.LineStyle != null)
            plotElement.LineStyle = (LineStyle)vLineData.Configuration.LineStyle;

        plotElement.LineWidth = vLineData.Configuration?.LineWidth ?? plotElement.LineWidth;
        plotElement.X = vLineData.X ?? plotElement.X;

    }

    private PlotElementData CreatePlot(string name, Service.VerticalLineData vLineData)
    {
        var plotElement = new VerticalLineData() { Name = name };

        UpdatePlot(plotElement, vLineData);

        return plotElement;
    }

    private void UpdatePlot(HorizontalLineData plotElement, Service.HorizontalLineData vLineData)
    {
        if (vLineData.Configuration?.LineColor != null)
            plotElement.LineColor = Convert(vLineData.Configuration.LineColor);

        if (vLineData.Configuration?.LineStyle != null)
            plotElement.LineStyle = (LineStyle)vLineData.Configuration.LineStyle;

        plotElement.LineWidth = vLineData.Configuration?.LineWidth ?? plotElement.LineWidth;
        plotElement.Y = vLineData.Y ?? plotElement.Y;

    }

    private PlotElementData CreatePlot(string name, Service.HorizontalLineData hLineData)
    {
        var plotElement = new HorizontalLineData() { Name = name };

        UpdatePlot(plotElement, hLineData);

        return plotElement;
    }

    private void Callbacks_OnPlot(object? sender, PlotEventArgs e)
    {
        var plotName = e.Name;
        var plotData = e.PlotData;

        if (plotData is Service.LineGraphData lineData)
        {
            int oldPlotElementIndex = -1;
            if (_plotElements.ContainsKey(plotName))
            {
                var oldPlotElement = _plotElements[plotName];
                oldPlotElementIndex = Plots.IndexOf(oldPlotElement);
                if (oldPlotElementIndex != -1)
                    Plots.Remove(oldPlotElement);
            }

            var plotElement = CreatePlot(plotName, lineData);

            _plotElements[plotName] = plotElement;
            if (oldPlotElementIndex != -1)
                Plots.Insert(oldPlotElementIndex, plotElement);
            else
                Plots.Add(plotElement);

            return;
        }

        if (plotData is Service.RectangleData rectangleData)
        {
            int oldPlotElementIndex = -1;
            if (_plotElements.ContainsKey(plotName))
            {
                var oldPlotElement = _plotElements[plotName];
                oldPlotElementIndex = Plots.IndexOf(oldPlotElement);
                if (oldPlotElementIndex != -1)
                    Plots.Remove(oldPlotElement);
            }

            var plotElement = CreatePlot(plotName, rectangleData);

            _plotElements[plotName] = plotElement;
            if (oldPlotElementIndex != -1)
                Plots.Insert(oldPlotElementIndex, plotElement);
            else
                Plots.Add(plotElement);

            return;
        }

        if (plotData is Service.VerticalLineData vLineData)
        {
            int oldPlotElementIndex = -1;
            if (_plotElements.ContainsKey(plotName))
            {
                var oldPlotElement = _plotElements[plotName];
                oldPlotElementIndex = Plots.IndexOf(oldPlotElement);
                if (oldPlotElementIndex != -1)
                    Plots.Remove(oldPlotElement);
            }

            var plotElement = CreatePlot(plotName, vLineData);

            _plotElements[plotName] = plotElement;
            if (oldPlotElementIndex != -1)
                Plots.Insert(oldPlotElementIndex, plotElement);
            else
                Plots.Add(plotElement);

            return;
        }

        if (plotData is Service.HorizontalLineData hLineData)
        {
            int oldPlotElementIndex = -1;
            if (_plotElements.ContainsKey(plotName))
            {
                var oldPlotElement = _plotElements[plotName];
                oldPlotElementIndex = Plots.IndexOf(oldPlotElement);
                if (oldPlotElementIndex != -1)
                    Plots.Remove(oldPlotElement);
            }

            var plotElement = CreatePlot(plotName, hLineData);

            _plotElements[plotName] = plotElement;
            if (oldPlotElementIndex != -1)
                Plots.Insert(oldPlotElementIndex, plotElement);
            else
                Plots.Add(plotElement);

            return;
        }
    }

    private void Callbacks_OnPlotUpdate(object? sender, PlotEventArgs e)
    {
        var plotName = e.Name;
        var plotData = e.PlotData;

        if (plotData is Service.LineGraphData lineData)
        {
            if (_plotElements.ContainsKey(plotName))
                UpdatePlot((LineGraphData)_plotElements[plotName], lineData);

            return;
        }

        if (plotData is Service.RectangleData rectangleData)
        {
            if (_plotElements.ContainsKey(plotName))
                UpdatePlot((RectangleData)_plotElements[plotName], rectangleData);

            return;
        }

        if (plotData is Service.VerticalLineData vLineData)
        {
            if (_plotElements.ContainsKey(plotName))
                UpdatePlot((VerticalLineData)_plotElements[plotName], vLineData);

            return;
        }

        if (plotData is Service.HorizontalLineData hLineData)
        {
            if (_plotElements.ContainsKey(plotName))
                UpdatePlot((HorizontalLineData)_plotElements[plotName], hLineData);

            return;
        }
    }

    private void Callbacks_WriteValue(object? sender, WriteValueEventArgs e)
    {
        if (_values.ContainsKey(e.Name))
        {
            _values[e.Name].Value = e.Value;
            return;
        }

        _values[e.Name] = new ValueWatch { Name = e.Name, Value = e.Value };
        _watches.Add(_values[e.Name]);
    }

    private void Callbacks_ConsoleOut(object? sender, ConsoleOutEventArgs e)
    {
        var text = Encoding.Unicode.GetString(e.Buffer, e.Offset, e.Count);
        Dispatcher.UIThread.Post(() => ConsoleOut.Insert(ConsoleOut.Text.Length, text));
    }

    private void Callbacks_OnError(object? sender, ScriptErrorEventArgs e)
    {
        ConsoleOut.Insert(ConsoleOut.Text.Length, e.Message);
    }

#endif

    public async Task Save()
    {
        File.WriteAllText(_path, Document.Text);
        IsModified = false;

    }

    public async Task Run(string branch)
    {
    }


    public async Task SaveAs(string fileName)
    {
    }

    public async Task Info()
    {
    }

    public void Dispose()
    {
        //_languageServer.Stop();
    }

    public bool HandleKeySequence(string keySequence)
    {
        return false;
    }
}
