using Avalonia.Controls;
using Metrino.Development.Studio.Library.ViewModels;

namespace Metrino.Development.Studio.Library;

public partial class GitBranchSelectorOverlay : UserControl
{
    public GitBranchSelectorOverlay()
    {
        InitializeComponent();

        BranchList.DoubleTapped += BranchList_DoubleTapped;
        TextPresenterControl.ShowCaret();
    }

    private void BranchList_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        var viewModel = DataContext as ProjectBuilderDocumentViewModel;
        if(viewModel != null)
        {
            viewModel.SwitchToBranch(BranchList.SelectedItem);
        }
    }
}