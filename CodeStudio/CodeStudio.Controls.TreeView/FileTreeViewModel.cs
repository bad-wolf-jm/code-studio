using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
//using Metrino.Development.Studio.Library.Messages;
using Metrino.Development.UI.Core;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Metrino.Development.UI.ViewModels;

public class FileTreeElement : TreeViewItem
{
    public FileTreeViewModel ParentView { get; set; }
}

public delegate void FileSelectedEventHandler(object sender, FileSelectedEventArgs path);

public class FileSelectedEventArgs : EventArgs
{
    private string _fullPath;
    public string FullPath => _fullPath;


    public FileSelectedEventArgs(string fullPath)
    {
        this._fullPath = fullPath;
    }
}

public partial class FileTreeViewModel : TreeViewModel<FileTreeElement>
{
    FileSystem _fileSystem;
    public ObservableCollection<ITreeNode> Roots { get; set; }

    public event FileSelectedEventHandler? FileSelected;

    [ObservableProperty]
    bool _isFocused = false;

    public FileTreeViewModel(FileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        Roots = new ObservableCollection<ITreeNode>();

        foreach (var r in _fileSystem.RootFolders)
            AddRoot(r);
    }

    public void Focus()
    {
        IsFocused = true;
    }

    public void AddRoot(string folder)
    {
        var newRoot = _fileSystem.AddRootFolder(folder);

        AddRoot(newRoot);
    }

    public void AddRoot(ITreeNode newRoot)
    {
        var newFolder = new FileTreeElement
        {
            IsFolder = true,
            IsExpanded = false,
            Level = 0,
            Data = newRoot,
            ParentView = this,
        };

        newFolder.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == "IsExpanded")
            {
                if (newFolder.IsExpanded)
                    DoExpandItem(newFolder);
                else
                    DoCollapseItem(newFolder);
            }
        };

        DisplayedItems.Add(newFolder);
        Roots.Add(newRoot);
    }


    public void RemoveRoot(FileTreeElement node)
    {
        CollapseItem(node);

        _fileSystem.RemoveRootFolder(node.Data as FileSystemNode);
        var index = DisplayedItems.IndexOf(node);
    }

    [RelayCommand]
    async Task OpenFolder(object parameter)
    {
        var folderPath = parameter as string;
        if (!string.IsNullOrEmpty(folderPath))
            AddRoot(folderPath);
    }

    [RelayCommand]
    void CloseFolder(object parameter)
    {
        if (parameter is FileTreeElement)
            RemoveRoot(parameter as FileTreeElement);
    }

    [RelayCommand(CanExecute = nameof(CanOpenFile))]
    void OpenFile(object parameter)
    {
        var data = parameter as FileSystemNode;

        if (data == null)
            return;

        FileSelected?.Invoke(this, new FileSelectedEventArgs(data.FullPath));
        //WeakReferenceMessenger.Default.Send(new OpenFileMessage(data.FullPath));
    }

    private bool CanOpenFile(object parameter)
    {
        var data = parameter as FileSystemNode;

        if (data == null)
            return false;

        var allowedExtensions = new string[] { ".lua", ".iolm", ".olx", ".trcx", ".trc", ".olmtest" };

        return allowedExtensions.Contains(Path.GetExtension(data.Name));
    }

    public bool HandleKeySequence(string keySequence)
    {
        return false;
    }
}
