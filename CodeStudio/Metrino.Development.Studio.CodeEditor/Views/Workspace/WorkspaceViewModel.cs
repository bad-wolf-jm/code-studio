using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Metrino.Development.Studio.Library.Messages;
using Metrino.Development.UI.Core;
using Metrino.Development.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace Metrino.Development.Studio.Library.ViewModels;

public partial class WorkspaceViewModel : ObservableObject, IDisposable
{
    IBackendServiceProvider _serviceProvider;
    Dictionary<string, int> _openedDocuments = new Dictionary<string, int>();

    ObservableCollection<DocumentViewModelBase> _documents;
    public ObservableCollection<DocumentViewModelBase> Documents => _documents;

    ApplicationModel _applicationModel;

    public INotificationManager NotificationManager { get; set; }

    ProjectBuilderDocumentViewModel _git;
    //DataFilesViewModel _fileDatabase;
    SettingsDocumentViewModel _settings;

    [ObservableProperty]
    bool _isFocused = false;

    public void Focus()
    {
        IsFocused = true;
    }

    public WorkspaceViewModel(ApplicationModel applicationModel)
    {
        _applicationModel = applicationModel;
        _serviceProvider = applicationModel.ServiceProvider;
        _documents = new ObservableCollection<DocumentViewModelBase>();
        _activeDocument = -1;

        var overlayContent =
        _git = new ProjectBuilderDocumentViewModel(applicationModel);
        _settings =  new SettingsDocumentViewModel(_applicationModel);
        //_fileDatabase = new DataFilesViewModel(_applicationModel);

        WeakReferenceMessenger.Default.Register<OpenFileMessage>(this, async (r, message) =>
        {
            await OpenDocument(message.Value, "develop");
        });

        WeakReferenceMessenger.Default.Register<CloseDocumentMessage>(this, (r, message) =>
        {
            CloseDocument(message.Value);
        });

        WeakReferenceMessenger.Default.Register<ExceptionMessage>(this, (r, message) =>
        {
            var notification = new Notification(message.Value.GetType().ToString(), message.Value.ToString(), NotificationType.Error, System.TimeSpan.Zero);
            NotificationManager?.Show(notification);
        });
    }

    public void CloseDocument(DocumentViewModelBase document)
    {
        _openedDocuments.Remove(document.Key);
        _documents.Remove(document);

        document.Dispose();
    }

