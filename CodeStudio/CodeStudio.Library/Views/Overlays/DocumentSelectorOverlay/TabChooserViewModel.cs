using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LibGit2Sharp;
using Metrino.Development.Studio.Library.Messages;
using Metrino.Development.UI.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Metrino.Development.Studio.Library.ViewModels;

public partial class TabChooserViewModel : ObservableObject, IOverlay
{
    private WorkspaceViewModel _workspaceViewModel;

    [ObservableProperty]
    string _filterText = string.Empty;

    [ObservableProperty]
    ObservableCollection<DocumentViewModelBase> _filteredInfo;

    [ObservableProperty]
    int _selectedIndex = 0;

    [ObservableProperty]
    int _caretIndex = 0;

    public TabChooserViewModel(WorkspaceViewModel workspaceViewModel)
    {
        _workspaceViewModel = workspaceViewModel;

        FilteredInfo = new ObservableCollection<DocumentViewModelBase>();
        foreach(var d in _workspaceViewModel.Documents)
            FilteredInfo.Add(d);

        SelectedIndex = 0;
    }

    public void HandleKeyPress(PhysicalKey key, string? symbol, KeyModifiers modifiers)
    {
        switch (key)
        {
            case PhysicalKey.ArrowDown:
                SelectedIndex = Math.Min(SelectedIndex + 1, FilteredInfo.Count - 1);
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
                {
                    var document = FilteredInfo.ElementAt(SelectedIndex) as DocumentViewModelBase;
                    if (document == null) { return; }

                    _workspaceViewModel.ActiveDocument = _workspaceViewModel.Documents.IndexOf(document);
                    WeakReferenceMessenger.Default.Send(new DismissOverlayMessage(this));
                }
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

    internal void SelectDocument(object? selectedItem)
    {
        var document = selectedItem as DocumentViewModelBase;
        if (document == null) { return; }

        _workspaceViewModel.ActiveDocument= _workspaceViewModel.Documents.IndexOf(document);
    }
}
