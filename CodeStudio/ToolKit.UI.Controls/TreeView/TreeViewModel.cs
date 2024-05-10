using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

using Metrino.Development.UI.Core;
using System.ComponentModel;

namespace Metrino.Development.UI.ViewModels;

public interface ITreeViewItem
{
    bool IsFolder { get; set; }
    bool IsExpanded { get; set; }
    int Level { get; set; }
    ITreeNode Data { get; set; }
    event PropertyChangedEventHandler? PropertyChanged;
}

public partial class TreeViewItem : ObservableObject, ITreeViewItem
{
    [ObservableProperty]
    bool _isFolder = false;

    [ObservableProperty]
    bool _isExpanded = false;

    [ObservableProperty]
    int _level = 0;

    [ObservableProperty]
    bool _disabled = false;

    [ObservableProperty]
    ITreeNode _data = null;

    [RelayCommand]
    void ToggleExpandedState()
    {
        IsExpanded = !IsExpanded;
    }

    public void NotifyInternalChange(PropertyChangedEventArgs args)
    {
        OnPropertyChanged(args.PropertyName);
    }
}

public partial class TreeViewModel<_ItemType> : ObservableObject where _ItemType : ITreeViewItem, new()
{
    ObservableCollection<ITreeViewItem> _displayedItems = new ObservableCollection<ITreeViewItem>();
    public ObservableCollection<ITreeViewItem> DisplayedItems => _displayedItems;

    [ObservableProperty]
    int _selectedItem;

    public void DoExpandItem(_ItemType item)
    {
        int index = _displayedItems.IndexOf(item);
        if (index == -1)
            return;

        var x = SelectedItem;

        item.Data.UpdateChildren();

        foreach (var child in item.Data.Children)
        {
            var newItem = new _ItemType
            {
                IsFolder = !child.IsLeaf(),
                IsExpanded = false,
                Level = item.Level + 1,
                Data = child
            };

            newItem.PropertyChanged += NewItem_PropertyChanged;

            _displayedItems.Insert(index + 1, newItem);

            index++;
        }

        SelectedItem = x;
    }

    private void NewItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender == null)
            return;

        if (e.PropertyName == "IsExpanded")
        {
            var treeItem = (_ItemType)sender;
            if (treeItem == null) return;

            if (treeItem.IsExpanded)
                DoExpandItem((_ItemType)treeItem);
            else
                DoCollapseItem((_ItemType)treeItem);
        }
    }

    public void ExpandItem(_ItemType item)
    {
        if (!item.IsFolder || item.IsExpanded)
            return;

        item.IsExpanded = true;
    }

    public void DoCollapseItem(_ItemType item)
    {
        int index = _displayedItems.IndexOf(item);
        if (index == -1)
            return;

        index++;
        var x = SelectedItem;

        while ((index < _displayedItems.Count) && (_displayedItems[index].Level > item.Level))
            _displayedItems.RemoveAt(index);

        SelectedItem = x;
    }

    public void CollapseItem(_ItemType item)
    {
        if (!item.IsFolder || !item.IsExpanded)
            return;

        item.IsExpanded = false;
    }
}
