using Avalonia;
using Avalonia.Controls.Primitives;
using System.Collections.Generic;

namespace Metrino.Development.Studio.Library.Controls;

public class PrettyFilePath : TemplatedControl
{
    public static readonly StyledProperty<string> PathProperty
        = AvaloniaProperty.Register<PrettyFilePath, string>(nameof(Path));

    public string Path
    {
        get => GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    public static readonly StyledProperty<IEnumerable<string>> PathComponentsProperty
        = AvaloniaProperty.Register<PrettyFilePath, IEnumerable<string>>(nameof(PathComponents));

    public IEnumerable<string> PathComponents
    {
        get => GetValue(PathComponentsProperty);
        set => SetValue(PathComponentsProperty, value);
    }

    static PrettyFilePath()
    {
        PathProperty.Changed.AddClassHandler<PrettyFilePath>(OnPathChanged);
    }

    private static void OnPathChanged(PrettyFilePath control, AvaloniaPropertyChangedEventArgs args)
    {
        control.PathComponents = control.Path?.Split("\\") ?? new string[] { };
    }
}