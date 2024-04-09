using Avalonia.Controls;
using Metrino.Development.Studio.Library.ViewModels;

namespace Metrino.Development.Studio.Library;

public partial class TabChooserOverlay : UserControl
{
    public TabChooserOverlay()
    {
        InitializeComponent();

        DocumentList.DoubleTapped += DocumentList_DoubleTapped;

    }

    private void DocumentList_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        var viewModel = DataContext as TabChooserViewModel;
        if (viewModel != null)
        {
            viewModel.SelectDocument(DocumentList.SelectedItem);
        }
    }
}