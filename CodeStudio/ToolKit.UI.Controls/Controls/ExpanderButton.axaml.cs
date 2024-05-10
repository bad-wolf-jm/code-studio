using Avalonia;
using Avalonia.Controls.Primitives;

namespace ToolKit.UI.Controls;

public class ExpanderButton : TemplatedControl
{
    public static readonly StyledProperty<bool> IsExpandedProperty
        = AvaloniaProperty.Register<MaterialIcon, bool>(nameof(IsExpanded));

    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }
}