using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Metrino.Development.Studio.Library.Messages;
using Metrino.Development.Studio.Library.ViewModels;
using Metrino.Development.UI.Core;
using System.Threading.Tasks;
using Avalonia.Input;
using System;
using Metrino.Development.UI.ViewModels;
using Avalonia.Threading;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace Metrino.Development.Studio.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public FileTreeViewModel ExplorerViewModel { get; set; }
    public WorkspaceViewModel Workspace { get; set; }

    [ObservableProperty]
    bool _commandMode = true;

    [ObservableProperty]
    bool _overlayVisible = false;

    [ObservableProperty]
    object _overlayContent = null;

    [ObservableProperty]
    string _currentPath = string.Empty;

    [ObservableProperty]
    string _currentBranch = string.Empty;

    [ObservableProperty]
    string _currentStatus = string.Empty;

    [ObservableProperty]
    string _currentMode = string.Empty;

    DispatcherTimer _commandResetTimer;
    double _commandTimeout = 500.0;
    double _timeSinceLastInput = 0.0;

    object _currentFocusPane = null;

    ApplicationModel _app;
    public MainWindowViewModel(ApplicationModel app)
    {
        _app = app;
        ExplorerViewModel = new FileTreeViewModel(_app.FileSystem);
        Workspace = new WorkspaceViewModel(_app);

        _currentFocusPane = ExplorerViewModel;

        WeakReferenceMessenger.Default.Register<DisplayOverlayMessage>(this, (r, message) =>
        {
            OverlayContent = message.Value;
            OverlayVisible = true;
        });

        WeakReferenceMessenger.Default.Register<DismissOverlayMessage>(this, (r, message) =>
        {
            OverlayContent = null;
            OverlayVisible = false;
        });

        Workspace.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(WorkspaceViewModel.CurrentPath))
            {
                CurrentPath = Workspace.CurrentPath;
            }
        };

        CurrentPath = Workspace.CurrentPath;
        CurrentBranch = _app.GitBranches.CurrentHead.FriendlyName;
        CurrentStatus = $"[+{_app.GitBranches.Added} ~{_app.GitBranches.Modified} -{_app.GitBranches.Removed}]";
        _app.GitBranches.PropertyChanged += GitBranches_PropertyChanged;

        _commandResetTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(10), IsEnabled = true };

        _commandResetTimer.Tick += _commandResetTimer_Tick;
        _commandResetTimer.Start();
    }

    private void _commandResetTimer_Tick(object? sender, EventArgs e)
    {
        _timeSinceLastInput += _commandResetTimer.Interval.TotalMilliseconds;
        if (_timeSinceLastInput > _commandTimeout)
            CurrentKeySequence = "";
    }

    private void GitBranches_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            CurrentBranch = _app.GitBranches.CurrentHead.FriendlyName;
            CurrentStatus = $"[+{_app.GitBranches.Added} ~{_app.GitBranches.Modified} -{_app.GitBranches.Removed}]";
        });
    }

    [RelayCommand]
    async Task OpenFile(object parameter)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var fileName = await Library.ViewModels.Utilities.SelectFile(desktop.MainWindow);
            WeakReferenceMessenger.Default.Send(new OpenFileMessage(fileName));
        }
    }

    [RelayCommand]
    async Task OpenFolder(object parameter)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var folderName = await Library.ViewModels.Utilities.SelectFolder(desktop.MainWindow);
            ExplorerViewModel.AddRoot(folderName);
        }
    }

    [RelayCommand]
    async Task SaveCurrentFileAs(object parameter)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var fileName = await Library.ViewModels.Utilities.SelectFileName(desktop.MainWindow);
            await Workspace.SaveCurrentFileAs(fileName);
        }
    }

    [ObservableProperty]
    string _currentKeySequence = string.Empty;

    string KeySymbol(PhysicalKey physicalKey, string? symbol, bool shift)
    {
        if (!string.IsNullOrEmpty(symbol))
            return symbol;

        switch (physicalKey)
        {
            case PhysicalKey.ArrowLeft:
                return "Left";
            case PhysicalKey.ArrowRight:
                return "Right";
            case PhysicalKey.ArrowUp:
                return "Up";
            case PhysicalKey.ArrowDown:
                return "Down";
            case PhysicalKey.Enter:
                return "CR";
            case PhysicalKey.Space:
                return "Space";
            case PhysicalKey.Tab:
                return "Tab";
            case PhysicalKey.Escape:
                return "ESC";
            default:
                break;
        }

        if ((physicalKey >= PhysicalKey.NumPad0) && (physicalKey <= PhysicalKey.NumPad9))
            return $"k{physicalKey}";

        if (physicalKey >= PhysicalKey.A && physicalKey <= PhysicalKey.Z)
        {
            int index = physicalKey - PhysicalKey.A;
            char ch = (char)(index + (shift ? 65 : 97));

            return ch.ToString();
        }

        string numbers = "`\\[],0123456789=";
        string shiftedNumbers = "~|{}<)!@#$%^&*(+";
        if (physicalKey >= PhysicalKey.Backquote && physicalKey <= PhysicalKey.Digit9)
            return (shift ? shiftedNumbers[physicalKey - PhysicalKey.Backquote] : numbers[physicalKey - PhysicalKey.Backquote]).ToString();

        if ((physicalKey >= PhysicalKey.F1) && (physicalKey <= PhysicalKey.F24))
            return $"{physicalKey}";

        string punctuation = "-.';/";
        string shiftedPunctuation = "_>\":?";
        if (physicalKey >= PhysicalKey.Minus && physicalKey <= PhysicalKey.Slash)
            return (shift ? shiftedPunctuation[physicalKey - PhysicalKey.Minus] : punctuation[physicalKey - PhysicalKey.Minus]).ToString();

        return string.Empty;
    }

    bool IsPrintable(PhysicalKey physicalKey)
    {
        if (physicalKey >= PhysicalKey.Backquote && physicalKey <= PhysicalKey.Slash)
            return true;

        return false;
    }

    string KeyToString(PhysicalKey physicalKey, string? symbol, KeyModifiers keyModifiers)
    {
        var sequence = new List<string>();

        if (((keyModifiers & KeyModifiers.Shift) == KeyModifiers.Shift) && !IsPrintable(physicalKey))
            sequence.Add("S");

        if ((keyModifiers & KeyModifiers.Control) == KeyModifiers.Control)
            sequence.Add("C");

        if ((keyModifiers & KeyModifiers.Alt) == KeyModifiers.Alt)
            sequence.Add("M");

        var keySymbol = KeySymbol(physicalKey, symbol, (keyModifiers & KeyModifiers.Shift) == KeyModifiers.Shift);

        if (sequence.Count > 0)
            return $"<{string.Join("-", sequence)}-{keySymbol}>";

        return keySymbol;
    }

    public void KeyPressed(PhysicalKey physicalKey, string? symbol, KeyModifiers keyModifiers)
    {
        if (OverlayVisible)
        {
            if (OverlayContent is IOverlay overlay)
            {
                // overlay gets the keypress
                overlay.HandleKeyPress(physicalKey, symbol, keyModifiers);
                return;
            }
        }

        switch (physicalKey)
        {
            // Ignore the modifier keys, they just appear as themselves modified by themselves
            case PhysicalKey.ControlLeft:
            case PhysicalKey.ControlRight:
            case PhysicalKey.ShiftLeft:
            case PhysicalKey.ShiftRight:
            case PhysicalKey.AltLeft:
            case PhysicalKey.AltRight:
                return;
            default:
                break;
        }


        var keyString = KeyToString(physicalKey, symbol, keyModifiers);
        _timeSinceLastInput = 0.0;

        if (keyString == ":")
            CurrentKeySequence = "";
        else
            CurrentKeySequence += keyString;


        switch(CurrentKeySequence)
        {
            case "<C-w>Left":
                _currentFocusPane = ExplorerViewModel;
                ExplorerViewModel.Focus();
                CurrentKeySequence = "";
                break;
            case "<C-w>Right":
                _currentFocusPane = Workspace;
                Workspace.Focus();
                CurrentKeySequence = "";
                break;
            case "i":
                CommandMode = false;
                _currentFocusPane = Workspace;
                Workspace.Focus();
                CurrentKeySequence = "";
                break;
            default:
                {
                    if(_currentFocusPane == Workspace)
                    {
                        if(Workspace.HandleKeySequence(CurrentKeySequence))
                        {
                            CurrentKeySequence = "";
                        }
                    }
                    else if (_currentFocusPane == ExplorerViewModel)
                    {
                        if (ExplorerViewModel.HandleKeySequence(CurrentKeySequence))
                        {
                            CurrentKeySequence = "";
                        }
                    }
                }
                break;
        }
    }

    public async Task ExecuteCommand(string? command)
    {
        switch (command)
        {
            case ":Tabs":
                await Workspace.DisplayTabChooserCommand.ExecuteAsync(null);
                break;
            case ":Info":
                await Workspace.DisplayInfoOverlayCommand.ExecuteAsync(null);
                break;
            case ":Settings":
                await Workspace.DisplaySettingsCommand.ExecuteAsync(null);
                break;
            case ":Checkout":
                await Workspace.DisplayOverlayCommand.ExecuteAsync(null);
                break;
            case ":Dismiss":
                OverlayVisible = false;
                break;
            case ":Run":
                await Workspace.RunCurrentFileCommand.ExecuteAsync(null);
                break;
            case ":OpenFolder":
                break;
            case ":OpenFile":
                break;
            case ":q":
                Workspace.CloseDocument();
                break;
            case ":qa!":
                Workspace.CloseAllCommand.Execute(null);
                break;
            case ":qa":
                Workspace.CloseAllSavedCommand.Execute(null);
                break;
            case ":wa":
                await Workspace.SaveAllFilesCommand.ExecuteAsync(null);
                break;
            case ":w":
                await Workspace.SaveCurrentFileCommand.ExecuteAsync(null);
                break;
            default:
                break;
        }
    }
}