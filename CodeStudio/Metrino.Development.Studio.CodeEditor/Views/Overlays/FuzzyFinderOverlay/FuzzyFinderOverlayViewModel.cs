using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
//using FuzzySharp;
using System.Collections.Generic;
using System.Linq;
using Metrino.Development.Core;
using System;
using Metrino.Development.UI.ViewModels;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Messaging;
using Metrino.Development.Studio.Library.Messages;

namespace Metrino.Development.UI.Core;

public class FilePath
{
    public string Path { get; set; }
    public List<TextSpan> TextSpans { get; set; }
}

public partial class FuzzyFinderOverlayViewModel : ObservableObject, IOverlay
{
//#if false
    [ObservableProperty]
    IEnumerable<FilePath> _info;

    [ObservableProperty]
    ObservableCollection<FilePath> _filteredInfo;

    [ObservableProperty]
    string _filterText = string.Empty;

    public FuzzyFinderOverlayViewModel()
    {
        Info = new List<FilePath>();
            
            //measurement.Info.Select(x =>
        //{
        //    Path = x.Item1,
        //    TextSpans = new List<TextSpan> { new TextSpan { Offset = 0, Length = x.Item1.Length, Highlight = true } },
        //    Value = x.Item2.Value,
        //    Unit = x.Item2.Unit
        //});

        FilteredInfo = [.. Info];

        PropertyChanged += OlmMeasurementMetadata_PropertyChanged;
        SelectedIndex = 0;
    }

    [ObservableProperty]
    int _selectedIndex = 0;

    [ObservableProperty]
    int _caretIndex = 0;

    public void HandleKeyPress(PhysicalKey key, string? symbol, KeyModifiers modifiers)
    {
        switch (key)
        {
            case PhysicalKey.ArrowDown:
                SelectedIndex = Math.Min(SelectedIndex + 1, FilteredInfo.Count() - 1);
                break;
            case PhysicalKey.ArrowUp:
                SelectedIndex = Math.Max(SelectedIndex - 1, 0);
                break;
            case PhysicalKey.ArrowLeft:
                if (CaretIndex > 0)
                    CaretIndex -= 1;
                break;
            case PhysicalKey.Home:
                CaretIndex = 0;
                break;
            case PhysicalKey.End:
                CaretIndex = FilterText.Length - 1;
                break;
            case PhysicalKey.ArrowRight:
                if (CaretIndex < FilterText.Length - 1)
                    CaretIndex += 1;
                break;
            case PhysicalKey.Enter:
                //_applicationModel.GitBranches.SetCurrentBranch(Branches.ElementAt(SelectedIndex).Payload);
                break;
            case PhysicalKey.Delete:
                FilterText = FilterText.Remove(CaretIndex);
                break;
            case PhysicalKey.Escape:
                if (!string.IsNullOrEmpty(FilterText))
                {
                    FilterText = string.Empty;
                    CaretIndex = 0;
                }
                else
                {
                    WeakReferenceMessenger.Default.Send(new DismissOverlayMessage(this));
                }
                break;
            case PhysicalKey.Backspace:
                if (CaretIndex > 0)
                {
                    CaretIndex -= 1;
                    FilterText = FilterText.Remove(CaretIndex);
                }
                break;
            default:
                if (!string.IsNullOrEmpty(symbol))
                {
                    FilterText = FilterText.Insert(CaretIndex, symbol);
                    CaretIndex += 1;
                }
                break;
        }
    }

    private void OlmMeasurementMetadata_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FilterText))
        {
            FilteredInfo.Clear();
            if (FilterText == "")
            {
                foreach (var entry in Info)
                {
                    FilteredInfo.Add(entry);
                }

                return;
            }

            var list = Info.Where(x => FuzzySearch.HasMatch(FilterText, x.Path));
            int index = 0;
            var textHighlights = new List<TextSpan>();
            var filterResults = new List<Tuple<double, FilePath>>();

            foreach (var element in list)
            {
                int[] positions = new int[element.Path.Length];
                var score = FuzzySearch.MatchPositions(FilterText, element.Path, positions);
                if (score > 0.0)
                {
                    var textSpans = FuzzySearch.PositionsToSpans(element.Path, positions);
                    var info = element;
                    //filterResults.Add(new Tuple<double, string>(score, new string
                    //{
                    //    Path = info.Path,
                    //    TextSpans = textSpans,
                    //    Value = info.Value,
                    //    Unit = info.Unit
                    //}));
                }

                index++;
            }

            var orderedFilteredResults = filterResults.OrderBy(x => -x.Item1);

            foreach (var x in orderedFilteredResults)
                FilteredInfo.Add(x.Item2);
        }
    }
}