    public void CloseDocument()
    {
        CloseDocument(_documents[ActiveDocument]);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentPath))]
    private int _activeDocument;

    public string CurrentPath
    {
        get
        {
            if (ActiveDocument != -1)
                return Documents[ActiveDocument].Path;

            return "(No document)";
        }
    }

    [ObservableProperty]
    private object _selectedDocument;

    [ObservableProperty]
    private object _overlayContent;

    public async Task OpenDocument(string path, string branch = "")
    {
        if (!File.Exists(path)) return;

        if (_openedDocuments.ContainsKey(path))
        {
            ActiveDocument = _openedDocuments[path];
            return;
        }

        var documentName = Path.GetFileName(path);
        var extension = Path.GetExtension(documentName);
        var newDocument = new DocumentViewModelBase
        {
            Key = path,
            Name = documentName,
            Path = path,
            IsModified = false,
            IsLoading = true
        };
        _documents.Add(newDocument);
        _openedDocuments[newDocument.Key] = _documents.IndexOf(newDocument);
        ActiveDocument = _openedDocuments[newDocument.Key];

        switch (extension)
        {
#if false
    case ".iolm":
            case ".olx":
            case ".trcx":
                {
                    var document = new OlmDocumentViewModel(_applicationModel);
                    await document.LoadPathAsync(path, branch);

                    newDocument.IsLoading = false;
                    newDocument.Document = document;
                    document.PropertyChanged += (object? sender, System.ComponentModel.PropertyChangedEventArgs e) =>
                    {
                        if (e.PropertyName == "IsModified")
                        {
                            newDocument.IsModified = document.IsModified;
                        }
                    };
                }
                break;
            case ".trc":
                {
                    var document = new OtdrDocumentViewModel(_serviceProvider);
                    await document.LoadPathAsync(path, branch);

                    newDocument.IsLoading = false;
                    newDocument.Document = document;
                }
                break;
#endif
            case ".cs":
            case ".csx":
            case ".lua":
                {
                    var document = new CodeDocumentViewModel(_applicationModel, path);
                    await document.LoadPathAsync(path, branch);

                    newDocument.IsLoading = false;
                    newDocument.Document = document;
                    document.PropertyChanged += (object? sender, System.ComponentModel.PropertyChangedEventArgs e) =>
                    {
                        if (e.PropertyName == "IsModified")
                        {
                            newDocument.IsModified = document.IsModified;
                        }
                    };
                }
                break;
#if false
            case ".olmtest":
                {
                    var document = new UnitTestDocumentViewModel(_applicationModel);
                    await document.LoadPathAsync(path);

                    newDocument.IsLoading = false;
                    newDocument.Document = document;
                    document.PropertyChanged += (object? sender, System.ComponentModel.PropertyChangedEventArgs e) =>
                    {
                        if (e.PropertyName == "IsModified")
                        {
                            newDocument.IsModified = document.IsModified;
                        }
                    };
                }
                break;
#endif
            default:
                {
                    var document = new DocumentViewModelBase();

                    newDocument.IsLoading = false;
                    newDocument.Document = document;
                    document.PropertyChanged += (object? sender, System.ComponentModel.PropertyChangedEventArgs e) =>
                    {
                        if (e.PropertyName == "IsModified")
                        {
                            newDocument.IsModified = document.IsModified;
                        }
                    };
                }
                break;
        }
    }

    [RelayCommand]
    void CloseAll(object parameter)
    {
        _openedDocuments.Clear();
        _documents.Clear();
    }

    [RelayCommand]
    void CloseAllSaved(object parameter)
    {
        var savedDocuments = _documents.Where(x => !x.IsModified);

        foreach (var d in savedDocuments)
            CloseDocument(d);
    }

    bool ActiveDocumentIsValid()
    {
        return ActiveDocument >= 0 && ActiveDocument < _documents.Count;
    }

    [RelayCommand]
    async Task SaveCurrentFile(object parameter)
    {
        if (!ActiveDocumentIsValid()) return;

        var document = _documents[ActiveDocument];
        await document.Save();
    }

    public async Task SaveCurrentFileAs(string path)
    {
        if (!ActiveDocumentIsValid()) return;

        var document = _documents[ActiveDocument];
        await document.SaveAs(path);
    }

    [RelayCommand]
    async Task SaveAllFiles(object parameter)
    {
        foreach (var document in _documents)
            await document.Save();
    }

    [RelayCommand]
    async Task RunCurrentFile(object parameter)
    {
        if (!ActiveDocumentIsValid()) return;

        var document = _documents[ActiveDocument];
        await document.Run(_applicationModel.GitBranches.CurrentHead.FriendlyName);
    }

    [RelayCommand]
    async Task DisplayInfoOverlay(object parameter)
    {
        if (!ActiveDocumentIsValid()) return;

        var document = _documents[ActiveDocument];
        await document.Info();
    }

    [RelayCommand]
    async Task DisplayTabChooser(object parameter)
    {
        var tabChooser = new TabChooserViewModel(this);
        WeakReferenceMessenger.Default.Send(new DisplayOverlayMessage(tabChooser));
    }


    [RelayCommand]
    async Task DisplayOverlay(object parameter)
    {
        WeakReferenceMessenger.Default.Send(new DisplayOverlayMessage(_git));
    }

    [RelayCommand]
    async Task DisplaySettings(object parameter)
    {
        WeakReferenceMessenger.Default.Send(new DisplayOverlayMessage(_settings));
    }

    public void Dispose()
    {
        foreach (var document in _documents)
        {
            document.Dispose();
        }
    }

    public bool HandleKeySequence(string keySequence)
    {
        if (!ActiveDocumentIsValid()) return false;

        var document = _documents[ActiveDocument];
        return document.HandleKeySequence(keySequence);
    }
}
