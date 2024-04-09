using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Metrino.Development.UI.Core;

public class FileSystemNodeData : ICloneable
{
    public string Name { get; set; } = String.Empty;
    public string FullPath { get; set; } = String.Empty;
    public bool IsDirectory { get; set; } = false;

    public FileSystemNodeData()
    {
    }

    public FileSystemNodeData(string fullPath)
    {
        FullPath = fullPath;
        Name = Path.GetFileName(FullPath);
        IsDirectory = Directory.Exists(FullPath);
    }

    public FileSystemNodeData(FileSystemInfo fileInfo) : this(fileInfo.FullName) { }

    public object Clone()
    {
        return new FileSystemNodeData
        {
            Name = Name,
            FullPath = FullPath,
            IsDirectory = IsDirectory
        };
    }
}

public class FileSystemNode : ITreeNode, INotifyPropertyChanged, IDisposable
{
    FileSystemNodeData? _data;
    public object? Data
    {
        get => _data;
        set
        {
            _data = (value as FileSystemNodeData);
            OnPropertyChanged(nameof(Data));
        }
    }

    bool _isPopulated = false;
    FileSystem? _parent = null;

    public event PropertyChangedEventHandler? PropertyChanged;

    public IList<ITreeNode> Children { get; set; } = new ObservableCollection<ITreeNode>();

    public string FullPath
    {
        get => _data?.FullPath ?? string.Empty;
        set
        {
            if (_data != null)
            {
                _data.FullPath = value;
                _data.Name = Path.GetFileName(FullPath);

                OnPropertyChanged(nameof(_data.Name));
            }

        }
    }

    public ITreeNode? Parent { get; set; } = null;
    public string Name
    {
        get => _data?.Name ?? String.Empty;
        set
        {
            if (_data != null)
            {
                _data.Name = value;
                OnPropertyChanged(nameof(_data.Name));
            }
        }
    }

    public FileSystemNode() { }

    public FileSystemNode(FileSystem parent)
    {
        _parent = parent;

        if (_parent == null) return;

        _parent.Deleted += OnDeleted;
        _parent.Created += OnCreated;
        _parent.Renamed += OnRenamed;
    }

    void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    int ComputeInsertionIndex(IList<ITreeNode> nodeList, FileSystemNode element)
    {
        var name = element.Name;
        var elementData = element.Data as FileSystemNodeData;

        if (nodeList.Count == 0)
            return -1;

        var firstChildData = (nodeList[0].Data as FileSystemNodeData);
        if (firstChildData.IsDirectory && elementData.IsDirectory && firstChildData.Name.CompareTo(name) > 0)
            return 0;

        if (!firstChildData.IsDirectory && elementData.IsDirectory)
            return 0;

        if (elementData.IsDirectory)
        {
            for (int i = 0; i < nodeList.Count; i++)
            {
                var childData = (nodeList[i].Data as FileSystemNodeData);

                if (!childData.IsDirectory || childData.Name.CompareTo(name) > 0)
                    return i;
            }

            return -1;
        }
        else
        {
            for (int i = 0; i < nodeList.Count; i++)
            {
                var childData = (nodeList[i].Data as FileSystemNodeData);

                if (childData.IsDirectory) continue;

                if (childData.Name.CompareTo(name) > 0)
                    return i;
            }

            return -1;
        }
    }

    private void OnRenamed(object? sender, FileRenameEventArgs e)
    {
        var oldPath = e.OldFullPath;
        var data = (Data as FileSystemNodeData);
        if ((data != null) && data.FullPath == oldPath)
        {
            FullPath = e.NewFullPath;
            var oldIndex = Parent.Children.IndexOf(this);
            if (oldIndex >= 0)
            {
                Parent.Children.Remove(this);

                var newIndex = ComputeInsertionIndex(Parent.Children, this);
                if (newIndex >= 0)
                    Parent.Children.Insert(newIndex, this);
                else
                    Parent.Children.Add(this);
            }
        }
    }

    private void OnCreated(object? sender, FileCreatedEventArgs e)
    {
        var newPath = e.FullPath;
        var data = (Data as FileSystemNodeData);
        if (_isPopulated && (data != null) && data.FullPath == Path.GetDirectoryName(newPath))
        {
            if (Directory.Exists(newPath))
            {
                var newElement = new FileSystemNode(_parent) { Parent = this, Data = new FileSystemNodeData(newPath) };
                var index = ComputeInsertionIndex(Children, newElement);
                if (index >= 0)
                    Children.Insert(index, newElement);
                else
                    Children.Add(newElement);
            }

            if (File.Exists(newPath))
            {
                var newElement = new FileSystemNode(_parent) { Parent = this, Data = new FileSystemNodeData(newPath) };
                var index = ComputeInsertionIndex(Children, newElement);
                if (index >= 0)
                    Children.Insert(index, newElement);
                else
                    Children.Add(newElement);
            }
        }
    }

