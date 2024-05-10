using Avalonia.Controls.Primitives;
using Avalonia;
using Material.Icons;

namespace Metrino.Development.Studio.Library.Controls;

public class MaterialIcon : TemplatedControl
{
    public static readonly StyledProperty<MaterialIconKind> KindProperty
        = AvaloniaProperty.Register<MaterialIcon, MaterialIconKind>(nameof(Kind));

    /// <summary>
    /// Gets or sets the icon to display.
    /// </summary>
    public MaterialIconKind Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }
}
