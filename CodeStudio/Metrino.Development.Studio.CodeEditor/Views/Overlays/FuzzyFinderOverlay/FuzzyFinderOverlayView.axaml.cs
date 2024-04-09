using Avalonia.Controls;

namespace Metrino.Development.Studio.Library
{
    public partial class FuzzyFinderOverlayView: UserControl
    {
        public FuzzyFinderOverlayView()
        {
            InitializeComponent();

            TextPresenterControl.ShowCaret();
        }
    }
}
