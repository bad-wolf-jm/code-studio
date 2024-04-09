using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using Metrino.Development.Studio.Library.Messages;
using Metrino.Development.Studio.ViewModels;
using Metrino.Development.Studio.Views;
using Metrino.Development.UI.Core;
using System;
using System.IO;

namespace Metrino.Development.Studio;

public partial class App : Application
{
    private ApplicationModel _applicationModel;
    private string _applicationConfigurationFile;
    private WindowNotificationManager _notificationManager;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);

            //var userConfig = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var userConfig = @"D:\Work\AppData";
            var appConfigRoot = Path.Combine(userConfig, "OlmDevTool");
            var appConfig = UI.Core.Utilities.CreateFolder(new string[] { appConfigRoot, "Config" });
            var appData = UI.Core.Utilities.CreateFolder(new string[] { appConfigRoot, "Data" });
            _applicationConfigurationFile = Path.Combine(appConfig, "Application.yaml");


            _applicationModel = new ApplicationModel();
            _applicationModel.Initialize(_applicationConfigurationFile, appData);

            desktop.MainWindow = new MainWindow { DataContext = new MainWindowViewModel(_applicationModel) };
            desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnMainWindowClose;
            desktop.ShutdownRequested += Lifetime_ShutdownRequested;


        }

        base.OnFrameworkInitializationCompleted();
    }

    private void Lifetime_ShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        _applicationModel.Shutdown(_applicationConfigurationFile);
    }
}