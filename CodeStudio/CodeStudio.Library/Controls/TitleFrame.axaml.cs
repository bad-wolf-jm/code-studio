using Avalonia;
using Avalonia.Controls;

namespace Metrino.Development.Studio.Library.Controls;

public class TitleFrame : ContentControl
{
    public static readonly StyledProperty<string> TitleProperty
        = AvaloniaProperty.Register<TitleFrame, string>(nameof(Title));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
}