using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Metrino.Development.Studio.ViewModels;
using System.Threading.Tasks;

namespace Metrino.Development.Studio.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        KeyDownEvent.AddClassHandler<TopLevel>(OnKeyDown);
        CommandLine.KeyDown += CommandLine_KeyDown;
        Focusable = true;
        GotFocus += MainWindow_GotFocus;
    }

    private void MainWindow_GotFocus(object? sender, GotFocusEventArgs e)
    {
        _commandLineFocused = false;

        var viewModel = DataContext as MainWindowViewModel;
        if (viewModel == null)
            return;

        viewModel.CommandMode = true;
    }

    private async void CommandLine_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CommandLine.Text = "";

            Focus();
        }

        if (e.Key == Key.Enter)
        {
            string? command = CommandLine?.Text;
            CommandLine.Text = "";

            Focus();

            var viewModel = DataContext as MainWindowViewModel;
            if (viewModel == null)
                return;

            await viewModel.ExecuteCommand(command);
            e.Handled = true;
        }
    }

    bool _commandLineFocused = false;

    private void OnKeyDown(TopLevel level, KeyEventArgs args)
    {
        var viewModel = DataContext as MainWindowViewModel;
        if (viewModel == null)
            return;

        if(_commandLineFocused)
        {
            args.Handled = false;
        }
        else if (viewModel.CommandMode)
        {
            if (args.KeySymbol == ":")
            {
                CommandLine.Text = ":";
                CommandLine.CaretIndex = 1;
                CommandLine.Focus();

                _commandLineFocused = true;
            }
            else
            {
                viewModel.KeyPressed(args.PhysicalKey, args.KeySymbol, args.KeyModifiers);

                if(!viewModel.CommandMode)
                {

                }
            }

            args.Handled = true;
        }
        else if (args.PhysicalKey == PhysicalKey.Escape)
        {
            viewModel.CommandMode = true;
        }
    }

    public void MinimizeWindow(object aSender, RoutedEventArgs args)
    {
        WindowState = WindowState.Minimized;
    }

    public void MaximizeWindow(object aSender, RoutedEventArgs arga)
    {
        if (WindowState == WindowState.Normal)
            WindowState = WindowState.Maximized;
        else
            WindowState = WindowState.Normal;
    }

    private void ExitApplication(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private bool _mouseDownForWindowMoving = false;
    private PointerPoint _originalPoint;

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_mouseDownForWindowMoving) return;

        PointerPoint currentPoint = e.GetCurrentPoint(this);
        Position = new PixelPoint(Position.X + (int)(currentPoint.Position.X - _originalPoint.Position.X),
            Position.Y + (int)(currentPoint.Position.Y - _originalPoint.Position.Y));
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (WindowState == WindowState.Maximized || WindowState == WindowState.FullScreen) return;

        _mouseDownForWindowMoving = true;
        _originalPoint = e.GetCurrentPoint(this);
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _mouseDownForWindowMoving = false;
    }

}