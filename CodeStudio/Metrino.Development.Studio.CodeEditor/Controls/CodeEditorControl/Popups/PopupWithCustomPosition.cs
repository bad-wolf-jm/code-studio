using Avalonia;
using Avalonia.Controls.Primitives;

namespace Metrino.Development.Studio.Library.Controls;

internal class PopupWithCustomPosition : Popup
{
    public Point Offset
    {
        get
        {
            return new Point(HorizontalOffset, VerticalOffset);
        }

        set
        {
            HorizontalOffset = value.X;
            VerticalOffset = value.Y;
        }
    }
}