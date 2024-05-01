using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Metrino.Development.Studio.Library.ViewModels;

namespace Metrino.Development.Studio.Library;

public partial class Workspace : UserControl
{
    public Workspace()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        (DataContext as WorkspaceViewModel).NotificationManager =
            new WindowNotificationManager(TopLevel.GetTopLevel(this)!)
            {
                Position = NotificationPosition.BottomRight
            };
    }

}