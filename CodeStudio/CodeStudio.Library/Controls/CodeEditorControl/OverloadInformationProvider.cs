using System.Collections.Generic;
using AvaloniaEdit.CodeCompletion;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Metrino.Development.Studio.Library.Controls;

partial class OverloadInformationProvider : ObservableObject, IOverloadProvider
{
    private readonly IList<(string header, string content)> _items;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentHeader))]
    [NotifyPropertyChangedFor(nameof(CurrentContent))]
    int _selectedIndex;

    public OverloadInformationProvider(IList<(string header, string content)> items)
    {
        _items = items;
        SelectedIndex = 0;
    }

    public int Count => _items.Count;
    public string CurrentIndexText => $"{SelectedIndex + 1} of {Count}";
    public object CurrentHeader => _items[SelectedIndex].header;
    public object CurrentContent => _items[SelectedIndex].content;
}