    private void OnDeleted(object? sender, FileDeletedEventArgs e)
    {
        var newPath = e.FullPath;
        var data = (Data as FileSystemNodeData);
        if (_isPopulated && (data != null) && data.FullPath == Path.GetDirectoryName(newPath))
        {
            for (int i = 0; i < Children.Count - 1; i++)
            {
                var childData = (Children[i].Data as FileSystemNodeData);

                if (childData.FullPath == newPath)
                {
                    Children.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public bool IsLeaf()
    {
        return !Directory.Exists(_data.FullPath);
    }

    private bool ShouldAdd(FileSystemInfo fileInfo)
    {
        return ((fileInfo.Attributes & FileAttributes.System) != FileAttributes.System) &&
               ((fileInfo.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden);
    }

    private void AddToChildren(IEnumerable<FileSystemInfo> listToAdd)
    {
        var nodes = listToAdd
            .OrderBy(d => d.Name)
            .Where(ShouldAdd)
            .Select(x => new FileSystemNode(_parent) { Parent = this, Data = new FileSystemNodeData(x) });

        foreach (var node in nodes)
            Children.Add(node);
    }

    public void UpdateChildren()
    {
        if (_isPopulated) return;

        Children.Clear();

        if (IsLeaf())
            return;

        var directoryInfo = new DirectoryInfo(_data.FullPath);
        AddToChildren(directoryInfo.GetDirectories());
        AddToChildren(directoryInfo.GetFiles());
        _isPopulated = true;
    }

    public object Clone()
    {
        return new FileSystemNode(_parent)
        {
            Data = (Data as FileSystemNodeData)?.Clone(),
            Parent = Parent
        };
    }

    public void Dispose()
    {
        if (_parent == null) return;

        _parent.Deleted -= OnDeleted;
        _parent.Created -= OnCreated;
        _parent.Renamed -= OnRenamed;
    }
}

public class FileSystem
{
    IList<FileSystemNode> _rootFolders = new ObservableCollection<FileSystemNode>();
    public IList<FileSystemNode> RootFolders => _rootFolders;

    IDictionary<string, FileSystemWatcher> _rootFolderWatchers = new Dictionary<string, FileSystemWatcher>();

    public event EventHandler<FileRenameEventArgs> Renamed;
    public event EventHandler<FileCreatedEventArgs> Created;
    public event EventHandler<FileDeletedEventArgs> Deleted;

    public FileSystemNode AddRootFolder(string folder)
    {
        var newRootFolder = new FileSystemNode(this) { Data = new FileSystemNodeData(folder) };

        _rootFolders.Add(newRootFolder);
        if (Directory.Exists(folder) && !_rootFolderWatchers.ContainsKey(folder))
        {
            _rootFolderWatchers[folder] = new FileSystemWatcher(folder)
            {
                NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName
                    | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Security
                    | NotifyFilters.Size,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            _rootFolderWatchers[folder].Created += FileSystem_Created;
            _rootFolderWatchers[folder].Deleted += FileSystem_Deleted;
            _rootFolderWatchers[folder].Renamed += FileSystem_Renamed;
        }

        return newRootFolder;
    }

    private void FileSystem_Renamed(object sender, RenamedEventArgs e)
    {
        Renamed?.Invoke(this, new FileRenameEventArgs { OldFullPath = e.OldFullPath, NewFullPath = e.FullPath });
    }

    private void FileSystem_Deleted(object sender, FileSystemEventArgs e)
    {
        Deleted?.Invoke(this, new FileDeletedEventArgs { FullPath = e.FullPath });
    }

    private void FileSystem_Created(object sender, FileSystemEventArgs e)
    {
        Created?.Invoke(this, new FileCreatedEventArgs { FullPath = e.FullPath });
    }

    public void RemoveRootFolder(FileSystemNode folder)
    {
        _rootFolders.Remove(folder);
        if (_rootFolderWatchers.ContainsKey(folder.FullPath))
        {
            _rootFolderWatchers.Remove(folder.FullPath);
        }
    }
}

public class FileDeletedEventArgs : EventArgs
{
    public string FullPath;
}

public class FileCreatedEventArgs : EventArgs
{
    public string FullPath;
}

public class FileRenameEventArgs : EventArgs
{
    public string OldFullPath;
    public string NewFullPath;
}
