using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Metrino.Development.Studio.Library.Messages;
using Metrino.Development.UI.ViewModels;
using System;
using System.Threading.Tasks;

namespace Metrino.Development.Studio.Library.ViewModels;

public partial class DocumentViewModelBase : ObservableObject, IDisposable, IDocument
{
    [ObservableProperty]
    string _key;

    [ObservableProperty]
    string _path;

    [ObservableProperty]
    string _name;

    [ObservableProperty]
    bool _isModified;

    [ObservableProperty]
    bool _isLoading;

    [ObservableProperty]
    IDocument _document;

    public virtual void Dispose() { }

    public virtual Task LoadPathAsync(string path, string branch = "")
    {
        throw new NotImplementedException();
    }

    public async Task Save()
    {
        await Document.Save();
    }

    public async Task Run(string branch)
    {
        await Document.Run(branch);
    }


    [RelayCommand]
    void Close(object parameter)
    {
        WeakReferenceMessenger.Default.Send(new CloseDocumentMessage(this));
    }

    public async Task SaveAs(string path)
    {
    }

    public async Task Info()
    {
        await Document.Info();
    }

    public bool HandleKeySequence(string keySequence)
    {
        return Document?.HandleKeySequence(keySequence) ?? false;
    }
}