using Avalonia.Controls;
using Avalonia.Input;
using Metrino.Development.Studio.Library.Controls;
using Metrino.Development.UI.Core;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Metrino.Development.Studio.Library;

public partial class FileTreeView : UserControl
{
    bool _isEditing;
    private int _insertIndex;

    public FileTreeView()
    {
        InitializeComponent();

        TreeView.UpdateCompleted += TreeView_UpdateCompleted;
        TreeView.UpdateCancelled += TreeView_UpdateCancelled;

        TreeView.InsertCompleted += TreeView_InsertCompleted;
        TreeView.InsertCancelled += TreeView_InsertCancelled;

        TreeView.DragStart += TreeView_DragStart;
    }

    private int FindIndex(FileSystemNode item)
    {
        for (int i = 0; i < TreeView.DisplayedItems.Count; i++)
        {
            if (TreeView.DisplayedItems[i].Data == item)
                return i;
        }

        return -1;
    }

    private FileSystemNode ClosestFolder(FileSystemNode item)
    {
        if (item == null) return null;

        if (item.IsLeaf())
            return item.Parent as FileSystemNode;

        return item;
    }

    int InsertItem(FileSystemNode? item, bool isDirectory)
    {
        var selectedItem = ClosestFolder(item);

        if (selectedItem == null) return -1;

        var index = FindIndex(selectedItem);

        var treeItem = TreeView.DisplayedItems[index];
        if (!treeItem.IsExpanded)
            treeItem.IsExpanded = true;

        var addFileToken = new FileSystemNodeData { FullPath = selectedItem.FullPath, Name = "", IsDirectory = isDirectory };
        var data = new FileSystemNode { Data = addFileToken };
        var itemToAdd = new Controls.TreeViewItem { Data = data, IsFolder = isDirectory, IsExpanded = false, Level = treeItem.Level + 1 };

        TreeView.InsertRow(index + 1, itemToAdd);

        return index + 1;
    }

    private void AddFileButton_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (_isEditing) return;

        _isEditing = true;

        _insertIndex = InsertItem(TreeView.SelectedItem as FileSystemNode, false);
    }

    private void AddFolderButton_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (_isEditing) return;

        _isEditing = true;

        _insertIndex = InsertItem(TreeView.SelectedItem as FileSystemNode, true);
    }

    private void TreeView_InsertCancelled(object? sender, InsertElementEventArgs e)
    {
        _isEditing = false;
        _insertIndex = -1;
    }

    private void TreeView_InsertCompleted(object? sender, InsertElementEventArgs e)
    {
        _isEditing = false;
        var element = e.InsertedElement as FileSystemNode;
        var elementData = element.Data as FileSystemNodeData;

        TreeView.DisplayedItems.RemoveAt(_insertIndex);

        if (string.IsNullOrEmpty(element.Name))
            return;

        var path = Path.Combine(element.FullPath, element.Name);

        if (elementData.IsDirectory)
        {
            if (!File.Exists(path) && !Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        else
        {
            if (!File.Exists(path) && !Directory.Exists(path))
                using (File.Create(path)) { }
        }
    }

    private void RenameButton_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (_isEditing) return;

        _isEditing = true;

        var selectedItem = TreeView.SelectedItem as FileSystemNode;

        if (selectedItem == null) return;

        TreeView.UpdateRow(FindIndex(selectedItem));
    }

    private void TreeView_UpdateCancelled(object? sender, UpdateElementEventArgs e)
    {
        _isEditing = false;
    }

    private void TreeView_UpdateCompleted(object? sender, UpdateElementEventArgs e)
    {
        _isEditing = false;

        var oldPath = (e.OldValue as FileSystemNode).FullPath;
        var newName = (e.NewValue as FileSystemNode).Name;
        var oldPathParent = Path.GetDirectoryName(oldPath);
        var newPath = Path.Combine(oldPathParent, newName);

        if(Directory.Exists(oldPath) && !Directory.Exists(newPath) && !File.Exists(newPath))
        {
            Directory.Move(oldPath, newPath);
            return;
        }

        if (File.Exists(oldPath) && !Directory.Exists(newPath) && !File.Exists(newPath))
        {
            File.Move(oldPath, newPath);
            return;
        }
    }

    private void RefreshButton_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        var button = sender as Button;
    }

    private void CollapseAllButton_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        int maxLevel = 1;
        if (TreeView.DisplayedItems.All(x => x.Level <= 1))
            maxLevel = 0;

        var newCollection = TreeView.DisplayedItems.Where(x => x.Level <= maxLevel).ToList();

        foreach ( var item in newCollection )
            item.IsExpanded = false;

        TreeView.DisplayedItems = new ObservableCollection<Controls.TreeViewItem>(newCollection);
    }

    private async void TreeView_DragStart(object? sender, DragStartEventArgs e)
    {
        var element = e.Element as FileSystemNode;
        if (element == null)
            return;

        var dragData = new DataObject();
        dragData.Set(DataFormats.FileNames, new string[] { element.FullPath });
        DragDropEffects effects = DragDropEffects.Copy;
        var result = await DragDrop.DoDragDrop(e.TriggerEvent, dragData, effects);
    }
}