using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LibGit2Sharp;
using Metrino.Development.Core;
using Metrino.Development.Studio.Library.Messages;
using Metrino.Development.UI.Core;
using Metrino.Development.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Metrino.Development.Studio.Library.ViewModels;

public partial class BranchData : ObservableObject
{
    [ObservableProperty]
    string _name = string.Empty;

    [ObservableProperty]
    bool _isCurrentHead = false;

    [ObservableProperty]
    bool _isRemote = true;

    [ObservableProperty]
    string _diffs = string.Empty;

    [ObservableProperty]
    string _lastCommit = string.Empty;

    [ObservableProperty]
    string _lastCommitBy = string.Empty;

    [ObservableProperty]
    List<TextSpan> _textSpans;

    public Branch? Payload { get; set; }
}

public partial class ProjectBuilderDocumentViewModel : ObservableObject, IDocument, IOverlay
{
    private ApplicationModel _applicationModel;

    ObservableCollection<BranchData> _branches = new ObservableCollection<BranchData>();
    public IEnumerable<BranchData> Branches => _branches;

    [ObservableProperty]
    string _filterText = string.Empty;

    [ObservableProperty]
    ObservableCollection<BranchData> _filteredInfo;

    [ObservableProperty]
    int _selectedIndex = 0;

    [ObservableProperty]
    int _caretIndex = 0;

    public ProjectBuilderDocumentViewModel(ApplicationModel applicationModel)
    {
        _applicationModel = applicationModel;

        int i = 0;
        foreach (var b in _applicationModel.GitBranches.Branches)
        {
            _branches.Add(new BranchData
            {
                Name = b.FriendlyName,
                IsCurrentHead = b.IsCurrentRepositoryHead,
                IsRemote = b.IsRemote,
                Diffs = $"\xf175{b.TrackingDetails.BehindBy} \xf176{b.TrackingDetails.BehindBy}",
                LastCommitBy = b.Tip.Author.Name,
                LastCommit = b.Tip.Author.When.DateTime.ToString(),
                TextSpans = new List<TextSpan> { new TextSpan { Offset = 0, Length = b.FriendlyName.Length, Highlight = true } },
                Payload = b
            });

            if(b.IsCurrentRepositoryHead)
                SelectedIndex = i;

            i++;
        }

        _applicationModel.GitBranches.PropertyChanged += GitBranches_PropertyChanged;

        FilteredInfo = new ObservableCollection<BranchData>();
        foreach (var entry in Branches)
            FilteredInfo.Add(entry);

        PropertyChanged += ProjectBuilderDocumentViewModel_PropertyChanged;
    }

    private void ProjectBuilderDocumentViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FilterText))
        {
            FilteredInfo.Clear();
            if (FilterText == "")
            {
                foreach (var entry in Branches)
                {
                    FilteredInfo.Add(entry);
                }

                return;
            }

            var list = Branches.Where(x => FuzzySearch.HasMatch(FilterText, x.Name));
            int index = 0;
            var textHighlights = new List<TextSpan>();
            var filterResults = new List<Tuple<double, BranchData>>();
            foreach (var element in list)
            {
                int[] positions = new int[element.Name.Length];
                var score = FuzzySearch.MatchPositions(FilterText, element.Name, positions);
                if (score > 0.0)
                {
                    var textSpans = FuzzySearch.PositionsToSpans(element.Name, positions);
                    var info = element;
                    filterResults.Add(new Tuple<double, BranchData>(score, new BranchData
                    {
                        Name = info.Name,
                        IsCurrentHead = info.IsCurrentHead,
                        TextSpans = textSpans,
                        IsRemote = info.IsRemote,
                        Payload = info.Payload
                    }));
                }

                index++;
            }

            var orderedFilteredResults = filterResults.OrderBy(x => x.Item1).Reverse();

            foreach (var x in orderedFilteredResults)
                FilteredInfo.Add(x.Item2);

            SelectedIndex = 0;
        }
    }

    private void GitBranches_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RepositoryManager.CurrentHead))
        {
            Dispatcher.UIThread.Post(() =>
            {
                foreach (var b in Branches)
                {
                    b.IsCurrentHead = (b.Name == _applicationModel.GitBranches.CurrentHead.FriendlyName);
                }
            });

            return;
        }

        if (e.PropertyName == nameof(RepositoryManager.Branches))
        {
            _branches.Clear();
            foreach (var b in _applicationModel.GitBranches.Branches)
            {
                _branches.Add(new BranchData
                {
                    Name = b.FriendlyName,
                    IsCurrentHead = b.IsCurrentRepositoryHead,
                    IsRemote = b.IsRemote,
                    TextSpans = new List<TextSpan> { new TextSpan { Offset = 0, Length = b.FriendlyName.Length, Highlight = true } },
                    Payload = b
                });
            }

            FilteredInfo.Clear();
            foreach (var entry in Branches)
                FilteredInfo.Add(entry);

        }
    }

    public async Task Run(string branch)
    {
    }

    public async Task Save()
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
        //throw new System.NotImplementedException();
    }

    internal void SwitchToBranch(object selectedItem)
    {
        var branchData = selectedItem as BranchData;
        if (branchData == null) return;
        if (branchData.Payload == null) return;

        _applicationModel.GitBranches.SetCurrentBranch(branchData.Payload);
    }

    public void HandleKeyPress(PhysicalKey key, string? symbol, KeyModifiers modifiers)
    {
        switch (key)
        {
            case PhysicalKey.ArrowDown:
                SelectedIndex = Math.Min(SelectedIndex + 1, Branches.Count() - 1);
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
                _applicationModel.GitBranches.SetCurrentBranch(Branches.ElementAt(SelectedIndex).Payload);
                break;
            case PhysicalKey.Delete:
                FilterText = FilterText.Remove(CaretIndex);
                break;
            case PhysicalKey.Escape:
                if(!string.IsNullOrEmpty(FilterText))
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
                if(CaretIndex > 0)
                {
                    CaretIndex -= 1;
                    FilterText = FilterText.Remove(CaretIndex);
                }
                break;
            default:
                if(!string.IsNullOrEmpty(symbol))
                {
                    FilterText = FilterText.Insert(CaretIndex, symbol);
                    CaretIndex += 1;
                }
                break;
        }
    }

    public bool HandleKeySequence(string keySequence)
    {
        return false;
    }
}
