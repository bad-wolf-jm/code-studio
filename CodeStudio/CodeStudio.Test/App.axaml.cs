using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Metrino.Development.Studio.Test.ViewModels;
using Metrino.Development.Studio.Test.Views;
using Metrino.Development.UI.Core;
using System.IO;

namespace Metrino.Development.Studio.Test
{
    public partial class App : Application
    {
        private ApplicationModel _applicationModel;
        private string _applicationConfigurationFile;
        private string _applicationdataRoot;
        MainWindowViewModel mainWindowViewModel;
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
                var userConfig = @"c:\Work\AppData";
                var appConfigRoot = Path.Combine(userConfig, "OlmDevTool_dev");
                var appConfig = UI.Core.Utilities.CreateFolder(new string[] { appConfigRoot, "Config" });
                var appData = UI.Core.Utilities.CreateFolder(new string[] { appConfigRoot, "Data" });
                _applicationConfigurationFile = Path.Combine(appConfig, "Application.yaml");

                _applicationModel = new ApplicationModel();
                _applicationModel.Initialize(_applicationConfigurationFile, appData);

                mainWindowViewModel = new MainWindowViewModel(_applicationModel);
                mainWindowViewModel.Initialize();

                desktop.MainWindow = new MainWindow { DataContext = mainWindowViewModel };
                desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnMainWindowClose;
                desktop.ShutdownRequested += Lifetime_ShutdownRequested;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void Lifetime_ShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
        {
            _applicationModel.Shutdown(_applicationConfigurationFile);
            mainWindowViewModel.Dispose();
        }
    }
}   