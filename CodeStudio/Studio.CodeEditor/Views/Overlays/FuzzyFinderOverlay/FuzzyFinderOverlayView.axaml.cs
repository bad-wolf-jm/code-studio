using Avalonia.Controls;

namespace Studio.Library
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
