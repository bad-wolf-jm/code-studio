using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Metrino.Development.UI.Core;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Metrino.Development.Studio.Library.Controls;

public partial class TreeViewItem : ObservableObject
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

    public void NotifyInternalChange(PropertyChangedEventArgs args)
    {
        OnPropertyChanged(args.PropertyName);
    }
}

public class UpdateElementEventArgs : EventArgs
{
    public ITreeNode? OldValue;
    public ITreeNode? NewValue;
}

public class InsertElementEventArgs : EventArgs
{
    public ITreeNode? InsertedElement;
}

public class DragStartEventArgs : EventArgs
{
    public PointerPressedEventArgs TriggerEvent;
    public ITreeNode? Element;
}

public partial class TreeViewControl : TemplatedControl
{
    public static readonly StyledProperty<ObservableCollection<ITreeNode>> ItemsSourceProperty
        = AvaloniaProperty.Register<TreeViewControl, ObservableCollection<ITreeNode>>(nameof(ItemsSource));

    public ObservableCollection<ITreeNode> ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly StyledProperty<ICommand> OpenItemCommandProperty
        = AvaloniaProperty.Register<TreeViewControl, ICommand>(nameof(OpenItemCommand));

    public ICommand OpenItemCommand
    {
        get => GetValue(OpenItemCommandProperty);
        set => SetValue(OpenItemCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> NameChangedProperty
        = AvaloniaProperty.Register<TreeViewControl, ICommand>(nameof(NameChanged));

    public ICommand NameChanged
    {
        get => GetValue(NameChangedProperty);
        set => SetValue(NameChangedProperty, value);
    }

    public static readonly StyledProperty<int> SelectedIndexProperty
        = AvaloniaProperty.Register<TreeViewControl, int>(nameof(SelectedIndex));

    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    int _previousSelectedIndex;
    int _editIndex = -1;
    bool _isInserting;
    ITreeNode? _elementBeingEdited;
    DataGrid _internalDataGrid;

    public static readonly StyledProperty<ITreeNode?> SelectedItemProperty
        = AvaloniaProperty.Register<TreeViewControl, ITreeNode?>(nameof(SelectedItem));

    public ITreeNode? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<TreeViewItem>> DisplayedItemsProperty
        = AvaloniaProperty.Register<TreeViewControl, ObservableCollection<TreeViewItem>>(nameof(DisplayedItems));

    public ObservableCollection<TreeViewItem> DisplayedItems
    {
        get => GetValue(DisplayedItemsProperty);
        set => SetValue(DisplayedItemsProperty, value);
    }


    public EventHandler<UpdateElementEventArgs> UpdateCompleted;
    public EventHandler<UpdateElementEventArgs> UpdateCancelled;

    public EventHandler<InsertElementEventArgs> InsertCompleted;
    public EventHandler<InsertElementEventArgs> InsertCancelled;

    public EventHandler<DragStartEventArgs> DragStart;

    public TreeViewControl()
    {
        ItemsSourceProperty.Changed.AddClassHandler<TreeViewControl>(OnItemsSourcePropertyChanged);
        SelectedIndexProperty.Changed.AddClassHandler<TreeViewControl>(OnSelectedIndexChanged);

        DoubleTapped += TreeViewControl_DoubleTapped;
        DisplayedItems = new ObservableCollection<TreeViewItem>();

        if (SelectedIndex >= 0 && SelectedIndex < DisplayedItems.Count)
            SelectedItem = DisplayedItems[SelectedIndex].Data;

    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _internalDataGrid = e.NameScope.Find<DataGrid>("internalDataGrid");
        if (_internalDataGrid == null)
            return;

        _internalDataGrid.BeginningEdit += _internalDataGrid_BeginningEdit;
        _internalDataGrid.CellEditEnded += _internalDataGrid_CellEditEnded;
        _internalDataGrid.PreparingCellForEdit += _internalDataGrid_PreparingCellForEdit;
        _internalDataGrid.CellPointerPressed += DoDrag;
        _internalDataGrid.PointerMoved += _internalDataGrid_PointerMoved;
    }

    DragStartEventArgs? _elementToDrag = null;
    Point _dragOrigin = new Point(0, 0);
    private void _internalDataGrid_PointerMoved(object? sender, PointerEventArgs e)
    {
        if ( _elementToDrag != null)
        {
            var moveVector = e.GetCurrentPoint(sender as Control).Position - _dragOrigin;
            var moveDistance = moveVector.X * moveVector.X + moveVector.Y * moveVector.Y;
            if (moveDistance > 4)
            {
                DragStart?.Invoke(this, _elementToDrag);
                _elementToDrag = null;
            }
        }
    }

    private void DoDrag(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        var rowData = e.Row.DataContext as TreeViewItem;
        if (rowData == null)
            return;

        _dragOrigin = e.PointerPressedEventArgs.GetCurrentPoint(_internalDataGrid as Control).Position;
        _elementToDrag = new DragStartEventArgs
        {
            TriggerEvent = e.PointerPressedEventArgs,
            Element = rowData.Data
        };
    }

    private void _internalDataGrid_PreparingCellForEdit(object? sender, DataGridPreparingCellForEditEventArgs e)
    {
        var tableRow = e.EditingElement;
        if (tableRow != null)
        {
            var textInput = tableRow.GetLogicalChildren();
            foreach (var item in textInput)
            {
                if (item is TextBox)
                {
                    var textBox = (TextBox)item;
                    textBox.Focus();

                    break;
                }
            }
        }
    }

    private void _internalDataGrid_BeginningEdit(object? sender, DataGridBeginningEditEventArgs e)
    {
        e.Cancel = (_editIndex == -1);
    }

    private void _internalDataGrid_CellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        switch (e.EditAction)
        {
            case DataGridEditAction.Cancel:
                if (_isInserting)
                {
                    InsertCancelled?.Invoke(this, new InsertElementEventArgs { InsertedElement = null });

                    DisplayedItems.RemoveAt(_editIndex);

                    SelectedIndex = _previousSelectedIndex;
                }
                else
                {
                    UpdateCancelled?.Invoke(this, new UpdateElementEventArgs { OldValue = null, NewValue = null });
                }
                _isInserting = false;
                _editIndex = -1;
                break;
            case DataGridEditAction.Commit:
                if (_isInserting)
                {
                    var insertedItem = DisplayedItems[_editIndex];
                    InsertCompleted?.Invoke(this, new InsertElementEventArgs { InsertedElement = insertedItem.Data });
                }
                else
                {
                    var updatedItem = DisplayedItems[_editIndex];
                    UpdateCompleted?.Invoke(this, new UpdateElementEventArgs { OldValue = _elementBeingEdited, NewValue = updatedItem.Data });
                }
                _isInserting = false;
                _editIndex = -1;
                break;
            default:
                break;
        }
    }

    private void OnSelectedIndexChanged(TreeViewControl control, AvaloniaPropertyChangedEventArgs args)
    {
        var index = (int)(args.NewValue ?? -1);
        if (index != -1)
            SelectedItem = DisplayedItems[index].Data;
        else
            SelectedItem = null;
    }

    TreeViewItem CreateTreeViewItem(bool isFolder, int level, ITreeNode itemRoot)
    {
        var newItem = new TreeViewItem { IsFolder = isFolder, IsExpanded = false, Level = level, Data = itemRoot };
        newItem.PropertyChanged += TreeItem_PropertyChanged;
        newItem.Disabled = !isFolder && !OpenItemCommand.CanExecute(itemRoot);

        if (newItem.Data.Children is INotifyCollectionChanged childrenChangedNotifier)
            childrenChangedNotifier.CollectionChanged += (s, e) =>
                ChildrenChangedNotifier_CollectionChanged(itemRoot, e.OldItems, e.NewItems);

        if (newItem.Data is INotifyPropertyChanged itemDataNotifier)
            itemDataNotifier.PropertyChanged += (s, e) =>
                TreeItemData_PropertyChanged(newItem, e);

        return newItem;
    }

    void OnItemsSourcePropertyChanged(TreeViewControl control, AvaloniaPropertyChangedEventArgs args)
    {
        DisplayedItems.Clear();

        if (args.NewValue is ObservableCollection<ITreeNode> treeRoots)
        {
            BeginBulkInsertion();

            foreach (var root in treeRoots)
                DisplayedItems.Add(CreateTreeViewItem(true, 0, root));

            EndBulkInsertion();

            SelectedIndex = 0;
            SelectedItem = DisplayedItems[0].Data;
        }
    }

    private void ChildrenChangedNotifier_CollectionChanged(ITreeNode parent, IList? oldItems, IList? newItems)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            if (_bulkInsertion) return;

            var parentIndex = DisplayedItems.Select(x => x.Data).ToList().IndexOf(parent);
            if (parentIndex == -1) return;

            int parentLevel = DisplayedItems[parentIndex].Level;
            int childLevel = parentLevel + 1;

            if (oldItems != null)
            {
                RemoveItems(oldItems, parentIndex);
            }

            if (newItems != null)
            {
                AddItems(parent, newItems, parentIndex, childLevel);
            }
        });
    }

    private void AddItems(ITreeNode parent, IList? newItems, int parentIndex, int childLevel)
    {
        foreach (var treeNode in newItems.OfType<ITreeNode>())
        {
            var childIndex = parent.Children.IndexOf(treeNode);
            int insertionIndex = parentIndex + 1;
            int max = DisplayedItems.Count;
            var newDisplayItem = CreateTreeViewItem(!treeNode.IsLeaf(), childLevel, treeNode);

            if (childIndex == 0)
            {

                DisplayedItems.Insert(insertionIndex, newDisplayItem);
            }
            else
            {
                while ((insertionIndex < max) && (DisplayedItems[insertionIndex].Level >= childLevel))
                {
                    if (DisplayedItems[insertionIndex].Level == childLevel)
                        childIndex--;

                    if (childIndex < 0)
                    {
                        DisplayedItems.Insert(insertionIndex, newDisplayItem);
                        break;
                    }

                    insertionIndex++;
                }

                if (childIndex < 0) continue;

                if ((insertionIndex < max) && DisplayedItems[insertionIndex].Level < childLevel)
                {
                    DisplayedItems.Insert(insertionIndex, newDisplayItem);
                }
                else
                {
                    DisplayedItems.Add(newDisplayItem);
                }
            }
        }
    }

    private void RemoveItems(IList? oldItems, int parentIndex)
    {
        foreach (var oldItem in oldItems.OfType<ITreeNode>())
        {
            int index = parentIndex + 1;
            int max = DisplayedItems.Count;

            while (index < max)
            {
                var currentItem = DisplayedItems[index];
                var currentLevel = currentItem.Level;
                if (currentItem.Data == oldItem)
                {
                    DisplayedItems.RemoveAt(index);
                    if (index >= DisplayedItems.Count) break;

                    while (DisplayedItems[index].Level > currentLevel)
                        DisplayedItems.RemoveAt(index);

                    break;
                }

                index++;
            }
        }
    }

    void TreeViewControl_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (!(e.Source is Control))
            return;

        var control = e.Source as Control;

        var element = control.DataContext;
        if (element is TreeViewItem item)
        {
            if (item.IsFolder)
            {
                item.IsExpanded = !item.IsExpanded;
            }
            else
            {
                if (OpenItemCommand.CanExecute(item.Data))
                    OpenItemCommand.Execute(item.Data);
            }
        }
    }

    void DoExpandItem(TreeViewItem item)
    {
        int index = DisplayedItems.IndexOf(item);
        if (index == -1)
            return;

        var x = SelectedItem;

        BeginBulkInsertion();

        item.Data.UpdateChildren();

        foreach (var child in item.Data.Children)
        {
            DisplayedItems.Insert(index + 1, CreateTreeViewItem(!child.IsLeaf(), item.Level + 1, child));

            index++;
        }

        EndBulkInsertion();

        SelectedItem = x;
    }

    bool _bulkInsertion = false;
    private void EndBulkInsertion()
    {
        _bulkInsertion = false;
    }

    private void BeginBulkInsertion()
    {
        _bulkInsertion = true;
    }

    void DoCollapseItem(TreeViewItem item)
    {
        int index = DisplayedItems.IndexOf(item);
        if (index == -1)
            return;

        index++;
        var x = SelectedItem;

        while ((index < DisplayedItems.Count) && (DisplayedItems[index].Level > item.Level))
            DisplayedItems.RemoveAt(index);

        SelectedItem = x;
    }

    void TreeItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender == null)
            return;

        if (e.PropertyName == "IsExpanded")
        {
            var treeItem = (TreeViewItem)sender;
            if (treeItem == null) return;

            if (treeItem.IsExpanded)
                DoExpandItem(treeItem);
            else
                DoCollapseItem(treeItem);
        }

    }

    void TreeItemData_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is TreeViewItem treeViewItem)
            treeViewItem.NotifyInternalChange(e);
    }

    void BeginEdit(int index, bool isInserting)
    {
        _previousSelectedIndex = SelectedIndex;

        SelectedIndex = index;

        _editIndex = index;
        _isInserting = isInserting;
        _elementBeingEdited = DisplayedItems[_editIndex].Data?.Clone() as ITreeNode;

        _internalDataGrid?.BeginEdit();
    }

    public void UpdateRow(int v)
    {
        BeginEdit(v, false);
    }

    public void InsertRow(int v, TreeViewItem item)
    {
        DisplayedItems.Insert(v, item);

        BeginEdit(v, true);
    }

    async void DoDrag(object? sender, PointerPressedEventArgs e)
    {
        //_internalDataGrid.
    }
}

