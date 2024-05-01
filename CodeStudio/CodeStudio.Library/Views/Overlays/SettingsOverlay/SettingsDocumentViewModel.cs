using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Metrino.Development.UI.Core;
using Metrino.Development.UI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Metrino.Development.Studio.Library.ViewModels;

public partial class SettingsDocumentViewModel : DocumentViewModelBase, IDocument, IOverlay
{
    [ObservableProperty]
    string _otdrRepositoryPath = string.Empty;

    [ObservableProperty]
    string _otdrBuildPath = string.Empty;

    [ObservableProperty]
    string _otdrBuildConfiguration = string.Empty;

    [ObservableProperty]
    string _otdrTargetFramework = string.Empty;

    [ObservableProperty]
    string _localTestDataBank = string.Empty;

    [ObservableProperty]
    string _remoteTestDataBank = string.Empty;

    [ObservableProperty]
    string _unitTestXmlRoot = string.Empty;

    [ObservableProperty]
    string _localTestResult = string.Empty;

    [ObservableProperty]
    bool _isModified = false;

    private ApplicationModel _applicationModel;

    private List<string> _targetFrameworks = new List<string>() { "net48" };
    public IEnumerable<string> TargetFrameworks => _targetFrameworks;

    private List<string> _buildConfigurations = new List<string>() { "Debug", "Release" };
    public IEnumerable<string> BuildConfigurations => _buildConfigurations;

    public SettingsDocumentViewModel(ApplicationModel applicationModel)
    {
        _applicationModel = applicationModel;

        OtdrBuildPath = _applicationModel.Configuration.OtdrBuildPath;
        OtdrBuildConfiguration = _applicationModel.Configuration.OtdrBuildConfiguration;
        OtdrRepositoryPath = _applicationModel.Configuration.OtdrRepositoryPath;
        OtdrTargetFramework = _applicationModel.Configuration.OtdrTargetFramework;
        LocalTestDataBank = _applicationModel.Configuration.LocalTestDataBank;
        RemoteTestDataBank = _applicationModel.Configuration.RemoteTestDataBank;
        UnitTestXmlRoot = _applicationModel.Configuration.UnitTestXmlRoot;
    }

    public async Task Run(string branch)
    {
    }

    public async Task Save()
    {
        _applicationModel.Configuration.OtdrBuildPath = OtdrBuildPath;
        _applicationModel.Configuration.OtdrBuildConfiguration = OtdrBuildConfiguration;
        _applicationModel.Configuration.OtdrRepositoryPath = OtdrRepositoryPath;
        _applicationModel.Configuration.OtdrTargetFramework = OtdrTargetFramework;
        _applicationModel.Configuration.LocalTestDataBank = LocalTestDataBank;
        _applicationModel.Configuration.RemoteTestDataBank = RemoteTestDataBank;
        _applicationModel.Configuration.UnitTestXmlRoot = UnitTestXmlRoot;
    }

    public async Task SaveAs(string fileName)
    {
    }

    [RelayCommand]
    async Task SelectFolder(object parameter)
    {
        var propertyName = parameter as string;
        if (propertyName == null)
            return;

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var folderName = await Utilities.SelectFolder(desktop.MainWindow);
            if(folderName == null) return;
            switch(propertyName)
            {
                case "OtdrRepositoryPath":
                    SetProperty(ref _otdrRepositoryPath, folderName, "OtdrRepositoryPath");
                    break;
                case "OtdrBuildPath":
                    SetProperty(ref _otdrBuildPath, folderName, "OtdrBuildPath");
                    break;
                case "LocalTestDataBank":
                    SetProperty(ref _localTestDataBank, folderName, "LocalTestDataBank");
                    break;
                case "RemoteTestDataBank":
                    SetProperty(ref _remoteTestDataBank, folderName, "RemoteTestDataBank");
                    break;
                case "UnitTestXmlRoot":
                    SetProperty(ref _unitTestXmlRoot, folderName, "UnitTestXmlRoot");
                    break;
                default:
                    break;
            }
        }
    }

    public void HandleKeyPress(PhysicalKey key, string? symbol, KeyModifiers modifiers)
    {
        //throw new System.NotImplementedException();
    }
}
